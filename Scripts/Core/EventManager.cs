using Godot;
using System;

namespace FirstGame.Core
{
	public static class EventManager
	{
		// 예시: 플레이어 사망 이벤트 (Example: Player Death Event)
		public static event Action OnPlayerDeath;

		public static void TriggerPlayerDeath()
		{
			OnPlayerDeath?.Invoke();
		}

		// 필요 시 전역 이벤트 추가 (Add more global events here as needed)
	}
}
