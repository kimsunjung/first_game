using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Core
{
	/// <summary>
	/// 사냥 계약 매니저 — 메인 QuestManager와 완전히 독립.
	/// 동시 최대 3개 수락, 완료 후 같은 계약 재수락 가능(반복 파밍 보조).
	/// 일일/주간/시간제한/숙제형 리셋 없음.
	/// 인스턴스 1개를 GameManager가 영구 보유. EventManager.ResetAll() 후
	/// Resubscribe로 적/보스/채광 이벤트 구독을 복원한다.
	/// </summary>
	public class HuntingContractManager
	{
		public const int MaxActive = 3;

		// 진행 상태 — 세이브 직렬화 대상. 순서 보존(UI 안정).
		private readonly List<ContractProgress> _active = new();
		public IReadOnlyList<ContractProgress> Active => _active;

		public event Action OnContractStateChanged;

		public HuntingContractManager()
		{
			Resubscribe();
		}

		public void Dispose()
		{
			EventManager.OnEnemyKilledTyped -= HandleEnemyKilled;
			EventManager.OnBossKilled -= HandleBossKilled;
			EventManager.OnOreMined -= HandleOreMined;
		}

		/// <summary>EventManager.ResetAll() 직후 호출 — 영구 인스턴스라 구독을 잃지 않게
		/// 재구독. 중복 방지 위해 -= 후 += (QuestManager.Resubscribe와 동일 패턴).
		/// Gather는 Inventory.OnItemPickedUp 경유(PlayerController가 NotifyItemAcquired 호출)
		/// 라 여기서 다루지 않는다.</summary>
		public void Resubscribe()
		{
			EventManager.OnEnemyKilledTyped -= HandleEnemyKilled;
			EventManager.OnEnemyKilledTyped += HandleEnemyKilled;
			EventManager.OnBossKilled -= HandleBossKilled;
			EventManager.OnBossKilled += HandleBossKilled;
			EventManager.OnOreMined -= HandleOreMined;
			EventManager.OnOreMined += HandleOreMined;
		}

		// ─── 조회 ──────────────────────────────────────────────
		public bool IsActive(string contractId)
		{
			foreach (var p in _active) if (p.ContractId == contractId) return true;
			return false;
		}

		public ContractProgress GetProgress(string contractId)
		{
			foreach (var p in _active) if (p.ContractId == contractId) return p;
			return null;
		}

		public bool CanAccept(string contractId)
		{
			if (string.IsNullOrEmpty(contractId)) return false;
			if (_active.Count >= MaxActive) return false;
			if (IsActive(contractId)) return false;
			return ContractsData.Find(contractId) != null;
		}

		/// <summary>지정 권역에서 지금 수락 가능한 계약 목록.</summary>
		public List<ContractData> AvailableForRegion(string region)
		{
			ContractsData.EnsureLoaded();
			var list = new List<ContractData>();
			foreach (var c in ContractsData.Contracts)
			{
				if (c.Region != region) continue;
				if (IsActive(c.Id)) continue;
				list.Add(c);
			}
			return list;
		}

		// ─── 수락/포기/완료 ────────────────────────────────────
		public bool Accept(string contractId)
		{
			ContractsData.EnsureLoaded();
			if (!CanAccept(contractId)) return false;
			_active.Add(new ContractProgress { ContractId = contractId, Progress = 0, TurnInReady = false });
			// 수락 직후 크래시/강제종료에도 유실되지 않도록 dirty 표시(throttle 경유 —
			// 포기/완료의 즉시 SaveGame과 달리 가벼운 영속). FlushBeforeExit/TickDirty가 flush.
			SaveManager.RequestAutoSave();
			OnContractStateChanged?.Invoke();
			return true;
		}

		/// <summary>보상 없이 active에서 제거. 포기는 즉시 저장(spec).</summary>
		public bool Abandon(string contractId)
		{
			int idx = _active.FindIndex(p => p.ContractId == contractId);
			if (idx < 0) return false;
			_active.RemoveAt(idx);
			OnContractStateChanged?.Invoke();
			SaveManager.SaveGame();
			return true;
		}

		public bool IsReady(string contractId)
		{
			var p = GetProgress(contractId);
			return p != null && p.TurnInReady;
		}

		/// <summary>
		/// 완료 보상 수령 + active 제거. 보상은 골드/EXP는 항상, 아이템은 인벤 부족 시
		/// PendingReward로 영속(손실 없음). QuestManager.CompleteQuest와 동일하게
		/// GameTransaction으로 중간상태(autosave/pending claim) 격리 후 명시 SaveGame.
		/// 성공 시 true. 미완료/미존재면 false(상태 불변).
		/// </summary>
		public bool Complete(string contractId, IPlayer player)
		{
			if (player == null) return false;
			var prog = GetProgress(contractId);
			if (prog == null || !prog.TurnInReady) return false;
			var def = ContractsData.Find(contractId);
			if (def == null) return false;

			using (GameTransaction.Begin())
			{
				if (def.GoldReward > 0)
					GameManager.Instance.PlayerGold += def.GoldReward;
				if (def.ExpReward > 0)
					EventManager.TriggerExpGained(def.ExpReward);

				if (!string.IsNullOrEmpty(def.RewardItemPath) && def.RewardItemQuantity > 0)
				{
					var item = GD.Load<ItemData>(def.RewardItemPath);
					if (item != null)
					{
						var inv = player.Inventory;
						bool added = inv != null && inv.CanAddItem(item, def.RewardItemQuantity)
							&& inv.AddItem(item, def.RewardItemQuantity, 0, fireAcquired: false);
						// 인벤 부족/추가 실패 시 PendingReward — 영속 보관, 손실 없음.
						if (!added)
							GameManager.Instance?.AddPendingReward(item, def.RewardItemQuantity);
					}
				}

				_active.RemoveAll(p => p.ContractId == contractId);
			}
			OnContractStateChanged?.Invoke();
			SaveManager.SaveGame();
			GD.Print($"[Contract] 완료: {def.Title}");
			return true;
		}

		// ─── 진행도 이벤트 핸들러 ─────────────────────────────
		private void HandleEnemyKilled(string enemyTypeName)
		{
			if (string.IsNullOrEmpty(enemyTypeName)) return;
			// 엘리트 affix prefix 제거 — QuestManager와 동일 유틸 사용(매칭 일관성).
			string normalized = EliteAffixUtil.StripElitePrefix(enemyTypeName);
			Advance(ContractType.Kill, c => c.TargetEnemyType == normalized);
		}

		private void HandleBossKilled(string bossId)
		{
			if (string.IsNullOrEmpty(bossId)) return;
			Advance(ContractType.BossKill, c => c.TargetBossId == bossId);
		}

		private void HandleOreMined(string oreItemPath)
		{
			if (string.IsNullOrEmpty(oreItemPath)) return;
			Advance(ContractType.Mining, c => c.TargetOreItemPath == oreItemPath);
		}

		/// <summary>Gather — 획득 누적형. PlayerController의 Inventory.OnItemPickedUp에서 호출.
		/// 완료 시 재료 차감 없음(v1 단순화: 손실 위험 제거).</summary>
		public void NotifyItemAcquired(ItemData item)
		{
			if (item == null || string.IsNullOrEmpty(item.ResourcePath)) return;
			Advance(ContractType.Gather, c => c.TargetItemPath == item.ResourcePath);
		}

		private void Advance(ContractType type, Func<ContractData, bool> match)
		{
			ContractsData.EnsureLoaded();
			bool changed = false;
			foreach (var prog in _active)
			{
				if (prog.TurnInReady) continue;
				var def = ContractsData.Find(prog.ContractId);
				if (def == null || def.Type != type || !match(def)) continue;
				prog.Progress = Math.Min(prog.Progress + 1, def.Goal);
				if (prog.Progress >= def.Goal) prog.TurnInReady = true;
				changed = true;
			}
			if (changed) OnContractStateChanged?.Invoke();
		}

		// ─── 세이브 ────────────────────────────────────────────
		public void RestoreFromSave(List<ContractProgress> saved)
		{
			_active.Clear();
			if (saved != null)
			{
				// contracts.json 로드 후, 더 이상 존재하지 않는 계약 id는 버린다.
				// (id 변경/삭제 시 보이지 않는 계약이 동시 3개 한도를 점유하는 결함 차단.)
				ContractsData.EnsureLoaded();
				bool canFilter = ContractsData.Contracts.Count > 0;
				foreach (var p in saved)
				{
					if (p == null || string.IsNullOrEmpty(p.ContractId)) continue;
					if (canFilter && ContractsData.Find(p.ContractId) == null)
					{
						GD.Print($"[Contract] 알 수 없는 계약 id 폐기: {p.ContractId}");
						continue;
					}
					_active.Add(new ContractProgress
					{
						ContractId = p.ContractId,
						Progress = p.Progress,
						TurnInReady = p.TurnInReady
					});
				}
			}
			OnContractStateChanged?.Invoke();
		}

		public List<ContractProgress> ToSaveList()
		{
			var outList = new List<ContractProgress>(_active.Count);
			foreach (var p in _active)
				outList.Add(new ContractProgress
				{
					ContractId = p.ContractId,
					Progress = p.Progress,
					TurnInReady = p.TurnInReady
				});
			return outList;
		}
	}
}
