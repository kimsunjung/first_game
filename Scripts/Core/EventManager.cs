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

		// 보스 페이즈 전환 (1→2→3)
		public static event Action<int> OnBossPhaseChanged;
		public static void TriggerBossPhaseChanged(int phase) => OnBossPhaseChanged?.Invoke(phase);

		// 퀘스트 진행도용 — 어떤 적이 죽었는지 식별 가능
		public static event Action<string> OnEnemyKilledTyped;
		public static void TriggerEnemyKilledTyped(string enemyTypeName)
		{
			OnEnemyKilledTyped?.Invoke(enemyTypeName);
		}

		// 사냥 계약 BossKill 진행용 — bossId 식별 가능. (OnBossDied는 bossId가 없음)
		public static event Action<string> OnBossKilled;
		public static void TriggerBossKilled(string bossId)
		{
			OnBossKilled?.Invoke(bossId);
		}

		// 사냥 계약 Mining 진행용 — 채광 성공한 광석 res:// 경로.
		public static event Action<string> OnOreMined;
		public static void TriggerOreMined(string oreItemPath)
		{
			OnOreMined?.Invoke(oreItemPath);
		}

		// 경험치 지급 이벤트
		public static event Action<int> OnExpGained;
		public static void TriggerExpGained(int amount)
		{
			OnExpGained?.Invoke(amount);
		}

		/// <summary>
		/// 씬 전환 시 모든 static 이벤트 초기화. dead 참조 누적 방지.
		/// 영구 인스턴스(QuestManager 등)의 구독은 즉시 자동 복원하여 호출자가
		/// 매번 Resubscribe를 기억할 필요 없게 한다.
		/// </summary>
		public static void ResetAll()
		{
			OnPlayerDeath = null;
			OnLevelUp = null;
			OnBossSpawned = null;
			OnBossHealthChanged = null;
			OnBossDied = null;
			OnEnemyKilled = null;
			OnEnemyKilledTyped = null;
			OnBossKilled = null;
			OnOreMined = null;
			OnExpGained = null;
			OnBossPhaseChanged = null;

			// QuestManager / HuntingContractManager는 GameManager 영구 인스턴스 —
			// 적 처치/보스/채광 이벤트 구독을 잃지 않도록 복원.
			GameManager.Instance?.QuestManager?.Resubscribe();
			GameManager.Instance?.ContractManager?.Resubscribe();
		}
	}
}
