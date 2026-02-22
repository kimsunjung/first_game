using Godot;
using System;
using FirstGame.Data;

namespace FirstGame.Core
{
	public static class QuestManager
	{
		public static QuestData CurrentQuest { get; private set; } = null;
		public static int KillProgress { get; private set; } = 0;

		public static event Action<QuestData> OnQuestAccepted;
		public static event Action<int, int> OnProgressUpdated; // (current, total)
		public static event Action<QuestData> OnQuestCompleted;

		public static bool HasActiveQuest => CurrentQuest != null;

		public static bool AcceptQuest(QuestData quest)
		{
			if (quest == null) return false;
			if (HasActiveQuest)
			{
				GD.Print("이미 진행 중인 퀘스트가 있습니다.");
				return false;
			}
			CurrentQuest = quest;
			KillProgress = 0;
			OnQuestAccepted?.Invoke(quest);
			GD.Print($"퀘스트 수락: {quest.QuestTitle}");
			return true;
		}

		public static void ReportKill(string enemyType)
		{
			if (CurrentQuest == null) return;
			if (enemyType != CurrentQuest.TargetEnemyType) return;

			KillProgress++;
			OnProgressUpdated?.Invoke(KillProgress, CurrentQuest.TargetCount);

			if (KillProgress >= CurrentQuest.TargetCount)
			{
				CompleteQuest();
			}
		}

		private static void CompleteQuest()
		{
			var completedQuest = CurrentQuest;
			CurrentQuest = null;
			KillProgress = 0;

			// 골드 보상
			if (GameManager.Instance != null)
				GameManager.Instance.PlayerGold += completedQuest.GoldReward;

			OnQuestCompleted?.Invoke(completedQuest);
			GD.Print($"퀘스트 완료! {completedQuest.QuestTitle} → 골드 +{completedQuest.GoldReward}");
		}

		public static void Reset()
		{
			CurrentQuest = null;
			KillProgress = 0;
		}
	}
}
