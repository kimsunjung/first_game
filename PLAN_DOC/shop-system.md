# 상점 시스템 구현 계획

> 상태 메모 (2026-04-15)
> 이 문서는 구현 당시의 계획서다. 현재 프로젝트는 `main.tscn` 단일 구조가 아니라 맵 씬이 `Scenes/Maps/*` 아래로 분리되어 있다.
> 문서 안의 `main.tscn` 참조는 현재의 `town.tscn`과 UI/오브젝트 씬 조합으로 해석해야 한다.

## Context
골드가 쌓이기만 하고 쓸 곳이 없음. 상점 NPC를 통해 아이템을 구매/판매할 수 있는 시스템을 추가하여 "적 처치 → 골드 획득 → 상점 구매 → 더 강한 적 처치" 경제 루프를 완성함.

## 유저 요구사항
- 판매 가격은 SellPrice 필드를 별도 생성
- 상점 열려있을 때 일시정지
- 구매/판매 후 자동저장 안함
- 장착중인 장비는 판매 불가
- 스택 아이템은 여러개 구매 가능

## 생성/수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scripts/Data/ItemData.cs` | **수정** | SellPrice 필드 추가 |
| `Scripts/Entities/Shop/ShopNPC.cs` | **신규** | Area2D 기반 상점 NPC (SavePoint 패턴 재사용) |
| `Scripts/UI/ShopUI.cs` | **신규** | 구매/판매 탭 UI (InventoryUI 패턴 재사용) |
| `Scripts/UI/InventoryUI.cs` | **수정** | 상점 열려있을 때 인벤토리 열기 차단 |
| `Scenes/Objects/shop_npc.tscn` | **신규** | ShopNPC 씬 |
| `Scenes/UI/shop_ui.tscn` | **신규** | ShopUI 씬 |
| `Resources/Items/health_potion.tres` | **수정** | SellPrice 설정 |
| `Resources/Items/iron_sword.tres` | **수정** | SellPrice 설정 |
| `Resources/Items/iron_armor.tres` | **수정** | SellPrice 설정 |
| `main.tscn` | **수정** | ShopNPC, ShopUI 배치 |

---

## 1단계: ItemData에 SellPrice 추가

### `Scripts/Data/ItemData.cs` (18행 아래 추가)

```csharp
[Export] public int Price { get; set; } = 0;       // 구매가 (기존)
[Export] public int SellPrice { get; set; } = 0;    // 판매가 (신규)
```

### 기존 아이템 tres 파일 SellPrice 설정

| 아이템 | Price (구매가) | SellPrice (판매가) |
|--------|---------------|-------------------|
| health_potion.tres | 50 | 25 |
| iron_sword.tres | 100 | 50 |
| iron_armor.tres | 120 | 60 |

---

## 2단계: ShopNPC 구현

### `Scripts/Entities/Shop/ShopNPC.cs` (신규)

SavePoint.cs의 Area2D + E키 상호작용 패턴을 재사용.

```csharp
using Godot;
using FirstGame.Data;

namespace FirstGame.Entities.Shop
{
    public partial class ShopNPC : Area2D
    {
        [Export] public ItemData[] ShopItems { get; set; }  // 판매 물건 목록
        [Export] public string ShopName { get; set; } = "상점";

        private bool _playerInRange = false;
        private Label _promptLabel;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            _promptLabel = GetNode<Label>("PromptLabel");
            _promptLabel.Visible = false;
        }

        public override void _Process(double delta)
        {
            if (_playerInRange && Input.IsActionJustPressed("interact"))
            {
                // 게임이 이미 일시정지 상태면 무시 (게임오버, 인벤토리, 상점 이미 열림)
                if (GetTree().Paused) return;

                var shopUI = GetTree().Root.GetNode<ShopUI>("Main/ShopUI");  // 경로는 씬 구조에 맞게 조정
                shopUI.OpenShop(ShopItems, ShopName);
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                _playerInRange = true;
                _promptLabel.Visible = true;
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                _playerInRange = false;
                _promptLabel.Visible = false;
            }
        }
    }
}
```

