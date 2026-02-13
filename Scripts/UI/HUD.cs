using Godot;
using System;
using FirstGame.Core;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
	public partial class HUD : CanvasLayer
	{
		private ProgressBar _healthBar;
		private Label _healthLabel; // Added HealthLabel
		private Label _goldLabel;   // Renamed GameLabel to GoldLabel
		private PlayerController _player;

		public override void _Ready()
		{
			// Get nodes by Unique Name (%)
			_healthBar = GetNode<ProgressBar>("%HealthBar");
			_healthLabel = GetNode<Label>("%HealthLabel"); // Get HealthLabel
			_goldLabel = GetNode<Label>("%GoldLabel");     // Get GoldLabel

			// Subscribe to Gold
			GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
			UpdateGoldDisplay(GameManager.Instance.PlayerGold);

			// Find and subscribe to Player
			// Note: If HUD is ready before Player, this might fail unless we wait or Player registers itself.
			// Reordering Main scene (HUD last) helps, but explicit registration is safer.
			// For now, we trust the Main scene reordering.
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
		}

		private void UpdateHealthDisplay(int currentHp, int maxHp)
		{
			_healthBar.MaxValue = maxHp;
			_healthBar.Value = currentHp;
			_healthLabel.Text = $"{currentHp} / {maxHp}"; // Update text
		}

		private void UpdateGoldDisplay(int gold)
		{
			_goldLabel.Text = $"Gold: {gold}";
		}
	}
}
