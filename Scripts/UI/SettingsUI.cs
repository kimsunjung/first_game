using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	public partial class SettingsUI : CanvasLayer
	{
		private HSlider _bgmSlider;
		private HSlider _sfxSlider;
		private CheckButton _shakeToggle;
		private Label _bgmValueLabel;
		private Label _sfxValueLabel;

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			Visible = false;

			_bgmSlider = GetNodeOrNull<HSlider>("%BgmSlider");
			_sfxSlider = GetNodeOrNull<HSlider>("%SfxSlider");
			_shakeToggle = GetNodeOrNull<CheckButton>("%ShakeToggle");
			_bgmValueLabel = GetNodeOrNull<Label>("%BgmValueLabel");
			_sfxValueLabel = GetNodeOrNull<Label>("%SfxValueLabel");

			if (_bgmSlider != null)
			{
				_bgmSlider.MinValue = 0;
				_bgmSlider.MaxValue = 100;
				_bgmSlider.Value = (AudioManager.Instance?.BgmVolume ?? 0.5f) * 100;
				_bgmSlider.ValueChanged += OnBgmChanged;
			}
			if (_sfxSlider != null)
			{
				_sfxSlider.MinValue = 0;
				_sfxSlider.MaxValue = 100;
				_sfxSlider.Value = (AudioManager.Instance?.SfxVolume ?? 0.8f) * 100;
				_sfxSlider.ValueChanged += OnSfxChanged;
			}
			if (_shakeToggle != null)
			{
				_shakeToggle.ButtonPressed = GameSettings.ScreenShakeEnabled;
				_shakeToggle.Toggled += OnShakeToggled;
			}

			UpdateLabels();
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				if (k.Keycode == Key.Escape || k.PhysicalKeycode == Key.Escape)
				{
					// 설정이 열려있으면 닫기
					if (Visible)
					{
						Visible = false;
						UIPauseManager.ReleasePause();
						return;
					}

					// 다른 UI가 일시정지 중이면 무시
					if (UIPauseManager.IsPaused) return;

					Visible = true;
					UIPauseManager.RequestPause();
				}
			}
		}

		private void OnBgmChanged(double value)
		{
			AudioManager.Instance?.SetBgmVolume((float)value / 100f);
			UpdateLabels();
		}

		private void OnSfxChanged(double value)
		{
			AudioManager.Instance?.SetSfxVolume((float)value / 100f);
			UpdateLabels();
		}

		private void OnShakeToggled(bool pressed)
		{
			GameSettings.ScreenShakeEnabled = pressed;
		}

		private void UpdateLabels()
		{
			if (_bgmValueLabel != null && _bgmSlider != null)
				_bgmValueLabel.Text = $"{(int)_bgmSlider.Value}%";
			if (_sfxValueLabel != null && _sfxSlider != null)
				_sfxValueLabel.Text = $"{(int)_sfxSlider.Value}%";
		}

		public override void _ExitTree()
		{
			if (_bgmSlider != null) _bgmSlider.ValueChanged -= OnBgmChanged;
			if (_sfxSlider != null) _sfxSlider.ValueChanged -= OnSfxChanged;
			if (_shakeToggle != null) _shakeToggle.Toggled -= OnShakeToggled;
			if (Visible) UIPauseManager.ReleasePause();
		}
	}
}
