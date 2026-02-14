# 버그 수정 (6건): 코드 리뷰 발견 사항

## Context
코드 리뷰에서 발견된 버그 6건을 수정한다. 적 이동속도 진동(유저 보고), AttackRange 섀도잉, 사망 후 공격 가능, 세이브 로드 예외, 장비 해제 피드백 없음, RemoveItem 검증 누락 등.

## 수정 파일 요약

| 파일 | 수정 내용 |
|------|-----------|
| `Scripts/Data/CharacterStats.cs` | AttackRange 프로퍼티 제거 |
| `Scripts/Data/PlayerStats.cs` | AttackRange 프로퍼티 추가 |
| `Scripts/Entities/Enemies/EnemyController.cs` | 추격 로직에 감속 구간 추가 |
| `Scripts/Entities/Player/PlayerController.cs` | GetInput()에 IsDead 체크 추가 |
| `Scripts/Core/SaveManager.cs` | LoadGame()에 File.Exists 체크 추가 |
| `Scripts/Data/Inventory.cs` | UnequipWeapon/Armor 실패 시 반환값 추가, RemoveItem amount 검증 |

---

## 수정 1: AttackRange 섀도잉 해소 [높음]

### 문제
`CharacterStats.cs`와 `EnemyStats.cs`에 동일 이름 `AttackRange`가 각각 정의됨.
- `CharacterStats.AttackRange` = 80.0f (기본값)
- `EnemyStats.AttackRange` = 70.0f (기본값)

C# property shadowing으로 Godot 직렬화/Duplicate() 시 어느 값이 적용될지 모호함.

### 수정 방법
CharacterStats에서 AttackRange를 제거하고, PlayerStats와 EnemyStats에서 각각 독립적으로 정의.

### `Scripts/Data/CharacterStats.cs` (13행 제거)

```csharp
// 변경 전
[Export] public float MoveSpeed { get; set; } = 300.0f;
[Export] public float AttackRange { get; set; } = 80.0f; // ← 이 줄 제거
[Export] public int MaxHealth { get; set; } = 100;

// 변경 후
[Export] public float MoveSpeed { get; set; } = 300.0f;
[Export] public int MaxHealth { get; set; } = 100;
```

### `Scripts/Data/PlayerStats.cs` (AttackRange 추가)

```csharp
// 변경 전
[Export] public int BaseDamage { get; set; } = 10;

// 변경 후
[Export] public int BaseDamage { get; set; } = 10;
[Export] public float AttackRange { get; set; } = 80.0f;
```

### `Scripts/Data/EnemyStats.cs` (변경 없음)
이미 `[Export] public float AttackRange { get; set; } = 70.0f;` 정의되어 있으므로 그대로 유지.

---

## 수정 2: 적 추격 로직 감속 구간 추가 [높음]

### 문제
현재 로직:
```
거리 <= AttackRange(70) → Velocity = Zero (정지)
거리 > AttackRange      → Velocity = direction * MoveSpeed (풀스피드)
```

플레이어와 붙어있을 때 매 프레임 "정지 ↔ 풀스피드"가 반복되어 빠르게 움직이는 것처럼 보임.

### 수정 방법
AttackRange 바로 바깥에 감속 구간(30px)을 추가. 거리에 비례하여 속도를 줄여서 부드럽게 접근.

### `Scripts/Entities/Enemies/EnemyController.cs` (_PhysicsProcess 수정)

```csharp
public override void _PhysicsProcess(double delta)
{
    if (!IsInstanceValid(_target))
    {
        FindTarget();
        return;
    }

    // 쿨타임 감소 (Decrease Cooldown)
    _attackTimer -= (float)delta;

    // 타겟 방향 및 거리 계산 (Calculate Direction and Distance)
    Vector2 direction = GlobalPosition.DirectionTo(_target.GlobalPosition);
    float distance = GlobalPosition.DistanceTo(_target.GlobalPosition);

    float stopBuffer = 30.0f; // 감속 구간 크기

    if (distance <= Stats.AttackRange)
    {
        // 공격 사거리 내: 정지 및 공격 (In Attack Range: Stop and Attack)
        Velocity = Vector2.Zero;
        TryAttack();
    }
    else if (distance <= Stats.AttackRange + stopBuffer)
    {
        // 감속 구간: 거리에 비례하여 천천히 접근 (Deceleration Zone)
        float ratio = (distance - Stats.AttackRange) / stopBuffer; // 0.0 ~ 1.0
        Velocity = direction * Stats.MoveSpeed * ratio;
    }
    else if (distance <= Stats.DetectionRange)
    {
        // 추적 사거리 내: 풀스피드로 이동 (In Chase Range: Full speed)
        Velocity = direction * Stats.MoveSpeed;
    }
    else
    {
        // 사거리 밖: 정지 (Out of range: Stop)
        Velocity = Vector2.Zero;
    }

    MoveAndSlide();
}
```

### 동작 도식

```
|--- AttackRange(70) ---|--- StopBuffer(30) ---|--- DetectionRange(200) ---|
        정지 + 공격         감속 접근 (0~100%)       풀스피드 추격
```

---

## 수정 3: 사망 후 공격 가능 [높음]

### 문제
`PlayerController.GetInput()`에 IsDead 체크가 없음. Die()에서 SetPhysicsProcess(false)를 호출하지만, 같은 프레임 내에서 공격 입력이 처리될 수 있음.

### `Scripts/Entities/Player/PlayerController.cs` (GetInput 수정)

```csharp
// 변경 전
private void GetInput()
{
    Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
    Velocity = inputDir * Stats.MoveSpeed;

    if (Input.IsActionJustPressed("attack"))
    {
        Attack();
    }
}

// 변경 후
private void GetInput()
{
    if (IsDead) return;

    Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
    Velocity = inputDir * Stats.MoveSpeed;

    if (Input.IsActionJustPressed("attack"))
    {
        Attack();
    }
}
```

