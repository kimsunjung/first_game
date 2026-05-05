using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 스킬창: Tab 키로 열고 닫기, 습득한 스킬 목록 표시
	public partial class SkillWindow : BaseUIWindow
	{
		private VBoxContainer _slotContainer;
		private IPlayer _player;

		private static readonly string[] SlotKeys = { "Q", "W", "E", "R" };
		private const int SlotWidth = 276;
		private const int SlotHeight = 56;
		private const int IconSize = 34;

		protected override void OnReadyInternal()
		{
			_slotContainer = GetNode<VBoxContainer>("%SkillSlotContainer");

			var pc = GameManager.Instance?.Player;
			if (pc != null) _player = pc;
		}

		protected override void OnOpened() => Refresh();

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

			// _Ready 시점에 미등록된 경우 재시도
			if (_player == null)
				_player = GameManager.Instance?.Player;
			if (_player == null) return;

			var learned = _player.Stats.LearnedSkills;

			for (int i = 0; i < 4; i++)
			{
				bool hasSkill = i < learned.Count;
				var panel = new PanelContainer();
				panel.CustomMinimumSize = new Vector2(SlotWidth, SlotHeight);
				panel.AddThemeStyleboxOverride("panel", CreateSlotStyle(hasSkill));

				var hbox = new HBoxContainer();
				hbox.AddThemeConstantOverride("separation", 8);
				hbox.Alignment = BoxContainer.AlignmentMode.Center;
				panel.AddChild(hbox);

				// 키 바인딩 레이블
				var keyLabel = new Label();
				keyLabel.Text = SlotKeys[i];
				keyLabel.CustomMinimumSize = new Vector2(28, 0);
				keyLabel.AddThemeFontSizeOverride("font_size", 12);
				keyLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
				keyLabel.HorizontalAlignment = HorizontalAlignment.Center;
				keyLabel.VerticalAlignment = VerticalAlignment.Center;
				hbox.AddChild(keyLabel);

				if (hasSkill)
				{
					var skill = learned[i];

					// 아이콘
					if (skill.Icon != null)
					{
						var icon = new TextureRect();
						icon.Texture = skill.Icon;
						icon.CustomMinimumSize = new Vector2(IconSize, IconSize);
						icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
						icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
						icon.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
						hbox.AddChild(icon);
					}

					var vbox = new VBoxContainer();
					vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
					vbox.AddThemeConstantOverride("separation", 1);

					var nameLabel = new Label();
					nameLabel.Text = skill.SkillName;
					nameLabel.AddThemeFontSizeOverride("font_size", 12);
					nameLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 0.75f));
					nameLabel.ClipText = true;
					vbox.AddChild(nameLabel);

					var infoLabel = new Label();
					infoLabel.Text = $"{skill.Description}\nMP {skill.MpCost} / 쿨 {skill.Cooldown:0.#}초";
					infoLabel.AddThemeFontSizeOverride("font_size", 9);
					infoLabel.AddThemeColorOverride("font_color", new Color(0.72f, 0.72f, 0.78f));
					infoLabel.ClipText = true;
					vbox.AddChild(infoLabel);

					hbox.AddChild(vbox);
				}
				else
				{
					var emptyLabel = new Label();
					emptyLabel.Text = "(비어있음)";
					emptyLabel.AddThemeColorOverride("font_color", new Color(0.48f, 0.48f, 0.52f));
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
				hint.AddThemeFontSizeOverride("font_size", 10);
				hint.HorizontalAlignment = HorizontalAlignment.Center;
				_slotContainer.AddChild(hint);
			}
		}

		private static StyleBoxFlat CreateSlotStyle(bool hasSkill)
		{
			var style = new StyleBoxFlat();
			style.BgColor = hasSkill
				? new Color(0.13f, 0.13f, 0.15f, 0.92f)
				: new Color(0.055f, 0.055f, 0.07f, 0.72f);
			style.BorderColor = hasSkill
				? new Color(0.52f, 0.45f, 0.3f, 0.95f)
				: new Color(0.28f, 0.26f, 0.22f, 0.95f);
			style.SetBorderWidthAll(1);
			style.SetCornerRadiusAll(3);
			style.ContentMarginLeft = 5;
			style.ContentMarginTop = 4;
			style.ContentMarginRight = 5;
			style.ContentMarginBottom = 4;
			return style;
		}
	}
}
