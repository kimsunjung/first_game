using System;
using Xunit;
using FirstGame.Data;

namespace FirstGame.Tests
{
	// 초반 기능 해금 게이트 + 엘리트 등장 게이트 정합 검증 (Godot 미의존).
	public class FeatureGateTests
	{
		[Fact]
		public void FeatureGate_Ordering_IsMonotonicByComplexity()
		{
			// 편의(창고) ≤ 동선(계약) ≤ 첫 복잡(제작) ≤ 후반(재련)
			Assert.True(FeatureGates.Storage <= FeatureGates.Contract);
			Assert.True(FeatureGates.Contract <= FeatureGates.Crafting);
			Assert.True(FeatureGates.Crafting <= FeatureGates.Reforge);
			Assert.Equal(1, FeatureGates.Storage);   // 사실상 게이트 없음
			Assert.Equal(3, FeatureGates.Contract);
			Assert.Equal(5, FeatureGates.Crafting);
			Assert.Equal(10, FeatureGates.Reforge);
		}

		[Fact]
		public void SkillShopGate_HubProgression_IsAscending()
		{
			// town(0) < outpost ≤ harbor < mountain
			Assert.True(0 < FeatureGates.SkillShopOutpost);
			Assert.True(FeatureGates.SkillShopOutpost <= FeatureGates.SkillShopHarbor);
			Assert.True(FeatureGates.SkillShopHarbor < FeatureGates.SkillShopMountain);
		}

		[Fact]
		public void LockMessage_ContainsLevelsAndFeature()
		{
			string msg = FeatureGates.LockMessage("제작대", 5, 2);
			Assert.Contains("제작대", msg);
			Assert.Contains("5", msg);
			Assert.Contains("2", msg);
		}

		// EnemySpawner.CanSpawnElite() 의 게이트 계산식과 동일:
		// gate = max(elite.minPlayerLevel, MapLevels.Get(zone))
		private static int EliteGate(string zone, int eliteMin)
			=> Math.Max(eliteMin, MapLevels.Get(zone));

		[Theory]
		// 초반 필드: 시작 직후(레벨<5) 엘리트 등장 0 — 글로벌 플로어가 zone권장보다 큼
		[InlineData("town_outskirts", 5)]   // zone 1 → max(5,1)=5
		[InlineData("field_1", 5)]          // zone 2 → max(5,2)=5
		[InlineData("green_meadow", 5)]     // zone 2 → max(5,2)=5
		// outpost 이상: zone 권장레벨이 플로어를 넘어 자연 상승
		[InlineData("field_2", 6)]          // zone 6 → max(5,6)=6
		[InlineData("field_3", 10)]         // zone 10
		[InlineData("dungeon_3", 13)]       // zone 13
		// coast / mountain 후반
		[InlineData("crab_beach", 12)]      // zone 12
		[InlineData("field_6_volcano", 18)] // zone 18
		[InlineData("mine_3", 18)]          // zone 18
		public void EliteGate_FollowsZoneRecommendedLevelWithFloor(string zone, int expected)
		{
			Assert.Equal(expected, EliteGate(zone, FeatureGates.EliteMinPlayerLevelFallback));
		}

		[Fact]
		public void EliteGate_NeverBelowGlobalFloor()
		{
			foreach (var z in new[] { "town", "town_outskirts", "field_1", "green_meadow", "goblin_woods" })
				Assert.True(EliteGate(z, 5) >= 5,
					$"{z} 엘리트 게이트가 글로벌 플로어(5) 미만이면 안 됨");
		}
	}
}
