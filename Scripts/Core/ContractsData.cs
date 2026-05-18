using System.Collections.Generic;
using System.Text.Json;
using Godot;
using FirstGame.Data;

namespace FirstGame.Core
{
	// 사냥 계약 매니페스트 로더 (Resources/Contracts/contracts.json).
	// 반복 파밍 보조 — 일일/시간제한/숙제 없음. CraftingData와 동일 FileAccess+JsonDocument 패턴.
	public static class ContractsData
	{
		private const string ManifestPath = "res://Resources/Contracts/contracts.json";

		public static List<ContractData> Contracts { get; private set; } = new();
		private static bool _loaded = false;

		public static void EnsureLoaded()
		{
			if (_loaded) return;
			_loaded = true;
			try
			{
				using var file = FileAccess.Open(ManifestPath, FileAccess.ModeFlags.Read);
				if (file == null) { GD.PrintErr($"ContractsData: 파일 열기 실패 ({ManifestPath})"); return; }
				var doc = JsonDocument.Parse(file.GetAsText());
				if (!doc.RootElement.TryGetProperty("contracts", out var arr)) return;
				var list = new List<ContractData>();
				foreach (var c in arr.EnumerateArray())
				{
					var cd = new ContractData
					{
						Id = Str(c, "id"),
						Title = Str(c, "title"),
						Description = Str(c, "desc"),
						Region = Str(c, "region"),
						Type = ParseType(Str(c, "type")),
						TargetEnemyType = Str(c, "targetEnemyType"),
						TargetItemPath = Str(c, "targetItemPath"),
						TargetBossId = Str(c, "targetBossId"),
						TargetOreItemPath = Str(c, "targetOreItemPath"),
						Goal = Int(c, "goal", 1),
						RecommendedLevel = Int(c, "recommendedLevel", 1),
						GoldReward = Int(c, "goldReward", 0),
						ExpReward = Int(c, "expReward", 0),
						RewardItemPath = Str(c, "rewardItemPath"),
						RewardItemQuantity = Int(c, "rewardItemQuantity", 1),
					};
					if (!string.IsNullOrEmpty(cd.Id)) list.Add(cd);
				}
				Contracts = list;
				GD.Print($"ContractsData: 계약 {Contracts.Count}개 로드");
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"ContractsData: 파싱 실패 - {e.Message}");
			}
		}

		public static ContractData Find(string id)
		{
			if (string.IsNullOrEmpty(id)) return null;
			foreach (var c in Contracts) if (c.Id == id) return c;
			return null;
		}

		private static ContractType ParseType(string s) => s switch
		{
			"Gather" => ContractType.Gather,
			"BossKill" => ContractType.BossKill,
			"Mining" => ContractType.Mining,
			_ => ContractType.Kill,
		};

		private static string Str(JsonElement e, string key)
			=> e.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : "";

		private static int Int(JsonElement e, string key, int def)
			=> e.TryGetProperty(key, out var v) && v.TryGetInt32(out var n) ? n : def;
	}
}
