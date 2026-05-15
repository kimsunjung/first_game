using Godot;

namespace FirstGame.Core
{
	/// <summary>
	/// 게임 시간 트래커. 10분 실시간 = 24시간 게임 시간.
	/// 시각 효과(CanvasModulate)는 던전 전용이므로 이 시스템은 시간 진행만 관리.
	/// 시작 시간: 06:00 (아침). HUD가 OnTimeChanged 이벤트로 표시 갱신.
	/// </summary>
	public static class DayNightCycle
	{
		// 10분 실시간 = 1게임일 (24시간)
		// 즉 1시간 게임 = 25초 실시간, 1분 게임 = 25/60초 실시간
		private const float RealSecondsPerGameDay = 600f; // 10 * 60
		private const float GameSecondsPerDay = 86400f;
		private const float Multiplier = GameSecondsPerDay / RealSecondsPerGameDay; // 144배속

		// 게임 시작 시간 — 06:00 (06 * 3600 = 21600 game seconds since midnight)
		private const float StartGameSeconds = 21600f;

		private static float _gameSeconds = StartGameSeconds;
		private static int _dayCount = 1;

		/// <summary>현재 게임 일자 (1일부터 시작).</summary>
		public static int Day => _dayCount;
		/// <summary>현재 게임 시간(시) 0~23.</summary>
		public static int Hour => (int)(_gameSeconds / 3600f) % 24;
		/// <summary>현재 게임 분 0~59.</summary>
		public static int Minute => (int)((_gameSeconds % 3600f) / 60f);

		public static event System.Action OnTimeChanged;

		public static void Tick(float deltaSeconds)
		{
			float prevHour = Hour;
			_gameSeconds += deltaSeconds * Multiplier;
			while (_gameSeconds >= GameSecondsPerDay)
			{
				_gameSeconds -= GameSecondsPerDay;
				_dayCount++;
			}
			if ((int)prevHour != Hour) OnTimeChanged?.Invoke();
		}

		/// <summary>SaveData 호환 — 0~1 정규화 시간(자정=0).</summary>
		public static float NormalizedTime
		{
			get => _gameSeconds / GameSecondsPerDay;
			set { _gameSeconds = Mathf.Clamp(value, 0f, 0.999f) * GameSecondsPerDay; }
		}

		public static void RestoreFromSave(float normalizedTime, int day)
		{
			NormalizedTime = normalizedTime;
			_dayCount = Mathf.Max(1, day);
			OnTimeChanged?.Invoke();
		}

		public static void ResetForNewGame()
		{
			_gameSeconds = StartGameSeconds;
			_dayCount = 1;
			OnTimeChanged?.Invoke();
		}

		public static string FormatTime() => $"{Hour:00}:{Minute:00}";
	}
}
