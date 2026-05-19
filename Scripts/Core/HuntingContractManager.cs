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
			// 수락 시점에 이미 보유한 재료를 즉시 진행도에 반영(Gather). NotifyItemAcquired
			// 가 유일 재계산 경로면 "수락 전부터 가진 재료"로는 영영 완료 불가했던 결함 차단.
			RecomputeGatherProgress();
			// 수락 직후 하드 크래시까지 보존하도록 Abandon/Complete 와 일관되게 즉시 SaveGame.
			// (UI 버튼 트리거라 빈번하지 않음. 트랜잭션 중이면 SaveManager 가드가 dirty 로 deferred.)
			OnContractStateChanged?.Invoke();
			SaveManager.SaveGame();
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
		public bool Complete(string contractId, IPlayer player, out bool rewardDeferred)
		{
			rewardDeferred = false;
			if (player == null) return false;
			var prog = GetProgress(contractId);
			if (prog == null) return false;
			var def = ContractsData.Find(contractId);
			if (def == null) return false;

			// Gather(납품형) — turn-in 시 목표 수량을 실제로 보유해야 하고 소모한다.
			// 완료 가능 여부는 *현재 인벤 보유*로 재계산한 뒤 판정한다(수락 전부터
			// 갖고 있던 재료, 또는 NotifyItemAcquired 미발화 상태도 인정 — TurnInReady
			// 게이트를 재계산 *뒤*로 두는 게 핵심). 비Gather는 기존 TurnInReady 사용.
			ItemData gatherItem = null;
			if (def.Type == ContractType.Gather)
			{
				if (string.IsNullOrEmpty(def.TargetItemPath)) return false;
				gatherItem = GD.Load<ItemData>(def.TargetItemPath);
				var inv0 = player.Inventory;
				int have = (gatherItem != null && inv0 != null) ? inv0.CountItem(gatherItem) : 0;
				prog.Progress = Math.Min(have, def.Goal);
				prog.TurnInReady = prog.Progress >= def.Goal && def.Goal > 0;
				if (!prog.TurnInReady)
				{
					OnContractStateChanged?.Invoke();
					GD.Print($"[Contract] 재료 부족 — {def.Title} 완료 보류 ({have}/{def.Goal})");
					return false;
				}
			}
			else if (!prog.TurnInReady) return false;

			using (GameTransaction.Begin())
			{
				// 납품 재료 소모 — 검증 통과 후 트랜잭션 내에서 원자 차감.
				if (def.Type == ContractType.Gather && gatherItem != null)
					player.Inventory.ConsumeItems(gatherItem, def.Goal);

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
						{
							GameManager.Instance?.AddPendingReward(item, def.RewardItemQuantity);
							rewardDeferred = true;
						}
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

		/// <summary>Gather(납품형) — 진행도 = *현재 인벤 보유 수량*(누적 아님). 완료 시
		/// Complete()가 목표 수량을 실제 소모한다(A안). 누적형이 아니므로 구매/판매/채광
		/// 이중카운트·수량 미반영 익스플로잇이 원천 차단된다. PlayerController의
		/// Inventory.OnItemPickedUp에서 호출.</summary>
		public void NotifyItemAcquired(ItemData item)
		{
			if (item == null || string.IsNullOrEmpty(item.ResourcePath)) return;
			if (RecomputeGatherProgress())
			{
				OnContractStateChanged?.Invoke();
				// 보유 기반이라 즉시 영속(인벤 autosave 순서 경합 차단). SaveGame은
				// 트랜잭션 중이면 자동 deferred(SaveManager 가드).
				SaveManager.SaveGame();
			}
		}

		/// <summary>모든 active Gather 계약의 진행도/완료대기를 *현재 인벤 보유*로
		/// 재계산. 변경이 있으면 true. 단일 재계산 경로 — Accept/복원후/획득 모두
		/// 이걸 호출해 "수락 전 보유 재료가 영영 미반영"되는 결함을 막는다.
		/// (OnContractStateChanged/SaveGame 은 호출자가 필요 시 발화.)</summary>
		public bool RecomputeGatherProgress()
		{
			ContractsData.EnsureLoaded();
			var inv = GameManager.Instance?.Player?.Inventory;
			if (inv == null) return false;
			bool changed = false;
			foreach (var prog in _active)
			{
				var def = ContractsData.Find(prog.ContractId);
				if (def == null || def.Type != ContractType.Gather) continue;
				if (string.IsNullOrEmpty(def.TargetItemPath)) continue;
				var it = GD.Load<ItemData>(def.TargetItemPath);
				if (it == null) continue;
				int have = Math.Min(inv.CountItem(it), def.Goal);
				bool ready = have >= def.Goal && def.Goal > 0;
				if (prog.Progress != have || prog.TurnInReady != ready)
				{
					prog.Progress = have;
					prog.TurnInReady = ready;
					changed = true;
				}
			}
			return changed;
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
				// Advance 는 Kill/BossKill/Mining 전용(Gather 는 NotifyItemAcquired 가
				// 보유기반으로 별도 처리). 이들은 빈번하거나(Kill) 직후 SaveGame 경로가
				// 따로 있어(Mining=MiningNode, Boss=EnemyController) 일반 autosave 수준
				// throttle 로 충분 — RequestAutoSave 유지(트랜잭션 중이면 dirty만).
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
					if (canFilter && ContractsData.Find(p.ContractId) == null)
					{
						GD.Print($"[Contract] 알 수 없는 계약 id 폐기: {p.ContractId}");
						continue;
					}
					// NOTE: "처치된 비반복 보스라 진행 불가" 폐기는 여기서 하지 않는다.
					// GameManager.DefeatedBosses 복원 순서에 의존하면(현재는 우연히 맞음)
					// 향후 복원 순서 변경 시 조용히 무력화된다. 대신 전체 복원이 끝난 뒤
					// PruneUnobtainable()을 호출(PlayerController.LoadFromSaveData 말미)해
					// 순서 비의존으로 처리한다.
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

		/// <summary>전체 세이브 복원이 끝난 뒤(=GameManager.DefeatedBosses 확정 이후)
		/// 1회 호출. 진행 불가(처치된 비반복 보스) 미완료 계약을 죽은 슬롯에서 제거한다.
		/// RestoreFromSave 와 분리해 복원 호출 순서에 의존하지 않게 한다.</summary>
		public void PruneUnobtainable()
		{
			ContractsData.EnsureLoaded();
			int before = _active.Count;
			_active.RemoveAll(p =>
			{
				if (p.TurnInReady) return false; // 수령만 하면 되므로 보스 재스폰 불필요 — 유지
				var def = ContractsData.Find(p.ContractId);
				if (def != null && IsBossContractUnobtainable(def))
				{
					GD.Print($"[Contract] 진행 불가 보스 계약 폐기: {p.ContractId}");
					return true;
				}
				return false;
			});
			if (_active.Count != before) OnContractStateChanged?.Invoke();
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
