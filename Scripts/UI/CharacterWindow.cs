using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	public partial class CharacterWindow : CanvasLayer
	{
		private Label _levelLabel;
		private Label _hpLabel;
		private Label _mpLabel;
		private Label _atkLabel;
		private Label _defLabel;
		private Label _spLabel;
		private Label _strLabel;
		private Label _conLabel;
		private Label _intLabel;
		private Button _strBtn;
		private Button _conBtn;
		private Button _intBtn;

		private IPlayer _player;

		public override void _Ready()
		{
			Visible = false;
			ProcessMode = ProcessModeEnum.Always;

			_levelLabel = GetNodeOrNull<Label>("%LevelInfo");
			_hpLabel = GetNodeOrNull<Label>("%HpInfo");
			_mpLabel = GetNodeOrNull<Label>("%MpInfo");
			_atkLabel = GetNodeOrNull<Label>("%AtkInfo");
			_defLabel = GetNodeOrNull<Label>("%DefInfo");
			_spLabel = GetNodeOrNull<Label>("%SpInfo");
			_strLabel = GetNodeOrNull<Label>("%StrInfo");
			_conLabel = GetNodeOrNull<Label>("%ConInfo");
			_intLabel = GetNodeOrNull<Label>("%IntInfo");
			_strBtn = GetNodeOrNull<Button>("%StrBtn");
			_conBtn = GetNodeOrNull<Button>("%ConBtn");
			_intBtn = GetNodeOrNull<Button>("%IntBtn");

			if (_strBtn != null) _strBtn.Pressed += () => AllocateStat("STR");
			if (_conBtn != null) _conBtn.Pressed += () => AllocateStat("CON");
			if (_intBtn != null) _intBtn.Pressed += () => AllocateStat("INT");

			var pc = GameManager.Instance?.Player;
			if (pc != null)
			{
				_player = pc;
				_player.Stats.OnStatPointsChanged += OnStatPointsChanged;
			}
		}

		public override void _ExitTree()
		{
			if (_player is GodotObject playerObj && IsInstanceValid(playerObj))
				_player.Stats.OnStatPointsChanged -= OnStatPointsChanged;
		}

		private void OnStatPointsChanged(int _)
		{
			RefreshDisplay();
		}

		/// <summary>모바일 버튼에서 직접 호출</summary>
		public void Toggle()
		{
			if (UIPauseManager.IsPaused && !Visible) return;
			Visible = !Visible;
			if (Visible) RefreshDisplay();
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				if (k.Keycode == Key.C || k.PhysicalKeycode == Key.C)
					Toggle();
			}
		}

		private void AllocateStat(string stat)
		{
			if (_player == null) return;
			_player.Stats.AllocateStat(stat);
			RefreshDisplay();
		}

		public void RefreshDisplay()
		{
			if (_player == null) return;
			var s = _player.Stats;
			if (_levelLabel != null) _levelLabel.Text = $"Lv.{s.Level}";
			if (_hpLabel != null) _hpLabel.Text = $"HP: {s.CurrentHealth} / {s.MaxHealth}";
			if (_mpLabel != null) _mpLabel.Text = $"MP: {s.CurrentMp} / {s.MaxMp}";
			if (_atkLabel != null) _atkLabel.Text = $"ATK: {s.BaseDamage}";
			if (_defLabel != null) _defLabel.Text = $"DEF: {s.Defense}";
			if (_spLabel != null) _spLabel.Text = $"SP: {s.StatPoints}";
			var prog = BalanceData.Progression;
			if (_strLabel != null) _strLabel.Text = $"STR:{s.StrPoints} (+{s.StrPoints * prog.StrAtkBonus}공)";
			if (_conLabel != null) _conLabel.Text = $"CON:{s.ConPoints} (+{s.ConPoints * prog.ConHpBonus}HP)";
			if (_intLabel != null) _intLabel.Text = $"INT:{s.IntPoints} (+{s.IntPoints * prog.IntMpBonus}MP)";
			bool hasSp = s.StatPoints > 0;
			if (_strBtn != null) _strBtn.Disabled = !hasSp;
			if (_conBtn != null) _conBtn.Disabled = !hasSp;
			if (_intBtn != null) _intBtn.Disabled = !hasSp;
		}
	}
}
