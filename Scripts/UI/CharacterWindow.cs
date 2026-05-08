using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	public partial class CharacterWindow : BaseUIWindow
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
		private Control _equipmentArea;

		private IPlayer _player;

		private enum EquipKey { Weapon, Armor, Helmet, Boots, Necklace, Ring1, Ring2, Bracelet }
		private const int SlotSize = 44;
		// 사람 모양 슬롯 배치 (180x250 EquipmentArea 기준)
		private static readonly (EquipKey key, Vector2 pos, string placeholder)[] _slotLayout =
		{
			(EquipKey.Helmet,   new Vector2( 68,   4), "모자"),
			(EquipKey.Necklace, new Vector2( 20,  52), "목"),
			(EquipKey.Ring1,    new Vector2(116,  52), "반지"),
			(EquipKey.Weapon,   new Vector2( 20, 100), "무기"),
			(EquipKey.Armor,    new Vector2( 68, 100), "방어구"),
			(EquipKey.Bracelet, new Vector2(116, 100), "팔찌"),
			(EquipKey.Ring2,    new Vector2(116, 148), "반지"),
			(EquipKey.Boots,    new Vector2( 68, 196), "신발"),
		};
		private readonly Dictionary<EquipKey, Panel> _slotPanels = new();

		protected override void OnReadyInternal()
		{
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
			_equipmentArea = GetNodeOrNull<Control>("%EquipmentArea");

			if (_strBtn != null) _strBtn.Pressed += () => AllocateStat("STR");
			if (_conBtn != null) _conBtn.Pressed += () => AllocateStat("CON");
			if (_intBtn != null) _intBtn.Pressed += () => AllocateStat("INT");

			BuildEquipmentSlots();

			var pc = GameManager.Instance?.Player;
			if (pc != null)
			{
				_player = pc;
				_player.Stats.OnStatPointsChanged += OnStatPointsChanged;
				if (_player.Inventory != null)
					_player.Inventory.OnEquipmentChanged += RefreshEquipmentSlots;
			}
		}

		private void BuildEquipmentSlots()
		{
			if (_equipmentArea == null) return;
			foreach (var (key, pos, placeholder) in _slotLayout)
			{
				var panel = new Panel();
				panel.Position = pos;
				panel.CustomMinimumSize = new Vector2(SlotSize, SlotSize);
				panel.Size = new Vector2(SlotSize, SlotSize);
				panel.AddThemeStyleboxOverride("panel", CreateSlotStyle(false));
				panel.MouseFilter = Control.MouseFilterEnum.Stop;

				var label = new Label
				{
					Text = placeholder,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Name = "Placeholder"
				};
				label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
				label.AddThemeFontSizeOverride("font_size", 10);
				label.AddThemeColorOverride("font_color", new Color(0.55f, 0.55f, 0.6f));
				panel.AddChild(label);

				EquipKey capturedKey = key;
				panel.GuiInput += ev =>
				{
					if (ev is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
						OnSlotClicked(capturedKey);
				};

				_equipmentArea.AddChild(panel);
				_slotPanels[key] = panel;
			}
		}

		private void RefreshEquipmentSlots()
		{
			if (_player?.Inventory == null) return;
			var inv = _player.Inventory;
			SetSlotItem(EquipKey.Weapon, inv.EquippedWeapon);
			SetSlotItem(EquipKey.Armor, inv.EquippedArmor);
			SetSlotItem(EquipKey.Helmet, inv.EquippedHelmet);
			SetSlotItem(EquipKey.Boots, inv.EquippedBoots);
			SetSlotItem(EquipKey.Necklace, inv.EquippedNecklace);
			SetSlotItem(EquipKey.Ring1, inv.EquippedRing1);
			SetSlotItem(EquipKey.Ring2, inv.EquippedRing2);
			SetSlotItem(EquipKey.Bracelet, inv.EquippedBracelet);
		}

		private void SetSlotItem(EquipKey key, ItemData item)
		{
			if (!_slotPanels.TryGetValue(key, out var panel)) return;

			// 기존 icon 제거 (Placeholder 라벨은 유지)
			foreach (Node c in panel.GetChildren())
				if (c.Name != "Placeholder") c.QueueFree();

			var placeholder = panel.GetNodeOrNull<Label>("Placeholder");
			if (placeholder != null) placeholder.Visible = item == null;
			panel.AddThemeStyleboxOverride("panel", CreateSlotStyle(item != null));

			if (item != null)
			{
				panel.TooltipText = item.ItemName;
				if (item.Icon != null)
				{
					var icon = new TextureRect
					{
						Texture = item.Icon,
						ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
						StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
						MouseFilter = Control.MouseFilterEnum.Ignore
					};
					icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
					panel.AddChild(icon);
				}
				else
				{
					var nameLabel = new Label
					{
						Text = item.ItemName,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						MouseFilter = Control.MouseFilterEnum.Ignore
					};
					nameLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
					nameLabel.AddThemeFontSizeOverride("font_size", 9);
					nameLabel.ClipText = true;
					panel.AddChild(nameLabel);
				}
			}
			else
			{
				panel.TooltipText = "";
			}
		}

		private void OnSlotClicked(EquipKey key)
		{
			if (_player?.Inventory == null) return;
			var inv = _player.Inventory;
			switch (key)
			{
				case EquipKey.Weapon: inv.UnequipWeapon(_player.Stats); break;
				case EquipKey.Armor: inv.UnequipArmor(_player.Stats); break;
				case EquipKey.Helmet: inv.UnequipExtra(Inventory.ExtraSlot.Helmet, _player.Stats); break;
				case EquipKey.Boots: inv.UnequipExtra(Inventory.ExtraSlot.Boots, _player.Stats); break;
				case EquipKey.Necklace: inv.UnequipExtra(Inventory.ExtraSlot.Necklace, _player.Stats); break;
				case EquipKey.Ring1: inv.UnequipExtra(Inventory.ExtraSlot.Ring1, _player.Stats); break;
				case EquipKey.Ring2: inv.UnequipExtra(Inventory.ExtraSlot.Ring2, _player.Stats); break;
				case EquipKey.Bracelet: inv.UnequipExtra(Inventory.ExtraSlot.Bracelet, _player.Stats); break;
			}
		}

		private static StyleBoxFlat CreateSlotStyle(bool occupied)
		{
			var style = new StyleBoxFlat();
			style.BgColor = occupied
				? new Color(0.13f, 0.13f, 0.15f, 0.92f)
				: new Color(0.05f, 0.05f, 0.07f, 0.7f);
			style.BorderColor = occupied
				? new Color(0.55f, 0.45f, 0.25f, 0.95f)
				: new Color(0.25f, 0.25f, 0.28f, 0.85f);
			style.SetBorderWidthAll(1);
			style.SetCornerRadiusAll(3);
			return style;
		}

		protected override void OnExitTreeInternal()
		{
			if (_player is GodotObject playerObj && IsInstanceValid(playerObj))
			{
				_player.Stats.OnStatPointsChanged -= OnStatPointsChanged;
				if (_player.Inventory != null)
					_player.Inventory.OnEquipmentChanged -= RefreshEquipmentSlots;
			}
		}

		protected override void OnOpened()
		{
			RefreshDisplay();
			RefreshEquipmentSlots();
		}

		private void OnStatPointsChanged(int _)
		{
			RefreshDisplay();
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			base._UnhandledInput(@event); // ui_cancel(Esc/뒤로가기) 처리
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
