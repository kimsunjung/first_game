using System.Collections.Generic;

namespace FirstGame.Data
{
	// 씬 파일명 → 한글 맵 이름. HUD 좌측하단 표시용.
	// Godot 미의존(단위테스트 가능). 미등록 씬은 파일명을 보기 좋게 폴백.
	public static class MapNames
	{
		private static readonly Dictionary<string, string> _names = new()
		{
			// 거점
			{ "town", "마을" },
			{ "field_outpost", "전초기지" },
			{ "harbor_village", "항구마을" },
			{ "mountain_refuge", "산악 피난처" },
			// town_region 사냥터
			{ "town_outskirts", "마을 외곽" },
			{ "green_meadow", "푸른 초원" },
			{ "goblin_woods", "고블린 숲" },
			{ "old_orc_road", "옛 오크 길" },
			{ "field_1", "필드 1" },
			{ "mine_1", "광산 1" },
			{ "dungeon_1", "던전 1 · 오크 워로드" },
			// outpost_region
			{ "field_2", "묘지 변경" },
			{ "ruined_crossroad", "폐허 교차로" },
			{ "field_3", "저주받은 황야" },
			{ "mine_2", "광산 2" },
			{ "dungeon_2", "던전 2 · 해골 왕" },
			{ "dungeon_3", "던전 3 · 고대 리치" },
			// coast_region
			{ "harbor_outskirts", "항구 외곽" },
			{ "crab_beach", "게 해변" },
			{ "pirate_camp", "해적 야영지" },
			{ "field_4_harbor", "해안 필드" },
			{ "dungeon_4_sunken_shrine", "가라앉은 신전 · 크라켄" },
			// mountain_region
			{ "snowfield_edge", "설원 변경" },
			{ "frozen_valley", "빙결 계곡" },
			{ "field_5_snowfield", "설원 필드 · 글래시어 타이탄" },
			{ "volcano_approach", "화산 진입로" },
			{ "lava_field", "용암 평원" },
			{ "field_6_volcano", "화산 필드 · 인페르노 드레이크" },
			{ "mine_3", "수정 광산 · 크리스탈 로드" },
		};

		// scenePathOrName: "res://Scenes/Maps/mine_3.tscn" 또는 "mine_3" 모두 허용.
		public static string Get(string scenePathOrName)
		{
			if (string.IsNullOrEmpty(scenePathOrName)) return "";
			string key = scenePathOrName;
			int slash = key.LastIndexOf('/');
			if (slash >= 0) key = key.Substring(slash + 1);
			if (key.EndsWith(".tscn")) key = key.Substring(0, key.Length - 5);

			if (_names.TryGetValue(key, out var n)) return n;
			// 폴백: snake_case → "Snake Case"
			return key.Replace('_', ' ').Trim();
		}
	}
}
