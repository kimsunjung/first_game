using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	/// <summary>
	/// Esc 키 / 모바일 뒤로가기 시 표시되는 일시정지 메뉴.
	/// 옵션: 메인화면으로 이동 / 설정 / 종료 / 취소.
	/// </summary>
	public partial class PauseMenu : CanvasLayer
	{
		private Panel _root;
		private bool _pausing = false;

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			BuildUI();
			Visible = false;
		}

		private void BuildUI()
		{
			_root = new Panel
			{
				AnchorLeft = 0.5f,
				AnchorTop = 0.5f,
				AnchorRight = 0.5f,
				AnchorBottom = 0.5f,
				OffsetLeft = -120,
				OffsetTop = -130,
				OffsetRight = 120,
				OffsetBottom = 130,
				ProcessMode = ProcessModeEnum.Always
			};
			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.08f, 0.1f, 0.95f),
				BorderColor = new Color(0.55f, 0.45f, 0.25f, 1f)
			};
			style.SetBorderWidthAll(2);
			style.SetCornerRadiusAll(4);
			_root.AddThemeStyleboxOverride("panel", style);
			AddChild(_root);

			var bg = new ColorRect
			{
				AnchorRight = 1, AnchorBottom = 1,
				Color = new Color(0, 0, 0, 0.45f),
				MouseFilter = Control.MouseFilterEnum.Stop,
				ProcessMode = ProcessModeEnum.Always
			};
			AddChild(bg);
			MoveChild(bg, 0);

			var vbox = new VBoxContainer
			{
				AnchorRight = 1, AnchorBottom = 1,
				OffsetLeft = 16, OffsetTop = 16,
				OffsetRight = -16, OffsetBottom = -16
			};
			vbox.AddThemeConstantOverride("separation", 8);
			_root.AddChild(vbox);

			var title = new Label
			{
				Text = "메뉴",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			title.AddThemeFontSizeOverride("font_size", 18);
			vbox.AddChild(title);

			AddMenuButton(vbox, "메인화면으로 이동", OnGoToMain);
			AddMenuButton(vbox, "설정", OnSettings);
			AddMenuButton(vbox, "종료", OnQuit);
			AddMenuButton(vbox, "취소", Close);
		}

		private void AddMenuButton(VBoxContainer parent, string text, System.Action onPressed)
		{
			var btn = new Button { Text = text, CustomMinimumSize = new Vector2(0, 36) };
			btn.AddThemeFontSizeOverride("font_size", 14);
			btn.Pressed += () => onPressed?.Invoke();
			parent.AddChild(btn);
		}

		public void Open()
		{
			if (Visible) return;
			Visible = true;
			_pausing = true;
			UIPauseManager.RequestPause();
		}

		public void Close()
		{
			if (!Visible) return;
			Visible = false;
			if (_pausing)
			{
				_pausing = false;
				UIPauseManager.ReleasePause();
			}
		}

		private void OnGoToMain()
		{
			Close();
			SaveManager.FlushBeforeExit();
			GetTree().ChangeSceneToFile("res://Scenes/UI/main_menu.tscn");
		}

		private void OnSettings()
		{
			var settings = GetTree()?.CurrentScene?.GetNodeOrNull<SettingsUI>("SettingsUI");
			if (settings != null)
			{
				Close();
				settings.Open();
			}
		}

		private void OnQuit()
		{
			SaveManager.FlushBeforeExit();
			GetTree().Quit();
		}
	}
}
