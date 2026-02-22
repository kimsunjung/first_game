using Godot;

namespace FirstGame.Data
{
	public enum ItemType
	{
		Consumable, // 포션 등 소비 아이템
		Weapon,     // 무기
		Armor,      // 방어구
		SkillBook   // 스킬북 (사용 시 스킬 습득)
	}

	public enum WeaponAttackType
	{
		Slice,  // 베기 (칼, 도끼)
		Pierce, // 찌르기 (창, 활)
		Crush   // 타격 (해머, 철퇴)
	}

	[GlobalClass]
	public partial class ItemData : Resource
	{
		[Export] public string ItemName { get; set; } = "";
		[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
		[Export] public Texture2D Icon { get; set; }
		[Export] public int Price { get; set; } = 0;
		[Export] public int SellPrice { get; set; } = 0;
		[Export] public ItemType Type { get; set; } = ItemType.Consumable;
		[Export] public bool IsStackable { get; set; } = true;
		[Export] public int MaxStack { get; set; } = 99;

		// 소비 아이템 효과
		[ExportGroup("Consumable Effects")]
		[Export] public int HealAmount { get; set; } = 0;

		// 장비 보너스
		[ExportGroup("Equipment Bonuses")]
		[Export] public int BonusDamage { get; set; } = 0;
		[Export] public int BonusMaxHealth { get; set; } = 0;
		[Export] public float BonusMoveSpeed { get; set; } = 0f;
		[Export] public WeaponAttackType AttackType { get; set; } = WeaponAttackType.Slice;

		// 스킬북 전용
		[ExportGroup("Skill Book")]
		[Export] public SkillData LearnedSkill { get; set; }
	}
}