### `Scenes/Objects/shop_npc.tscn` (신규)

save_point.tscn과 동일한 구조:
```
ShopNPC (Area2D)
├── Sprite2D          ← NPC 스프라이트
├── CollisionShape2D  ← 상호작용 범위 (원형, 반경 80~100)
└── PromptLabel (Label) ← "E키로 상점 열기" 텍스트
```

- ShopItems: 에디터에서 health_potion, iron_sword, iron_armor 할당
- PromptLabel.Text = "E키: 상점 (Press E: Shop)"

**참고: save_point.tscn 구조 (복사 기반)**
```
[gd_scene load_steps=3 format=3 uid="uid://b8x4y2z5w4v0"]

[ext_resource type="Script" uid="uid://crhp7cy0gjh6o" path="res://Scripts/Entities/SavePoint/SavePoint.cs" id="1_saveP"]

[sub_resource type="CircleShape2D" id="CircleShape2D_save"]
radius = 50.0

[node name="SavePoint" type="Area2D"]
collision_layer = 1
collision_mask = 1
script = ExtResource("1_saveP")

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("CircleShape2D_save")
debug_color = Color(0.2, 0.8, 0.2, 0.42)

[node name="SaveIcon" type="Label" parent="."]
...
text = "[SAVE]"

[node name="PromptLabel" type="Label" parent="."]
visible = false
...
text = "Press E to Save"
```

→ ShopNPC용으로 변환 시: 스크립트 경로, 노드 이름, 텍스트만 변경. **uid는 새로 생성 필수** (UID 충돌 주의).

---

## 3단계: ShopUI 구현

### `Scripts/UI/ShopUI.cs` (신규)

InventoryUI.cs의 토글 + 일시정지 패턴을 재사용. process_mode = Always(3) 필수.

```csharp
using Godot;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
    public partial class ShopUI : CanvasLayer
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

        private PlayerController _player;
        private Inventory _inventory;
        private ItemData[] _shopItems;
        private ItemData _selectedBuyItem;

        public override void _Ready()
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

            _closeButton.Pressed += CloseShop;
            _confirmBuyButton.Pressed += OnConfirmBuy;
            _quantitySpinBox.ValueChanged += OnQuantityChanged;

            _messageLabel.Visible = false;
            _quantityPanel.Visible = false;
            Visible = false;
        }

        public override void _Process(double delta)
        {
            // E키 또는 Escape로 상점 닫기
            if (Visible && (Input.IsActionJustPressed("interact") || Input.IsActionJustPressed("ui_cancel")))
            {
                CloseShop();
            }
        }

        // --- 상점 열기/닫기 ---

        public void OpenShop(ItemData[] shopItems, string shopName)
        {
            _shopItems = shopItems;

            // 플레이어 연결
            var players = GetTree().GetNodesInGroup("Player");
            if (players.Count > 0 && players[0] is PlayerController player)
            {
                _player = player;
                _inventory = player.Inventory;
            }
            else return;

            Visible = true;
            GetTree().Paused = true;
            _quantityPanel.Visible = false;

            RefreshBuyTab();
            RefreshSellTab();
            UpdateGoldDisplay();
        }

        public void CloseShop()
        {
            Visible = false;
            GetTree().Paused = false;
            _quantityPanel.Visible = false;
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
                return;
            }

            // 인벤토리 공간 체크
            bool canAdd = _inventory.AddItem(item, quantity);
            if (!canAdd)
            {
                ShowMessage("가방이 꽉 찼습니다! (Inventory full!)");
                return;
            }

            // 구매 성공
            GameManager.Instance.PlayerGold -= totalCost;
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
            if (slot.Item == _inventory.EquippedWeapon || slot.Item == _inventory.EquippedArmor)
            {
                ShowMessage("장착 중인 장비는 판매할 수 없습니다!");
                return;
            }

            int sellPrice = slot.Item.SellPrice;
            string itemName = slot.Item.ItemName;

            GameManager.Instance.PlayerGold += sellPrice;
            _inventory.RemoveItem(slotIndex, 1);

            ShowMessage($"{itemName} 판매! (+{sellPrice}G)");
            UpdateGoldDisplay();
            RefreshSellTab();
        }

        // --- UI 헬퍼 ---

        private PanelContainer CreateItemPanel(ItemData item, bool isBuyMode)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(280, 64);

            var hbox = new HBoxContainer();
            panel.AddChild(hbox);

            // 아이콘
            if (item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = item.Icon;
                icon.CustomMinimumSize = new Vector2(48, 48);
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
            priceLabel.AddThemeFontSizeOverride("font_size", 12);
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
            panel.CustomMinimumSize = new Vector2(280, 64);

            var hbox = new HBoxContainer();
            panel.AddChild(hbox);

            // 아이콘
            if (slot.Item.Icon != null)
            {
                var icon = new TextureRect();
                icon.Texture = slot.Item.Icon;
                icon.CustomMinimumSize = new Vector2(48, 48);
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
            priceLabel.AddThemeFontSizeOverride("font_size", 12);
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
```

