using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Core
{
	/// <summary>
	/// 메인 퀘스트 1개씩만 진행. NPC 상호작용으로 부여/완료.
	/// EventManager의 적 처치 / Inventory 픽업 / 씬 진입 이벤트로 진행도 업데이트.
	/// 인스턴스 1개를 GameManager가 보유.
	/// </summary>
	public class QuestManager
	{
		public QuestData ActiveQuest { get; private set; }
		public int Progress { get; private set; }
		public HashSet<string> CompletedQuestIds { get; } = new();

		public event Action OnQuestStateChanged;

		public bool HasActiveQuest => ActiveQuest != null;
		public bool IsActiveQuestComplete =>
			HasActiveQuest && Progress >= ActiveQuest.TargetCount;

		public QuestManager()
		{
			Resubscribe();
		}

		public void Dispose()
		{
			EventManager.OnEnemyKilledTyped -= HandleEnemyKilled;
		}

		/// <summary>
		/// EventManager.ResetAll() 직후 호출해 적 처치 이벤트 구독을 복원.
		/// QuestManager는 GameManager 영구 인스턴스이므로 새 게임 시작 후에도
		/// Kill 진행도 추적이 끊기지 않도록 명시적으로 다시 구독한다.
		/// 중복 등록 방지를 위해 -= 후 += 패턴 사용.
		/// </summary>
		public void Resubscribe()
		{
			EventManager.OnEnemyKilledTyped -= HandleEnemyKilled;
			EventManager.OnEnemyKilledTyped += HandleEnemyKilled;
		}

		/// <summary>해당 NPC가 줄 수 있는 다음 퀘스트 존재 여부 확인용.</summary>
		public bool CanAccept(QuestData quest)
		{
			if (quest == null) return false;
			if (HasActiveQuest) return false;
			if (CompletedQuestIds.Contains(quest.QuestId)) return false;
			return true;
		}

		public void StartQuest(QuestData quest)
		{
			if (!CanAccept(quest)) return;
			ActiveQuest = quest;
			Progress = quest.Type == QuestType.Gather ? CountInventory(quest.TargetItem) : 0;
			OnQuestStateChanged?.Invoke();
		}

		/// <summary>
		/// 인벤 공간 부족 등으로 보상 지급이 불가해 완료가 보류된 경우. UI 토스트로 사용자에게 안내.
		/// </summary>
		public event Action<QuestData> OnRewardBlocked;

		/// <summary>
		/// 활성 퀘스트 완료 처리. 모든 검증/보상/차감이 성공하면 true 반환.
		/// 인벤 공간 부족 또는 Gather 재료가 사라진 경우 false 반환 — ActiveQuest는 유지되어
		/// 사용자가 정리 후 다시 시도 가능. UI는 반환값으로 다이얼로그 닫기 여부 결정.
		/// </summary>
		public bool CompleteQuest(IPlayer player)
		{
			if (!IsActiveQuestComplete || player == null) return false;

			// 트랜잭션 격리 — 두 가지 다 막아야 함.
			// 1) pending claim: ConsumeItems가 만든 빈 슬롯을 보상이 가로채는 race
			// 2) autosave: 보상 지급(AddItem)이 퀘스트 완료 마킹보다 *먼저* 발생하므로, throttle
			//    만료 시 OnInventoryChanged → RequestAutoSave → 즉시 SaveGame이 "보상은 받았는데
			//    퀘스트는 아직 활성" 중간 상태를 디스크에 박을 수 있음.
			// QuestDialog가 완료 직후 SaveGame을 호출해 dispose 후 최종 상태를 영속화한다.
			using (GameTransaction.Begin())
			{
				return CompleteQuestInternal(player);
			}
		}

		private bool CompleteQuestInternal(IPlayer player)
		{
			var quest = ActiveQuest;
			var inv = player.Inventory;

			// Gather 재검증 — 재료를 판매/소비한 뒤 완료 시도하는 케이스 차단.
			// Progress는 과거 획득 이벤트 누적이라 현재 인벤 상태와 어긋날 수 있다.
			if (quest.Type == QuestType.Gather && quest.TargetItem != null)
			{
				if (inv == null || !inv.HasItems(quest.TargetItem, quest.TargetCount))
				{
					int actual = inv?.CountItem(quest.TargetItem) ?? 0;
					GD.Print($"[Quest] {quest.QuestTitle} — 재료 부족(현재 {actual}/{quest.TargetCount}). 완료 보류");
					Progress = actual;
					OnQuestStateChanged?.Invoke();
					OnRewardBlocked?.Invoke(quest);
					return false;
				}
			}

			int rewardQty = quest.RewardItem != null ? Math.Max(1, quest.RewardItemQuantity) : 0;
			bool gatherPending = quest.Type == QuestType.Gather && quest.TargetItem != null && inv != null;

			// 보상 인벤 공간 확인. Gather 차감으로 빈 슬롯 생기는 케이스는 destructive 시뮬레이션 후
			// 실패 시 롤백.
			if (quest.RewardItem != null && inv != null && !inv.CanAddItem(quest.RewardItem, rewardQty))
			{
				bool feasibleAfterConsume = false;
				if (gatherPending)
				{
					if (!inv.ConsumeItems(quest.TargetItem, quest.TargetCount))
					{
						GD.PrintErr($"[Quest] ConsumeItems 실패 — {quest.QuestTitle} 보상 보류");
						OnRewardBlocked?.Invoke(quest);
						return false;
					}
					feasibleAfterConsume = inv.CanAddItem(quest.RewardItem, rewardQty);
					if (!feasibleAfterConsume)
						inv.AddItem(quest.TargetItem, quest.TargetCount, fireAcquired: false); // 롤백
					else
						gatherPending = false; // 이미 차감됨 — 아래 중복 차감 방지
				}
				if (!feasibleAfterConsume)
				{
					GD.Print($"[Quest] 인벤토리 공간 부족 — {quest.QuestTitle} 보상 지급 보류 (정리 후 재시도)");
					OnRewardBlocked?.Invoke(quest);
					return false;
				}
			}

			// Gather 차감 — 반환값 검사 (위 HasItems 통과 후 race로 사라지는 케이스 방어)
			if (gatherPending)
			{
				if (!inv.ConsumeItems(quest.TargetItem, quest.TargetCount))
				{
					GD.PrintErr($"[Quest] ConsumeItems 실패(race) — {quest.QuestTitle} 보상 보류");
					OnRewardBlocked?.Invoke(quest);
					return false;
				}
			}

			// 보상 지급 — 검증 모두 통과 후에만 실행
			GameManager.Instance.PlayerGold += quest.GoldReward;
			if (quest.ExpReward > 0)
				EventManager.TriggerExpGained(quest.ExpReward);
			if (quest.RewardItem != null && inv != null)
				inv.AddItem(quest.RewardItem, rewardQty);

			CompletedQuestIds.Add(quest.QuestId);
			ActiveQuest = null;
			Progress = 0;
			OnQuestStateChanged?.Invoke();
			GD.Print($"[Quest] 완료: {quest.QuestTitle}");
			return true;
		}

		public void NotifyNpcTalked(string npcId, IPlayer player)
		{
			if (!HasActiveQuest) return;
			if (ActiveQuest.Type != QuestType.Deliver) return;
			if (ActiveQuest.TargetNpcId != npcId) return;
			Progress = ActiveQuest.TargetCount; // Deliver는 즉시 완료
			OnQuestStateChanged?.Invoke();
		}

		public void NotifySceneEntered(string sceneName)
		{
			if (!HasActiveQuest) return;
			if (ActiveQuest.Type != QuestType.Explore) return;
			if (ActiveQuest.TargetScene == sceneName)
			{
				Progress = ActiveQuest.TargetCount;
				OnQuestStateChanged?.Invoke();
			}
		}

		public void NotifyItemAcquired(ItemData item, IPlayer player)
		{
			if (!HasActiveQuest || item == null) return;
			if (ActiveQuest.Type != QuestType.Gather) return;
			if (ActiveQuest.TargetItem == null) return;
			if (item.ResourcePath != ActiveQuest.TargetItem.ResourcePath) return;

			Progress = CountInventory(ActiveQuest.TargetItem, player);
			OnQuestStateChanged?.Invoke();
		}

		private void HandleEnemyKilled(string enemyTypeName)
		{
			if (!HasActiveQuest) return;
			if (ActiveQuest.Type != QuestType.Kill) return;
			if (string.IsNullOrEmpty(enemyTypeName)) return;
			// 엘리트 prefix 무시 (예: "엘리트 Orc" → "Orc"로 매칭)
			string normalized = enemyTypeName.StartsWith("엘리트 ")
				? enemyTypeName.Substring(4) : enemyTypeName;
			if (normalized != ActiveQuest.TargetEnemyType) return;

			Progress = Math.Min(Progress + 1, ActiveQuest.TargetCount);
			OnQuestStateChanged?.Invoke();
		}

		private static int CountInventory(ItemData item, IPlayer player = null)
		{
			player ??= GameManager.Instance?.Player;
			if (player?.Inventory == null || item == null) return 0;
			return player.Inventory.CountItem(item);
		}

		/// <summary>
		/// 해당 NPC가 지금 부여할 수 있는 메인 퀘스트.
		/// chain 순서를 보장하기 위해 "manifest 첫 미완료 퀘스트"가 이 NPC꺼일 때만 반환.
		/// 선행 퀘스트를 건너뛰고 뒷 퀘스트를 부여하지 않음.
		/// </summary>
		public QuestData FindNextQuestForNpc(string npcId)
		{
			if (HasActiveQuest) return null;
			// quest_manifest.tres에 명시된 순서로 검색.
			// (DirAccess + .tres 디렉터리 열거는 Android export의 .remap 처리와 충돌 가능)
			var manifest = GD.Load<QuestManifest>("res://Resources/Quests/quest_manifest.tres");
			if (manifest?.MainQuests == null) return null;
			foreach (var quest in manifest.MainQuests)
			{
				if (quest == null) continue;
				if (CompletedQuestIds.Contains(quest.QuestId)) continue;
				// 첫 미완료 퀘스트 — 이 NPC가 부여자면 반환, 아니면 null
				return quest.GiverNpcId == npcId ? quest : null;
			}
			return null;
		}

		// ─── 세이브 ────────────────────────────────────────────
		public void RestoreFromSave(string activeQuestPath, int progress, IEnumerable<string> completedIds)
		{
			CompletedQuestIds.Clear();
			if (completedIds != null)
				foreach (var id in completedIds) CompletedQuestIds.Add(id);

			if (!string.IsNullOrEmpty(activeQuestPath))
			{
				ActiveQuest = GD.Load<QuestData>(activeQuestPath);
				Progress = progress;
			}
			else
			{
				ActiveQuest = null;
				Progress = 0;
			}
			OnQuestStateChanged?.Invoke();
		}

		public string ActiveQuestPath => ActiveQuest?.ResourcePath ?? "";
	}
}
