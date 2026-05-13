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

		/// <summary>장신구(Ring/Necklace/Bracelet) 드롭용. 1~2개 affix 무작위 — type 중복 없음.</summary>
		public static List<ItemAffix> GenerateForJewelry()
		{
			int count = _rng.Next(1, 3); // 1 또는 2
			var picked = new HashSet<ItemAffixType>();
			var result = new List<ItemAffix>(count);
			int safety = 16;
			while (result.Count < count && safety-- > 0)
			{
				var (type, min, max) = Pool[_rng.Next(Pool.Length)];
				if (!picked.Add(type)) continue;
				float value = (type == ItemAffixType.BonusCritRate || type == ItemAffixType.BonusMoveSpeed)
					? (float)(_rng.NextDouble() * (max - min) + min)
					: _rng.Next((int)min, (int)max + 1);
				result.Add(new ItemAffix { Type = type, Value = value });
			}
			return result;
		}

		public static bool IsJewelry(ItemType t) =>
			t == ItemType.Necklace || t == ItemType.Ring || t == ItemType.Bracelet;
	}
}