### `Scenes/UI/shop_ui.tscn` (신규)

process_mode = 3 (Always) 필수.

**참고: inventory_ui.tscn 구조 (패턴 참고용)**
```
[gd_scene format=3 uid="uid://c5v8q2m1n0qa"]
[ext_resource type="Script" path="res://Scripts/UI/InventoryUI.cs" id="1_invUI"]
[node name="InventoryUI" type="CanvasLayer"]
process_mode = 3
script = ExtResource("1_invUI")
[node name="PanelContainer" type="PanelContainer" parent="."]
anchors_preset = 8  (화면 중앙)
...
```

**ShopUI 씬 노드 트리:**
```
ShopUI (CanvasLayer) [process_mode = 3 (Always)]
└── PanelContainer (앵커: 화면 중앙, 크기 500x500 정도)
    └── MarginContainer (margin 16)
        └── VBoxContainer
            ├── Label (%ShopTitleLabel)              ← "상점 (Shop)"
            ├── TabContainer (%TabContainer)
            │   ├── VBoxContainer [탭 이름: "구매"]
            │   │   └── ScrollContainer
            │   │       └── GridContainer (%BuyGrid) [columns=1]
            │   └── VBoxContainer [탭 이름: "판매"]
            │       └── ScrollContainer
            │           └── GridContainer (%SellGrid) [columns=1]
            ├── Control (%QuantityPanel) [visible=false]
            │   └── HBoxContainer
            │       ├── Label (%QuantityItemLabel)
            │       ├── SpinBox (%QuantitySpinBox) [min=1, step=1]
            │       ├── Label (%TotalPriceLabel)
            │       └── Button (%ConfirmBuyButton) ← "구매 확정"
            ├── Label (%ShopGoldLabel)               ← "보유 골드: 0G"
            ├── Label (%ShopMessageLabel) [visible=false]
            └── Button (%CloseButton)                ← "닫기 (Close)"
```

**주의: 모든 `%` 노드는 `unique_name_in_owner = true` 설정 필수**

---

## 4단계: InventoryUI 충돌 방지

### `Scripts/UI/InventoryUI.cs` (_Process 수정)

상점이 열려있을 때(게임 일시정지 + 인벤토리 비활성) I키로 인벤토리가 열리면 안 됨.

**현재 코드 (51~72행):**
```csharp
public override void _Process(double delta)
{
    if (Input.IsActionJustPressed("inventory"))
    {
        if (_player != null && _player.IsDead) return;

        GD.Print("InventoryUI: I key pressed (Toggle)");
        Visible = !Visible;
        GetTree().Paused = Visible;
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
```

**수정 후:**
```csharp
public override void _Process(double delta)
{
    if (Input.IsActionJustPressed("inventory"))
    {
        if (_player != null && _player.IsDead) return;

        // 다른 UI가 일시정지를 걸었으면 인벤토리 열기 차단
        if (GetTree().Paused && !Visible) return;

        GD.Print("InventoryUI: I key pressed (Toggle)");
        Visible = !Visible;
        GetTree().Paused = Visible;
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
```

