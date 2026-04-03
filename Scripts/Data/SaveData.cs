using System.Collections.Generic;

namespace FirstGame.Data
{
	public class SavedItemSlot
	{
		public string ItemPath { get; set; }
		public int Quantity { get; set; }
		public int EnhancementLevel { get; set; } = 0;
	}

	public class SaveData
	{
		public const int LatestVersion = 3;
		public int Version { get; set; } = LatestVersion;

		public float PlayerPosX { get; set; }
		public float PlayerPosY { get; set; }
		public int PlayerHealth { get; set; }
		public int PlayerMaxHealth { get; set; }
		public int PlayerMp { get; set; }
		public int PlayerGold { get; set; }
		public int PlayerLevel { get; set; } = 1;
		public int PlayerExp { get; set; } = 0;
		public string Timestamp { get; set; }

		public List<SavedItemSlot> InventoryItems { get; set; } = new();
		public string EquippedWeaponPath { get; set; }
		public string EquippedArmorPath { get; set; }
		public string EquippedAccessoryPath { get; set; }
		public List<string> QuickSlotPaths { get; set; } = new();
		public List<string> LearnedSkillPaths { get; set; } = new(); // 습득한 스킬 경로

		// 스탯 포인트
		public int StatPoints { get; set; } = 0;
		public int StrPoints { get; set; } = 0;
		public int ConPoints { get; set; } = 0;
		public int IntPoints { get; set; } = 0;

		// 퀘스트 상태
		public string CurrentQuestPath { get; set; } = "";
		public int QuestKillProgress { get; set; } = 0;

		// ─── v2: 월드 상태 ────────────────────────────────────
		public string CurrentScene { get; set; } = "";
		public float DayTime { get; set; } = 0.3f;
		public List<string> DefeatedBosses { get; set; } = new();
		public List<string> CompletedQuestIds { get; set; } = new();

		// ─── v3: 강화 시스템 + 보물상자 ───────────────────────
		public int EquippedWeaponEnhancement { get; set; } = 0;
		public int EquippedArmorEnhancement { get; set; } = 0;
		public int EquippedAccessoryEnhancement { get; set; } = 0;
		public List<string> OpenedChests { get; set; } = new();
	}
}
