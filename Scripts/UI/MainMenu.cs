using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	public partial class MainMenu : CanvasLayer
	{
		private Button _newGameBtn;
		private Button _continueBtn;
		private Button _settingsBtn;
		private Button _quitBtn;
		private CanvasLayer _settingsUI;
		private PanelContainer _confirmPanel;
		private Button _confirmYes;
		private Button _confirmNo;

		public override void _Ready()
		{
			_newGameBtn = GetNode<Button>("%NewGameBtn");
			_continueBtn = GetNode<Button>("%ContinueBtn");
			_settingsBtn = GetNode<Button>("%SettingsBtn");
			_quitBtn = GetNode<Button>("%QuitBtn");
			_settingsUI = GetNodeOrNull<CanvasLayer>("SettingsUI");
			_confirmPanel = GetNode<PanelContainer>("%ConfirmPanel");
			_confirmYes = GetNode<Button>("%ConfirmYes");
			_confirmNo = GetNode<Button>("%ConfirmNo");

			_newGameBtn.Pressed += OnNewGamePressed;
			_continueBtn.Pressed += OnContinuePressed;
			_settingsBtn.Pressed += OnSettingsPressed;
			_quitBtn.Pressed += OnQuitPressed;
			_confirmYes.Pressed += OnConfirmNewGame;
			_confirmNo.Pressed += () => _confirmPanel.Visible = false;

			_confirmPanel.Visible = false;

			// 세이브 파일 존재 여부로 이어하기 버튼 활성화
			bool hasSave = SaveManager.HasSave() || SaveManager.HasSave("manual");
			_continueBtn.Disabled = !hasSave;

			// 메인 메뉴에서는 일시정지 해제
			GetTree().Paused = false;
		}

		private void OnNewGamePressed()
		{
			if (SaveManager.HasSave() || SaveManager.HasSave("manual"))
			{
				_confirmPanel.Visible = true;
				return;
			}

			StartNewGame();
		}

		private void OnConfirmNewGame()
		{
			_confirmPanel.Visible = false;
			DeleteSaveFile("autosave");
			DeleteSaveFile("manual");
			StartNewGame();
		}

		private void StartNewGame()
		{
			SaveManager.PendingLoadData = null;
			GetTree().ChangeSceneToFile("res://Scenes/Maps/town.tscn");
		}

		private void OnContinuePressed()
		{
			SaveManager.LoadGame();
		}

		private void OnSettingsPressed()
		{
			if (_settingsUI != null)
				_settingsUI.Visible = !_settingsUI.Visible;
		}

		public override void _ExitTree()
		{
			if (_newGameBtn != null) _newGameBtn.Pressed -= OnNewGamePressed;
			if (_continueBtn != null) _continueBtn.Pressed -= OnContinuePressed;
			if (_settingsBtn != null) _settingsBtn.Pressed -= OnSettingsPressed;
			if (_quitBtn != null) _quitBtn.Pressed -= OnQuitPressed;
			if (_confirmYes != null) _confirmYes.Pressed -= OnConfirmNewGame;
		}

		private void OnQuitPressed()
		{
			GetTree().Quit();
		}

		private void DeleteSaveFile(string slot)
		{
			string path = ProjectSettings.GlobalizePath($"user://saves/{slot}.json");
			if (System.IO.File.Exists(path))
			{
				System.IO.File.Delete(path);
				GD.Print($"세이브 삭제: {slot}");
			}
		}
	}
}
