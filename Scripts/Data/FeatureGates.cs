namespace FirstGame.Data
{
	// 초반 정보량/난이도 폭주 완화용 기능 해금 레벨 게이트 (v1, 2026-05-19).
	// PlayerStats.Level 기준 — SaveData 변경 없음. NPC 진입 단계에서 차단하고
	// HUD 안내 메시지를 띄운다. 수치는 여기 한 곳에서만 관리(중앙화).
	//
	// 설계 의도:
	//  - 창고는 파밍 게임 기본 편의라 사실상 게이트 없음(Lv.1).
	//  - 계약 보드는 사냥 동선의 핵심이라 낮게(Lv.3).
	//  - 제작은 재료 개념이 붙는 첫 복잡 기능(Lv.5).
	//  - 재련은 affix 등 정보량이 가장 큰 후반 준비 기능(Lv.10).
	//  - 고급 스킬 상점은 허브별로 다름 → SkillShopNPC.MinPlayerLevel(.tscn) 으로
	//    개별 지정. 여기 상수는 town 외 허브의 기본 권장값을 문서화한 참고치.
	// Godot 미의존(단위 테스트 대상).
	public static class FeatureGates
	{
		public const int Storage = 1;   // 사실상 게이트 없음(편의 기능)
		public const int Contract = 3;
		public const int Crafting = 5;
		public const int Reforge = 10;

		// town 외 허브 스킬 상점 권장 게이트(참고). 실제 적용은 .tscn의
		// SkillShopNPC.MinPlayerLevel Export 로 한다(town=0).
		public const int SkillShopOutpost = 8;
		public const int SkillShopHarbor = 8;
		public const int SkillShopMountain = 12;

		// 엘리트/희귀 변종 등장 글로벌 최소 레벨. 실제 게이트는
		// max(EliteMinPlayerLevel, 해당 zone 권장레벨)로 계산(EnemySpawner).
		// balance json elite.minPlayerLevel 이 우선(여기는 폴백 상수).
		public const int EliteMinPlayerLevelFallback = 5;

		/// <summary>안내 메시지 — "{기능}은 Lv.{N}부터 이용할 수 있습니다."</summary>
		public static string LockMessage(string featureName, int requiredLevel, int currentLevel)
			=> $"{featureName}은(는) Lv.{requiredLevel}부터 이용할 수 있습니다. (현재 Lv.{currentLevel})";
	}
}
