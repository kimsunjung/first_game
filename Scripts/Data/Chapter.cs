namespace FirstGame.Data
{
	public enum Chapter
	{
		Prologue = 0, // 새 게임 ~ field_outpost 첫 진입
		Chapter1 = 1, // field_outpost 진입 ~ 오크 워로드 처치
		Chapter2 = 2, // 오크 워로드 처치 ~ 스켈레톤 킹 처치
		Chapter3 = 3, // 스켈레톤 킹 처치 ~ dungeon_3 봉인 해제
		Final = 4,    // 봉인 해제 ~ 고대 리치 처치
		Ending = 5    // 고대 리치 처치 후
	}

	/// <summary>
	/// 챕터 플래그 식별자 — 누적되는 boolean 마커. SaveData.ChapterFlags에 저장.
	/// 가장 진행된 플래그가 현재 챕터를 결정.
	/// </summary>
	public static class ChapterFlags
	{
		public const string OutpostEntered = "flag_outpost_entered";
		public const string OrcWarlordKilled = "flag_orc_warlord_killed";
		public const string SkeletonKingKilled = "flag_skeleton_king_killed";
		public const string AbyssUnsealed = "flag_abyss_unsealed";
		public const string LichKilled = "flag_lich_killed";

		// 권역 졸업 표지자 v2 — coast/mountain 보스(반복). Chapter enum/엔딩과 무관한
		// 누적 마커(대사/계약/통계용). CurrentChapter 계산에는 미반영(엔딩형 아님).
		public const string KrakenKilled = "flag_kraken_killed";
		public const string GlacierTitanKilled = "flag_glacier_titan_killed";
		public const string InfernoDrakeKilled = "flag_inferno_drake_killed";
		public const string CrystalLordKilled = "flag_crystal_lord_killed";
	}
}
