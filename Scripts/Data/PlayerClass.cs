namespace FirstGame.Data
{
	/// <summary>
	/// 신규 게임 시 선택하는 캐릭터 클래스.
	/// 세이브에 정수로 저장(int 캐스팅). 누락 시 0(Warrior)로 폴백 → 기존 세이브 호환.
	/// </summary>
	public enum PlayerClass
	{
		Warrior = 0, // 전사 — STR 중심. 검·창. 시작 스킬: PowerStrike
		Mage = 1,    // 마법사 — INT 중심. 지팡이. 시작 스킬: FireBolt
		Archer = 2   // 궁수 — DEX 중심. 활·단검. 시작 스킬: Dash (또는 신규 ArrowVolley)
	}

	public static class PlayerClassUtil
	{
		public static string DisplayName(PlayerClass c) => c switch
		{
			PlayerClass.Warrior => "전사",
			PlayerClass.Mage => "마법사",
			PlayerClass.Archer => "궁수",
			_ => "?"
		};

		public static string Description(PlayerClass c) => c switch
		{
			PlayerClass.Warrior => "근접 전투의 강자. 검을 휘둘러 적을 베어 넘긴다.",
			PlayerClass.Mage => "원소를 다루는 마법사. 지팡이로 마법을 발사한다.",
			PlayerClass.Archer => "민첩한 사냥꾼. 활과 회피로 적을 농락한다.",
			_ => ""
		};
	}
}
