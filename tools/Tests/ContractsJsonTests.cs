using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FirstGame.Data;
using Xunit;

namespace FirstGame.Tests
{
	// Godot 미의존 contracts.json 무결성 테스트.
	// HuntingContractManager는 Godot 의존이라 링크 불가 — 런타임 동작(최대 3개·중복
	// 수락 금지·unknown restore 폐기)은 validate.py + 수동 체크리스트가 담당하고,
	// 여기서는 매니페스트 데이터 정합(타입·타깃·보상·A안 Gather 설명·반복보스 id)을
	// 고정 검증한다. JSON은 System.Text.Json으로 직접 파싱.
	public class ContractsJsonTests
	{
		// HuntingContractManager.RepeatableBossIds 미러(Godot 링크 불가 → 안정성 픽스처).
		// 이 집합과 실제 코드/씬 정합은 validate.py R14가 별도로 잡는다.
		private static readonly HashSet<string> RepeatableBossIds = new()
		{
			"kraken_d4", "glacier_titan_f5", "inferno_drake_f6", "crystal_lord_m3"
		};

		// 비반복 1회성 스토리 보스(처치 후 재스폰 안 됨 — 현상금 1회성).
		private static readonly HashSet<string> OneShotStoryBossIds = new()
		{
			"orc_warlord_d1", "skeleton_king_d2", "ancient_lich_d3"
		};

		private sealed class Contract
		{
			public string id { get; set; } = "";
			public string title { get; set; } = "";
			public string desc { get; set; } = "";
			public string region { get; set; } = "";
			public string type { get; set; } = "";
			public string targetEnemyType { get; set; } = "";
			public string targetItemPath { get; set; } = "";
			public string targetBossId { get; set; } = "";
			public string targetOreItemPath { get; set; } = "";
			public int goal { get; set; }
			public int recommendedLevel { get; set; }
			public int goldReward { get; set; }
			public int expReward { get; set; }
			public string rewardItemPath { get; set; } = "";
			public int rewardItemQuantity { get; set; } = 1;
		}

		private sealed class Manifest
		{
			public List<Contract> contracts { get; set; } = new();
		}

		private static string RepoRoot()
		{
			var dir = new DirectoryInfo(AppContext.BaseDirectory);
			while (dir != null)
			{
				if (File.Exists(Path.Combine(dir.FullName, "Resources", "Contracts", "contracts.json")))
					return dir.FullName;
				dir = dir.Parent;
			}
			throw new InvalidOperationException("repo root(Resources/Contracts/contracts.json) 탐색 실패");
		}

		private static List<Contract> Load()
		{
			string path = Path.Combine(RepoRoot(), "Resources", "Contracts", "contracts.json");
			var opts = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
			var m = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(path), opts);
			Assert.NotNull(m);
			Assert.NotEmpty(m.contracts);
			return m.contracts;
		}

		private static bool TryType(string s, out ContractType t) => Enum.TryParse(s, out t);

		[Fact]
		public void ContractIds_AreUnique()
		{
			var ids = Load().Select(c => c.id).ToList();
			Assert.All(ids, id => Assert.False(string.IsNullOrWhiteSpace(id)));
			Assert.Equal(ids.Count, ids.Distinct().Count());
		}

		[Fact]
		public void EveryContract_HasValidTypeAndPositiveGoal()
		{
			foreach (var c in Load())
			{
				Assert.True(TryType(c.type, out _), $"{c.id}: 알 수 없는 type '{c.type}'");
				Assert.True(c.goal > 0, $"{c.id}: goal은 양수여야 함");
				Assert.True(c.recommendedLevel >= 1, $"{c.id}: recommendedLevel >= 1");
				Assert.True(c.goldReward >= 0 && c.expReward >= 0, $"{c.id}: 보상 음수 금지");
			}
		}

		[Fact]
		public void RewardItem_QuantityPositive_WhenPathSet()
		{
			foreach (var c in Load())
				if (!string.IsNullOrEmpty(c.rewardItemPath))
					Assert.True(c.rewardItemQuantity > 0, $"{c.id}: rewardItemPath 있으면 수량>0");
		}

		[Fact]
		public void TypeSpecificTargets_ArePresent()
		{
			foreach (var c in Load())
			{
				TryType(c.type, out var t);
				switch (t)
				{
					case ContractType.Kill:
						Assert.False(string.IsNullOrWhiteSpace(c.targetEnemyType), $"{c.id}: Kill은 targetEnemyType 필수");
						break;
					case ContractType.Gather:
						Assert.False(string.IsNullOrWhiteSpace(c.targetItemPath), $"{c.id}: Gather는 targetItemPath 필수");
						break;
					case ContractType.BossKill:
						Assert.False(string.IsNullOrWhiteSpace(c.targetBossId), $"{c.id}: BossKill은 targetBossId 필수");
						break;
					case ContractType.Mining:
						Assert.False(string.IsNullOrWhiteSpace(c.targetOreItemPath), $"{c.id}: Mining은 targetOreItemPath 필수");
						break;
				}
			}
		}

