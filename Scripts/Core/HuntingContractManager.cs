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

		// EnemySpawner.RepeatableBoss=true 인 보스만 반복 파밍 가능. 그 외 스토리 보스
		// (orc_warlord_d1/skeleton_king_d2/ancient_lich_d3)는 1회 처치 후 재스폰되지
		// 않으므로(EnemySpawner.TrySpawnBoss 억제), 그 보스를 노린 BossKill 계약은
		// "1회성 현상금"으로 다룬다 — 이미 처치했으면 수락/복원 불가(영구 불가 계약 차단).
		private static readonly System.Collections.Generic.HashSet<string> RepeatableBossIds = new()
		{
			"kraken_d4", "glacier_titan_f5", "inferno_drake_f6", "crystal_lord_m3",
		};

		// 진행 상태 — 세이브 직렬화 대상. 순서 보존(UI 안정).
		private readonly List<ContractProgress> _active = new();
		public IReadOnlyList<ContractProgress> Active => _active;

		/// <summary>BossKill 계약인데 비반복 보스를 이미 처치해 다시는 스폰되지 않는 상태
		/// (영구 진행 불가)면 true. 데이터/보스 미해결이면 false(보수적으로 허용).</summary>
		private static bool IsBossContractUnobtainable(ContractData c)
		{
			if (c == null || c.Type != ContractType.BossKill) return false;
			if (string.IsNullOrEmpty(c.TargetBossId)) return false;
			if (RepeatableBossIds.Contains(c.TargetBossId)) return false;
			return GameManager.Instance?.IsBossDefeated(c.TargetBossId) == true;
		}

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
			var def = ContractsData.Find(contractId);
			if (def == null) return false;
			// 이미 처치된 비반복 스토리 보스 계약은 재수락 불가(1회성 현상금).
			if (IsBossContractUnobtainable(def)) return false;
			return true;
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
				// 영구 진행 불가(처치된 비반복 보스) 계약은 보드에 노출하지 않음.
				if (IsBossContractUnobtainable(c)) continue;
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
			if (changed)
			{
				OnContractStateChanged?.Invoke();
				// 진행도 영속화 — Inventory.AddItem이 OnItemPickedUp 보다 *먼저*
				// OnInventoryChanged→autosave를 발생시켜, 이 진행 증가가 다음 저장에서
				// 누락되는 창을 막는다(GameTransaction 중에는 dirty만 표시 후 종료 시 flush).
				SaveManager.RequestAutoSave();
			}
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
					var def = canFilter ? ContractsData.Find(p.ContractId) : null;
					if (canFilter && def == null)
					{
						GD.Print($"[Contract] 알 수 없는 계약 id 폐기: {p.ContractId}");
						continue;
					}
					// 완료대기(TurnInReady)면 보스 재스폰 불필요 — 그대로 수령 가능하므로 유지.
					// 미완료 + 처치된 비반복 보스면 영구 진행 불가라 폐기(죽은 슬롯 차단).
					if (def != null && !p.TurnInReady && IsBossContractUnobtainable(def))
					{
						GD.Print($"[Contract] 진행 불가 보스 계약 폐기: {p.ContractId}");
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
