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

		// 보스 이벤트
		public static event Action<int, string> OnBossSpawned;  // (maxHp, bossName)
		public static event Action<int, int> OnBossHealthChanged;  // (hp, maxHp)
		public static event Action OnBossDied;

		public static void TriggerBossSpawned(int maxHp, string bossName)
		{
			OnBossSpawned?.Invoke(maxHp, bossName);
		}
		public static void TriggerBossHealthChanged(int hp, int maxHp)
		{
			OnBossHealthChanged?.Invoke(hp, maxHp);
		}
		public static void TriggerBossDied()
		{
			OnBossDied?.Invoke();
		}

		// 적 처치 카운트 (보스 스폰 트리거용)
		public static event Action OnEnemyKilled;
		public static void TriggerEnemyKilled()
		{
			OnEnemyKilled?.Invoke();
		}

		// 경험치 지급 이벤트
		public static event Action<int> OnExpGained;
		public static void TriggerExpGained(int amount)
		{
			OnExpGained?.Invoke(amount);
		}

		/// <summary>
		/// 씬 전환 시 모든 static 이벤트 초기화.
		/// dead 참조 누적 방지.
		/// </summary>
		public static void ResetAll()
		{
			OnPlayerDeath = null;
			OnLevelUp = null;
			OnBossSpawned = null;
			OnBossHealthChanged = null;
			OnBossDied = null;
			OnEnemyKilled = null;
			OnExpGained = null;
		}
	}
}
