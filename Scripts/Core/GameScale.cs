namespace FirstGame.Core
{
	// 캐릭터(플레이어/적/NPC) 시각 크기 전역 배율.
	// 콜리전/상호작용 반경/공격 사거리는 미적용 → 전투 밸런스·상호작용 불변(시각 전용 다운스케일).
	public static class GameScale
	{
		// 2026-05-18: 전체 캐릭터 시각 크기 20% 축소 요청 반영.
		public const float CharacterVisual = 0.8f;
	}
}
