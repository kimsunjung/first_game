using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
    public partial class InventoryUI : BaseUIWindow
    {
        private enum FilterMode { All, Equipment, Consumable, Material }

        private GridContainer _grid;
        private Label _itemInfoLabel;
        private Button _useButton;
        private VBoxContainer _quickSlotPanel;
        private Button[] _quickSlotButtons;

        private Inventory _inventory;
        private IPlayer _player;
        private int _selectedSlot = -1;
        private int _hoveredSlotIndex = -1;
        private FilterMode _filterMode = FilterMode.All;
        private Button[] _filterButtons;

        private const int SlotSize = 40;
        private const int IconSize = 28;

        private static readonly string[] FilterIconPaths =
        {
            "res://Resources/Generated/GPT/Icons/UI/all_items_filter.png",
            "res://Resources/Generated/GPT/Icons/UI/equipment_filter.png",
            "res://Resources/Generated/GPT/Icons/UI/consumable_filter.png",
            "res://Resources/Generated/GPT/Icons/UI/material_filter.png",
        };

        protected override void OnReadyInternal()
        {
            _grid = GetNode<GridContainer>("%ItemGrid");
            _itemInfoLabel = GetNode<Label>("%ItemInfoLabel");
            _useButton = GetNode<Button>("%UseButton");

            _useButton.Pressed += OnUsePressed;

            CreateFilterButtons();
            CreateQuickSlotRow();

            // Player 연결: _Ready 시점에 GameManager.Player가 아직 null일 수 있어 지연 바인딩도 지원
            TryBindPlayer();
        }

        private void CreateQuickSlotRow()
        {
            // _useButton 다음에 "퀵슬롯 등록: [1][2][3][4]" 패널을 동적 생성.
            // consumable 선택 시에만 보이고, 다른 아이템에서는 숨김.
            var useParent = _useButton.GetParent();
            if (useParent == null) return;

            _quickSlotPanel = new VBoxContainer();
            _quickSlotPanel.AddThemeConstantOverride("separation", 2);
            _quickSlotPanel.Visible = false;

            var prompt = new Label
            {
                Text = "퀵슬롯 등록",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            prompt.AddThemeFontSizeOverride("font_size", 11);
            _quickSlotPanel.AddChild(prompt);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 4);
            row.Alignment = BoxContainer.AlignmentMode.Center;

            _quickSlotButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = (i + 1).ToString(),
                    CustomMinimumSize = new Vector2(36, 36)
                };
                btn.AddThemeFontSizeOverride("font_size", 14);
                btn.Pressed += () => OnQuickSlotPressed(idx);
                row.AddChild(btn);
                _quickSlotButtons[i] = btn;
            }
            _quickSlotPanel.AddChild(row);

            useParent.AddChild(_quickSlotPanel);
            useParent.MoveChild(_quickSlotPanel, _useButton.GetIndex() + 1);
        }

        private void OnQuickSlotPressed(int slotIdx)
        {
            if (_selectedSlot < 0 || _selectedSlot >= _inventory.Slots.Count) return;
            var item = _inventory.Slots[_selectedSlot].Item;
            if (item == null || item.Type != ItemType.Consumable) return;

            _inventory.AssignQuickSlot(slotIdx, item);
            // 등록 후 패널은 유지 — 다른 슬롯으로 즉시 변경 가능
        }

        private void TryBindPlayer()
        {
            if (_player != null) return; // 이미 바인딩됨
            var player = GameManager.Instance?.Player;
            if (player == null || player.Inventory == null) return;

            _player = player;
            _inventory = player.Inventory;
            _inventory.OnInventoryChanged += RefreshGrid;
            RefreshGrid();
        }

        protected override bool CanOpen()
        {
            if (_player == null) TryBindPlayer();
            // 플레이어/인벤토리 미바인딩 시 열기 차단 (RefreshGrid에서 NPE 방지)
            if (_player == null || _inventory == null) return false;
            return !_player.IsDead;
        }
        protected override void OnOpened() => RefreshGrid();

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("inventory"))
                Toggle();
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
            // 타이틀 HBox 찾기 (VBox의 첫 번째 자식).
            // _grid는 ScrollContainer 안으로 옮겨졌으므로 GetParent()가 더 이상 VBox가 아님.
            // VBox는 grid → scroll → vbox 경로로 두 단계 위.
            var gridParent = _grid.GetParent();
            var vbox = gridParent is ScrollContainer ? gridParent.GetParent() : gridParent;
            var titleHBox = vbox?.GetChild(0) as HBoxContainer;
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
                FilterMode.Equipment => item.Type.IsEquipment(),
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
                slotPanel.MouseFilter = Control.MouseFilterEnum.Pass;

                var vbox = new VBoxContainer();
                vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                vbox.Alignment = BoxContainer.AlignmentMode.Center;
                vbox.MouseFilter = Control.MouseFilterEnum.Pass;
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
                        icon.MouseFilter = Control.MouseFilterEnum.Ignore;
                        vbox.AddChild(icon);
                    }
                    else
                    {
                        var nameLabel = new Label();
                        nameLabel.Text = slot.Item.ItemName;
                        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        nameLabel.AddThemeFontSizeOverride("font_size", 10);
                        nameLabel.AddThemeColorOverride("font_color", GetRarityColor(slot.Item.Rarity));
                        nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
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
                        qtyLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
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
                        enhLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
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
            else if (Inventory.IsExtraEquipType(item.Type))
            {
                // 신규 부위별 장비 — affix 있는 인스턴스는 base + affix = total 형식으로 통합 표시.
                // 예: base 공격력 +3, affix +1 → "공격력: +4 (+1)"
                int affixDmg = 0, affixDef = 0, affixHp = 0, affixMp = 0;
                float affixCrit = 0f, affixSpd = 0f;
                if (slot.Affixes != null)
                {
                    foreach (var a in slot.Affixes)
                    {
                        switch (a.Type)
                        {
                            case ItemAffixType.BonusDamage:    affixDmg  += (int)a.Value; break;
                            case ItemAffixType.BonusDefense:   affixDef  += (int)a.Value; break;
                            case ItemAffixType.BonusMaxHealth: affixHp   += (int)a.Value; break;
                            case ItemAffixType.BonusMaxMp:     affixMp   += (int)a.Value; break;
                            case ItemAffixType.BonusCritRate:  affixCrit += a.Value; break;
                            case ItemAffixType.BonusMoveSpeed: affixSpd  += a.Value; break;
                        }
                    }
                }

                int totalDmg = item.BonusDamage + affixDmg;
                int totalHp  = item.BonusMaxHealth + affixHp;
                int totalDef = item.BonusDefense + affixDef;
                int totalMp  = item.BonusMaxMp + affixMp;
                float totalCrit = item.BonusCritRate + affixCrit;
                float totalSpd  = item.BonusMoveSpeed + affixSpd;

                if (totalDmg > 0) info += FormatStat("공격력", totalDmg, affixDmg);
                if (totalHp  > 0) info += FormatStat("HP",   totalHp,  affixHp);
                if (totalDef > 0) info += FormatStat("방어력", totalDef, affixDef);
                if (totalMp  > 0) info += FormatStat("MP",   totalMp,  affixMp);
                if (totalCrit > 0f) info += FormatStatPercent("치명타", totalCrit, affixCrit);
                if (totalSpd  > 0f) info += FormatStatFloat("이동속도", totalSpd, affixSpd);
            }

            _itemInfoLabel.Text = info;
            _useButton.Visible = true;
            _useButton.Text = item.Type == ItemType.Consumable ? "사용"
                : item.Type == ItemType.Material ? "사용 불가" : "장착";
            if (_quickSlotPanel != null)
                _quickSlotPanel.Visible = item.Type == ItemType.Consumable;
            RefreshGrid();
        }

        // affix 합산 표시 헬퍼 — affix 분이 0이면 괄호 생략, 0이 아니면 "(+N)" 동봉.
        private static string FormatStat(string label, int total, int affixPart)
            => affixPart > 0
                ? $"\n{label}: +{total} (+{affixPart})"
                : $"\n{label}: +{total}";

        private static string FormatStatPercent(string label, float total, float affixPart)
            => affixPart > 0f
                ? $"\n{label}: +{total * 100f:0.#}% (+{affixPart * 100f:0.#}%)"
                : $"\n{label}: +{total * 100f:0.#}%";

        private static string FormatStatFloat(string label, float total, float affixPart)
            => affixPart > 0f
                ? $"\n{label}: +{total:0.00} (+{affixPart:0.00})"
                : $"\n{label}: +{total:0.##}";

        private void ClearSelection()
        {
            _selectedSlot = -1;
            _itemInfoLabel.Text = "아이템을 선택하세요";
            _useButton.Visible = false;
            if (_quickSlotPanel != null) _quickSlotPanel.Visible = false;
        }

        private void OnUsePressed()
        {
            if (_selectedSlot < 0) return;
            var slot = _inventory.Slots[_selectedSlot];
            // 반지는 슬롯이 두 개라 둘 다 차 있으면 어디에 교체할지 사용자가 직접 선택.
            // 비어 있는 슬롯이 있으면 기본 EquipItem이 자동으로 그 자리로 채워준다.
            if (slot.Item.Type == ItemType.Ring
                && _inventory.EquippedRing1 != null
                && _inventory.EquippedRing2 != null)
            {
                ShowRingSlotPicker(_selectedSlot);
                return;
            }
            _inventory.UseItem(_selectedSlot, _player.Stats);
        }

        private void ShowRingSlotPicker(int invSlotIndex)
        {
            // AcceptDialog + 명시적 두 버튼 + 취소 버튼. Esc/뒤로가기로 닫혀도 사이드 이펙트 없음.
            // ConfirmationDialog의 Canceled 신호는 Esc/창닫기로도 발생해 의도치 않은 교체를
            // 일으키므로 버튼별 CustomAction을 사용한다.
            var dialog = new AcceptDialog
            {
                Title = "반지 교체",
                DialogText = "어느 반지 슬롯과 교체하시겠습니까?\n\n"
                             + $"반지 1: {_inventory.EquippedRing1?.ItemName ?? "(비어있음)"}\n"
                             + $"반지 2: {_inventory.EquippedRing2?.ItemName ?? "(비어있음)"}",
                ProcessMode = ProcessModeEnum.Always,
                Exclusive = true
            };
            // 기본 OK 버튼 숨김
            dialog.GetOkButton().Visible = false;
            dialog.AddButton("반지 1", true, "ring1");
            dialog.AddButton("반지 2", true, "ring2");
            dialog.AddCancelButton("취소");

            dialog.CustomAction += action =>
            {
                if (action == "ring1") DoRingSwap(invSlotIndex, Inventory.ExtraSlot.Ring1);
                else if (action == "ring2") DoRingSwap(invSlotIndex, Inventory.ExtraSlot.Ring2);
                dialog.Hide();
            };
            dialog.VisibilityChanged += () =>
            {
                if (!dialog.Visible) dialog.QueueFree();
            };
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
        }

        private void DoRingSwap(int invSlotIndex, Inventory.ExtraSlot target)
        {
            if (invSlotIndex < 0 || invSlotIndex >= _inventory.Slots.Count) return;
            _inventory.EquipItemToSlot(invSlotIndex, target, _player.Stats);
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

        protected override void OnExitTreeInternal()
        {
            if (_inventory != null)
                _inventory.OnInventoryChanged -= RefreshGrid;

            if (_useButton != null) _useButton.Pressed -= OnUsePressed;
        }
    }
}
