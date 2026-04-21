# 아이템/인벤토리/장비 시스템 + 적 드롭 구현 계획

> 상태 메모 (2026-04-15)
> 이 문서는 구현 당시의 계획서다. 현재 프로젝트는 `main.tscn` 단일 구조가 아니라 맵 씬이 `Scenes/Maps/*` 아래로 분리되어 있다.
> 문서 안의 `main.tscn` 참조는 현재의 `town.tscn`, `field_1.tscn` 등 맵 씬 배치로 해석해야 한다.

## Context
골드가 쌓이기만 하고 쓸 곳이 없음. 적을 처치해도 골드 외 보상이 없어 성장 체감이 부족함. 아이템 시스템(소비 + 장비)과 그리드형 인벤토리 UI를 추가하고, 적 처치 시 아이템이 드롭되도록 하여 **"적 처치 → 아이템 획득 → 장비 강화/포션 사용 → 더 강한 적 처치"** 성장 루프를 만듦.

## 생성/수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scripts/Data/ItemData.cs` | **신규** | 아이템 Resource 클래스 (이름, 타입, 효과, 아이콘) |
| `Scripts/Data/Inventory.cs` | **신규** | 인벤토리 로직 (추가, 제거, 사용, 장착) |
| `Scripts/UI/InventoryUI.cs` | **신규** | 그리드형 인벤토리 UI 컨트롤러 |
| `Scenes/UI/inventory_ui.tscn` | **신규** | 인벤토리 UI 씬 |
| `Resources/Items/health_potion.tres` | **신규** | 체력 포션 데이터 |
| `Resources/Items/iron_sword.tres` | **신규** | 철검 데이터 |
| `Resources/Items/iron_armor.tres` | **신규** | 철갑옷 데이터 |
| `Scripts/Entities/Player/PlayerController.cs` | **수정** | Inventory 인스턴스 보유, 장비 효과 적용 |
| `Scripts/Data/EnemyStats.cs` | **수정** | 드롭 테이블 추가 |
| `Scripts/Entities/Enemies/EnemyController.cs` | **수정** | 사망 시 아이템 드롭 |
| `Scripts/Data/SaveData.cs` | **수정** | 인벤토리/장비 저장 |
| `Scripts/Core/SaveManager.cs` | **수정** | 인벤토리/장비 저장/불러오기 |
| `Scripts/UI/HUD.cs` | **수정** | 아이템 획득 알림 |
| `Scenes/UI/hud.tscn` | **수정** | 획득 알림 라벨 추가 |
| `main.tscn` | **수정** | InventoryUI 추가 |
| `project.godot` | **수정** | 인벤토리 토글 키 (I키) 추가 |

---

## 1단계: 아이템 데이터 구조

### 1-1. ItemData.cs 신규 (`Scripts/Data/ItemData.cs`)

```csharp
using Godot;

namespace FirstGame.Data
{
    public enum ItemType
    {
        Consumable, // 포션 등 소비 아이템
        Weapon,     // 무기
        Armor       // 방어구
    }

    [GlobalClass]
    public partial class ItemData : Resource
    {
        [Export] public string ItemName { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }
        [Export] public int Price { get; set; } = 0;
        [Export] public ItemType Type { get; set; } = ItemType.Consumable;
        [Export] public bool IsStackable { get; set; } = true;
        [Export] public int MaxStack { get; set; } = 99;

        // 소비 아이템 효과
        [ExportGroup("Consumable Effects")]
        [Export] public int HealAmount { get; set; } = 0;

        // 장비 보너스
        [ExportGroup("Equipment Bonuses")]
        [Export] public int BonusDamage { get; set; } = 0;
        [Export] public int BonusMaxHealth { get; set; } = 0;
        [Export] public float BonusMoveSpeed { get; set; } = 0f;
    }
}
```

> `[ExportGroup]`으로 Inspector에서 소비/장비 속성을 구분.
> `[GlobalClass]`로 Godot 에디터에서 리소스 생성 가능.

### 1-2. 아이템 리소스 파일 (.tres)

