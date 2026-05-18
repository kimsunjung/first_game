using FirstGame.Data;
using Xunit;

namespace FirstGame.Tests
{
	public class MapNamesTests
	{
		[Theory]
		[InlineData("res://Scenes/Maps/mine_3.tscn", "수정 광산 · 크리스탈 로드")]
		[InlineData("res://Scenes/Maps/town.tscn", "마을")]
		[InlineData("harbor_village", "항구마을")]
		[InlineData("dungeon_2.tscn", "던전 2 · 해골 왕")]
		public void Get_KnownScenes_ReturnsKoreanName(string input, string expected)
			=> Assert.Equal(expected, MapNames.Get(input));

		[Fact]
		public void Get_UnknownScene_FallsBackToPrettifiedName()
			=> Assert.Equal("some new map", MapNames.Get("res://Scenes/Maps/some_new_map.tscn"));

		[Fact]
		public void Get_NullOrEmpty_ReturnsEmpty()
		{
			Assert.Equal("", MapNames.Get(null));
			Assert.Equal("", MapNames.Get(""));
		}
	}
}
