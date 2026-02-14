using Godot;
using FirstGame.Data;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
    public partial class InventoryUI : CanvasLayer
    {
        private GridContainer _grid;
        private Label _itemInfoLabel;
        private Button _useButton;
        private Button _unequipWeaponButton;
        private Button _unequipArmorButton;
        private Label _weaponLabel;
        private Label _armorLabel;

        private Inventory _inventory;
        private PlayerController _player;
        private int _selectedSlot = -1;
        private int _hoveredSlotIndex = -1; // 마우스 오버된 슬롯 추적 (Track hovered slot)

        public override void _Ready()
        {
            _grid = GetNode<GridContainer>("%ItemGrid");
            _itemInfoLabel = GetNode<Label>("%ItemInfoLabel");
            _useButton = GetNode<Button>("%UseButton");
            _unequipWeaponButton = GetNode<Button>("%UnequipWeaponButton");
            _unequipArmorButton = GetNode<Button>("%UnequipArmorButton");
            _weaponLabel = GetNode<Label>("%WeaponLabel");
            _armorLabel = GetNode<Label>("%ArmorLabel");

            _useButton.Pressed += OnUsePressed;
            _unequipWeaponButton.Pressed += OnUnequipWeaponPressed;
            _unequipArmorButton.Pressed += OnUnequipArmorPressed;

            Visible = false;

            // Player 연결 (Connect Player)
            // HUD와 마찬가지로 플레이어보다 늦게 준비될 수 있음. (Might be ready after player)
            var players = GetTree().GetNodesInGroup("Player");
            if (players.Count > 0 && players[0] is PlayerController player)
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
                if (GetTree().Paused && !Visible) return;

                GD.Print("InventoryUI: I key pressed (Toggle)");
                Visible = !Visible;
                GetTree().Paused = Visible; // 인벤토리 열면 일시정지 (Pause when open)
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

        private void RefreshGrid()
        {
            // 기존 슬롯 UI 제거 (Remove existing slots)
            foreach (Node child in _grid.GetChildren())
                child.QueueFree();

            // 슬롯 생성 (20칸) (Create 20 slots)
            for (int i = 0; i < Inventory.MaxSlots; i++)
            {
                var slotPanel = new PanelContainer();
                slotPanel.CustomMinimumSize = new Vector2(64, 64);

                var vbox = new VBoxContainer();
                slotPanel.AddChild(vbox);

                if (i < _inventory.Slots.Count)
                {
                    var slot = _inventory.Slots[i];

                    // 아이콘 또는 이름 (Icon or Name)
                    if (slot.Item.Icon != null)
                    {
                        var icon = new TextureRect();
                        icon.Texture = slot.Item.Icon;
                        icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
                        icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                        vbox.AddChild(icon);
                    }
                    else
                    {
                        var nameLabel = new Label();
                        nameLabel.Text = slot.Item.ItemName;
                        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        nameLabel.AddThemeFontSizeOverride("font_size", 10);
                        vbox.AddChild(nameLabel);
                    }

                    // 수량 표시 (스택 가능한 경우) (Display quantity)
                    if (slot.Item.IsStackable && slot.Quantity > 1)
                    {
                        var qtyLabel = new Label();
                        qtyLabel.Text = $"x{slot.Quantity}";
                        qtyLabel.HorizontalAlignment = HorizontalAlignment.Right;
                        qtyLabel.AddThemeFontSizeOverride("font_size", 11);
                        vbox.AddChild(qtyLabel);
                    }

                    // 클릭 이벤트 (Click Event)
                    int slotIndex = i; // 클로저용 캡처 (Capture for closure)
                    slotPanel.GuiInput += (inputEvent) =>
                    {
                        if (inputEvent is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                        {
                            SelectSlot(slotIndex);
                        }
                    };

                    // 마우스 오버 추적 (Track mouse hover for quick slot assignment)
                    slotPanel.MouseEntered += () => _hoveredSlotIndex = slotIndex;
                    slotPanel.MouseExited += () =>
                    {
                        if (_hoveredSlotIndex == slotIndex) _hoveredSlotIndex = -1;
                    };
                }

                _grid.AddChild(slotPanel);
            }

            // 선택 초기화 (Reset Selection)
            _selectedSlot = -1;
            _itemInfoLabel.Text = "아이템을 선택하세요 (Select an item)";
            _useButton.Visible = false;
        }

        private void SelectSlot(int index)
        {
            if (index < 0 || index >= _inventory.Slots.Count) return;
            _selectedSlot = index;
            var slot = _inventory.Slots[index];

            _itemInfoLabel.Text = $"{slot.Item.ItemName}\n{slot.Item.Description}";
            _useButton.Visible = true;
            _useButton.Text = slot.Item.Type == ItemType.Consumable ? "사용 (Use)" : "장착 (Equip)";
        }

        private void RefreshEquipment()
        {
            _weaponLabel.Text = _inventory.EquippedWeapon != null
                ? $"무기: {_inventory.EquippedWeapon.ItemName} (+{_inventory.EquippedWeapon.BonusDamage} DMG)"
                : "무기: 없음 (Weapon: None)";
            _armorLabel.Text = _inventory.EquippedArmor != null
                ? $"방어구: {_inventory.EquippedArmor.ItemName} (+{_inventory.EquippedArmor.BonusMaxHealth} HP)"
                : "방어구: 없음 (Armor: None)";
        }

        private void OnUsePressed()
        {
            if (_selectedSlot >= 0)
                _inventory.UseItem(_selectedSlot, _player);
        }

        private void OnUnequipWeaponPressed()
        {
            _inventory.UnequipWeapon(_player);
        }

        private void OnUnequipArmorPressed()
        {
            _inventory.UnequipArmor(_player);
        }

        public override void _ExitTree()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshGrid;
                _inventory.OnEquipmentChanged -= RefreshEquipment;
            }
        }
    }
}
