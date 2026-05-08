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
			// ui_cancel/Escape은 "닫기 전용". 모바일 뒤로가기 합성 이벤트와의 일관성을 위해 열기는 안 함.
			// 게임 도중 설정 진입은 메인 메뉴 경유로만 가능.
			if (!Visible) return;

			bool cancelPressed =
				(@event is InputEventKey k && k.Pressed && !k.Echo &&
					(k.Keycode == Key.Escape || k.PhysicalKeycode == Key.Escape))
				|| @event.IsActionPressed("ui_cancel");

			if (cancelPressed)
			{
				Visible = false;
				UIPauseManager.ReleasePause();
				GetViewport().SetInputAsHandled();
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
