namespace FirstGame.Data
{
	// 사냥 계약 타입. 세이브에는 ContractProgress.ContractId(문자열)만 저장되고
	// 타입 자체는 직렬화되지 않지만, EnumStabilityTests로 값 고정(혹시 다른 곳에서
	// (int) 비교가 생겨도 안전하도록).
	public enum ContractType
	{
		Kill = 0,     // 특정 적 N마리 처치
		Gather = 1,   // 특정 아이템 N개 납품(A안 — 완료 시 ConsumeItems로 소모, 보유기반 진행)
		BossKill = 2, // 특정 보스 N회 처치
		Mining = 3    // 특정 광석 N회 채광
	}

	// contracts.json 매니페스트의 한 항목. 런타임 정의(세이브 비직렬화).
	public class ContractData
	{
		public string Id = "";
		public string Title = "";
		public string Description = "";
		public string Region = "";
		public ContractType Type = ContractType.Kill;

		// 타입별 타깃 (해당 타입에서만 사용)
		public string TargetEnemyType = "";   // Kill — EnemyTypeName(엘리트 prefix 제거 후 매칭)
		public string TargetItemPath = "";    // Gather — 납품 아이템 res:// 경로(완료 시 Goal 개 소모)
		public string TargetBossId = "";      // BossKill — EnemySpawner.BossId
		public string TargetOreItemPath = ""; // Mining — MiningNode.OreItem res:// 경로

		public int Goal = 1;
		public int RecommendedLevel = 1;

		// 보상 — 골드/EXP는 항상 지급 가능, 아이템은 인벤 부족 시 PendingReward 경유(손실 없음).
		public int GoldReward = 0;
		public int ExpReward = 0;
		public string RewardItemPath = "";
		public int RewardItemQuantity = 1;
	}

	// 활성 계약의 진행 상태. SaveData.ActiveContracts로 직렬화되므로
	// System.Text.Json 호환 public get/set 프로퍼티만 사용(SavedItemSlot과 동일 규약).
	public class ContractProgress
	{
		public string ContractId { get; set; } = "";
		public int Progress { get; set; } = 0;
		// Progress가 Goal에 도달하면 true — 완료 보상 수령 가능 상태.
		// 진행 이벤트에서 셋되며 세이브/로드에도 보존(재계산 불필요).
		public bool TurnInReady { get; set; } = false;
	}
}
