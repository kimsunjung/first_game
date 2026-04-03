using Godot;

namespace FirstGame.Core
{
	/// <summary>
	/// UI 일시정지를 카운터로 관리합니다.
	/// RequestPause/ReleasePause를 쌍으로 호출하면 모든 UI가 닫혔을 때만 실제로 unpause됩니다.
	/// </summary>
	public static class UIPauseManager
	{
		private static int _pauseCount = 0;

		public static bool IsPaused => _pauseCount > 0;

		public static void RequestPause()
		{
			_pauseCount++;
			ApplyPause();
		}

		public static void ReleasePause()
		{
			_pauseCount--;
			if (_pauseCount < 0) _pauseCount = 0;
			ApplyPause();
		}

		public static void Reset()
		{
			_pauseCount = 0;
			ApplyPause();
		}

		private static void ApplyPause()
		{
			var tree = Engine.GetMainLoop() as SceneTree;
			if (tree != null)
				tree.Paused = _pauseCount > 0;
		}
	}
}