---

## 수정 4: LoadGame 파일 없을 때 예외 [높음]

### 문제
`SaveManager.LoadGame()`에서 slot을 직접 지정하여 호출할 경우, 해당 파일이 없으면 `File.ReadAllText()`에서 `FileNotFoundException` 발생. slot이 null일 때는 `HasSave()` 체크가 있지만, 직접 slot을 지정하면 체크 없이 바로 읽기 시도.

### `Scripts/Core/SaveManager.cs` (LoadGame 수정, 93행 앞에 추가)

```csharp
// 변경 전
string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
string json = File.ReadAllText(path);
PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);

// 변경 후
string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
if (!File.Exists(path))
{
    GD.PrintErr($"저장 파일을 찾을 수 없습니다: {path}");
    return;
}
string json = File.ReadAllText(path);
PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);
```

---

## 수정 5: 장비 해제 실패 시 반환값 추가 [중간]

### 문제
가방이 꽉 차서 장비 해제가 실패해도 아무 피드백 없이 무시됨. UI에서 해제 버튼을 눌러도 아무 반응이 없어 유저가 혼란.

### `Scripts/Data/Inventory.cs` (UnequipWeapon/UnequipArmor 반환값 변경)

```csharp
// 변경 전
public void UnequipWeapon(PlayerController player)
{
    if (EquippedWeapon == null) return;
    if (Slots.Count >= MaxSlots) return;
    ...
}

public void UnequipArmor(PlayerController player)
{
    if (EquippedArmor == null) return;
    if (Slots.Count >= MaxSlots) return;
    ...
}

// 변경 후
public bool UnequipWeapon(PlayerController player)
{
    if (EquippedWeapon == null) return false;
    if (Slots.Count >= MaxSlots)
    {
        GD.Print("가방이 꽉 차서 장비를 해제할 수 없습니다! (Inventory full, cannot unequip!)");
        return false;
    }

    player.Stats.BaseDamage -= EquippedWeapon.BonusDamage;
    AddItem(EquippedWeapon, 1);
    EquippedWeapon = null;
    OnEquipmentChanged?.Invoke();
    return true;
}

public bool UnequipArmor(PlayerController player)
{
    if (EquippedArmor == null) return false;
    if (Slots.Count >= MaxSlots)
    {
        GD.Print("가방이 꽉 차서 장비를 해제할 수 없습니다! (Inventory full, cannot unequip!)");
        return false;
    }

    player.Stats.MaxHealth -= EquippedArmor.BonusMaxHealth;
    if (player.Stats.CurrentHealth > player.Stats.MaxHealth)
        player.Stats.CurrentHealth = player.Stats.MaxHealth;
    AddItem(EquippedArmor, 1);
    EquippedArmor = null;
    OnEquipmentChanged?.Invoke();
    return true;
}
```

EquipItem에서 기존 장비 교체 시에도 반환값을 확인하도록 수정:

```csharp
// EquipItem 내부 (기존 장비 교체 부분)

// 변경 전
if (EquippedWeapon != null)
    UnequipWeapon(player);

// 변경 후
if (EquippedWeapon != null)
{
    if (!UnequipWeapon(player)) return; // 해제 실패 시 장착도 중단
}
```

방어구도 동일하게:
```csharp
// 변경 전
if (EquippedArmor != null)
    UnequipArmor(player);

// 변경 후
if (EquippedArmor != null)
{
    if (!UnequipArmor(player)) return; // 해제 실패 시 장착도 중단
}
```

---

## 수정 6: RemoveItem amount 검증 [중간]

### 문제
`Inventory.RemoveItem()`에서 amount가 현재 Quantity보다 클 경우 음수가 됨. 현재는 `<= 0` 체크로 슬롯이 제거되어 큰 문제는 아니지만 방어적 코드 필요.

### `Scripts/Data/Inventory.cs` (RemoveItem 수정)

```csharp
// 변경 전
public void RemoveItem(int slotIndex, int amount = 1)
{
    if (slotIndex < 0 || slotIndex >= Slots.Count) return;

    Slots[slotIndex].Quantity -= amount;
    if (Slots[slotIndex].Quantity <= 0)
        Slots.RemoveAt(slotIndex);

    OnInventoryChanged?.Invoke();
}

// 변경 후
public void RemoveItem(int slotIndex, int amount = 1)
{
    if (slotIndex < 0 || slotIndex >= Slots.Count) return;

    Slots[slotIndex].Quantity = Math.Max(0, Slots[slotIndex].Quantity - amount);
    if (Slots[slotIndex].Quantity <= 0)
        Slots.RemoveAt(slotIndex);

    OnInventoryChanged?.Invoke();
}
```

---

## 검증 방법

| # | 항목 | 검증 |
|---|------|------|
| 1 | AttackRange 섀도잉 | 에디터에서 PlayerStats, EnemyStats의 AttackRange가 독립적으로 표시/설정되는지 확인 |
| 2 | 적 감속 접근 | 플레이어 옆에서 적이 떨림 없이 부드럽게 정지하는지 확인 |
| 3 | 사망 후 공격 | 사망 직후 Space 연타 → 공격 로그가 안 뜨는지 확인 |
| 4 | LoadGame 예외 | 저장 파일 삭제 후 Load Save 버튼 → 크래시 없이 에러 로그만 출력되는지 확인 |
| 5 | 장비 해제 피드백 | 가방 20칸 꽉 채우기 → 장비 해제 시도 → "가방이 꽉 찼습니다" 로그 출력 확인 |
| 6 | RemoveItem 검증 | 정상적인 아이템 사용/제거가 기존과 동일하게 동작하는지 확인 |