**추가되는 줄: `if (GetTree().Paused && !Visible) return;`**
이미 일시정지 상태인데 인벤토리가 닫혀있다면 → 상점 또는 게임오버가 일시정지를 건 것 → 열기 차단.

---

## 5단계: main.tscn에 배치

**현재 main.tscn 구조:**
```
Main (Node2D)
├── EnemySpawner
├── SavePoint
├── Player
├── HUD
└── InventoryUI
```

**수정 후:**
```
Main (Node2D)
├── EnemySpawner
├── SavePoint
├── ShopNPC          ← 추가 (Scenes/Objects/shop_npc.tscn 인스턴스)
├── Player
├── HUD
├── InventoryUI
└── ShopUI           ← 추가 (Scenes/UI/shop_ui.tscn 인스턴스)
```

- ShopNPC 위치: SavePoint 근처 (예: position = Vector2(600, 200))
- ShopUI: CanvasLayer이므로 위치 지정 불필요

---

## 주의사항

1. **ShopUI의 process_mode = 3 (Always)** 필수. 일시정지 중에도 UI 입력을 받아야 함
2. **ShopNPC의 process_mode = 기본값(Inherit)** 유지. 게임 일시정지 시 E키 중복 입력 방지
3. **장착 중인 장비 판매 차단**: ResourcePath 비교가 아닌 참조 비교(`==`) 사용
4. **AddItem 반환값 확인**: 구매 시 인벤토리 가득 참 체크 필수
5. **자동저장 안함**: TryBuyItem/TrySellItem에 SaveManager.SaveGame() 호출 없음
6. **tscn 파일 uid**: 새로 생성 시 기존 uid와 충돌하지 않도록 주의 (Godot 에디터에서 생성 권장)

## 검증 방법

1. **구매**: 상점 열기 → 포션 구매 → 골드 차감 + 인벤토리에 추가 확인
2. **스택 구매**: 포션 3개 구매 → 골드 150 차감 + 포션 x3 확인
3. **골드 부족**: 골드 부족 상태에서 구매 → "골드가 부족합니다" 메시지 확인
4. **가방 꽉 참**: 가방 20칸 채우고 구매 → "가방이 꽉 찼습니다" 메시지 확인
5. **판매**: 아이템 판매 → SellPrice만큼 골드 증가 + 인벤토리에서 제거 확인
6. **장착 장비 판매 차단**: 장착 중인 무기/방어구가 판매 탭에 안 나오는지 확인
7. **UI 충돌**: 상점 열린 상태에서 I키 → 인벤토리 안 열리는지 확인
8. **상점 닫기**: E키 또는 닫기 버튼 → 상점 닫히고 게임 재개 확인
9. **자동저장 안함**: 구매/판매 후 사망 → Load Save → 구매/판매 전 상태인지 확인

---

## 지역별 장비 상점 재분배 v2 (2026-05-19)

### 배경/문제
기존: `shop_npc.tscn` 기본 ShopItems = 21종 무기/방어구 전부가 **town 한 곳**에
집중(steel/frostfang/tide/guardian_amulet/shadow_ring 등 중·고티어 포함). 결과:
(1) 초반 town 상점 정보 과부하, (2) 지역 진행에 따른 장비 성장 동선 부재,
(3) 다른 3거점은 소모품만 판매 → 거점 정체성 약함.

### 변경 (데이터 전용 — .tscn ShopItems/ShopName, 코드·SaveData 무변경)
- **town (`Scenes/Objects/shop_npc.tscn` 기본 목록, ShopName "기본 장비 상점")**
  21→**14 기본티어만**: iron_sword/iron_spear/battle_axe/long_bow(궁수)/
  apprentice_robe(법사 방어구)/iron_armor/leather_armor/leather_cap/iron_helm/
  leather_boots/iron_boots/iron_ring/bronze_bracelet/wolf_necklace. 전 Common~
  Uncommon·저가. 직업별 최소 1종(전사 무기·궁수 활·법사 로브) 보장.
  ※ town ShopNPC만 기본 목록 사용(타 3거점 머천트는 ShopItems override).
