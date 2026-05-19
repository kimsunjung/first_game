using System;
using System.IO;
using System.Linq;
using FirstGame.Data;
using Xunit;

namespace FirstGame.Tests
{
	// 신규 맵 추가 시 MapNames 한글 등록 누락을 잡는다(HUD 좌하단 표시·미니맵
	// 이름 일관성). Scenes/Maps 디렉토리를 직접 스캔 — Godot 미의존.
	public class MapCoverageTests
	{
		private static string RepoRoot()
		{
			var dir = new DirectoryInfo(AppContext.BaseDirectory);
			while (dir != null)
			{
				if (Directory.Exists(Path.Combine(dir.FullName, "Scenes", "Maps")))
					return dir.FullName;
				dir = dir.Parent;
			}
			throw new InvalidOperationException("repo root(Scenes/Maps) 탐색 실패");
		}

		[Fact]
		public void EveryMapScene_HasRegisteredKoreanName()
		{
			string mapsDir = Path.Combine(RepoRoot(), "Scenes", "Maps");
			var scenes = Directory.GetFiles(mapsDir, "*.tscn", SearchOption.TopDirectoryOnly)
								  .Select(Path.GetFileNameWithoutExtension)
								  .OrderBy(x => x)
								  .ToList();
			Assert.NotEmpty(scenes);
			var missing = scenes.Where(s => !MapNames.IsRegistered(s)).ToList();
			Assert.True(missing.Count == 0,
				"MapNames 미등록 맵: " + string.Join(", ", missing));
		}
	}
}
