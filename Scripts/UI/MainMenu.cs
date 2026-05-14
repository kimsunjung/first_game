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
		private SettingsUI _settingsUI;
		private PanelContainer _confirmPanel;
		private Button _confirmYes;
		private Button _confirmNo;

		public override void _Ready()
		{
			_newGameBtn = GetNode<Button>("%NewGameBtn");
			_continueBtn = GetNode<Button>("%ContinueBtn");
			_settingsBtn = GetNode<Button>("%SettingsBtn");
			_quitBtn = GetNode<Button>("%QuitBtn");
			_settingsUI = GetNodeOrNull<SettingsUI>("SettingsUI");
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

			ShowClassSelect();
		}

		private void OnConfirmNewGame()
		{
			_confirmPanel.Visible = false;
			DeleteSaveFile("autosave");
			DeleteSaveFile("manual");
			ShowClassSelect();
		}

		private void StartNewGame()
		{
			GameManager.Instance?.ResetForNewGame();
			SaveManager.PendingLoadData = null;
			GetTree().ChangeSceneToFile("res://Scenes/Maps/town.tscn");
		}

		// 신규 게임 시 클래스 선택 모달. 풀스크린 dim ColorRect로 외부 클릭 차단 +
		// 취소 버튼으로 닫기 가능. tscn 수정 없이 동적 생성.
		private Control _classSelectRoot;
		private void ShowClassSelect()
		{
			if (_classSelectRoot != null && IsInstanceValid(_classSelectRoot))
			{
				_classSelectRoot.Visible = true;
				return;
			}

			// 풀스크린 dim — 배경 클릭 차단.
			var dim = new ColorRect
			{
				Color = new Color(0, 0, 0, 0.55f),
				AnchorLeft = 0, AnchorTop = 0,
				AnchorRight = 1, AnchorBottom = 1,
				OffsetLeft = 0, OffsetTop = 0,
				OffsetRight = 0, OffsetBottom = 0,
				GrowHorizontal = Control.GrowDirection.Both,
				GrowVertical = Control.GrowDirection.Both,
				MouseFilter = Control.MouseFilterEnum.Stop,
				ProcessMode = Node.ProcessModeEnum.Always
			};
			AddChild(dim);
			_classSelectRoot = dim;

			var panel = new PanelContainer
			{
				AnchorLeft = 0.5f, AnchorRight = 0.5f,
				AnchorTop = 0.5f, AnchorBottom = 0.5f,
				OffsetLeft = -240, OffsetRight = 240,
				OffsetTop = -140, OffsetBottom = 140,
				ProcessMode = Node.ProcessModeEnum.Always
			};
			dim.AddChild(panel);

			var vbox = new VBoxContainer { CustomMinimumSize = new Vector2(440, 240) };
			vbox.AddThemeConstantOverride("separation", 8);
			panel.AddChild(vbox);

			var title = new Label
			{
				Text = "클래스 선택",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			title.AddThemeFontSizeOverride("font_size", 20);
			vbox.AddChild(title);

			AddClassButton(vbox, FirstGame.Data.PlayerClass.Warrior);
			AddClassButton(vbox, FirstGame.Data.PlayerClass.Mage);
			AddClassButton(vbox, FirstGame.Data.PlayerClass.Archer);

			var cancel = new Button
			{
				Text = "취소",
				CustomMinimumSize = new Vector2(0, 36)
			};
			cancel.Pressed += CloseClassSelect;
			vbox.AddChild(cancel);
		}

		private void CloseClassSelect()
		{
			if (_classSelectRoot != null && IsInstanceValid(_classSelectRoot))
			{
				_classSelectRoot.QueueFree();
				_classSelectRoot = null;
			}
			// 잔존 정적 클래스 선택값 안전 클리어.
			SaveManager.PendingNewGameClass = null;
		}

		private void AddClassButton(VBoxContainer vbox, FirstGame.Data.PlayerClass cls)
		{
			var btn = new Button
			{
				Text = $"{FirstGame.Data.PlayerClassUtil.DisplayName(cls)} — {FirstGame.Data.PlayerClassUtil.Description(cls)}",
				CustomMinimumSize = new Vector2(0, 44)
			};
			btn.AddThemeFontSizeOverride("font_size", 13);
			btn.Pressed += () =>
			{
				SaveManager.PendingNewGameClass = cls;
				if (_classSelectRoot != null && IsInstanceValid(_classSelectRoot))
					_classSelectRoot.QueueFree();
				_classSelectRoot = null;
				StartNewGame();
			};
			vbox.AddChild(btn);
		}

		private void OnContinuePressed()
		{
			SaveManager.LoadGame();
		}

		private void OnSettingsPressed()
		{
			_settingsUI?.Toggle();
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
			// .bak도 함께 삭제 — 미삭제 시 다음 첫 저장 후 본 파일이 깨졌을 때
			// TryReadSaveFile이 새 게임에 이전 플레이를 부활시킬 수 있음.
			string bak = path + ".bak";
			if (System.IO.File.Exists(bak)) System.IO.File.Delete(bak);
		}
	}
}