**`Resources/Items/health_potion.tres`**
```
[gd_resource type="Resource" script_class="ItemData" load_steps=2 format=3]

[ext_resource type="Script" uid="<ItemData.cs UID>" path="res://Scripts/Data/ItemData.cs" id="1"]

[resource]
script = ExtResource("1")
ItemName = "Health Potion"
Description = "HP를 30 회복합니다"
Price = 50
Type = 0
IsStackable = true
MaxStack = 99
HealAmount = 30
```

**`Resources/Items/iron_sword.tres`**
```
[gd_resource type="Resource" script_class="ItemData" load_steps=2 format=3]

[ext_resource type="Script" uid="<ItemData.cs UID>" path="res://Scripts/Data/ItemData.cs" id="1"]

[resource]
script = ExtResource("1")
ItemName = "Iron Sword"
Description = "공격력 +5"
Price = 100
Type = 1
IsStackable = false
MaxStack = 1
BonusDamage = 5
```

**`Resources/Items/iron_armor.tres`**
```
[gd_resource type="Resource" script_class="ItemData" load_steps=2 format=3]

[ext_resource type="Script" uid="<ItemData.cs UID>" path="res://Scripts/Data/ItemData.cs" id="1"]

[resource]
script = ExtResource("1")
ItemName = "Iron Armor"
Description = "최대 체력 +20"
Price = 120
Type = 2
IsStackable = false
MaxStack = 1
BonusMaxHealth = 20
```

> `.tres` 파일의 `uid`는 빌드 시 Godot이 자동 생성. `<ItemData.cs UID>`는 실제 ItemData.cs.uid 파일의 값으로 대체.
> 또는 Godot 에디터에서 **Resources 폴더 우클릭 → New Resource → ItemData** 로 생성해도 됨.

---

## 2단계: 인벤토리 시스템

### 2-1. Inventory.cs 신규 (`Scripts/Data/Inventory.cs`)

```csharp
using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Entities.Player;

namespace FirstGame.Data
{
    public class InventorySlot
    {
        public ItemData Item { get; set; }
        public int Quantity { get; set; }
    }

    public class Inventory
    {
        public const int MaxSlots = 20;

        public List<InventorySlot> Slots { get; private set; } = new();

        // 장비 슬롯
        public ItemData EquippedWeapon { get; private set; }
        public ItemData EquippedArmor { get; private set; }

        // UI 갱신용 이벤트
        public event Action OnInventoryChanged;
        public event Action<ItemData> OnItemPickedUp;   // HUD 알림용
        public event Action OnEquipmentChanged;

        // --- 인벤토리 조작 ---

        public bool AddItem(ItemData item, int amount = 1)
        {
            // 스택 가능한 아이템: 기존 슬롯에 추가
            if (item.IsStackable)
            {
                var existing = Slots.Find(s => s.Item.ResourcePath == item.ResourcePath);
                if (existing != null)
                {
                    existing.Quantity = Math.Min(existing.Quantity + amount, item.MaxStack);
                    OnInventoryChanged?.Invoke();
                    OnItemPickedUp?.Invoke(item);
                    return true;
                }
            }

            // 새 슬롯 필요
            if (Slots.Count >= MaxSlots) return false; // 인벤토리 가득 참

            Slots.Add(new InventorySlot { Item = item, Quantity = amount });
            OnInventoryChanged?.Invoke();
            OnItemPickedUp?.Invoke(item);
            return true;
        }

        public void RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;

            Slots[slotIndex].Quantity -= amount;
            if (Slots[slotIndex].Quantity <= 0)
                Slots.RemoveAt(slotIndex);

            OnInventoryChanged?.Invoke();
        }

        // --- 아이템 사용 ---

        public void UseItem(int slotIndex, PlayerController player)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.Item.Type == ItemType.Consumable)
            {
                // 포션 사용: HP 회복
                player.Stats.CurrentHealth += slot.Item.HealAmount;
                GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount}");
                RemoveItem(slotIndex, 1);
            }
            else if (slot.Item.Type == ItemType.Weapon || slot.Item.Type == ItemType.Armor)
            {
                EquipItem(slotIndex, player);
            }
        }

        // --- 장비 장착/해제 ---

        public void EquipItem(int slotIndex, PlayerController player)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];
            var item = slot.Item;

            if (item.Type == ItemType.Weapon)
            {
                // 기존 무기 해제 → 인벤토리로
                if (EquippedWeapon != null)
                    UnequipWeapon(player);

                EquippedWeapon = item;
                player.Stats.BaseDamage += item.BonusDamage;
                RemoveItem(slotIndex, 1);
            }
            else if (item.Type == ItemType.Armor)
            {
                if (EquippedArmor != null)
                    UnequipArmor(player);

                EquippedArmor = item;
                player.Stats.MaxHealth += item.BonusMaxHealth;
                player.Stats.CurrentHealth += item.BonusMaxHealth; // 장착 시 추가 HP도 채움
                RemoveItem(slotIndex, 1);
            }

            OnEquipmentChanged?.Invoke();
            GD.Print($"{item.ItemName} 장착!");
        }

        public void UnequipWeapon(PlayerController player)
        {
            if (EquippedWeapon == null) return;
            player.Stats.BaseDamage -= EquippedWeapon.BonusDamage;
            AddItem(EquippedWeapon, 1);
            EquippedWeapon = null;
            OnEquipmentChanged?.Invoke();
        }

        public void UnequipArmor(PlayerController player)
        {
            if (EquippedArmor == null) return;
            player.Stats.MaxHealth -= EquippedArmor.BonusMaxHealth;
            if (player.Stats.CurrentHealth > player.Stats.MaxHealth)
                player.Stats.CurrentHealth = player.Stats.MaxHealth;
            AddItem(EquippedArmor, 1);
            EquippedArmor = null;
            OnEquipmentChanged?.Invoke();
        }
    }
}
```

