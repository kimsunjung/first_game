using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace FirstGame.Core
{
	// 제작 레시피 매니페스트 로더 (Resources/Recipes/recipes.json).
	// 확정 제작 — 확률/뽑기 없음. BalanceData와 동일한 FileAccess+JsonDocument 패턴.
	public static class CraftingData
	{
		private const string RecipePath = "res://Resources/Recipes/recipes.json";

		public struct Ingredient { public string Path; public int Qty; }
		public class Recipe
		{
			public string Id;
			public string Name;
			public string Desc;
			public int Gold;
			public List<Ingredient> Materials = new();
			public string ResultPath;
			public int ResultQty;
		}

		public static List<Recipe> Recipes { get; private set; } = new();
		private static bool _loaded = false;

		public static void EnsureLoaded()
		{
			if (_loaded) return;
			_loaded = true;
			try
			{
				using var file = FileAccess.Open(RecipePath, FileAccess.ModeFlags.Read);
				if (file == null) { GD.PrintErr($"CraftingData: 파일 열기 실패 ({RecipePath})"); return; }
				var doc = JsonDocument.Parse(file.GetAsText());
				if (!doc.RootElement.TryGetProperty("recipes", out var arr)) return;
				var list = new List<Recipe>();
				foreach (var r in arr.EnumerateArray())
				{
					var rec = new Recipe
					{
						Id = Str(r, "id"),
						Name = Str(r, "name"),
						Desc = Str(r, "desc"),
						Gold = r.TryGetProperty("gold", out var g) ? g.GetInt32() : 0,
					};
					if (r.TryGetProperty("result", out var res))
					{
						rec.ResultPath = Str(res, "path");
						rec.ResultQty = res.TryGetProperty("qty", out var rq) ? rq.GetInt32() : 1;
					}
					if (r.TryGetProperty("materials", out var mats))
						foreach (var m in mats.EnumerateArray())
							rec.Materials.Add(new Ingredient
							{
								Path = Str(m, "path"),
								Qty = m.TryGetProperty("qty", out var mq) ? mq.GetInt32() : 1
							});
					if (!string.IsNullOrEmpty(rec.ResultPath)) list.Add(rec);
				}
				Recipes = list;
				GD.Print($"CraftingData: 레시피 {Recipes.Count}개 로드");
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"CraftingData: 파싱 실패 - {e.Message}");
			}
		}

		private static string Str(JsonElement e, string key)
			=> e.TryGetProperty(key, out var v) ? v.GetString() : "";
	}
}
