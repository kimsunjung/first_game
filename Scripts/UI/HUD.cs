using Godot;
using System;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	public partial class HUD : CanvasLayer
	{
		private ProgressBar _healthBar;
		private Label _healthLabel;
		private Label _goldLabel;
		private IPlayer _player;

		private Control _gameOverPanel;
		private Button _restartButton;

		private Label _saveNotification;
		private Label _itemPickupNotification;

		// 퀵슬롯 UI
		private TextureRect[] _quickSlotIcons = new TextureRect[4];
		private Label[] _quickSlotQtys = new Label[4];

		// MP / 레벨 / EXP (없을 수 있으므로 null-safe)
		private ProgressBar _mpBar;
		private Label _mpLabel;
		private Label _levelLabel;
		private ProgressBar _expBar;
		private Label _levelUpLabel;


		public override void _Ready()
		{
			_healthBar = GetNode<ProgressBar>("%HealthBar");
			_healthLabel = GetNode<Label>("%HealthLabel");
			_goldLabel = GetNode<Label>("%GoldLabel");

			_gameOverPanel = GetNode<Control>("%GameOverPanel");
			_restartButton = GetNode<Button>("%RestartButton");
			_gameOverPanel.Visible = false;

			_saveNotification = GetNode<Label>("%SaveNotification");
			_saveNotification.Visible = false;

			_itemPickupNotification = GetNode<Label>("%ItemPickupNotification");
			_itemPickupNotification.Visible = false;

			for (int i = 0; i < 4; i++)
			{
				_quickSlotIcons[i] = GetNode<TextureRect>($"%QuickSlotIcon{i + 1}");
				_quickSlotQtys[i] = GetNode<Label>($"%QuickSlotQty{i + 1}");
			}

			// null-safe: 씬에 노드가 없어도 크래시 없음
			_mpBar = GetNodeOrNull<ProgressBar>("%MpBar");
			_mpLabel = GetNodeOrNull<Label>("%MpLabel");
			_levelLabel = GetNodeOrNull<Label>("%LevelLabel");
			_expBar = GetNodeOrNull<ProgressBar>("%ExpBar");
			_levelUpLabel = GetNodeOrNull<Label>("%LevelUpLabel");
			if (_levelUpLabel != null) _levelUpLabel.Visible = false;

			_restartButton.Pressed += OnRestartPressed;
			EventManager.OnPlayerDeath += ShowGameOver;
			EventManager.OnLevelUp += ShowLevelUp;
			SaveManager.OnGameSaved += ShowSaveNotification;

			if (GameManager.Instance != null)
			{
				GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
				UpdateGoldDisplay(GameManager.Instance.PlayerGold);
			}

			// 씬 트리 _Ready 순서 보장을 위해 플레이어 바인딩은 지연 실행
			CallDeferred(nameof(BindPlayer));
		}

		private void BindPlayer()
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;

			_player = player;
			_player.Stats.OnHealthChanged += UpdateHealthDisplay;
			_player.Stats.OnMpChanged += UpdateMpDisplay;
			_player.Stats.OnLevelUp += UpdateLevelDisplay;
			_player.Stats.OnExpChanged += UpdateExpDisplay;
			_player.Inventory.OnItemPickedUp += ShowItemPickup;
			_player.Inventory.OnQuickSlotChanged += UpdateQuickSlotDisplay;
			_player.Inventory.OnInventoryChanged += UpdateQuickSlotDisplay;

			UpdateHealthDisplay(_player.Stats.CurrentHealth, _player.Stats.MaxHealth);
			UpdateMpDisplay(_player.Stats.CurrentMp, _player.Stats.MaxMp);
			UpdateLevelDisplay(_player.Stats.Level);
			UpdateExpDisplay(_player.Stats.Exp, _player.Stats.ExpToNextLevel);
			UpdateQuickSlotDisplay();
		}

		public override void _ExitTree()
		{
			if (GameManager.Instance != null)
				GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;

			if (_player is GodotObject playerObj && IsInstanceValid(playerObj))
			{
				_player.Stats.OnHealthChanged -= UpdateHealthDisplay;
				_player.Stats.OnMpChanged -= UpdateMpDisplay;
				_player.Stats.OnLevelUp -= UpdateLevelDisplay;
				_player.Stats.OnExpChanged -= UpdateExpDisplay;
				if (_player.Inventory != null)
				{
					_player.Inventory.OnItemPickedUp -= ShowItemPickup;
					_player.Inventory.OnQuickSlotChanged -= UpdateQuickSlotDisplay;
					_player.Inventory.OnInventoryChanged -= UpdateQuickSlotDisplay;
				}
			}

			EventManager.OnPlayerDeath -= ShowGameOver;
			EventManager.OnLevelUp -= ShowLevelUp;
			SaveManager.OnGameSaved -= ShowSaveNotification;
		}

		private void ShowGameOver()
		{
			AudioManager.Instance?.PlaySFX("player_death.wav");
			_gameOverPanel.Visible = true;
			UIPauseManager.RequestPause();
		}

		private async void ShowSaveNotification()
		{
			_saveNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_saveNotification.Visible = false;
		}

		private async void ShowItemPickup(ItemData item)
		{
			_itemPickupNotification.Text = $"획득: {item.ItemName}";
			_itemPickupNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_itemPickupNotification.Visible = false;
		}

		private async void ShowLevelUp(int newLevel)
		{
			if (_levelUpLabel == null) return;
			_levelUpLabel.Text = $"LEVEL UP! Lv.{newLevel}";
			_levelUpLabel.Visible = true;
			await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this) && _levelUpLabel != null)
				_levelUpLabel.Visible = false;
		}

		private void OnRestartPressed()
		{
			_gameOverPanel.Visible = false;
			UIPauseManager.Reset();
			SaveManager.LoadGame();
		}

		private void UpdateHealthDisplay(int currentHp, int maxHp)
		{
			_healthBar.MaxValue = maxHp;
			_healthBar.Value = currentHp;
			_healthLabel.Text = $"{currentHp} / {maxHp}";
		}

		private void UpdateMpDisplay(int currentMp, int maxMp)
		{
			if (_mpBar == null) return;
			_mpBar.MaxValue = maxMp;
			_mpBar.Value = currentMp;
			if (_mpLabel != null)
				_mpLabel.Text = $"{currentMp}/{maxMp}";
		}

		private void UpdateLevelDisplay(int level)
		{
			if (_levelLabel == null) return;
			_levelLabel.Text = $"Lv.{level}";
		}

		private void UpdateExpDisplay(int exp, int expToNext)
		{
			if (_expBar == null) return;
			_expBar.MaxValue = expToNext;
			_expBar.Value = exp;
		}

		private void UpdateGoldDisplay(int gold)
		{
			_goldLabel.Text = $"골드: {gold}";
		}

		private void UpdateQuickSlotDisplay()
		{
			if (_player == null) return;

			for (int i = 0; i < 4; i++)
			{
				var item = _player.Inventory.QuickSlots[i];
				if (item != null)
				{
					_quickSlotIcons[i].Texture = item.Icon;
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
