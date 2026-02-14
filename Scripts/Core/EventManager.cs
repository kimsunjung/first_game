using Godot;
using System;

namespace FirstGame.Core
{
	public static class EventManager
	{
		// Example: Player Death Event
		public static event Action OnPlayerDeath;

		public static void TriggerPlayerDeath()
		{
			OnPlayerDeath?.Invoke();
		}

		// Add more global events here as needed
	}
}
