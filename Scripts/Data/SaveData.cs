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
		public const int LatestVersion = 5;
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

		// ─── v4: 부위별 장비 슬롯 (모자/신발/목걸이/반지×2/팔찌) ─
		// 누락 시 빈 문자열로 로드되어 자동 마이그레이션됨.
		public string EquippedHelmetPath { get; set; } = "";
		public string EquippedBootsPath { get; set; } = "";
		public string EquippedNecklacePath { get; set; } = "";
		public string EquippedRing1Path { get; set; } = "";
		public string EquippedRing2Path { get; set; } = "";
		public string EquippedBraceletPath { get; set; } = "";

		// ─── v5: 절차적 필드맵 seed (씬 경로 → seed). 첫 진입 시 랜덤 → 저장,
		// 재진입/리로드 시 같은 seed로 재생성해 플레이어 좌표가 장애물 안에 들어가지 않게 한다.
		public Dictionary<string, int> FieldSeeds { get; set; } = new();

		// ─── 보류 보상함: 인벤 가득 등으로 즉시 지급 못한 보상(주로 보스 드랍).
		// 게임 시작 시 자동 재시도 + 사용자 알림. 필드 드랍과 달리 세이브에 영속화되어
		// 앱 종료/OS kill에도 손실되지 않는다.
		public List<SavedItemSlot> PendingRewardItems { get; set; } = new();
	}
}
