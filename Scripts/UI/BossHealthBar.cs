using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	public partial class BossHealthBar : CanvasLayer
	{
		private ProgressBar _hpBar;
		private Label _bossNameLabel;
		private Label _hpLabel;

		public override void _Ready()
		{
			Visible = false;
			_hpBar = GetNodeOrNull<ProgressBar>("%BossHpBar");
			_bossNameLabel = GetNodeOrNull<Label>("%BossNameLabel");
			_hpLabel = GetNodeOrNull<Label>("%BossHpLabel");

			EventManager.OnBossSpawned += ShowBoss;
			EventManager.OnBossHealthChanged += UpdateHp;
			EventManager.OnBossDied += HideBoss;
		}

		public override void _ExitTree()
		{
			EventManager.OnBossSpawned -= ShowBoss;
			EventManager.OnBossHealthChanged -= UpdateHp;
			EventManager.OnBossDied -= HideBoss;
		}

		private void ShowBoss(int maxHp, string bossName)
		{
			Visible = true;
			if (_hpBar != null) { _hpBar.MaxValue = maxHp; _hpBar.Value = maxHp; }
			if (_bossNameLabel != null) _bossNameLabel.Text = bossName;
			if (_hpLabel != null) _hpLabel.Text = $"{maxHp} / {maxHp}";
		}

		private void UpdateHp(int hp, int maxHp)
		{
			if (_hpBar != null) _hpBar.Value = hp;
			if (_hpLabel != null) _hpLabel.Text = $"{hp} / {maxHp}";
		}

		private void HideBoss()
		{
			Visible = false;
		}
	}
}
