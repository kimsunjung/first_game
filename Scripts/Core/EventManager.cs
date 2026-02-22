using Godot;
using System;

namespace FirstGame.Core
{
	public static class EventManager
	{
		public static event Action OnPlayerDeath;
		public static event Action<int> OnLevelUp; // (새 레벨)

		public static void TriggerPlayerDeath()
		{
			OnPlayerDeath?.Invoke();
		}

		public static void TriggerLevelUp(int newLevel)
		{
			OnLevelUp?.Invoke(newLevel);
		}
	}
}