> `UseItem`에서 PlayerController 참조 필요 → `using FirstGame.Entities.Player;` 추가.
> 아이템 비교는 `ResourcePath`로 수행 (같은 .tres 파일이면 같은 아이템).

---

## 3단계: 인벤토리 UI (그리드형)

### 3-1. InventoryUI.cs 신규 (`Scripts/UI/InventoryUI.cs`)

```csharp
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

            // Player 연결
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
            // I키로 토글
            if (Input.IsActionJustPressed("inventory"))
            {
                Visible = !Visible;
                GetTree().Paused = Visible; // 인벤토리 열면 일시정지
                if (Visible) RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            // 기존 슬롯 UI 제거
            foreach (Node child in _grid.GetChildren())
                child.QueueFree();

            // 슬롯 생성 (20칸)
            for (int i = 0; i < Inventory.MaxSlots; i++)
            {
                var slotPanel = new PanelContainer();
                slotPanel.CustomMinimumSize = new Vector2(64, 64);

                var vbox = new VBoxContainer();
                slotPanel.AddChild(vbox);

                if (i < _inventory.Slots.Count)
                {
                    var slot = _inventory.Slots[i];

                    // 아이콘 또는 이름
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

                    // 수량 표시 (스택 가능한 경우)
                    if (slot.Item.IsStackable && slot.Quantity > 1)
                    {
                        var qtyLabel = new Label();
                        qtyLabel.Text = $"x{slot.Quantity}";
                        qtyLabel.HorizontalAlignment = HorizontalAlignment.Right;
                        qtyLabel.AddThemeFontSizeOverride("font_size", 11);
                        vbox.AddChild(qtyLabel);
                    }

                    // 클릭 이벤트
                    int slotIndex = i; // 클로저용 캡처
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

            // 선택 초기화
            _selectedSlot = -1;
            _itemInfoLabel.Text = "아이템을 선택하세요";
            _useButton.Visible = false;
        }

        private void SelectSlot(int index)
        {
            if (index < 0 || index >= _inventory.Slots.Count) return;
            _selectedSlot = index;
            var slot = _inventory.Slots[index];

            _itemInfoLabel.Text = $"{slot.Item.ItemName}\n{slot.Item.Description}";
            _useButton.Visible = true;
            _useButton.Text = slot.Item.Type == ItemType.Consumable ? "사용" : "장착";
        }

        private void RefreshEquipment()
        {
            _weaponLabel.Text = _inventory.EquippedWeapon != null
                ? $"무기: {_inventory.EquippedWeapon.ItemName} (+{_inventory.EquippedWeapon.BonusDamage} DMG)"
                : "무기: 없음";
            _armorLabel.Text = _inventory.EquippedArmor != null
                ? $"방어구: {_inventory.EquippedArmor.ItemName} (+{_inventory.EquippedArmor.BonusMaxHealth} HP)"
                : "방어구: 없음";
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
```

