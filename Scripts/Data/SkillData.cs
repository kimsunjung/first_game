using Godot;

namespace FirstGame.Data
{
	public enum SkillType
	{
		PowerStrike, // 다음 공격 2배 데미지
		HealSelf,    // HP 회복
		Dash,        // 일시 속도 증가
		FireBolt     // 원거리 적 공격
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
	}
}
