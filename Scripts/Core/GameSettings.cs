namespace FirstGame.Core
{
	/// <summary>
	/// 게임 설정값 (Core 레이어).
	/// UI에서 설정하고, Entities에서 참조.
	/// </summary>
	public static class GameSettings
	{
		public static bool ScreenShakeEnabled { get; set; } = true;
	}
}
