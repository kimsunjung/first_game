using Godot;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.UI
{
    public partial class ShopUI : BaseUIWindow
    {
        private Label _titleLabel;
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
            _titleLabel = GetNode<Label>("%ShopTitleLabel");
            _tabContainer = GetNode<TabContainer>("%TabContainer");
            _buyGrid = GetNode<GridContainer>("%BuyGrid");
            _sellGrid = GetNode<GridContainer>("%SellGrid");
            _goldLabel = GetNode<Label>("%ShopGoldLabel");
            _messageLabel = GetNode<Label>("%ShopMessageLabel");
            _closeButton = GetNode<Button>("%CloseButton");
            // 탭 폰트 축소 — 화면에 더 많은 아이템 보이도록
            _tabContainer.AddThemeFontSizeOverride("font_size", 10);
            _goldLabel.AddThemeFontSizeOverride("font_size", 10);

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
            if (_titleLabel != null && !string.IsNullOrEmpty(shopName))
                _titleLabel.Text = BuildShopHeader(shopName);

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

            // 현재 플레이어 클래스 — 타 직업 전용 무기는 진열에서 제외(필터).
            var playerCls = GameManager.Instance?.Player?.Stats.PlayerClass;

            foreach (var item in _shopItems)
            {
                if (item == null) continue;
                // 상점 차단 — 적/보스 드랍 전용 무기는 진열 자체 제외.
                if (item.IsShopBlocked) continue;
                // 클래스 전용 장비는 현재 직업과 일치할 때만 진열(무기/방어구/장신구 공통).
                // AvailableToAllClasses=true(기본) 품목은 항상 노출(소모품/공용 장신구).
                if (!item.AvailableToAllClasses
                    && playerCls.HasValue && item.RequiredClass != playerCls.Value)
                    continue;
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
                // 장착 중인 슬롯은 판매 목록에서 제외 — RemoveItem이 이미 차단하지만
                // 골드를 먼저 지급한 뒤 제거 실패하면 무한 골드 취득 버그 발생.
                if (slot.IsEquipped) continue;
                int slotIndex = i;  // 클로저용 캡처
                var panel = CreateSellPanel(slot, slotIndex);
                _sellGrid.AddChild(panel);
            }
        }

        private void TrySellItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventory.Slots.Count) return;
            var slot = _inventory.Slots[slotIndex];

            // mutation 경계 방어 — RefreshSellTab이 장착 슬롯을 숨기지만 stale UI/추가 guard에서
            // 골드 선지급 후 제거 실패가 다시 살아날 수 있어 여기서 한 번 더 차단.
            if (slot.IsEquipped)
            {
                ShowMessage("장착 중인 아이템은 판매할 수 없습니다.");
                return;
            }

            int sellPrice = slot.Item.SellPrice;
            string itemName = slot.Item.ItemName;

            // 트랜잭션으로 묶어 "아이템 제거 ↔ 골드 지급" 중간 상태 저장을 차단.
            // RemoveItem이 OnInventoryChanged → RequestAutoSave를 트리거할 때 throttle이 지난
            // 상태면 "아이템 없음 + 골드 미증가"가 디스크에 박혀 종료 타이밍에 판매 금액 유실.
            // 트랜잭션 종료 후 명시적 SaveGame으로 일관 상태 즉시 보존(throttle 우회).
            using (GameTransaction.Begin())
            {
                // 반드시 제거 성공 후에 골드 지급 — 무한 골드 exploit 차단의 핵심.
                if (!_inventory.RemoveItem(slotIndex, 1))
                {
                    ShowMessage("아이템을 판매할 수 없습니다.");
                    return;
                }

                GameManager.Instance.PlayerGold += sellPrice;
                AudioManager.Instance?.PlaySFX("shop_sell.wav");
            }
            SaveManager.SaveGame();

            ShowMessage($"{itemName} 판매! (+{sellPrice}G)");
            UpdateGoldDisplay();
            RefreshSellTab();
        }

        // --- UI 헬퍼 ---

        private PanelContainer CreateItemPanel(ItemData item, bool isBuyMode)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(260, 20);
            panel.MouseFilter = Control.MouseFilterEnum.Pass;

            var hbox = new HBoxContainer();
            hbox.MouseFilter = Control.MouseFilterEnum.Pass;
            panel.AddChild(hbox);

            // 아이콘
            if (item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = item.Icon;
                icon.CustomMinimumSize = new Vector2(16, 16);
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
            nameLabel.AddThemeFontSizeOverride("font_size", 10);
            nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            vbox.AddChild(nameLabel);

            string spec = BuildSpecText(item);
            if (!string.IsNullOrEmpty(spec))
            {
                var specLabel = new Label();
                specLabel.Text = spec;
                specLabel.AddThemeFontSizeOverride("font_size", 8);
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
            buyButton.MouseFilter = Control.MouseFilterEnum.Pass;
            // 무기 클래스 제한 — 플레이어 클래스와 다르면 잠금 표시.
            var playerCls = FirstGame.Core.GameManager.Instance?.Player?.Stats.PlayerClass;
            if (isBuyMode && item.Type == ItemType.Weapon && !item.AvailableToAllClasses
                && playerCls.HasValue && item.RequiredClass != playerCls.Value)
            {
                buyButton.Text = $"{FirstGame.Data.PlayerClassUtil.DisplayName(item.RequiredClass)} 전용";
                buyButton.Disabled = true;
            }
            else
            {
                buyButton.Text = "구매";
                buyButton.Pressed += () => OnBuyItemSelected(item);
            }
            hbox.AddChild(buyButton);

            return panel;
        }

        private PanelContainer CreateSellPanel(InventorySlot slot, int slotIndex)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(260, 20);
            panel.MouseFilter = Control.MouseFilterEnum.Pass;

            var hbox = new HBoxContainer();
            hbox.MouseFilter = Control.MouseFilterEnum.Pass;
            panel.AddChild(hbox);

            // 아이콘
            if (slot.Item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = slot.Item.Icon;
                icon.CustomMinimumSize = new Vector2(16, 16);
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
            // 소모품 — Heal/Mana/Buff 설명
            if (item.Type == ItemType.Consumable)
            {
                switch (item.UseEffect)
                {
                    case ItemUseEffect.Heal:        if (item.HealAmount > 0) parts.Add($"HP {item.HealAmount} 회복"); break;
                    case ItemUseEffect.RestoreMana: if (item.HealAmount > 0) parts.Add($"MP {item.HealAmount} 회복"); break;
                    case ItemUseEffect.Buff:
                        if (item.BuffMoveSpeed   > 0) parts.Add($"이속 +{item.BuffMoveSpeed:0.##}");
                        if (item.BuffAttackSpeed > 0) parts.Add($"공속 +{item.BuffAttackSpeed * 100:0}%");
                        if (item.BuffBaseDamage  > 0) parts.Add($"공격 +{item.BuffBaseDamage}");
                        if (item.BuffDefense     > 0) parts.Add($"방어 +{item.BuffDefense}");
                        if (item.BuffCritRate    > 0) parts.Add($"치명 +{item.BuffCritRate * 100:0}%");
                        if (item.BuffDurationSec > 0) parts.Add($"{item.BuffDurationSec:0}초");
                        break;
                    case ItemUseEffect.ReturnToTown: parts.Add("마을 귀환"); break;
                    case ItemUseEffect.Teleport:     parts.Add("순간이동"); break;
                    case ItemUseEffect.ReviveOnDeath: parts.Add("사망 시 자동 부활"); break;
                    case ItemUseEffect.CureStatus:   parts.Add("상태이상 해제"); break;
                }
                // 설명 텍스트도 있으면 짧게 첨부
                if (parts.Count == 0 && !string.IsNullOrEmpty(item.Description))
                    parts.Add(item.Description);
            }
            // 장비 보너스
            if (item.BonusDamage != 0)      parts.Add($"공격+{item.BonusDamage}");
            if (item.BonusMaxHealth != 0)   parts.Add($"HP+{item.BonusMaxHealth}");
            if (item.BonusDefense != 0)     parts.Add($"방어+{item.BonusDefense}");
            if (item.BonusMoveSpeed != 0)   parts.Add($"이동+{item.BonusMoveSpeed:0}");
            if (item.BonusCritRate != 0)    parts.Add($"치명+{item.BonusCritRate * 100:0}%");
            return parts.Count > 0 ? string.Join("  ", parts) : "";
        }

        private static string BuildShopHeader(string shopName)
        {
            if (shopName.Contains("무기/방어구")) return "장비";
            if (shopName.Contains("소모품")) return "소모품";
            return shopName;
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
