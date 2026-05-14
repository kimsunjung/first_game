using System.Collections.Generic;

namespace FirstGame.Data
{
	public class SavedItemSlot
	{
		public string ItemPath { get; set; }
		public int Quantity { get; set; }
		public int EnhancementLevel { get; set; } = 0;
		// v7: 인스턴스별 affix.
		public List<ItemAffix> Affixes { get; set; } = new();
		// v11+: 슬롯 장착 상태 — 인벤 슬롯이 IsEquipped면 직렬화. 누락 시 false.
		public bool IsEquipped { get; set; } = false;
	}

	public class SaveData
	{
		public const int LatestVersion = 11;
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
		// v10: DEX 추가 (궁수 클래스 핵심 스탯). 누락 시 0으로 로드.
		public int DexPoints { get; set; } = 0;

		// v10: 캐릭터 클래스(Warrior/Mage/Archer). 신규 게임 시작 시 선택.
		// 누락 시 Warrior(0)로 로드되어 기존 세이브 호환.
		public int PlayerClassId { get; set; } = 0;

		// v10: 메인 스토리 챕터 플래그. 진행에 따라 추가 누적.
		// 예: "flag_outpost_entered", "flag_orc_warlord_killed" 등.
		public List<string> ChapterFlags { get; set; } = new();

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

		// ─── v7: 장신구 등 장착 슬롯의 affix.
		public List<ItemAffix> EquippedHelmetAffixes { get; set; } = new();
		public List<ItemAffix> EquippedBootsAffixes { get; set; } = new();
		public List<ItemAffix> EquippedNecklaceAffixes { get; set; } = new();
		public List<ItemAffix> EquippedRing1Affixes { get; set; } = new();
		public List<ItemAffix> EquippedRing2Affixes { get; set; } = new();
		public List<ItemAffix> EquippedBraceletAffixes { get; set; } = new();

		// ─── v11: 망토/벨트/장갑 슬롯. 누락 시 빈 문자열로 로드 (BackfillV11 자동 처리).
		public string EquippedCloakPath { get; set; } = "";
		public List<ItemAffix> EquippedCloakAffixes { get; set; } = new();
		public string EquippedBeltPath { get; set; } = "";
		public List<ItemAffix> EquippedBeltAffixes { get; set; } = new();
		public string EquippedGlovesPath { get; set; } = "";
		public List<ItemAffix> EquippedGlovesAffixes { get; set; } = new();

		// ─── v5: 절차적 필드맵 seed (씬 경로 → seed). 첫 진입 시 랜덤 → 저장,
		// 재진입/리로드 시 같은 seed로 재생성해 플레이어 좌표가 장애물 안에 들어가지 않게 한다.
		public Dictionary<string, int> FieldSeeds { get; set; } = new();

		// ─── v6: 방문한 씬 경로 목록 — 텔레포트 NPC가 "한 번이라도 다녀온 곳만"
		// 목적지 활성화하는 기준. SceneManager.ChangeScene이 자동 기록.
		public List<string> VisitedScenes { get; set; } = new();

		// ─── v8: 채광 완료된 광산 노드 ID 목록. 씬 재진입 시 해당 노드는 즉시 QueueFree.
		// 누락 시 빈 리스트로 로드되어 모든 노드 다시 활성.
		public List<string> MinedNodes { get; set; } = new();

		// ─── 보류 보상함: 인벤 가득 등으로 즉시 지급 못한 보상(주로 보스 드랍).
		// 게임 시작 시 자동 재시도 + 사용자 알림. 필드 드랍과 달리 세이브에 영속화되어
		// 앱 종료/OS kill에도 손실되지 않는다.
		public List<SavedItemSlot> PendingRewardItems { get; set; } = new();
	}
}
