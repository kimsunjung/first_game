using Godot;

namespace FirstGame.Data
{
	/// <summary>Consumable이 어떤 효과를 발휘할지. UseItem 분기가 ReturnToTown bool에 직접 의존하지 않게 분리.</summary>
	public enum ItemUseEffect
	{
		None,         // 효과 없음 (소비 안 됨)
		Heal,         // HealAmount만큼 HP 회복
		ReturnToTown  // 즉시 마을 이동
	}

	public enum ItemType
	{
		Consumable, // 포션 등 소비 아이템
		Weapon,     // 무기
		Armor,      // 방어구 (몸통)
		SkillBook,  // 스킬북 (사용 시 스킬 습득)
		Material,   // 재료
		Accessory,  // 레거시 — 기존 세이브 호환용. 마이그 시 Necklace로 자동 변환.
		Helmet,     // 모자 / 투구
		Boots,      // 신발
		Necklace,   // 목걸이
		Ring,       // 반지
		Bracelet    // 팔찌
	}

	public static class ItemTypeExtensions
	{
		/// <summary>장비(장착 가능) 타입인지.</summary>
		public static bool IsEquipment(this ItemType t) =>
			t == ItemType.Weapon || t == ItemType.Armor || t == ItemType.Accessory ||
			t == ItemType.Helmet || t == ItemType.Boots || t == ItemType.Necklace ||
			t == ItemType.Ring || t == ItemType.Bracelet || t == ItemType.SkillBook;
	}

	public enum WeaponAttackType
	{
		Slice,  // 베기 (칼, 도끼)
		Pierce, // 찌르기 (창, 활)
		Crush   // 타격 (해머, 철퇴)
	}

	public enum ItemRarity
	{
		Common,   // 흰색 (일반)
		Uncommon, // 초록색 (고급)
		Rare,     // 파란색 (희귀)
		Epic,     // 보라색 (영웅)
		Legendary // 노란색 (전설)
	}

	[GlobalClass]
	public partial class ItemData : Resource
	{
		[Export] public string ItemName { get; set; } = "";
		[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
		[Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
		[Export] public Texture2D Icon { get; set; }
		[Export] public int Price { get; set; } = 0;
		[Export] public int SellPrice { get; set; } = 0;
		[Export] public ItemType Type { get; set; } = ItemType.Consumable;
		[Export] public bool IsStackable { get; set; } = true;
		[Export] public int MaxStack { get; set; } = 99;

		// 소비 아이템 효과
		[ExportGroup("Consumable Effects")]
		[Export] public ItemUseEffect UseEffect { get; set; } = ItemUseEffect.Heal;
		[Export] public int HealAmount { get; set; } = 0;

		// 장비 보너스
		[ExportGroup("Equipment Bonuses")]
		[Export] public int BonusDamage { get; set; } = 0;
		[Export] public int BonusMaxHealth { get; set; } = 0;
		[Export] public int BonusDefense { get; set; } = 0;
		[Export] public float BonusMoveSpeed { get; set; } = 0f;
		[Export] public int BonusMaxMp { get; set; } = 0;
		[Export] public float BonusCritRate { get; set; } = 0f;
		[Export] public WeaponAttackType AttackType { get; set; } = WeaponAttackType.Slice;

		// 스킬북 전용
		[ExportGroup("Skill Book")]
		[Export] public SkillData LearnedSkill { get; set; }
	}

	/// <summary>장신구 등 인스턴스별 랜덤 옵션(affix) 종류.</summary>
	public enum ItemAffixType
	{
		BonusDamage,
		BonusDefense,
		BonusMaxHealth,
		BonusMaxMp,
		BonusCritRate,
		BonusMoveSpeed
	}

	/// <summary>아이템 인스턴스에 붙는 단일 옵션. POCO — System.Text.Json으로 직접 직렬화.</summary>
	public class ItemAffix
	{
		public ItemAffixType Type { get; set; }
		public float Value { get; set; }
	}
}
