using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 스킬창: Tab 키로 열고 닫기. 상단 탭으로 전사/마법사/궁수 분류. 레벨순 정렬.
	public partial class SkillWindow : BaseUIWindow
	{
		private VBoxContainer _slotContainer;
		private HBoxContainer _tabContainer;
		private IPlayer _player;
		private PlayerClass _currentTab = PlayerClass.Warrior;
		private readonly Dictionary<PlayerClass, Button> _tabButtons = new();

		private static readonly string[] SlotKeys = { "Q", "W", "E", "R", "T", "Y" };
		public const int SlotCount = 6;
		private const int SlotWidth = 256;
		private const int SlotHeight = 34;
		private const int IconSize = 26;

		protected override void OnReadyInternal()
		{
			_slotContainer = GetNode<VBoxContainer>("%SkillSlotContainer");
			BuildTabBar();

			var pc = GameManager.Instance?.Player;
			if (pc != null)
			{
				_player = pc;
				_currentTab = pc.Stats.PlayerClass;
			}
		}

		private void BuildTabBar()
		{
			if (_slotContainer == null) return;
			_tabContainer = new HBoxContainer();
			_tabContainer.AddThemeConstantOverride("separation", 2);
			_tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			AddTab(PlayerClass.Warrior, "전사");
			AddTab(PlayerClass.Mage,    "마법사");
			AddTab(PlayerClass.Archer,  "궁수");

			// VBoxContainer 첫 번째에 삽입
			var parent = _slotContainer.GetParent();
			if (parent is VBoxContainer parentVbox)
			{
				parentVbox.AddChild(_tabContainer);
				parentVbox.MoveChild(_tabContainer, _slotContainer.GetIndex());
			}
		}

		private void AddTab(PlayerClass cls, string label)
		{
			var btn = new Button
			{
				Text = label,
				CustomMinimumSize = new Vector2(70, 26),
				ToggleMode = true
			};
			btn.AddThemeFontSizeOverride("font_size", 11);
			btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			btn.Pressed += () => OnTabPressed(cls);
			_tabContainer.AddChild(btn);
			_tabButtons[cls] = btn;
		}

		private void OnTabPressed(PlayerClass cls)
		{
			_currentTab = cls;
			Refresh();
		}

		protected override void OnOpened()
		{
			// 플레이어 클래스 탭 기본 선택
			if (_player == null) _player = GameManager.Instance?.Player;
			if (_player != null) _currentTab = _player.Stats.PlayerClass;
			Refresh();
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			base._UnhandledInput(@event);
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				var key = k.Keycode != Key.None ? k.Keycode : k.PhysicalKeycode;
				if (key == Key.Tab) Toggle();
			}
		}

		private void Refresh()
		{
			foreach (Node child in _slotContainer.GetChildren()) child.QueueFree();
			UpdateTabStyles();

			if (_player == null) _player = GameManager.Instance?.Player;
			if (_player == null) return;

			bool isOwnTab = _player.Stats.PlayerClass == _currentTab;

			// 능동 스킬 — 플레이어 본인 탭일 때만 GetActiveSkillAt(슬롯) 기준으로 Q/W/E/R/T/Y 표시.
			// 실제 발동/모바일 버튼과 동일한 함수를 써서 라벨이 어긋나지 않게 보장.
			int activeRowCount = 0;
			if (isOwnTab)
			{
				int activeCount = _player.Stats.ActiveSkillCount;
				if (activeCount > 0)
				{
					AddSectionHeader("능동 스킬");
				}
				for (int i = 0; i < activeCount; i++)
				{
					var skill = _player.Stats.GetActiveSkillAt(i);
					if (skill == null) continue;
					BuildSkillRow(skill, i < SlotCount ? SlotKeys[i] : "·");
					activeRowCount++;
				}
			}

			// 패시브 — 별도 섹션. 능동 슬롯 인덱스를 차지하지 않으므로 슬롯 키 없이 표시.
			// 다른 클래스 탭에서는 해당 탭 클래스의 학습 가능 스킬(주로 AvailableToAllClasses) 미리보기로 함께 표시.
			var passiveRows = new List<SkillData>();
			var otherTabRows = new List<SkillData>();
			foreach (var s in _player.Stats.LearnedSkills)
			{
				if (s == null) continue;
				bool tabMatch = s.AvailableToAllClasses || s.RequiredClass == _currentTab;
				if (!tabMatch) continue;
				if (isOwnTab)
				{
					if (s.IsPassive) passiveRows.Add(s);
				}
				else
				{
					otherTabRows.Add(s);
				}
			}
			passiveRows.Sort((a, b) => a.RequiredLevel.CompareTo(b.RequiredLevel));
			otherTabRows.Sort((a, b) => a.RequiredLevel.CompareTo(b.RequiredLevel));

			if (passiveRows.Count > 0)
			{
				AddSectionHeader("패시브");
				foreach (var s in passiveRows) BuildSkillRow(s, "·");
			}

			if (!isOwnTab)
			{
				if (otherTabRows.Count > 0) AddSectionHeader($"{PlayerClassUtil.DisplayName(_currentTab)} 공유 스킬");
				foreach (var s in otherTabRows) BuildSkillRow(s, "·");
			}

			if (activeRowCount == 0 && passiveRows.Count == 0 && otherTabRows.Count == 0)
			{
				var hint = new Label
				{
					Text = $"[{PlayerClassUtil.DisplayName(_currentTab)}] 학습한 스킬이 없습니다.\n스킬 상점에서 스킬북을 구매하세요!",
					HorizontalAlignment = HorizontalAlignment.Center
				};
				hint.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
				hint.AddThemeFontSizeOverride("font_size", 10);
				_slotContainer.AddChild(hint);
			}
		}

		private void AddSectionHeader(string text)
		{
			var label = new Label
			{
				Text = text,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			label.AddThemeFontSizeOverride("font_size", 10);
			label.AddThemeColorOverride("font_color", new Color(0.85f, 0.75f, 0.45f));
			_slotContainer.AddChild(label);
		}

		private void UpdateTabStyles()
		{
			foreach (var kv in _tabButtons)
			{
				kv.Value.ButtonPressed = kv.Key == _currentTab;
				kv.Value.AddThemeColorOverride("font_color",
					kv.Key == _currentTab
						? new Color(1f, 0.85f, 0.3f)
						: new Color(0.65f, 0.65f, 0.65f));
			}
		}

		private void BuildSkillRow(SkillData skill, string keyText)
		{
			var panel = new PanelContainer();
			panel.CustomMinimumSize = new Vector2(SlotWidth, SlotHeight);
			panel.AddThemeStyleboxOverride("panel", CreateSlotStyle(true));

			var hbox = new HBoxContainer();
			hbox.AddThemeConstantOverride("separation", 6);
			hbox.Alignment = BoxContainer.AlignmentMode.Center;
			panel.AddChild(hbox);

			var keyLabel = new Label
			{
				Text = keyText,
				CustomMinimumSize = new Vector2(22, 0),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			keyLabel.AddThemeFontSizeOverride("font_size", 11);
			keyLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
			hbox.AddChild(keyLabel);

			if (skill.Icon != null)
			{
				var icon = new TextureRect
				{
					Texture = skill.Icon,
					CustomMinimumSize = new Vector2(IconSize, IconSize),
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
				};
				hbox.AddChild(icon);
			}

			var vbox = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			vbox.AddThemeConstantOverride("separation", 1);
			var nameLabel = new Label { Text = $"Lv.{skill.RequiredLevel} {skill.SkillName}", ClipText = true };
			nameLabel.AddThemeFontSizeOverride("font_size", 12);
			nameLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 0.75f));
			vbox.AddChild(nameLabel);
			var infoLabel = new Label
			{
				Text = $"{skill.Description} | MP {skill.MpCost} / 쿨 {skill.Cooldown:0.#}초",
				ClipText = true
			};
			infoLabel.AddThemeFontSizeOverride("font_size", 9);
			infoLabel.AddThemeColorOverride("font_color", new Color(0.72f, 0.72f, 0.78f));
			vbox.AddChild(infoLabel);
			hbox.AddChild(vbox);

			_slotContainer.AddChild(panel);
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