### 3-2. inventory_ui.tscn 씬 구조 (`Scenes/UI/inventory_ui.tscn`)

```
InventoryUI (CanvasLayer) ← InventoryUI.cs, process_mode=3 (Always)
└── PanelContainer (앵커 중앙, min_size 450x400)
    └── MarginContainer (margins: 16)
        └── VBoxContainer
            ├── HBoxContainer (헤더)
            │   ├── Label "인벤토리" (font_size=24)
            │   └── Control (스페이서, h_size_flags: Expand+Fill)
            ├── HSeparator
            ├── ItemGrid (GridContainer) ← Unique Name, columns=5
            ├── HSeparator
            ├── ItemInfoLabel (Label) ← Unique Name, text="아이템을 선택하세요", min_height=50
            ├── UseButton (Button) ← Unique Name, text="사용", Visible=false
            ├── HSeparator
            ├── Label "장비" (font_size=18)
            ├── HBoxContainer (장비 표시)
            │   ├── VBoxContainer
            │   │   ├── WeaponLabel (Label) ← Unique Name, text="무기: 없음"
            │   │   └── UnequipWeaponButton (Button) ← Unique Name, text="해제"
            │   └── VBoxContainer
            │       ├── ArmorLabel (Label) ← Unique Name, text="방어구: 없음"
            │       └── UnequipArmorButton (Button) ← Unique Name, text="해제"
```

> 모든 `%` 참조 노드에 `unique_name_in_owner = true` 설정 필수.

---

## 4단계: 적 드롭 시스템

### 4-1. EnemyStats.cs 수정

드롭 관련 필드 추가:

```csharp
[ExportGroup("Drop Table")]
[Export] public ItemData[] PossibleDrops { get; set; }
[Export] public float DropChance { get; set; } = 0.5f; // 50% 확률
```

### 4-2. EnemyController.cs 수정

`Die()` 메서드에 드롭 로직 추가:

```csharp
using FirstGame.Entities.Player; // 추가 필요

private void Die()
{
    GD.Print("적 사망! (Enemy Died!)");
    GameManager.Instance.PlayerGold += 10;

    // 아이템 드롭
    if (Stats.PossibleDrops != null && Stats.PossibleDrops.Length > 0)
    {
        if (GD.Randf() <= Stats.DropChance)
        {
            int index = (int)GD.RandRange(0, Stats.PossibleDrops.Length - 1);
            var droppedItem = Stats.PossibleDrops[index];
            var players = GetTree().GetNodesInGroup("Player");
            if (players.Count > 0 && players[0] is PlayerController player)
            {
                player.Inventory.AddItem(droppedItem);
            }
        }
    }

    SaveManager.SaveGame(); // 적 처치 후 자동저장
    QueueFree();
}
```

### 4-3. enemy.tscn 또는 에디터에서 드롭 테이블 설정

EnemyStats Inspector에서 `PossibleDrops` 배열에 아이템 .tres 파일을 드래그하여 할당.
- health_potion.tres
- iron_sword.tres
- iron_armor.tres

또는 EnemyStats 리소스 파일에 직접 설정.

---

## 5단계: 기존 파일 수정

### 5-1. PlayerController.cs 수정

**Inventory 인스턴스 추가:**

```csharp
using FirstGame.Data; // 이미 있음

public Inventory Inventory { get; private set; }
```

**`_Ready`에 추가 (Stats 초기화 직후, PendingLoadData 확인 전):**

```csharp
Inventory = new Inventory();
```

**세이브 데이터 적용 부분에 인벤토리 복원 추가:**

