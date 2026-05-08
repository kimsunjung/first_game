using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	public partial class SettingsUI : BaseUIWindow
	{
		private HSlider _bgmSlider;
		private HSlider _sfxSlider;
		private CheckButton _shakeToggle;
		private Label _bgmValueLabel;
		private Label _sfxValueLabel;

		protected override void OnReadyInternal()
		{
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

		protected override void OnExitTreeInternal()
		{
			if (_bgmSlider != null) _bgmSlider.ValueChanged -= OnBgmChanged;
			if (_sfxSlider != null) _sfxSlider.ValueChanged -= OnSfxChanged;
			if (_shakeToggle != null) _shakeToggle.Toggled -= OnShakeToggled;
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
	}
}
