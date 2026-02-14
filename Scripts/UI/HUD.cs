using Godot;
using System;
using FirstGame.Core;
using FirstGame.Entities.Player;
using FirstGame.Data;

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
		private Label _itemPickupNotification; // 아이템 획득 알림 (Item pickup notification)

		// 퀵슬롯 UI (Quick Slot UI)
		private TextureRect[] _quickSlotIcons = new TextureRect[4];
		private Label[] _quickSlotQtys = new Label[4];

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

			// 아이템 획득 알림 (Item Pickup Notification)
			_itemPickupNotification = GetNode<Label>("%ItemPickupNotification");
			_itemPickupNotification.Visible = false;

			// 퀵슬롯 노드 연결 (Connect Quick Slot nodes)
			for (int i = 0; i < 4; i++)
			{
				_quickSlotIcons[i] = GetNode<TextureRect>($"%QuickSlotIcon{i + 1}");
				_quickSlotQtys[i] = GetNode<Label>($"%QuickSlotQty{i + 1}");
			}

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
				_player.Inventory.OnItemPickedUp += ShowItemPickup; // 아이템 획득 구독 (Subscribe to item pickup)
				_player.Inventory.OnQuickSlotChanged += UpdateQuickSlotDisplay; // 퀵슬롯 변경 구독
				_player.Inventory.OnInventoryChanged += UpdateQuickSlotDisplay; // 인벤토리 변경 시 수량 갱신
				UpdateHealthDisplay(_player.Stats.CurrentHealth, _player.Stats.MaxHealth);
				UpdateQuickSlotDisplay();
			}
		}

		public override void _ExitTree()
		{
			if (GameManager.Instance != null)
				GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;

			if (_player != null && IsInstanceValid(_player))
			{
				_player.Stats.OnHealthChanged -= UpdateHealthDisplay;
				if (_player.Inventory != null)
				{
					_player.Inventory.OnItemPickedUp -= ShowItemPickup;
					_player.Inventory.OnQuickSlotChanged -= UpdateQuickSlotDisplay;
					_player.Inventory.OnInventoryChanged -= UpdateQuickSlotDisplay;
				}
			}

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

		// 아이템 획득 알림 (Show Item Pickup Notification)
		private async void ShowItemPickup(ItemData item)
		{
			_itemPickupNotification.Text = $"획득: {item.ItemName}";
			_itemPickupNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_itemPickupNotification.Visible = false;
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

		// 퀵슬롯 UI 갱신 (Update Quick Slot Display)
		private void UpdateQuickSlotDisplay()
		{
			if (_player == null) return;

			for (int i = 0; i < 4; i++)
			{
				var item = _player.Inventory.QuickSlots[i];
				if (item != null)
				{
					_quickSlotIcons[i].Texture = item.Icon;
					// 인벤토리에서 해당 아이템의 수량 찾기
					var slot = _player.Inventory.Slots.Find(s => s.Item.ResourcePath == item.ResourcePath);
					_quickSlotQtys[i].Text = slot != null ? $"x{slot.Quantity}" : "x0";
				}
				else
				{
					_quickSlotIcons[i].Texture = null;
					_quickSlotQtys[i].Text = "";
				}
			}
		}
	}
}
