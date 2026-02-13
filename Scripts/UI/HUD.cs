using Godot;
using System;
using FirstGame.Core;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
	public partial class HUD : CanvasLayer
	{
		private ProgressBar _healthBar;
		private Label _healthLabel; // 체력 라벨 (Health Label)
		private Label _goldLabel;   // 골드 라벨 (Gold Label)
		private PlayerController _player;

		private Control _gameOverPanel;
		private Button _restartButton;

		private Label _saveNotification; // 저장 알림 라벨 추가

		public override void _Ready()
		{
			// 고유 이름(%)으로 노드 가져오기
			_healthBar = GetNode<ProgressBar>("%HealthBar");
			_healthLabel = GetNode<Label>("%HealthLabel");
			_goldLabel = GetNode<Label>("%GoldLabel");

            // 게임 오버 노드 (Game Over Nodes)
			_gameOverPanel = GetNode<Control>("%GameOverPanel");
			_restartButton = GetNode<Button>("%RestartButton");
			_gameOverPanel.Visible = false;

            // 저장 알림 노드
            _saveNotification = GetNode<Label>("%SaveNotification");
            _saveNotification.Visible = false;

			_restartButton.Pressed += OnRestartPressed;
			EventManager.OnPlayerDeath += ShowGameOver;
            SaveManager.OnGameSaved += ShowSaveNotification;

			// 골드 변경 구독 (Subscribe to Gold)
			GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
			UpdateGoldDisplay(GameManager.Instance.PlayerGold);

			// 플레이어 찾기 및 구독 (Find and subscribe to Player)
			// HUD가 플레이어보다 먼저 준비될 수 있으므로 주의 필요.
			// 메인 씬 순서 조정(HUD를 마지막에)이 도움이 되지만, 명시적 등록이 더 안전함.
			// 지금은 씬 순서를 믿고 진행.
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0 && players[0] is PlayerController player)
			{
				_player = player;
				_player.Stats.OnHealthChanged += UpdateHealthDisplay;
				UpdateHealthDisplay(_player.Stats.CurrentHealth, _player.Stats.MaxHealth);
			}
		}

		public override void _ExitTree()
		{
			if (GameManager.Instance != null)
				GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;

			if (_player != null && IsInstanceValid(_player))
				_player.Stats.OnHealthChanged -= UpdateHealthDisplay;

			EventManager.OnPlayerDeath -= ShowGameOver;
            SaveManager.OnGameSaved -= ShowSaveNotification;
		}

		private void ShowGameOver()
		{
			_gameOverPanel.Visible = true;
			GetTree().Paused = true;
		}

        // 저장 알림 표시 (Show Save Notification)
        private async void ShowSaveNotification()
        {
            _saveNotification.Visible = true;
            await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
            if (IsInstanceValid(this))
                _saveNotification.Visible = false;
        }

		private void OnRestartPressed()
		{
            // 재시작 대신 저장된 게임 불러오기
			SaveManager.LoadGame(); 
		}

		private void UpdateHealthDisplay(int currentHp, int maxHp)
		{
			_healthBar.MaxValue = maxHp;
			_healthBar.Value = currentHp;
			_healthLabel.Text = $"{currentHp} / {maxHp}"; // 텍스트 업데이트
		}

		private void UpdateGoldDisplay(int gold)
		{
			_goldLabel.Text = $"골드: {gold}"; // "Gold:" -> "골드:"
		}
	}
}
