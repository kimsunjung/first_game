using Godot;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.UI
{
    public partial class ShopUI : BaseUIWindow
    {
        private TabContainer _tabContainer;
        private GridContainer _buyGrid;
        private GridContainer _sellGrid;
        private Label _goldLabel;
        private Label _messageLabel;
        private Button _closeButton;

        // 수량 선택 UI
        private Control _quantityPanel;
        private Label _quantityLabel;
        private SpinBox _quantitySpinBox;
        private Button _confirmBuyButton;
        private Label _totalPriceLabel;

        private IPlayer _player;
        private Inventory _inventory;
        private ItemData[] _shopItems;
        private ItemData _selectedBuyItem;

        protected override void OnReadyInternal()
        {
            _tabContainer = GetNode<TabContainer>("%TabContainer");
            _buyGrid = GetNode<GridContainer>("%BuyGrid");
            _sellGrid = GetNode<GridContainer>("%SellGrid");
            _goldLabel = GetNode<Label>("%ShopGoldLabel");
            _messageLabel = GetNode<Label>("%ShopMessageLabel");
            _closeButton = GetNode<Button>("%CloseButton");

            _quantityPanel = GetNode<Control>("%QuantityPanel");
            _quantityLabel = GetNode<Label>("%QuantityItemLabel");
            _quantitySpinBox = GetNode<SpinBox>("%QuantitySpinBox");
            _confirmBuyButton = GetNode<Button>("%ConfirmBuyButton");
            _totalPriceLabel = GetNode<Label>("%TotalPriceLabel");

            _closeButton.Pressed += Close;
            _confirmBuyButton.Pressed += OnConfirmBuy;
            _quantitySpinBox.ValueChanged += OnQuantityChanged;

            _messageLabel.Visible = false;
            _quantityPanel.Visible = false;
        }

        // --- 상점 진입점 (NPC 상호작용에서 호출) ---

        public void OpenShop(ItemData[] shopItems, string shopName)
        {
            var player = GameManager.Instance?.Player;
            if (player == null) return;

            _shopItems = shopItems;
            _player = player;
            _inventory = player.Inventory;

            if (Visible) OnOpened(); // 이미 열린 상태면 내용만 갱신
            else Open();
        }

        protected override void OnOpened()
        {
            _quantityPanel.Visible = false;
            RefreshBuyTab();
            RefreshSellTab();
            UpdateGoldDisplay();
        }

        protected override void OnClosed()
        {
            _quantityPanel.Visible = false;
            _selectedBuyItem = null;
        }

        // --- 구매 탭 ---

        private void RefreshBuyTab()
        {
            foreach (Node child in _buyGrid.GetChildren())
                child.QueueFree();

            if (_shopItems == null) return;

            foreach (var item in _shopItems)
            {
                var panel = CreateItemPanel(item, isBuyMode: true);
                _buyGrid.AddChild(panel);
            }
        }

        private void OnBuyItemSelected(ItemData item)
        {
            _selectedBuyItem = item;

            if (item.IsStackable)
            {
                // 스택 아이템: 수량 선택 패널 표시
                _quantityPanel.Visible = true;
                _quantityLabel.Text = item.ItemName;
                int maxAffordable = item.Price > 0 ? GameManager.Instance.PlayerGold / item.Price : 99;
                _quantitySpinBox.MinValue = 1;
                _quantitySpinBox.MaxValue = Mathf.Max(1, Mathf.Min(maxAffordable, item.MaxStack));
                _quantitySpinBox.Value = 1;
                UpdateTotalPrice();
            }
            else
            {
                // 비스택 아이템: 즉시 1개 구매
                TryBuyItem(item, 1);
            }
        }

        private void OnQuantityChanged(double value)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            if (_selectedBuyItem == null) return;
            int total = _selectedBuyItem.Price * (int)_quantitySpinBox.Value;
            _totalPriceLabel.Text = $"합계: {total}G";
        }

        private void OnConfirmBuy()
        {
            if (_selectedBuyItem == null) return;
            TryBuyItem(_selectedBuyItem, (int)_quantitySpinBox.Value);
            _quantityPanel.Visible = false;
        }

        private void TryBuyItem(ItemData item, int quantity)
        {
            int totalCost = item.Price * quantity;

            // 골드 부족 체크
            if (GameManager.Instance.PlayerGold < totalCost)
            {
                ShowMessage("골드가 부족합니다! (Not enough gold!)");
                AudioManager.Instance?.PlaySFX("shop_fail.wav");
                return;
            }

            // 인벤토리 공간 체크
            bool canAdd = _inventory.AddItem(item, quantity);
            if (!canAdd)
            {
                ShowMessage("가방이 꽉 찼습니다! (Inventory full!)");
                AudioManager.Instance?.PlaySFX("shop_fail.wav");
                return;
            }

            // 구매 성공
            GameManager.Instance.PlayerGold -= totalCost;
            AudioManager.Instance?.PlaySFX("shop_buy.wav");
            ShowMessage($"{item.ItemName} x{quantity} 구매! (-{totalCost}G)");
            UpdateGoldDisplay();
            RefreshSellTab();  // 판매 탭도 갱신
        }

        // --- 판매 탭 ---

        private void RefreshSellTab()
        {
            foreach (Node child in _sellGrid.GetChildren())
                child.QueueFree();

            if (_inventory == null) return;

            for (int i = 0; i < _inventory.Slots.Count; i++)
            {
                var slot = _inventory.Slots[i];

                // 장착 중인 장비는 판매 불가 → 목록에서 제외
                if (slot.Item == _inventory.EquippedWeapon) continue;
                if (slot.Item == _inventory.EquippedArmor) continue;
                if (slot.Item == _inventory.EquippedAccessory) continue;

                int slotIndex = i;  // 클로저용 캡처
                var panel = CreateSellPanel(slot, slotIndex);
                _sellGrid.AddChild(panel);
            }
        }

        private void TrySellItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventory.Slots.Count) return;
            var slot = _inventory.Slots[slotIndex];

            // 장착 중인 장비 이중 체크
            if (slot.Item == _inventory.EquippedWeapon || slot.Item == _inventory.EquippedArmor || slot.Item == _inventory.EquippedAccessory)
            {
                ShowMessage("장착 중인 장비는 판매할 수 없습니다!");
                return;
            }

            int sellPrice = slot.Item.SellPrice;
            string itemName = slot.Item.ItemName;

            GameManager.Instance.PlayerGold += sellPrice;
            AudioManager.Instance?.PlaySFX("shop_sell.wav");
            _inventory.RemoveItem(slotIndex, 1);

            ShowMessage($"{itemName} 판매! (+{sellPrice}G)");
            UpdateGoldDisplay();
            RefreshSellTab();
        }

        // --- UI 헬퍼 ---

        private PanelContainer CreateItemPanel(ItemData item, bool isBuyMode)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(260, 36);

            var hbox = new HBoxContainer();
            panel.AddChild(hbox);

            // 아이콘
            if (item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = item.Icon;
                icon.CustomMinimumSize = new Vector2(24, 24);
                icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
                icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                hbox.AddChild(icon);
            }

            // 이름 + 가격
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            var nameLabel = new Label();
            nameLabel.Text = item.ItemName;
            vbox.AddChild(nameLabel);

            var priceLabel = new Label();
            priceLabel.Text = $"{item.Price}G";
            priceLabel.AddThemeFontSizeOverride("font_size", 11);
            vbox.AddChild(priceLabel);
            hbox.AddChild(vbox);

            // 구매 버튼
            var buyButton = new Button();
            buyButton.Text = "구매";
            buyButton.Pressed += () => OnBuyItemSelected(item);
            hbox.AddChild(buyButton);

            return panel;
        }

        private PanelContainer CreateSellPanel(InventorySlot slot, int slotIndex)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(260, 36);

            var hbox = new HBoxContainer();
            panel.AddChild(hbox);

            // 아이콘
            if (slot.Item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = slot.Item.Icon;
                icon.CustomMinimumSize = new Vector2(24, 24);
                icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
                icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                hbox.AddChild(icon);
            }

            // 이름 + 판매가 + 수량
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            var nameLabel = new Label();
            nameLabel.Text = slot.Quantity > 1 ? $"{slot.Item.ItemName} x{slot.Quantity}" : slot.Item.ItemName;
            vbox.AddChild(nameLabel);

            var priceLabel = new Label();
            priceLabel.Text = $"판매가: {slot.Item.SellPrice}G";
            priceLabel.AddThemeFontSizeOverride("font_size", 11);
            vbox.AddChild(priceLabel);
            hbox.AddChild(vbox);

            // 판매 버튼
            var sellButton = new Button();
            sellButton.Text = "판매";
            sellButton.Pressed += () => TrySellItem(slotIndex);
            hbox.AddChild(sellButton);

            return panel;
        }

        private void UpdateGoldDisplay()
        {
            _goldLabel.Text = $"보유 골드: {GameManager.Instance.PlayerGold}G";
        }

        protected override void OnExitTreeInternal()
        {
            if (_closeButton != null) _closeButton.Pressed -= Close;
            if (_confirmBuyButton != null) _confirmBuyButton.Pressed -= OnConfirmBuy;
            if (_quantitySpinBox != null) _quantitySpinBox.ValueChanged -= OnQuantityChanged;
        }

        private async void ShowMessage(string text)
        {
            _messageLabel.Text = text;
            _messageLabel.Visible = true;
            await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
            if (IsInstanceValid(this))
                _messageLabel.Visible = false;
        }
    }
}
