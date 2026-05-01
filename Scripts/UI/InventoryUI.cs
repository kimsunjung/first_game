using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
    public partial class InventoryUI : CanvasLayer
    {
        private enum FilterMode { All, Equipment, Consumable, Material }

        private GridContainer _grid;
        private Label _itemInfoLabel;
        private Button _useButton;
        private Button _unequipWeaponButton;
        private Button _unequipArmorButton;
        private Button _unequipAccessoryButton;
        private Label _weaponLabel;
        private Label _armorLabel;
        private Label _accessoryLabel;

        private Inventory _inventory;
        private IPlayer _player;
        private int _selectedSlot = -1;
        private int _hoveredSlotIndex = -1;
        private FilterMode _filterMode = FilterMode.All;
        private Button[] _filterButtons;

        private const int SlotSize = 48;
        private const int IconSize = 32;

        private static readonly string[] FilterIconPaths =
        {
            "res://Resources/Generated/GPT/Icons/UI/all_items_filter.png",
            "res://Resources/Generated/GPT/Icons/UI/equipment_filter.png",
            "res://Resources/Generated/GPT/Icons/UI/consumable_filter.png",
            "res://Resources/Generated/GPT/Icons/UI/material_filter.png",
        };

        public override void _Ready()
        {
            _grid = GetNode<GridContainer>("%ItemGrid");
            _itemInfoLabel = GetNode<Label>("%ItemInfoLabel");
            _useButton = GetNode<Button>("%UseButton");
            _unequipWeaponButton = GetNode<Button>("%UnequipWeaponButton");
            _unequipArmorButton = GetNode<Button>("%UnequipArmorButton");
            _unequipAccessoryButton = GetNode<Button>("%UnequipAccessoryButton");
            _weaponLabel = GetNode<Label>("%WeaponLabel");
            _armorLabel = GetNode<Label>("%ArmorLabel");
            _accessoryLabel = GetNode<Label>("%AccessoryLabel");

            _useButton.Pressed += OnUsePressed;
            _unequipWeaponButton.Pressed += OnUnequipWeaponPressed;
            _unequipArmorButton.Pressed += OnUnequipArmorPressed;
            _unequipAccessoryButton.Pressed += OnUnequipAccessoryPressed;

            CreateFilterButtons();
            Visible = false;

            // 트리 일시정지 중에도 키보드 토글 작동하도록
            ProcessMode = ProcessModeEnum.Always;

            // Player 연결
            var player = GameManager.Instance?.Player;
            if (player != null)
            {
                _player = player;
                _inventory = player.Inventory;
                _inventory.OnInventoryChanged += RefreshGrid;
                _inventory.OnEquipmentChanged += RefreshEquipment;
                RefreshGrid();
                RefreshEquipment();
            }
        }

        public override void _Process(double delta)
        {
            // I키로 토글 (Toggle with I key)
            if (Input.IsActionJustPressed("inventory"))
            {
                // 플레이어 사망 시 인벤토리 열기 차단
                if (_player != null && _player.IsDead) return;

                // 다른 UI가 일시정지를 걸었으면 인벤토리 열기 차단 (상점, 게임오버 등)
                if (UIPauseManager.IsPaused && !Visible) return;

                GD.Print("InventoryUI: I key pressed (Toggle)");
                Visible = !Visible;
                if (Visible)
                    UIPauseManager.RequestPause();
                else
                    UIPauseManager.ReleasePause();
                if (Visible) 
                {
                    GD.Print("InventoryUI: Opened");
                    RefreshGrid();
                }
                else
                {
                    GD.Print("InventoryUI: Closed");
                }
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            // 인벤토리가 열려있고, 슬롯 위에 마우스가 있을 때 숫자키 입력 처리
            if (Visible && _hoveredSlotIndex != -1 && @event is InputEventKey k && k.Pressed && !k.Echo)
            {
                // Keycode 또는 PhysicalKeycode로 감지 (키보드 레이아웃 호환)
                var key = k.Keycode != Key.None ? k.Keycode : k.PhysicalKeycode;
                if (key == Key.Key1) AssignQuickSlot(0);
                else if (key == Key.Key2) AssignQuickSlot(1);
                else if (key == Key.Key3) AssignQuickSlot(2);
                else if (key == Key.Key4) AssignQuickSlot(3);
            }
        }

        private void AssignQuickSlot(int quickSlotIdx)
        {
             if (_hoveredSlotIndex < 0 || _hoveredSlotIndex >= _inventory.Slots.Count) return;
             var item = _inventory.Slots[_hoveredSlotIndex].Item;
             // 소모품만 등록 가능하도록 제한할 수도 있음 (Optional: restrict to consumables)
             if (item.Type == ItemType.Consumable)
             {
                 _inventory.AssignQuickSlot(quickSlotIdx, item);
             }
             else
             {
                 GD.Print("소모품만 퀵슬롯에 등록할 수 있습니다! (Consumables only!)");
             }
        }

        private void CreateFilterButtons()
        {
            // 타이틀 HBox 찾기 (VBox의 첫 번째 자식)
            var vbox = _grid.GetParent();
            var titleHBox = vbox.GetChild(0) as HBoxContainer;
            if (titleHBox == null) return;

            var filterBox = new HBoxContainer();
            filterBox.AddThemeConstantOverride("separation", 2);

            string[] labels = { "전체", "장비", "소모", "재료" };
            FilterMode[] modes = { FilterMode.All, FilterMode.Equipment, FilterMode.Consumable, FilterMode.Material };
            _filterButtons = new Button[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                var btn = new Button();
                btn.Text = "";
                btn.TooltipText = labels[i];
                btn.Icon = GD.Load<Texture2D>(FilterIconPaths[i]);
                btn.ExpandIcon = true;
                btn.AddThemeFontSizeOverride("font_size", 10);
                btn.CustomMinimumSize = new Vector2(30, 26);
                var mode = modes[i];
                btn.Pressed += () => SetFilter(mode);
                filterBox.AddChild(btn);
                _filterButtons[i] = btn;
            }

            titleHBox.AddChild(filterBox);
            UpdateFilterButtonStyles();
        }

        private void SetFilter(FilterMode mode)
        {
            _filterMode = mode;
            ClearSelection();
            UpdateFilterButtonStyles();
            RefreshGrid();
        }

        private void UpdateFilterButtonStyles()
        {
            if (_filterButtons == null) return;
            FilterMode[] modes = { FilterMode.All, FilterMode.Equipment, FilterMode.Consumable, FilterMode.Material };
            for (int i = 0; i < _filterButtons.Length; i++)
            {
                _filterButtons[i].Modulate = modes[i] == _filterMode
                    ? new Color(1, 1, 0.5f)
                    : new Color(0.7f, 0.7f, 0.7f);
            }
        }

        private bool PassesFilter(ItemData item)
        {
            return _filterMode switch
            {
                FilterMode.Equipment => item.Type is ItemType.Weapon or ItemType.Armor or ItemType.Accessory or ItemType.SkillBook,
                FilterMode.Consumable => item.Type == ItemType.Consumable,
                FilterMode.Material => item.Type == ItemType.Material,
                _ => true
            };
        }

        private void RefreshGrid()
        {
            // 기존 슬롯 UI 제거
            foreach (Node child in _grid.GetChildren())
                child.QueueFree();

            // 필터 적용된 슬롯 목록
            var filtered = new System.Collections.Generic.List<(int origIdx, InventorySlot slot)>();
            bool selectedVisible = false;
            for (int idx = 0; idx < _inventory.Slots.Count; idx++)
            {
                if (PassesFilter(_inventory.Slots[idx].Item))
                {
                    filtered.Add((idx, _inventory.Slots[idx]));
                    if (idx == _selectedSlot)
                        selectedVisible = true;
                }
            }

            if (_selectedSlot >= 0 && !selectedVisible)
                ClearSelection();

            // 슬롯 생성 (20칸)
            for (int i = 0; i < Inventory.MaxSlots; i++)
            {
                var slotPanel = new PanelContainer();
                slotPanel.CustomMinimumSize = new Vector2(SlotSize, SlotSize);
                slotPanel.AddThemeStyleboxOverride("panel", CreateSlotStyle(false, false, Colors.White));

                var vbox = new VBoxContainer();
                vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                vbox.Alignment = BoxContainer.AlignmentMode.Center;
                slotPanel.AddChild(vbox);

                if (i < filtered.Count)
                {
                    var (origIdx, slot) = filtered[i];
                    bool isSelected = origIdx == _selectedSlot;
                    slotPanel.AddThemeStyleboxOverride("panel", CreateSlotStyle(true, isSelected, GetRarityColor(slot.Item.Rarity)));

                    // 아이콘 또는 이름 (Icon or Name)
                    if (slot.Item.Icon != null)
                    {
                        var icon = new TextureRect();
                        icon.Texture = slot.Item.Icon;
                        icon.CustomMinimumSize = new Vector2(IconSize, IconSize);
                        icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                        icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                        icon.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
                        icon.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                        vbox.AddChild(icon);
                    }
                    else
                    {
                        var nameLabel = new Label();
                        nameLabel.Text = slot.Item.ItemName;
                        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        nameLabel.AddThemeFontSizeOverride("font_size", 10);
                        nameLabel.AddThemeColorOverride("font_color", GetRarityColor(slot.Item.Rarity));
                        vbox.AddChild(nameLabel);
                    }

                    // 수량 표시 (스택 가능한 경우) (Display quantity)
                    if (slot.Item.IsStackable && slot.Quantity > 1)
                    {
                        var qtyLabel = new Label();
                        qtyLabel.Text = $"x{slot.Quantity}";
                        qtyLabel.HorizontalAlignment = HorizontalAlignment.Right;
                        qtyLabel.AddThemeFontSizeOverride("font_size", 9);
                        qtyLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 0.7f));
                        vbox.AddChild(qtyLabel);
                    }

                    // 강화 수치 표시
                    if (slot.EnhancementLevel > 0)
                    {
                        var enhLabel = new Label();
                        enhLabel.Text = $"+{slot.EnhancementLevel}";
                        enhLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        enhLabel.AddThemeFontSizeOverride("font_size", 9);
                        enhLabel.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 0.6f));
                        vbox.AddChild(enhLabel);
                    }

                    // 클릭 이벤트 — 원본 인덱스 사용
                    int capturedOrigIdx = origIdx;
                    slotPanel.GuiInput += (inputEvent) =>
                    {
                        if (inputEvent is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                        {
                            SelectSlot(capturedOrigIdx);
                        }
                    };

                    // 마우스 오버 추적 — 원본 인덱스 사용
                    slotPanel.MouseEntered += () => _hoveredSlotIndex = capturedOrigIdx;
                    slotPanel.MouseExited += () =>
                    {
                        if (_hoveredSlotIndex == capturedOrigIdx) _hoveredSlotIndex = -1;
                    };
                }

                _grid.AddChild(slotPanel);
            }
        }

        private void SelectSlot(int index)
        {
            if (index < 0 || index >= _inventory.Slots.Count) return;
            _selectedSlot = index;
            var slot = _inventory.Slots[index];
            var item = slot.Item;

            int enhLevel = slot.EnhancementLevel;
            string info = $"{Inventory.GetEnhancedName(item, enhLevel)}";
            if (item.Rarity != ItemRarity.Common)
                info += $" [{item.Rarity}]";
            info += $"\n{item.Description}";

            // 강화 보너스 포함 장비 비교
            var (slotDmg, _, slotDef) = Inventory.GetEnhancementBonuses(item, enhLevel);
            if (item.Type == ItemType.Weapon && item.BonusDamage > 0)
            {
                int totalDmg = item.BonusDamage + slotDmg;
                var (eqDmg, _, _) = Inventory.GetEnhancementBonuses(_inventory.EquippedWeapon, _inventory.EquippedWeaponEnhancement);
                int currentDmg = (_inventory.EquippedWeapon?.BonusDamage ?? 0) + eqDmg;
                int diff = totalDmg - currentDmg;
                string sign = diff >= 0 ? "+" : "";
                info += $"\n공격력: +{totalDmg} (현재 대비 {sign}{diff})";
            }
            else if (item.Type == ItemType.Armor && item.BonusMaxHealth > 0)
            {
                int currentHp = _inventory.EquippedArmor?.BonusMaxHealth ?? 0;
                int diff = item.BonusMaxHealth - currentHp;
                string sign = diff >= 0 ? "+" : "";
                info += $"\nHP: +{item.BonusMaxHealth} (현재 대비 {sign}{diff})";
                if (slotDef > 0 || _inventory.EquippedArmorEnhancement > 0)
                {
                    var (_, _, eqDef) = Inventory.GetEnhancementBonuses(_inventory.EquippedArmor, _inventory.EquippedArmorEnhancement);
                    int defDiff = slotDef - eqDef;
                    string defSign = defDiff >= 0 ? "+" : "";
                    info += $"\n방어력: +{slotDef} (현재 대비 {defSign}{defDiff})";
                }
            }
            else if (item.Type == ItemType.Accessory)
            {
                var (eqAccDmg, _, eqAccDef) = Inventory.GetEnhancementBonuses(_inventory.EquippedAccessory, _inventory.EquippedAccessoryEnhancement);
                int currentDef = (_inventory.EquippedAccessory?.BonusDefense ?? 0) + eqAccDef;
                int totalDef = item.BonusDefense + slotDef;
                int diff = totalDef - currentDef;
                string sign = diff >= 0 ? "+" : "";
                info += $"\n방어력: +{totalDef} (현재 대비 {sign}{diff})";
            }

            _itemInfoLabel.Text = info;
            _useButton.Visible = true;
            _useButton.Text = item.Type == ItemType.Consumable ? "사용"
                : item.Type == ItemType.Material ? "사용 불가" : "장착";
            RefreshGrid();
        }

        private void ClearSelection()
        {
            _selectedSlot = -1;
            _itemInfoLabel.Text = "아이템을 선택하세요";
            _useButton.Visible = false;
        }

        private void RefreshEquipment()
        {
            _weaponLabel.Text = _inventory.EquippedWeapon != null
                ? $"무기: {Inventory.GetEnhancedName(_inventory.EquippedWeapon, _inventory.EquippedWeaponEnhancement)}"
                : "무기: 없음";
            _armorLabel.Text = _inventory.EquippedArmor != null
                ? $"방어구: {Inventory.GetEnhancedName(_inventory.EquippedArmor, _inventory.EquippedArmorEnhancement)}"
                : "방어구: 없음";
            _accessoryLabel.Text = _inventory.EquippedAccessory != null
                ? $"악세: {Inventory.GetEnhancedName(_inventory.EquippedAccessory, _inventory.EquippedAccessoryEnhancement)}"
                : "악세: 없음";
        }

        private void OnUsePressed()
        {
            if (_selectedSlot >= 0)
                _inventory.UseItem(_selectedSlot, _player.Stats);
        }

        private void OnUnequipWeaponPressed()
        {
            _inventory.UnequipWeapon(_player.Stats);
        }

        private void OnUnequipArmorPressed()
        {
            _inventory.UnequipArmor(_player.Stats);
        }

        private void OnUnequipAccessoryPressed()
        {
            _inventory.UnequipAccessory(_player.Stats);
        }

        private static Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),   // 초록
                ItemRarity.Rare => new Color(0.3f, 0.5f, 1.0f),       // 파랑
                ItemRarity.Epic => new Color(0.7f, 0.3f, 0.9f),       // 보라
                ItemRarity.Legendary => new Color(1.0f, 0.8f, 0.0f),  // 노랑
                _ => new Color(1, 1, 1)                                // 흰색 (Common)
            };
        }

        private static StyleBoxFlat CreateSlotStyle(bool occupied, bool selected, Color rarityColor)
        {
            var style = new StyleBoxFlat();
            style.BgColor = occupied
                ? new Color(0.13f, 0.13f, 0.15f, 0.92f)
                : new Color(0.055f, 0.055f, 0.07f, 0.72f);
            style.BorderColor = selected
                ? new Color(1.0f, 0.82f, 0.28f, 1.0f)
                : (occupied && rarityColor != Colors.White
                    ? rarityColor
                    : new Color(0.28f, 0.26f, 0.22f, 0.95f));
            style.SetBorderWidthAll(selected ? 3 : 1);
            style.SetCornerRadiusAll(3);
            style.ContentMarginLeft = 4;
            style.ContentMarginTop = 4;
            style.ContentMarginRight = 4;
            style.ContentMarginBottom = 4;
            return style;
        }

        /// <summary>모바일 버튼에서 직접 호출</summary>
        public void Toggle()
        {
            if (_player != null && _player.IsDead) return;
            if (UIPauseManager.IsPaused && !Visible) return;
            Visible = !Visible;
            if (Visible) { UIPauseManager.RequestPause(); RefreshGrid(); }
            else UIPauseManager.ReleasePause();
        }

        public override void _ExitTree()
        {
            // 인벤토리가 열린 채로 씬 전환 시 일시정지 카운터 해제
            if (Visible)
                UIPauseManager.ReleasePause();

            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshGrid;
                _inventory.OnEquipmentChanged -= RefreshEquipment;
            }

            if (_useButton != null) _useButton.Pressed -= OnUsePressed;
            if (_unequipWeaponButton != null) _unequipWeaponButton.Pressed -= OnUnequipWeaponPressed;
            if (_unequipArmorButton != null) _unequipArmorButton.Pressed -= OnUnequipArmorPressed;
            if (_unequipAccessoryButton != null) _unequipAccessoryButton.Pressed -= OnUnequipAccessoryPressed;
        }
    }
}
