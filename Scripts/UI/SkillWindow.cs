using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 스킬창: Tab 키로 열고 닫기, 습득한 스킬 목록 표시
	public partial class SkillWindow : CanvasLayer
	{
		private VBoxContainer _slotContainer;
		private IPlayer _player;

		private static readonly string[] SlotKeys = { "Q", "W", "E", "R" };

		public override void _Ready()
		{
			_slotContainer = GetNode<VBoxContainer>("%SkillSlotContainer");
			Visible = false;

			var pc = GameManager.Instance?.Player;
			if (pc != null) _player = pc;
		}

		/// <summary>모바일 버튼에서 직접 호출</summary>
		public void Toggle()
		{
			Visible = !Visible;
			if (Visible) Refresh();
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				var key = k.Keycode != Key.None ? k.Keycode : k.PhysicalKeycode;
				if (key == Key.Tab)
					Toggle();
			}
		}

		private void Refresh()
		{
			foreach (Node child in _slotContainer.GetChildren())
				child.QueueFree();

			if (_player == null) return;

			var learned = _player.Stats.LearnedSkills;

			for (int i = 0; i < 4; i++)
			{
				var panel = new PanelContainer();
				panel.CustomMinimumSize = new Vector2(260, 48);

				var hbox = new HBoxContainer();
				panel.AddChild(hbox);

				// 키 바인딩 레이블
				var keyLabel = new Label();
				keyLabel.Text = SlotKeys[i];
				keyLabel.CustomMinimumSize = new Vector2(20, 0);
				keyLabel.AddThemeFontSizeOverride("font_size", 12);
				keyLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
				keyLabel.VerticalAlignment = VerticalAlignment.Center;
				hbox.AddChild(keyLabel);

				if (i < learned.Count)
				{
					var skill = learned[i];

					// 아이콘
					if (skill.Icon != null)
					{
						var icon = new TextureRect();
						icon.Texture = skill.Icon;
						icon.CustomMinimumSize = new Vector2(24, 24);
						icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
						hbox.AddChild(icon);
					}

					var vbox = new VBoxContainer();
					vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

					var nameLabel = new Label();
					nameLabel.Text = skill.SkillName;
					nameLabel.AddThemeFontSizeOverride("font_size", 12);
					nameLabel.ClipText = true;
					vbox.AddChild(nameLabel);

					var infoLabel = new Label();
					infoLabel.Text = $"{skill.Description}";
					infoLabel.AddThemeFontSizeOverride("font_size", 10);
					infoLabel.ClipText = true;
					vbox.AddChild(infoLabel);

					hbox.AddChild(vbox);
				}
				else
				{
					var emptyLabel = new Label();
					emptyLabel.Text = "─  (비어있음)";
					emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
					emptyLabel.VerticalAlignment = VerticalAlignment.Center;
					hbox.AddChild(emptyLabel);
				}

				_slotContainer.AddChild(panel);
			}

			// 빈 스킬창 안내
			if (learned.Count == 0)
			{
				var hint = new Label();
				hint.Text = "스킬 상점에서 스킬북을 구매하세요!";
				hint.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
				hint.HorizontalAlignment = HorizontalAlignment.Center;
				_slotContainer.AddChild(hint);
			}
		}
	}
}
