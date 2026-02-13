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
