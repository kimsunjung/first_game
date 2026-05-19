using System.Collections.Generic;

namespace FirstGame.Data
{
	// 씬 → 권장 레벨(진입 권장 캐릭터 레벨). 포탈 프롬프트에 "(권장 Lv.N+)"로
	// 표시. contracts.json recommendedLevel + game_balance.json zones 난이도 +
	// 월드 진행 순서(CLAUDE.md)에서 도출. Godot 미의존(단위테스트 가능).
	// 미등록/거점(0)은 표시 생략.
	public static class MapLevels
	{
		private static readonly Dictionary<string, int> _levels = new()
		{
			// 거점(0 = 표시 안 함 — 안전지대)
			{ "town", 0 },
			{ "field_outpost", 0 },
			{ "harbor_village", 0 },
			{ "mountain_refuge", 0 },
			// town_region
			{ "town_outskirts", 1 },
			{ "green_meadow", 2 },
			{ "goblin_woods", 3 },
			{ "old_orc_road", 4 },
			{ "field_1", 2 },
			{ "mine_1", 4 },
			{ "dungeon_1", 5 },
			// outpost_region
			{ "field_2", 6 },
			{ "ruined_crossroad", 7 },
			{ "field_3", 10 },
			{ "mine_2", 7 },
			{ "dungeon_2", 9 },
			{ "dungeon_3", 13 },
			// coast_region
			{ "harbor_outskirts", 11 },
			{ "crab_beach", 12 },
			{ "pirate_camp", 12 },
			{ "field_4_harbor", 11 },
			{ "dungeon_4_sunken_shrine", 14 },
			// mountain_region
			{ "snowfield_edge", 15 },
			{ "frozen_valley", 16 },
			{ "field_5_snowfield", 16 },
			{ "volcano_approach", 17 },
			{ "lava_field", 18 },
			{ "field_6_volcano", 18 },
			{ "mine_3", 18 },
		};

		private static string ToKey(string scenePathOrName)
		{
			string key = scenePathOrName;
			int slash = key.LastIndexOf('/');
			if (slash >= 0) key = key.Substring(slash + 1);
			if (key.EndsWith(".tscn")) key = key.Substring(0, key.Length - 5);
			return key;
		}

		// 0 = 권장 레벨 표시 안 함(거점/미등록). 그 외 = 진입 권장 레벨.
		public static int Get(string scenePathOrName)
		{
			if (string.IsNullOrEmpty(scenePathOrName)) return 0;
			return _levels.TryGetValue(ToKey(scenePathOrName), out var lv) ? lv : 0;
		}
	}
}
