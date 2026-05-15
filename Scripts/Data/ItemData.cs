using Godot;

namespace FirstGame.Data
{
	/// <summary>Consumable이 어떤 효과를 발휘할지. UseItem 분기가 ReturnToTown bool에 직접 의존하지 않게 분리.</summary>
	public enum ItemUseEffect
	{
		None,         // 효과 없음 (소비 안 됨)
		Heal,         // HealAmount만큼 HP 회복
		ReturnToTown, // 즉시 마을 이동
		RestoreMana,  // HealAmount만큼 MP 회복 (이름은 HP 공용이지만 enum 케이스로 분기)
		Buff,         // BuffMoveSpeed/BuffAttackSpeed/BuffDurationSec 적용 후 자동 해제
		Teleport,     // TeleportTargetScene으로 즉시 이동 (방문한 씬만 허용)
		CureStatus,   // 상태이상 해제 (HealAmount=비트마스크: 1=중독, 2=빙결)
		ReviveOnDeath // 인벤에 있으면 사망 시 자동 소비 → HP 50% 부활
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
		Bracelet,   // 팔찌
		Cloak,      // 망토 (등)
		Belt,       // 벨트 (허리)
		Gloves      // 장갑 (양손)
	}

	public static class ItemTypeExtensions
	{
		/// <summary>장비(장착 가능) 타입인지.</summary>
		public static bool IsEquipment(this ItemType t) =>
			t == ItemType.Weapon || t == ItemType.Armor || t == ItemType.Accessory ||
			t == ItemType.Helmet || t == ItemType.Boots || t == ItemType.Necklace ||
			t == ItemType.Ring || t == ItemType.Bracelet || t == ItemType.SkillBook ||
			t == ItemType.Cloak || t == ItemType.Belt || t == ItemType.Gloves;
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
		// 공격속도 — 1.0이 베이스. 0.15는 공격 cooldown을 15% 단축(공격 15% 더 빠름).
		[Export] public float BonusAttackSpeed { get; set; } = 0f;
		// HP 흡수 — 적중 데미지의 N% 만큼 회복. 0.01 = 1%. 장신구에 미약하게 붙음.
		[Export] public float BonusLifesteal { get; set; } = 0f;
		[Export] public WeaponAttackType AttackType { get; set; } = WeaponAttackType.Slice;

		// 상점 차단 — true면 일반 상점에서 진열·판매 안 됨. 적 드랍/보스 전용 무기에 사용.
		[Export] public bool IsShopBlocked { get; set; } = false;

		// 챕터 게이트 — 일반 상점이 GameManager.CurrentChapter >= MinRequiredChapter 인 경우에만 진열.
		// 기본값 Prologue(0)이면 게이트 없음(처음부터 진열). 고급 소모품/스크롤에 진행도 게이트를 걸 때 사용.
		[Export] public Chapter MinRequiredChapter { get; set; } = Chapter.Prologue;

		// 임시 buff (소모품 — 속도 물약 등). 사용 시 N초간 효과 적용 후 자동 해제.
		[ExportGroup("Consumable Buff")]
		[Export] public float BuffMoveSpeed { get; set; } = 0f;
		[Export] public float BuffAttackSpeed { get; set; } = 0f;
		[Export] public float BuffDurationSec { get; set; } = 0f;
		// 신규 buff 종: 데미지·방어·치명타. 모두 정수/소수 다양한 단위로 합산.
		[Export] public int BuffBaseDamage { get; set; } = 0;
		[Export] public int BuffDefense { get; set; } = 0;
		[Export] public float BuffCritRate { get; set; } = 0f;

		// 클래스 제한 — 무기에서 사용. AvailableToAllClasses=true(기본)면 누구나 장착.
		// 무기 .tres에서 false + RequiredClass 지정으로 클래스 전용 무기 표현.
		// 방어구/소비/재료는 기본값 그대로 두면 전 클래스 공유.
		[ExportGroup("Class")]
		[Export] public PlayerClass RequiredClass { get; set; } = PlayerClass.Warrior;
		[Export] public bool AvailableToAllClasses { get; set; } = true;

		// 스킬북 전용
		[ExportGroup("Skill Book")]
		[Export] public SkillData LearnedSkill { get; set; }

		// 무게 — 인벤토리 총 무게 합산에 사용. 소비/재료 0.1, 장신구 0.3, 무기 2~5, 갑옷 3~8
		// 기본값 1.0이면 EffectiveWeight()가 Type별 합리적 기본을 반환.
		// 명시적으로 .tres에서 Weight를 설정한 경우 그대로 사용.
		[ExportGroup("Weight")]
		[Export] public float Weight { get; set; } = 1.0f;

		/// <summary>Type별 합리적 기본 무게. 신규 .tres가 Weight를 명시했으면 그 값 사용.</summary>
		public float EffectiveWeight()
		{
			// 명시 가중치(0.1 미만이거나 1.0 아닌 값)면 그대로 반환
			if (Weight != 1.0f) return Weight;
			// 기본 1.0(미설정)인 경우 카테고리별 추정치 반환
			return Type switch
			{
				ItemType.Consumable => 0.1f,
				ItemType.Material   => 0.5f,
				ItemType.Necklace or ItemType.Ring or ItemType.Bracelet => 0.3f,
				ItemType.SkillBook  => 0.5f,
				ItemType.Weapon     => 3.0f,
				ItemType.Armor      => 5.0f,
				ItemType.Helmet     => 2.5f,
				ItemType.Boots      => 2.0f,
				ItemType.Cloak      => 2.5f,
				ItemType.Belt       => 1.5f,
				ItemType.Gloves     => 1.5f,
				_                   => 1.0f
			};
		}

		// 순간이동 주문서 전용 — TeleportTargetScene + TeleportTargetPos (Vector2)
		[ExportGroup("Teleport Scroll")]
		[Export] public string TeleportTargetScene { get; set; } = "";
		[Export] public Godot.Vector2 TeleportTargetPos { get; set; } = Godot.Vector2.Zero;
	}

	/// <summary>장신구 등 인스턴스별 랜덤 옵션(affix) 종류.</summary>
	public enum ItemAffixType
	{
		BonusDamage,
		BonusDefense,
		BonusMaxHealth,
		BonusMaxMp,
		BonusCritRate,
		BonusMoveSpeed,
		BonusAttackSpeed,
		BonusLifesteal
	}

	/// <summary>아이템 인스턴스에 붙는 단일 옵션. POCO — System.Text.Json으로 직접 직렬화.</summary>
	public class ItemAffix
	{
		public ItemAffixType Type { get; set; }
		public float Value { get; set; }
	}
}
