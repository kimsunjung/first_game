using Godot;

namespace FirstGame.Data
{
	public enum EliteAffix
	{
		None,        // 기본 엘리트(스탯만 강화, 특수 능력 없음)
		Berserker,   // 광폭: 공격력 ↑↑, 공격속도 ↑
		Vampiric,    // 흡혈: HP 대량 ↑ + 매초 자가 재생
		Swift,       // 민첩: 이동속도 ↑↑, 공격속도 ↑↑
		Tough        // 강건: HP ↑↑↑, Defense ↑, 이동속도 ↓
	}

	public static class EliteAffixUtil
	{
		public struct AffixParams
		{
			public float HpMul;
			public float AtkMul;
			public float MoveSpeedMul;
			public float AttackCooldownMul;
			public int DefenseBonus;
			public float HpRegenPerSec; // MaxHealth 비율 — 0이면 재생 없음
			public Color Modulate;
			public string NamePrefix;
		}

		public static AffixParams Get(EliteAffix affix) => affix switch
		{
			EliteAffix.Berserker => new AffixParams
			{
				HpMul = 1.5f, AtkMul = 2.0f, MoveSpeedMul = 1.0f, AttackCooldownMul = 0.7f,
				DefenseBonus = 0, HpRegenPerSec = 0f,
				Modulate = new Color(1.6f, 0.4f, 0.4f, 1f),
				NamePrefix = "광폭한"
			},
			EliteAffix.Vampiric => new AffixParams
			{
				HpMul = 2.5f, AtkMul = 1.3f, MoveSpeedMul = 1.0f, AttackCooldownMul = 1.0f,
				DefenseBonus = 0, HpRegenPerSec = 0.02f,
				Modulate = new Color(1.4f, 0.4f, 1.2f, 1f),
				NamePrefix = "흡혈"
			},
			EliteAffix.Swift => new AffixParams
			{
				HpMul = 1.5f, AtkMul = 1.3f, MoveSpeedMul = 1.5f, AttackCooldownMul = 0.6f,
				DefenseBonus = 0, HpRegenPerSec = 0f,
				Modulate = new Color(0.6f, 1.5f, 0.8f, 1f),
				NamePrefix = "민첩한"
			},
			EliteAffix.Tough => new AffixParams
			{
				HpMul = 3.0f, AtkMul = 1.2f, MoveSpeedMul = 0.75f, AttackCooldownMul = 1.0f,
				DefenseBonus = 5, HpRegenPerSec = 0f,
				Modulate = new Color(0.6f, 0.6f, 1.4f, 1f),
				NamePrefix = "강건한"
			},
			_ => new AffixParams
			{
				HpMul = 1.0f, AtkMul = 1.0f, MoveSpeedMul = 1.0f, AttackCooldownMul = 1.0f,
				DefenseBonus = 0, HpRegenPerSec = 0f,
				Modulate = new Color(1.4f, 0.7f, 1.3f, 1f),
				NamePrefix = "엘리트"
			}
		};

		public static EliteAffix RollRandom()
		{
			// 0=Berserker, 1=Vampiric, 2=Swift, 3=Tough — None은 폴백용이라 굴리지 않음
			int idx = (int)(GD.Randi() % 4);
			return (EliteAffix)(idx + 1);
		}

		// 엘리트 prefix를 제거해 base enemy type name을 돌려준다. quest kill 매칭에 사용.
		// 신규 affix prefix가 추가되면 이 배열만 갱신 — QuestManager가 affix를 직접 알 필요 없음.
		private static readonly string[] _allPrefixes =
			{ "엘리트 ", "광폭한 ", "흡혈 ", "민첩한 ", "강건한 " };

		public static string StripElitePrefix(string enemyTypeName)
		{
			if (string.IsNullOrEmpty(enemyTypeName)) return enemyTypeName;
			foreach (var p in _allPrefixes)
				if (enemyTypeName.StartsWith(p)) return enemyTypeName.Substring(p.Length);
			return enemyTypeName;
		}
	}
}
