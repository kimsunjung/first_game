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

            // 인벤토리 공간 사전 확인
            if (!_inventory.CanAddItem(item, quantity))
            {
                ShowMessage("가방이 꽉 찼습니다! (Inventory full!)");
                AudioManager.Instance?.PlaySFX("shop_fail.wav");
                return;
            }

            // 골드 먼저 차감 후 AddItem — 역순이면 골드 차감 전 상태가 저장될 수 있음
            GameManager.Instance.PlayerGold -= totalCost;
            _inventory.AddItem(item, quantity);
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
                // 장착 장비는 인벤에서 이미 빠져 있으므로 별도 필터 불필요. 동일 .tres
                // 복제품(예: 같은 무기 2개)을 가지고 있을 때 본 비교가 판매 가능한 사본까지
                // 숨겨버리던 결함 제거.
                int slotIndex = i;  // 클로저용 캡처
                var panel = CreateSellPanel(slot, slotIndex);
                _sellGrid.AddChild(panel);
            }
        }

        private void TrySellItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventory.Slots.Count) return;
            var slot = _inventory.Slots[slotIndex];
            // 장착 장비는 인벤에서 빠져 있으므로 슬롯에 같은 .tres가 있다는 건 별도 사본.
            // ItemData 참조 비교는 .tres 공유 자원 특성상 사본도 막아버려 판매 불가 결함을 만든다.
            // → 슬롯 존재 자체가 "이건 판매 가능한 사본"임을 보장.

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
            panel.MouseFilter = Control.MouseFilterEnum.Pass;

            var hbox = new HBoxContainer();
            hbox.MouseFilter = Control.MouseFilterEnum.Pass;
            panel.AddChild(hbox);

            // 아이콘
            if (item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = item.Icon;
                icon.CustomMinimumSize = new Vector2(24, 24);
                icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
                icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                icon.MouseFilter = Control.MouseFilterEnum.Ignore;
                hbox.AddChild(icon);
            }

            // 이름 + 스펙 + 가격
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            vbox.MouseFilter = Control.MouseFilterEnum.Pass;
            var nameLabel = new Label();
            nameLabel.Text = item.ItemName;
            nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            vbox.AddChild(nameLabel);

            string spec = BuildSpecText(item);
            if (!string.IsNullOrEmpty(spec))
            {
                var specLabel = new Label();
                specLabel.Text = spec;
                specLabel.AddThemeFontSizeOverride("font_size", 10);
                specLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 1f));
                specLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
                vbox.AddChild(specLabel);
            }

            var priceLabel = new Label();
            priceLabel.Text = $"{item.Price}G";
            priceLabel.AddThemeFontSizeOverride("font_size", 11);
            priceLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            vbox.AddChild(priceLabel);
            hbox.AddChild(vbox);

            // 구매 버튼 — PASS: 클릭은 그대로 처리되면서 드래그가 ScrollContainer까지 전달됨
            var buyButton = new Button();
            buyButton.Text = "구매";
            buyButton.MouseFilter = Control.MouseFilterEnum.Pass;
            buyButton.Pressed += () => OnBuyItemSelected(item);
            hbox.AddChild(buyButton);

            return panel;
        }

        private PanelContainer CreateSellPanel(InventorySlot slot, int slotIndex)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(260, 36);
            panel.MouseFilter = Control.MouseFilterEnum.Pass;

            var hbox = new HBoxContainer();
            hbox.MouseFilter = Control.MouseFilterEnum.Pass;
            panel.AddChild(hbox);

            // 아이콘
            if (slot.Item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = slot.Item.Icon;
                icon.CustomMinimumSize = new Vector2(24, 24);
                icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
                icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                icon.MouseFilter = Control.MouseFilterEnum.Ignore;
                hbox.AddChild(icon);
            }

            // 이름 + 판매가 + 수량
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            vbox.MouseFilter = Control.MouseFilterEnum.Pass;
            var nameLabel = new Label();
            nameLabel.Text = slot.Quantity > 1 ? $"{slot.Item.ItemName} x{slot.Quantity}" : slot.Item.ItemName;
            nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            vbox.AddChild(nameLabel);

            var priceLabel = new Label();
            priceLabel.Text = $"판매가: {slot.Item.SellPrice}G";
            priceLabel.AddThemeFontSizeOverride("font_size", 11);
            priceLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            vbox.AddChild(priceLabel);
            hbox.AddChild(vbox);

            // 판매 버튼 — PASS: 클릭은 그대로 처리되면서 드래그가 ScrollContainer까지 전달됨
            var sellButton = new Button();
            sellButton.Text = "판매";
            sellButton.MouseFilter = Control.MouseFilterEnum.Pass;
            sellButton.Pressed += () => TrySellItem(slotIndex);
            hbox.AddChild(sellButton);

            return panel;
        }

        private static string BuildSpecText(ItemData item)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (item.BonusDamage != 0)      parts.Add($"공격+{item.BonusDamage}");
            if (item.BonusMaxHealth != 0)   parts.Add($"HP+{item.BonusMaxHealth}");
            if (item.BonusDefense != 0)     parts.Add($"방어+{item.BonusDefense}");
            if (item.BonusMoveSpeed != 0)   parts.Add($"이동+{item.BonusMoveSpeed:0}");
            if (item.BonusCritRate != 0)    parts.Add($"치명+{item.BonusCritRate * 100:0}%");
            return parts.Count > 0 ? string.Join("  ", parts) : "";
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
