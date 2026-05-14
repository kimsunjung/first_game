using Godot;

namespace FirstGame.Data
{
	public enum SkillType
	{
		PowerStrike, // 전사 — 다음 공격 N배 데미지
		HealSelf,    // 공통 — HP 회복
		Dash,        // 공통(궁수 가능) — 일시 속도 증가
		FireBolt,    // 마법사 — 원거리 단일 공격(파이어)
		ArrowShot,   // 궁수 시작 — 원거리 단일 공격(중간 사거리)
		Whirlwind,   // 전사 — 자신 주변 광역
		IceShard,    // 마법사 — 원거리 단일 공격(아이스)
		MultiShot,   // 궁수 — 전방 콘 광역
		// 패시브 — IsPassive=true. 학습 시 즉시 효과 적용.
		LifestealPassive,  // 공격 시 데미지의 N% HP 회복
		HpRegenPassive,    // 매초 N HP 자동 회복
		CritBoostPassive,  // 크리율 +N%
		SpeedBoostPassive  // 이속 +N
	}

	public enum PassiveType
	{
		None = 0,
		Lifesteal,
		HpRegen,
		CritBoost,
		SpeedBoost
	}

	[GlobalClass]
	public partial class SkillData : Resource
	{
		[Export] public string SkillName { get; set; } = "스킬";
		[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
		[Export] public Texture2D Icon { get; set; }
		[Export] public SkillType Type { get; set; } = SkillType.PowerStrike;
		[Export] public int MpCost { get; set; } = 15;
		[Export] public float Cooldown { get; set; } = 5.0f;
		[Export] public int RequiredLevel { get; set; } = 1;
		// 클래스 제한. AvailableToAllClasses=true(기본)면 모든 클래스 사용 가능 — 새 스킬 .tres
		// 추가 시 안전한 기본. 특정 클래스 전용으로 만들려면 RequiredClass 지정 + 본 필드 false.
		[Export] public PlayerClass RequiredClass { get; set; } = PlayerClass.Warrior;
		[Export] public bool AvailableToAllClasses { get; set; } = true;

		// 스킬별 추가 파라미터
		[ExportGroup("Skill Parameters")]
		[Export] public int HealAmount { get; set; } = 0;       // HealSelf용
		[Export] public float DurationSeconds { get; set; } = 0f; // Dash용
		[Export] public int BonusDamageMultiplier { get; set; } = 2; // PowerStrike배율
		[Export] public ElementType Element { get; set; } = ElementType.None; // 속성 데미지 (None이면 무속성)

		// ─── 패시브 ──────────────────────────────────────────────────
		// IsPassive=true면 능동 발동 없이 학습 즉시 PlayerStats가 효과 적용.
		// SkillShopUI/SkillWindow가 능동 슬롯에 등록하지 않고 별도 패시브 영역에 표시.
		[ExportGroup("Passive")]
		[Export] public bool IsPassive { get; set; } = false;
		[Export] public PassiveType PassiveKind { get; set; } = PassiveType.None;
		// 패시브 효과량 — Lifesteal: 0.05=5%, HpRegen: 매초 HP, CritBoost: 0.05=5%, SpeedBoost: 30
		[Export] public float PassiveValue { get; set; } = 0f;
	}
}
