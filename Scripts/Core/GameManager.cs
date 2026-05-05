using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core.Interfaces;

namespace FirstGame.Core
{
	public partial class GameManager : Node
	{
		// 싱글톤 패턴 (Singleton Pattern)
		public static GameManager Instance { get; private set; }

		// 예시: 전역 상태 (골드) (Example Global State)
		private int _playerGold = 0;
		public int PlayerGold 
		{ 
			get => _playerGold;
			set
			{
				_playerGold = value;
				OnGoldChanged?.Invoke(_playerGold);
			}
		}

		// 플레이어 참조 (Player reference — set by PlayerController)
		public IPlayer Player { get; set; }

		// 활성 적 캐시 (Active enemy cache — avoids GetNodesInGroup every frame)
		private readonly List<Node2D> _activeEnemies = new();
		public IReadOnlyList<Node2D> ActiveEnemies => _activeEnemies;
		public void RegisterEnemy(Node2D enemy) => _activeEnemies.Add(enemy);
		public void UnregisterEnemy(Node2D enemy) => _activeEnemies.Remove(enemy);

		// 처치한 보스 목록
		private readonly HashSet<string> _defeatedBosses = new();
		public IReadOnlyCollection<string> DefeatedBosses => _defeatedBosses;
		public void RecordBossDefeat(string bossId) => _defeatedBosses.Add(bossId);
		public bool IsBossDefeated(string bossId) => _defeatedBosses.Contains(bossId);
		public void RestoreDefeatedBosses(List<string> bosses)
		{
			_defeatedBosses.Clear();
			foreach (var b in bosses) _defeatedBosses.Add(b);
		}

		public void ResetForNewGame()
		{
			PlayerGold = 0;
			_defeatedBosses.Clear();
			_activeEnemies.Clear();
			EventManager.ResetAll();
		}

		// UI 업데이트용 이벤트 (Event for UI updates)
		public event Action<int> OnGoldChanged;

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				BalanceData.Load();
				GD.Print("게임 매니저 초기화됨 (GameManager Initialized)");
			}
			else
			{
				QueueFree();
			}
		}

		public override void _ExitTree()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		// 모바일 뒤로가기 버튼 처리: 열린 UI 창이 있으면 닫고, 없으면 무시 (즉시 종료 방지)
		public override void _Notification(int what)
		{
			if (what == NotificationWMGoBackRequest)
			{
				FirstGame.UI.WindowManager.CloseTop();
			}
		}
	}
}
