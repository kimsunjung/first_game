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

		public void CompleteQuest(IPlayer player)
		{
			if (!IsActiveQuestComplete || player == null) return;
			var quest = ActiveQuest;

			// 보상 지급
			GameManager.Instance.PlayerGold += quest.GoldReward;
			if (quest.ExpReward > 0)
				EventManager.TriggerExpGained(quest.ExpReward);
			if (quest.RewardItem != null && player.Inventory != null)
				player.Inventory.AddItem(quest.RewardItem, Math.Max(1, quest.RewardItemQuantity));

			// Gather 퀘스트면 수집 아이템 차감
			if (quest.Type == QuestType.Gather && quest.TargetItem != null && player.Inventory != null)
				player.Inventory.ConsumeItems(quest.TargetItem, quest.TargetCount);

			CompletedQuestIds.Add(quest.QuestId);
			ActiveQuest = null;
			Progress = 0;
			OnQuestStateChanged?.Invoke();
			GD.Print($"[Quest] 완료: {quest.QuestTitle}");
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