```csharp
if (SaveManager.PendingLoadData != null)
{
    var data = SaveManager.PendingLoadData;
    // ... 기존 위치/HP/골드 복원 ...

    // 인벤토리 복원
    if (data.InventoryItems != null)
    {
        foreach (var savedSlot in data.InventoryItems)
        {
            var item = GD.Load<ItemData>(savedSlot.ItemPath);
            if (item != null)
                Inventory.AddItem(item, savedSlot.Quantity);
        }
    }

    // 장비 복원
    if (!string.IsNullOrEmpty(data.EquippedWeaponPath))
    {
        var weapon = GD.Load<ItemData>(data.EquippedWeaponPath);
        if (weapon != null)
        {
            Inventory.AddItem(weapon);
            Inventory.EquipItem(Inventory.Slots.Count - 1, this);
        }
    }
    if (!string.IsNullOrEmpty(data.EquippedArmorPath))
    {
        var armor = GD.Load<ItemData>(data.EquippedArmorPath);
        if (armor != null)
        {
            Inventory.AddItem(armor);
            Inventory.EquipItem(Inventory.Slots.Count - 1, this);
        }
    }

    SaveManager.PendingLoadData = null;
}
```

> **주의**: 장비 복원 시 `AddItem` + `EquipItem`을 **중괄호 블록** 안에 넣어야 함. C#에서 `if` 없이 두 줄이 붙으면 EquipItem이 항상 실행됨.

### 5-2. SaveData.cs 수정

인벤토리/장비 저장 필드 추가:

```csharp
using System.Collections.Generic;

namespace FirstGame.Data
{
    public class SavedItemSlot
    {
        public string ItemPath { get; set; }  // "res://Resources/Items/health_potion.tres"
        public int Quantity { get; set; }
    }

    public class SaveData
    {
        // ... 기존 필드 유지 ...
        public List<SavedItemSlot> InventoryItems { get; set; } = new();
        public string EquippedWeaponPath { get; set; }
        public string EquippedArmorPath { get; set; }
    }
}
```

### 5-3. SaveManager.cs 수정

`SaveGame`에서 인벤토리 데이터 수집 추가:

```csharp
// 기존 SaveData 생성 코드 아래에 추가:
data.InventoryItems = new List<SavedItemSlot>();
foreach (var slot in playerCtrl.Inventory.Slots)
{
    data.InventoryItems.Add(new SavedItemSlot
    {
        ItemPath = slot.Item.ResourcePath,
        Quantity = slot.Quantity
    });
}

if (playerCtrl.Inventory.EquippedWeapon != null)
    data.EquippedWeaponPath = playerCtrl.Inventory.EquippedWeapon.ResourcePath;
if (playerCtrl.Inventory.EquippedArmor != null)
    data.EquippedArmorPath = playerCtrl.Inventory.EquippedArmor.ResourcePath;
```

> `using System.Collections.Generic;` 추가, `using FirstGame.Data;` 이미 있음.

### 5-4. HUD.cs 수정

아이템 획득 알림 추가:

**추가할 필드:**
```csharp
private Label _itemPickupNotification;
```

**`_Ready`에 추가:**
```csharp
_itemPickupNotification = GetNode<Label>("%ItemPickupNotification");
_itemPickupNotification.Visible = false;

// 플레이어 인벤토리 이벤트 구독
if (_player != null)
    _player.Inventory.OnItemPickedUp += ShowItemPickup;
```

**`_ExitTree`에 추가:**
```csharp
if (_player != null && _player.Inventory != null)
    _player.Inventory.OnItemPickedUp -= ShowItemPickup;
```

**새 메서드:**
```csharp
private async void ShowItemPickup(ItemData item)
{
    _itemPickupNotification.Text = $"획득: {item.ItemName}";
    _itemPickupNotification.Visible = true;
    await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
    if (IsInstanceValid(this))
        _itemPickupNotification.Visible = false;
}
```

### 5-5. hud.tscn 수정

아이템 획득 알림 라벨 추가 (SaveNotification 아래):

