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

		// 메인 퀘스트 매니저 (1개씩만 진행)
		public QuestManager QuestManager { get; } = new();

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
			QuestManager.RestoreFromSave("", 0, null);
			EventManager.ResetAll(); // 내부에서 QuestManager.Resubscribe 자동 호출
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

		// throttle 만료 시점에 dirty save를 자동 flush (RequestAutoSave 후 추가 행동 없이 앱이
		// 그대로 멈춰도 30초 뒤에는 보존되도록).
		public override void _Process(double delta)
		{
			SaveManager.TickDirty();
		}

		// 모바일 뒤로가기 버튼 처리: 열린 UI 창이 있으면 닫고, 없으면 종료 확인 다이얼로그.
		public override void _Notification(int what)
		{
			if (what == NotificationWMGoBackRequest)
			{
				if (FirstGame.UI.WindowManager.CloseTop()) return;
				ShowExitConfirm();
			}
		}

		private ConfirmationDialog _exitDialog;
		private bool _exitDialogPausing = false;

		private void ShowExitConfirm()
		{
			if (_exitDialog == null || !IsInstanceValid(_exitDialog))
			{
				_exitDialog = new ConfirmationDialog
				{
					Title = "종료",
					DialogText = "게임을 종료하시겠습니까?",
					OkButtonText = "예",
					CancelButtonText = "아니오",
					ProcessMode = ProcessModeEnum.Always
				};
				_exitDialog.Confirmed += () =>
				{
					SaveManager.FlushDirtySave(); // 종료 직전 dirty 진행도 보존
					GetTree().Quit();
				};
				_exitDialog.VisibilityChanged += OnExitDialogVisibilityChanged;
				GetTree().Root.AddChild(_exitDialog);
			}
			if (_exitDialog.Visible) return;
			_exitDialogPausing = true;
			UIPauseManager.RequestPause();
			_exitDialog.PopupCentered();
		}

		private void OnExitDialogVisibilityChanged()
		{
			if (_exitDialog == null || _exitDialog.Visible) return;
			if (_exitDialogPausing)
			{
				_exitDialogPausing = false;
				UIPauseManager.ReleasePause();
			}
		}
	}
}
