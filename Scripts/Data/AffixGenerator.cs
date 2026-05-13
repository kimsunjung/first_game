using System;
using System.Collections.Generic;

namespace FirstGame.Data
{
	/// <summary>장신구 드롭 시 인스턴스별 랜덤 옵션(affix)을 생성. 정적 유틸.</summary>
	public static class AffixGenerator
	{
		private static readonly Random _rng = new();

		// (type, min, max). CritRate/MoveSpeed만 소수, 나머지는 int 캐스팅.
		private static readonly (ItemAffixType type, float min, float max)[] Pool =
		{
			(ItemAffixType.BonusDamage,     1f,  5f),
			(ItemAffixType.BonusDefense,    1f,  5f),
			(ItemAffixType.BonusMaxHealth, 10f, 50f),
			(ItemAffixType.BonusMaxMp,      5f, 30f),
			(ItemAffixType.BonusCritRate,   0.01f, 0.05f),
			(ItemAffixType.BonusMoveSpeed,  0.01f, 0.05f),
		};

		/// <summary>장신구(Ring/Necklace/Bracelet) 드롭용. rarity에 따라 개수·값 차등 — type 중복 없음.
		/// Common 1개 / Uncommon 1~2 / Rare 2 / Epic 2~3 / Legendary 3, 값은 ×1.0/×1.2/×1.5/×2.0/×2.5.</summary>
		public static List<ItemAffix> GenerateForJewelry(ItemRarity rarity = ItemRarity.Common)
		{
			var (countMin, countMax, valueMul) = rarity switch
			{
				ItemRarity.Common    => (1, 1, 1.0f),
				ItemRarity.Uncommon  => (1, 2, 1.2f),
				ItemRarity.Rare      => (2, 2, 1.5f),
				ItemRarity.Epic      => (2, 3, 2.0f),
				ItemRarity.Legendary => (3, 3, 2.5f),
				_                    => (1, 1, 1.0f)
			};
			int count = _rng.Next(countMin, countMax + 1);
			var picked = new HashSet<ItemAffixType>();
			var result = new List<ItemAffix>(count);
			int safety = 16;
			while (result.Count < count && safety-- > 0)
			{
				var (type, min, max) = Pool[_rng.Next(Pool.Length)];
				if (!picked.Add(type)) continue;
				float baseValue = (type == ItemAffixType.BonusCritRate || type == ItemAffixType.BonusMoveSpeed)
					? (float)(_rng.NextDouble() * (max - min) + min)
					: _rng.Next((int)min, (int)max + 1);
				float scaled = baseValue * valueMul;
				// 정수형 affix는 곱 후 반올림.
				if (type != ItemAffixType.BonusCritRate && type != ItemAffixType.BonusMoveSpeed)
					scaled = (float)System.Math.Round(scaled);
				result.Add(new ItemAffix { Type = type, Value = scaled });
			}
			return result;
		}

		public static bool IsJewelry(ItemType t) =>
			t == ItemType.Necklace || t == ItemType.Ring || t == ItemType.Bracelet;
	}
}