- **field_outpost (ShopName "전초기지 무구상")** 중티어 추가:
  steel_sword/hunter_bow/steel_armor/chainmail_armor/steel_helm/knight_boots/
  silver_bracelet + 기존 소모품 유지(보급 차단 방지 위해 무게이트 유지).
- **harbor_village** 해안 정체성: fishermans_bow/composite_bow/tide_staff/
  storm_armor/storm_robe/storm_ring/storm_boots + 기존 해양 소모품.
- **mountain_refuge** 설원·화산 고티어: frostfang_staff/frost_armor/flame_armor/
  frost_robe/glacier_boots/flame_boots/fire_ring/ice_ring + 기존 저항 소모품.
- 전 추가 품목 Price>0 (Price=0 = 드랍/퀘스트 전용이라 상점 제외). 전 .tres 기존
  자산, 신규 PNG 없음. validate.py(uid·경로)·balance·build·test·diff green.

### 알려진 데이터 갭 (Priority A 후속 TODO, 본 패스 범위 밖)
- **법사 중티어 무기 부재**: starter_staff(Lv1 기증, +1dmg)와 rare
  frostfang/tide_staff(≈Lv11+, 1000g) 사이 *완성된* 지팡이 없음. wooden/
  frost/nature/crystal/shadow_staff 전부 "밸런스 미정" placeholder(Bonus 무).
  town이 과거 판매하던 staff는 rare frostfang/tide(올바르게 harbor/mountain
  으로 이전)뿐 → 회귀 아님. 법사는 스킬 중심이라 영향 완화. **수정엔 스탯
  밸런싱 설계 필요(컨텐츠 기획) → 보류·문서화.**
- **활 아이콘 미스매치(cosmetic, P3)**: long_bow→phoenix_bow.png,
  fishermans_bow→winter_bow.png. 올바른 long_bow.png/fishermans_bow.png
  미존재 → 신규 PNG 금지 규칙상 보류. 카테고리(활→활)는 일치라 저심각.

### v2 핫픽스 (Codex 어드버서리얼 리뷰 후속)
- **[P1] 활 진열 누락 수정**: `hunter_bow`/`composite_bow`는 `IsShopBlocked=true`(드랍 전용)
  라 ShopUI line 103에서 제외됨. outpost 무구상에서 `hunter_bow` 제거(중티어 활 후보 부재
  → 외피상 warrior-leaning 정체성으로 정착), harbor에서 `composite_bow` 제거(coast 궁수는
  `fishermans_bow`만 유지). 대신 **mountain에 `winter_bow`(서리)·`phoenix_bow`(불) 추가**
  — 둘 다 IsShopBlocked 미설정, Rarity 2, 설원·화산 테마 정합. 결과: 활 진행 동선
  town `long_bow`(rar1) → harbor `fishermans_bow`(rar1) → mountain `winter_bow`/`phoenix_bow`(rar2).
- **[P2] 클래스 필터를 무기 외 장비로 확장**: `ShopUI.RefreshBuyTab`의
  `item.Type == ItemType.Weapon && !item.AvailableToAllClasses` → `!item.AvailableToAllClasses`.
  방어구/장신구도 `RequiredClass`+`AvailableToAllClasses=false`라면 동일 직업에만 진열.
  소모품·공용 장신구(`AvailableToAllClasses=true` 기본)는 영향 없음.
  방지 효과: 법사가 plate armor 못 입는데 살 수 있던 데드 골드 지출 차단.
- **[P3] 레시피 골드 비용 규칙**: gold ≥ 결과물 판매가 총합 = "제작은 사용용, 되팔아 차익
  금지". v2 신규 2건 보정: `town_leather_armor` 40→100G(leather_armor sell 75 ≤ 100),
  `town_field_remedy` 20→60G(health_potion×2 sell 50 ≤ 60). recipes.json `_rule` 키로 명시.