```
[node name="ItemPickupNotification" type="Label" parent="."]
unique_name_in_owner = true
visible = false
anchors_preset = 7
anchor_left = 0.5
anchor_top = 1.0
anchor_right = 0.5
anchor_bottom = 1.0
offset_left = -80.0
offset_top = -80.0
offset_right = 80.0
offset_bottom = -50.0
grow_horizontal = 2
grow_vertical = 0
theme_override_font_sizes/font_size = 18
text = "획득: "
horizontal_alignment = 1
```

> SaveNotification 위에 배치 (offset_top: -80, SaveNotification은 -50).

### 5-6. project.godot 수정

`[input]` 섹션에 `inventory` 액션 추가 (I키, physical_keycode=73):

```
inventory={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":73,"key_label":0,"unicode":73,"location":0,"echo":false,"script":null)
]
}
```

### 5-7. main.tscn 수정

InventoryUI 인스턴스 추가 (HUD 아래):

```
Main (Node2D)
├── EnemySpawner
├── SavePoint
├── Player
├── HUD
└── InventoryUI ← inventory_ui.tscn 인스턴스
```

---

## 이벤트 흐름

```
[아이템 드롭]
Enemy.Die() → DropChance 확률 판정 → Player.Inventory.AddItem()
→ OnItemPickedUp → HUD "획득: Iron Sword" 표시
→ OnInventoryChanged → InventoryUI 그리드 갱신

[아이템 사용]
I키 → InventoryUI 열림 (게임 일시정지)
→ 슬롯 클릭 → 아이템 정보 표시
→ "사용" 버튼 → Inventory.UseItem()
  - Consumable: HP 회복 + 인벤토리에서 제거
  - Equipment: 장착 → Stats 보너스 적용

[장비 해제]
"해제" 버튼 → Inventory.UnequipWeapon/Armor()
→ Stats 보너스 제거 → 아이템 인벤토리로 복귀

[저장/불러오기]
SaveManager.SaveGame → 인벤토리 Slots + 장비 ResourcePath 저장
→ LoadGame → PendingLoadData에 포함 → _Ready에서 복원
```

## 구현 순서 (권장)

1. **ItemData.cs** — 기반 데이터 구조
2. **Inventory.cs** — 인벤토리 로직 (UI 없이 테스트 가능)
3. **아이템 .tres 파일 3개** — 테스트용 아이템 데이터
4. **PlayerController.cs 수정** — Inventory 인스턴스 보유
5. **EnemyStats.cs + EnemyController.cs 수정** — 적 드롭
6. **HUD.cs + hud.tscn 수정** — 획득 알림
7. **InventoryUI.cs + inventory_ui.tscn** — 그리드 UI
8. **SaveData.cs + SaveManager.cs 수정** — 인벤토리 저장/불러오기
9. **project.godot + main.tscn** — I키 입력 + InventoryUI 배치

## 주의사항

- `.tres` 파일의 `<ItemData.cs UID>`는 ItemData.cs 파일 생성 후 Godot이 자동 생성하는 `.uid` 파일의 값으로 대체할 것.
- `Inventory.cs`의 `UseItem`/`EquipItem`이 `PlayerController`를 참조하므로 **순환 참조 없음** (Data → Entities.Player 단방향).
- 장비 복원 시 `AddItem` + `EquipItem`을 **중괄호 블록 `{}`** 안에 넣어야 함 (C# if문 주의).
- `InventoryUI`의 `process_mode = 3 (Always)` 필수 — 인벤토리 열 때 게임 일시정지됨.
- 아이템 리소스(.tres)는 `Resources/Items/` 디렉토리에 생성. 디렉토리가 없으면 먼저 만들 것.

## 검증 방법

1. **아이템 드롭**: 적 처치 → 50% 확률로 "획득: Health Potion" 등 알림
2. **인벤토리 확인**: I키 → 그리드에 획득한 아이템 표시
3. **포션 사용**: 포션 선택 → "사용" → HP 회복, 인벤토리에서 제거
4. **장비 장착**: 무기/방어구 선택 → "장착" → 하단 장비란에 표시, 스탯 반영
5. **장비 해제**: "해제" 버튼 → 인벤토리로 복귀, 스탯 원복
6. **저장 연동**: 아이템 획득 후 적 처치(자동저장) → 사망 → Load Save → 아이템 유지