		// A안 정합: Gather는 완료 시 소모형이므로 설명에 소모/납품 의도가 드러나야 한다
		// (UI 라벨·ConsumeItems 동작과 사용자 기대 일치).
		[Fact]
		public void GatherContracts_DescribeConsumeSemantics()
		{
			foreach (var c in Load())
			{
				if (!TryType(c.type, out var t) || t != ContractType.Gather) continue;
				string d = c.desc ?? "";
				Assert.True(d.Contains("소모") || d.Contains("납품"),
					$"{c.id}: Gather 설명에 '소모/납품' 의도가 없음 (A안 정합) → '{d}'");
			}
		}

		// BossKill 타깃은 반복 보스 또는 알려진 1회성 스토리 보스 중 하나여야 한다.
		// 알 수 없는 boss id가 새로 들어오면(오타·드리프트) 즉시 실패.
		[Fact]
		public void BossKillTargets_AreKnownBossIds()
		{
			foreach (var c in Load())
			{
				if (!TryType(c.type, out var t) || t != ContractType.BossKill) continue;
				bool known = RepeatableBossIds.Contains(c.targetBossId)
							 || OneShotStoryBossIds.Contains(c.targetBossId);
				Assert.True(known, $"{c.id}: 알 수 없는 보스 id '{c.targetBossId}'");
			}
		}

		// 반복 가능 계약(완료 후 재수락)은 enhance_stone(강화/재련 게이트 통화)을
		// 보상으로 주면 무한 faucet → 1회성 BossKill만 허용. balance.py B5 미러.
		[Fact]
		public void RepeatableContracts_DoNotGrantEnhanceStone()
		{
			foreach (var c in Load())
			{
				TryType(c.type, out var t);
				bool oneShot = t == ContractType.BossKill && OneShotStoryBossIds.Contains(c.targetBossId);
				if (oneShot) continue; // 1회성만 enhance_stone 허용
				bool grantsStone = (c.rewardItemPath ?? "").Contains("enhance_stone");
				Assert.False(grantsStone, $"{c.id}: 반복 계약이 enhance_stone 지급(무한 faucet 금지)");
			}
		}

		// 4개 권역 모두 Kill 계약을 최소 1개 가져야 한다(가장 단순한 사냥 동기).
		// 권역에 사냥터가 있어도 Kill 계약이 비면 "이 권역에서 무엇을 잡을지" 안내가
		// 약해진다. 권역별 진입점 보장.
		[Fact]
		public void EveryRegion_HasAtLeastOneKillContract()
		{
			var byRegion = Load()
				.Where(c => TryType(c.type, out var t) && t == ContractType.Kill)
				.GroupBy(c => c.region)
				.ToDictionary(g => g.Key, g => g.Count());
			foreach (var region in new[] { "town_region", "outpost_region", "coast_region", "mountain_region" })
			{
				Assert.True(byRegion.TryGetValue(region, out var n) && n >= 1,
					$"{region}: Kill 계약이 최소 1개 필요 (현재 {byRegion.GetValueOrDefault(region, 0)}개)");
			}
		}

		// 모든 RepeatableBoss 는 최소 1개의 BossKill 계약으로 노출되어야 한다.
		// 보스가 있는데 계약이 없으면 플레이어가 반복 처치할 동기가 약해진다.
		[Fact]
		public void EveryRepeatableBoss_HasContract()
		{
			var contractBosses = Load()
				.Where(c => TryType(c.type, out var t) && t == ContractType.BossKill)
				.Select(c => c.targetBossId)
				.ToHashSet();
			foreach (var boss in RepeatableBossIds)
			{
				Assert.Contains(boss, contractBosses);
			}
		}

		// 반복 계약 총 보상가치(gold + rewardItem SellPrice × qty) 캡 — balance.py B6 미러.
		// 여기서는 item SellPrice 를 파일에서 직접 못 읽으므로 골드+레벨 비만 검사하고
		// 본격적인 SellPrice 합산은 balance.py 가 담당한다. xUnit 측은 골드/Lv 캡만.
		[Fact]
		public void RepeatableContracts_GoldPerLevel_WithinCap()
		{
			foreach (var c in Load())
			{
				TryType(c.type, out var t);
				bool oneShot = t == ContractType.BossKill && OneShotStoryBossIds.Contains(c.targetBossId);
				if (oneShot) continue;
				int lvl = Math.Max(1, c.recommendedLevel);
				double perLv = (double)c.goldReward / lvl;
				// balance.py 가 60G/Lv 초과부터 경고하므로 xUnit 은 120G/Lv 를 에러로 잡는다
				// (경고 영역은 의도된 튜닝 가능 — 명백한 폭주만 차단).
				Assert.True(perLv <= 120.0, $"{c.id}: 반복 계약 보상 {c.goldReward}G/Lv{lvl} = {perLv:F0}/Lv 초과(상한 120)");
			}
		}
	}
}
