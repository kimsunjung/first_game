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
		// 클래스 제한 — None이면 모든 클래스 사용 가능. 특정 클래스 지정 시 그 클래스만 학습/구매 가능.
		// 신규 게임 시 클래스 시작 스킬은 이 값에 따라 자동 부여.
		[Export] public PlayerClass RequiredClass { get; set; } = PlayerClass.Warrior;
		// 정말로 모든 클래스 공통 스킬임을 명시할 때 true. RequiredClass 무시.
		[Export] public bool AvailableToAllClasses { get; set; } = false;

		// 스킬별 추가 파라미터
		[ExportGroup("Skill Parameters")]
		[Export] public int HealAmount { get; set; } = 0;       // HealSelf용
		[Export] public float DurationSeconds { get; set; } = 0f; // Dash용
		[Export] public int BonusDamageMultiplier { get; set; } = 2; // PowerStrike배율
		[Export] public ElementType Element { get; set; } = ElementType.None; // 속성 데미지 (None이면 무속성)
	}
}
