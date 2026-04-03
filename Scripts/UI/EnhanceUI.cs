using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.UI
{
	public partial class EnhanceUI : CanvasLayer
	{
		private static int MaxEnhancement => BalanceData.Enhancement.MaxLevel;
		private static float[] SuccessRates => BalanceData.Enhancement.SuccessRates;

		// 노드 참조 (에디터에서 % 유니크 이름으로 연결)
		private Label _titleLabel;
		private GridContainer _equipGrid;
		private VBoxContainer _detailPanel;
		private Label _itemNameLabel;
		private Label _currentStatsLabel;
		private Label _nextStatsLabel;
		private Label _successRateLabel;
		private Label _costLabel;
		private Button _enhanceButton;
		private Label _messageLabel;
		private Button _closeButton;

		private IPlayer _player;
		private Inventory _inventory;
		private readonly Random _rng = new();

		// 선택된 강화 대상
		private enum TargetKind { Inventory, EquippedWeapon, EquippedArmor, EquippedAccessory }
		private TargetKind _targetKind;
		private int _targetSlotIndex;
		private bool _hasSelection = false;

		// 강화 가능 엔트리 목록
		private readonly List<EnhanceEntry> _entries = new();

		private struct EnhanceEntry
		{
			public TargetKind Kind;
			public int SlotIndex;  // Inventory일 때만 유효
			public ItemData Item;
			public int Level;
		}

		public override void _Ready()
		{
			_titleLabel = GetNode<Label>("%EnhanceTitleLabel");
			_equipGrid = GetNode<GridContainer>("%EquipGrid");
			_detailPanel = GetNode<VBoxContainer>("%DetailPanel");
			_itemNameLabel = GetNode<Label>("%ItemNameLabel");
			_currentStatsLabel = GetNode<Label>("%CurrentStatsLabel");
			_nextStatsLabel = GetNode<Label>("%NextStatsLabel");
			_successRateLabel = GetNode<Label>("%SuccessRateLabel");
			_costLabel = GetNode<Label>("%CostLabel");
			_enhanceButton = GetNode<Button>("%EnhanceButton");
			_messageLabel = GetNode<Label>("%EnhanceMessageLabel");
			_closeButton = GetNode<Button>("%EnhanceCloseButton");

			_closeButton.Pressed += CloseEnhance;
			_enhanceButton.Pressed += OnEnhancePressed;

			_messageLabel.Visible = false;
			_detailPanel.Visible = false;
			Visible = false;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!Visible) return;
			if (@event.IsActionPressed("ui_cancel") && !@event.IsEcho())
			{
				CloseEnhance();
				GetViewport().SetInputAsHandled();
			}
		}

		public void OpenEnhance(string smithName)
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;

			_player = player;
			_inventory = player.Inventory;
			_hasSelection = false;

			_titleLabel.Text = smithName;
			_detailPanel.Visible = false;
			_messageLabel.Visible = false;

			Visible = true;
			UIPauseManager.RequestPause();

			RefreshEquipList();
		}

		public void CloseEnhance()
		{
			Visible = false;
			UIPauseManager.ReleasePause();
		}

		private void RefreshEquipList()
		{
			foreach (Node child in _equipGrid.GetChildren())
				child.QueueFree();

			_entries.Clear();

			// 1) 장착 중인 장비
			if (_inventory.EquippedWeapon != null)
				_entries.Add(new EnhanceEntry
				{
					Kind = TargetKind.EquippedWeapon,
					Item = _inventory.EquippedWeapon,
					Level = _inventory.EquippedWeaponEnhancement
				});
			if (_inventory.EquippedArmor != null)
				_entries.Add(new EnhanceEntry
				{
					Kind = TargetKind.EquippedArmor,
					Item = _inventory.EquippedArmor,
					Level = _inventory.EquippedArmorEnhancement
				});
			if (_inventory.EquippedAccessory != null)
				_entries.Add(new EnhanceEntry
				{
					Kind = TargetKind.EquippedAccessory,
					Item = _inventory.EquippedAccessory,
					Level = _inventory.EquippedAccessoryEnhancement
				});

			// 2) 인벤토리 내 장비 아이템
			for (int i = 0; i < _inventory.Slots.Count; i++)
			{
				var slot = _inventory.Slots[i];
				if (slot.Item.Type == ItemType.Weapon || slot.Item.Type == ItemType.Armor || slot.Item.Type == ItemType.Accessory)
				{
					_entries.Add(new EnhanceEntry
					{
						Kind = TargetKind.Inventory,
						SlotIndex = i,
						Item = slot.Item,
						Level = slot.EnhancementLevel
					});
				}
			}

			foreach (var entry in _entries)
			{
				var panel = CreateEntryPanel(entry);
				_equipGrid.AddChild(panel);
			}
		}

		private PanelContainer CreateEntryPanel(EnhanceEntry entry)
		{
			var panel = new PanelContainer();
			panel.CustomMinimumSize = new Vector2(260, 36);

			var hbox = new HBoxContainer();
			panel.AddChild(hbox);

			// 아이콘
			if (entry.Item.Icon != null)
			{
				var icon = new TextureRect();
				icon.Texture = entry.Item.Icon;
				icon.CustomMinimumSize = new Vector2(24, 24);
				icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
				icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				hbox.AddChild(icon);
			}

			var vbox = new VBoxContainer();
			vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			var nameLabel = new Label();
			nameLabel.Text = Inventory.GetEnhancedName(entry.Item, entry.Level);
			vbox.AddChild(nameLabel);

			var tagLabel = new Label();
			tagLabel.AddThemeFontSizeOverride("font_size", 10);
			bool isEquipped = entry.Kind != TargetKind.Inventory;
			bool isMax = entry.Level >= MaxEnhancement;
			tagLabel.Text = isMax ? "최대 강화" : (isEquipped ? "[장착중]" : "[보관중]");
			tagLabel.Modulate = isMax ? new Color(1f, 0.85f, 0.2f) : (isEquipped ? new Color(0.4f, 0.8f, 1f) : Colors.White);
			vbox.AddChild(tagLabel);

			hbox.AddChild(vbox);

			var selectBtn = new Button();
			selectBtn.Text = "선택";
			selectBtn.Disabled = isMax;
			var capturedEntry = entry;
			selectBtn.Pressed += () => SelectEntry(capturedEntry);
			hbox.AddChild(selectBtn);

			return panel;
		}

		private void SelectEntry(EnhanceEntry entry)
		{
			_targetKind = entry.Kind;
			_targetSlotIndex = entry.SlotIndex;
			_hasSelection = true;
			_detailPanel.Visible = true;

			int currentLevel = entry.Level;
			int cost = GetCost(currentLevel);
			float rate = GetSuccessRate(currentLevel);

			// 아이템 이름
			_itemNameLabel.Text = Inventory.GetEnhancedName(entry.Item, currentLevel);

			// 현재 스탯
			var (curDmg, _, curDef) = Inventory.GetEnhancementBonuses(entry.Item, currentLevel);
			_currentStatsLabel.Text = FormatBonuses(entry.Item, curDmg, curDef);

			// 다음 스탯
			var (nextDmg, _, nextDef) = Inventory.GetEnhancementBonuses(entry.Item, currentLevel + 1);
			_nextStatsLabel.Text = $"→ +{currentLevel + 1}: {FormatBonuses(entry.Item, nextDmg, nextDef)}";

			// 성공률
			_successRateLabel.Text = $"성공률: {rate * 100:0}%";
			_successRateLabel.Modulate = rate >= 1f ? new Color(0.3f, 1f, 0.3f)
				: rate >= 0.7f ? new Color(1f, 1f, 0.3f)
				: rate >= 0.5f ? new Color(1f, 0.6f, 0.2f)
				: new Color(1f, 0.3f, 0.3f);

			// 실패 패널티
			string penalty = currentLevel < BalanceData.Enhancement.NoPenaltyMaxLevel + 1 ? "실패 시: 변화 없음"
				: currentLevel < BalanceData.Enhancement.DestroyMinLevel ? "실패 시: -1 단계 하락"
				: "실패 시: 아이템 파괴!";
			_successRateLabel.Text += $"  ({penalty})";

			// 비용
			_costLabel.Text = $"비용: {cost}G (보유: {GameManager.Instance.PlayerGold}G)";
			bool canAfford = GameManager.Instance.PlayerGold >= cost;
			_costLabel.Modulate = canAfford ? Colors.White : new Color(1f, 0.4f, 0.4f);

			_enhanceButton.Disabled = !canAfford;
			_enhanceButton.Text = canAfford ? "강화하기" : "골드 부족";
		}

		private void OnEnhancePressed()
		{
			if (!_hasSelection) return;

			// 현재 대상 찾기
			ItemData item;
			int currentLevel;

			switch (_targetKind)
			{
				case TargetKind.EquippedWeapon:
					item = _inventory.EquippedWeapon;
					currentLevel = _inventory.EquippedWeaponEnhancement;
					break;
				case TargetKind.EquippedArmor:
					item = _inventory.EquippedArmor;
					currentLevel = _inventory.EquippedArmorEnhancement;
					break;
				case TargetKind.EquippedAccessory:
					item = _inventory.EquippedAccessory;
					currentLevel = _inventory.EquippedAccessoryEnhancement;
					break;
				default: // Inventory
					if (_targetSlotIndex < 0 || _targetSlotIndex >= _inventory.Slots.Count)
					{
						ShowMessage("대상 아이템을 찾을 수 없습니다.");
						return;
					}
					item = _inventory.Slots[_targetSlotIndex].Item;
					currentLevel = _inventory.Slots[_targetSlotIndex].EnhancementLevel;
					break;
			}

			if (item == null)
			{
				ShowMessage("대상 아이템을 찾을 수 없습니다.");
				RefreshEquipList();
				_detailPanel.Visible = false;
				_hasSelection = false;
				return;
			}

			if (currentLevel >= MaxEnhancement)
			{
				ShowMessage("이미 최대 강화 단계입니다.");
				return;
			}

			int cost = GetCost(currentLevel);
			if (GameManager.Instance.PlayerGold < cost)
			{
				ShowMessage("골드가 부족합니다!");
				AudioManager.Instance?.PlaySFX("shop_fail.wav");
				return;
			}

			// 골드 차감
			GameManager.Instance.PlayerGold -= cost;

			// 확률 판정
			float rate = GetSuccessRate(currentLevel);
			bool success = _rng.NextDouble() < rate;

			if (success)
			{
				// 성공
				int newLevel = currentLevel + 1;
				ApplyEnhancement(newLevel);
				AudioManager.Instance?.PlaySFX("craft_success.wav");
				ShowMessage($"강화 성공! {Inventory.GetEnhancedName(item, newLevel)}");
			}
			else
			{
				// 실패
				if (currentLevel < BalanceData.Enhancement.NoPenaltyMaxLevel + 1)
				{
					// 낮은 단계 실패: 변화 없음
					AudioManager.Instance?.PlaySFX("shop_fail.wav");
					ShowMessage("강화에 실패했습니다. (변화 없음)");
				}
				else if (currentLevel < BalanceData.Enhancement.DestroyMinLevel)
				{
					// 중간 단계 실패: -1 하락
					int newLevel = currentLevel - 1;
					ApplyEnhancement(newLevel);
					AudioManager.Instance?.PlaySFX("shop_fail.wav");
					ShowMessage($"강화 실패! {Inventory.GetEnhancedName(item, newLevel)}로 하락!");
				}
				else
				{
					// 고단계 실패: 아이템 파괴
					DestroyTarget();
					AudioManager.Instance?.PlaySFX("shop_fail.wav");
					ShowMessage($"{item.ItemName}이(가) 파괴되었습니다!");
					_detailPanel.Visible = false;
					_hasSelection = false;
				}
			}

			RefreshEquipList();

			// 파괴되지 않았으면 상세 패널 갱신
			if (_hasSelection)
			{
				var refreshedEntry = FindCurrentEntry();
				if (refreshedEntry.HasValue)
					SelectEntry(refreshedEntry.Value);
				else
				{
					_detailPanel.Visible = false;
					_hasSelection = false;
				}
			}
		}

		private void ApplyEnhancement(int newLevel)
		{
			var target = _player.Stats as IEquipTarget;
			if (target == null) return;

			switch (_targetKind)
			{
				case TargetKind.EquippedWeapon:
				case TargetKind.EquippedArmor:
				case TargetKind.EquippedAccessory:
					var equipType = _targetKind switch
					{
						TargetKind.EquippedWeapon => ItemType.Weapon,
						TargetKind.EquippedArmor => ItemType.Armor,
						_ => ItemType.Accessory
					};
					_inventory.SetEquippedEnhancement(equipType, newLevel, target);
					break;
				default:
					if (_targetSlotIndex >= 0 && _targetSlotIndex < _inventory.Slots.Count)
					{
						_inventory.Slots[_targetSlotIndex].EnhancementLevel = newLevel;
						_inventory.NotifyChanged();
					}
					break;
			}
		}

		private void DestroyTarget()
		{
			var target = _player.Stats as IEquipTarget;
			if (target == null) return;

			switch (_targetKind)
			{
				case TargetKind.EquippedWeapon:
					_inventory.DestroyEquippedItem(ItemType.Weapon, target);
					break;
				case TargetKind.EquippedArmor:
					_inventory.DestroyEquippedItem(ItemType.Armor, target);
					break;
				case TargetKind.EquippedAccessory:
					_inventory.DestroyEquippedItem(ItemType.Accessory, target);
					break;
				default:
					if (_targetSlotIndex >= 0 && _targetSlotIndex < _inventory.Slots.Count)
					{
						_inventory.Slots.RemoveAt(_targetSlotIndex);
						_inventory.NotifyChanged();
					}
					break;
			}
		}

		private EnhanceEntry? FindCurrentEntry()
		{
			switch (_targetKind)
			{
				case TargetKind.EquippedWeapon when _inventory.EquippedWeapon != null:
					return new EnhanceEntry { Kind = TargetKind.EquippedWeapon, Item = _inventory.EquippedWeapon, Level = _inventory.EquippedWeaponEnhancement };
				case TargetKind.EquippedArmor when _inventory.EquippedArmor != null:
					return new EnhanceEntry { Kind = TargetKind.EquippedArmor, Item = _inventory.EquippedArmor, Level = _inventory.EquippedArmorEnhancement };
				case TargetKind.EquippedAccessory when _inventory.EquippedAccessory != null:
					return new EnhanceEntry { Kind = TargetKind.EquippedAccessory, Item = _inventory.EquippedAccessory, Level = _inventory.EquippedAccessoryEnhancement };
				case TargetKind.Inventory when _targetSlotIndex >= 0 && _targetSlotIndex < _inventory.Slots.Count:
					var slot = _inventory.Slots[_targetSlotIndex];
					return new EnhanceEntry { Kind = TargetKind.Inventory, SlotIndex = _targetSlotIndex, Item = slot.Item, Level = slot.EnhancementLevel };
				default:
					return null;
			}
		}

		private static int GetCost(int currentLevel) => BalanceData.Enhancement.CostBase * (currentLevel + 1);

		private static float GetSuccessRate(int currentLevel)
		{
			if (currentLevel < 0 || currentLevel >= SuccessRates.Length) return 0f;
			return SuccessRates[currentLevel];
		}

		private static string FormatBonuses(ItemData item, int dmgBonus, int defBonus)
		{
			if (item == null) return "";
			return item.Type switch
			{
				ItemType.Weapon => $"강화 보너스: ATK +{dmgBonus}",
				ItemType.Armor => $"강화 보너스: DEF +{defBonus}",
				ItemType.Accessory => item.BonusDamage > 0 ? $"강화 보너스: ATK +{dmgBonus}" : $"강화 보너스: DEF +{defBonus}",
				_ => ""
			};
		}

		public override void _ExitTree()
		{
			if (Visible)
				UIPauseManager.ReleasePause();

			if (_closeButton != null) _closeButton.Pressed -= CloseEnhance;
			if (_enhanceButton != null) _enhanceButton.Pressed -= OnEnhancePressed;
		}

		private async void ShowMessage(string text)
		{
			_messageLabel.Text = text;
			_messageLabel.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.5), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_messageLabel.Visible = false;
		}
	}
}
