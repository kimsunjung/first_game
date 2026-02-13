# 전투 루프 완성 계획 (적 공격 + 스포너 + 게임오버)

## Context
현재 플레이어가 적을 일방적으로 공격만 가능하고, 적은 쫓아오기만 할 뿐 공격하지 못함. 적이 1마리 고정이라 죽으면 할 일이 없음. 플레이어도 죽지 않음. 이 세 기능을 추가하면 **"적 스폰 → 전투 → 사망/처치 → 반복"** 의 핵심 게임 루프가 완성됨.

## 생성/수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scripts/Data/EnemyStats.cs` | **수정** | `AttackCooldown` 필드 추가 |
| `Scripts/Entities/Enemies/EnemyController.cs` | **수정** | 공격 로직 추가 |
| `Scripts/Entities/Enemies/EnemySpawner.cs` | **신규** | 적 스포너 |
| `Scripts/Entities/Player/PlayerController.cs` | **수정** | 사망 처리 추가 |
| `Scripts/Core/GameManager.cs` | **수정** | 씬 리로드 시 싱글톤 버그 수정 |
| `Scripts/UI/HUD.cs` | **수정** | 게임오버 패널 로직 추가 |
| `Scenes/UI/hud.tscn` | **수정** | 게임오버 패널 노드 추가, HealthBar show_percentage 수정 |
| `main.tscn` | **수정** | 수동 배치된 Enemy 제거, EnemySpawner 추가 |

---

## 1단계: 적 공격 기능

### 1-1. EnemyStats.cs 수정

`AttackCooldown` Export 필드 추가:

```csharp
[Export] public float AttackCooldown { get; set; } = 1.0f; // 공격 간격(초)
```

기존 필드(`DetectionRange`, `AttackRange`, `BaseDamage`)는 그대로 유지.

### 1-2. EnemyController.cs 수정

**추가할 필드:**
```csharp
private float _attackTimer = 0f;
```

**`_PhysicsProcess` 로직 변경** - 기존 if/else를 3단계로 재구성:

```csharp
public override void _PhysicsProcess(double delta)
{
    if (_target == null)
    {
        FindTarget();
        return;
    }

    Vector2 direction = GlobalPosition.DirectionTo(_target.GlobalPosition);
    float distance = GlobalPosition.DistanceTo(_target.GlobalPosition);

    // 쿨다운 타이머 감소
    _attackTimer -= (float)delta;

    if (distance <= Stats.AttackRange)
    {
        // 공격 범위 내: 정지 + 공격 시도
        Velocity = Vector2.Zero;
        TryAttack();
    }
    else if (distance <= Stats.DetectionRange)
    {
        // 감지 범위 내: 추격
        Velocity = direction * Stats.MoveSpeed;
    }
    else
    {
        // 범위 밖: 대기
        Velocity = Vector2.Zero;
    }

    MoveAndSlide();
}
```

**새 메서드 추가:**
```csharp
private void TryAttack()
{
    if (_attackTimer <= 0f && _target is IDamageable target)
    {
        target.TakeDamage(Stats.BaseDamage);
        _attackTimer = Stats.AttackCooldown;
        GD.Print($"Enemy attacked for {Stats.BaseDamage} damage!");
    }
}
```

> 참고: 기존 코드에서 `MoveAndSlide()`가 추격 분기 안에만 있었는데, 모든 경우에 호출되도록 if/else 밖으로 이동.

---

## 2단계: 적 스포너 시스템

### 2-1. EnemySpawner.cs 신규 (`Scripts/Entities/Enemies/EnemySpawner.cs`)

```csharp
using Godot;

namespace FirstGame.Entities.Enemies
{
    public partial class EnemySpawner : Node2D
    {
        [Export] public PackedScene EnemyScene { get; set; }
        [Export] public float SpawnInterval { get; set; } = 3.0f;
        [Export] public int MaxEnemies { get; set; } = 5;
        [Export] public float SpawnRadius { get; set; } = 300.0f;

        private float _spawnTimer = 0f;

        public override void _PhysicsProcess(double delta)
        {
            _spawnTimer -= (float)delta;

            if (_spawnTimer <= 0f)
            {
                _spawnTimer = SpawnInterval;
                TrySpawnEnemy();
            }
        }

        private void TrySpawnEnemy()
        {
            int currentCount = GetTree().GetNodesInGroup("Enemy").Count;
            if (currentCount >= MaxEnemies) return;

            var enemy = EnemyScene.Instantiate<Node2D>();
            // 스포너 위치 기준 랜덤 반경 내 배치
            var randomOffset = new Vector2(
                (float)GD.RandRange(-SpawnRadius, SpawnRadius),
                (float)GD.RandRange(-SpawnRadius, SpawnRadius)
            );
            enemy.GlobalPosition = GlobalPosition + randomOffset;
            GetParent().AddChild(enemy);
        }
    }
}
```

### 2-2. main.tscn 수정

- 기존 수동 배치된 `Enemy` 노드 **제거**
- `EnemySpawner` 노드 추가 (Player 앞에 배치)
- EnemySpawner의 `EnemyScene` 프로퍼티에 `res://Scenes/Characters/enemy.tscn` 할당

변경 후 씬 트리:
```
Main (Node2D)
├── EnemySpawner (Node2D) ← EnemySpawner.cs, position=(400, 300) 화면 중앙 근처
├── Player
└── HUD
```

> tscn 파일에서 EnemySpawner 노드 추가 시, `EnemyScene` 프로퍼티는 에디터 Inspector에서 드래그 할당하거나, tscn에 직접 작성:
> ```
> [node name="EnemySpawner" type="Node2D" parent="."]
> script = ExtResource("...")
> EnemyScene = ExtResource("enemy_scene_id")
> ```

---

## 3단계: 플레이어 사망 + 게임오버

### 3-0. GameManager.cs 수정 (필수 버그 수정)

현재 싱글톤 패턴에 문제가 있음. `GetTree().ReloadCurrentScene()` 호출 시:
1. 모든 노드가 해제되지만 `static Instance`는 해제된 객체를 계속 참조
2. 새 GameManager의 `_Ready`에서 `Instance == null`이 **false** (dangling reference)
3. 새 GameManager가 `QueueFree()`되어 버림

**수정: `_ExitTree`에서 Instance 정리 추가**

```csharp
public override void _ExitTree()
{
    if (Instance == this)
        Instance = null;
}
```

이 한 줄이 없으면 게임오버 후 재시작이 작동하지 않음.

### 3-1. PlayerController.cs 수정

`TakeDamage` 메서드에 사망 처리 추가:

```csharp
private bool _isDead = false;

public void TakeDamage(int damage)
{
    if (_isDead) return; // 이미 죽었으면 무시

    Stats.CurrentHealth -= damage;
    GD.Print($"Player took {damage} damage. HP: {Stats.CurrentHealth}/{Stats.MaxHealth}");

    if (Stats.CurrentHealth <= 0)
    {
        Die();
    }
}

private void Die()
{
    _isDead = true;
    GD.Print("Player Died!");
    EventManager.TriggerPlayerDeath(); // 이미 EventManager.cs에 구현되어 있음
    SetPhysicsProcess(false); // 이동/입력 비활성화
}
```

> `EventManager.OnPlayerDeath`와 `TriggerPlayerDeath()`는 이미 `Scripts/Core/EventManager.cs`에 존재함. 호출만 하면 됨.

### 3-2. HUD.cs 수정

**추가할 필드:**
```csharp
private Control _gameOverPanel;
private Button _restartButton;
```

**`_Ready`에 추가:**
```csharp
_gameOverPanel = GetNode<Control>("%GameOverPanel");
_restartButton = GetNode<Button>("%RestartButton");
_gameOverPanel.Visible = false;

_restartButton.Pressed += OnRestartPressed;
EventManager.OnPlayerDeath += ShowGameOver;
```

**`_ExitTree`에 추가:**
```csharp
EventManager.OnPlayerDeath -= ShowGameOver;
```

**새 메서드 추가:**
```csharp
private void ShowGameOver()
{
    _gameOverPanel.Visible = true;
    GetTree().Paused = true;
}

private void OnRestartPressed()
{
    GetTree().Paused = false;
    GetTree().ReloadCurrentScene();
}
```

### 3-3. hud.tscn 수정

**기존 수정:** HealthBar의 `show_percentage = true` → 제거 (또는 `false`로 변경). HealthLabel과 중복됨.

**GameOver 패널 추가** (MarginContainer와 같은 레벨, HUD 직속 자식):

```
HUD (CanvasLayer) ← ProcessMode = Always (일시정지 중에도 UI 작동)
├── MarginContainer (기존 HUD 요소들)
│   └── ...
└── GameOverPanel (PanelContainer) ← Unique Name, Visible=false, Full Rect, 반투명 배경
    └── CenterContainer (Full Rect)
        └── VBoxContainer
            ├── GameOverLabel (Label) ← text="GAME OVER", font_size=48, 가운데 정렬
            └── RestartButton (Button) ← Unique Name, text="다시 시작"
```

tscn으로 작성 시:
```
[node name="HUD" type="CanvasLayer"]
process_mode = 3

[node name="GameOverPanel" type="PanelContainer" parent="."]
unique_name_in_owner = true
visible = false
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2

[node name="CenterContainer" type="CenterContainer" parent="GameOverPanel"]
layout_mode = 2
size_flags_horizontal = 3
size_flags_vertical = 3

[node name="VBoxContainer" type="VBoxContainer" parent="GameOverPanel/CenterContainer"]
layout_mode = 2

[node name="GameOverLabel" type="Label" parent="GameOverPanel/CenterContainer/VBoxContainer"]
layout_mode = 2
theme_override_font_sizes/font_size = 48
text = "GAME OVER"
horizontal_alignment = 1

[node name="RestartButton" type="Button" parent="GameOverPanel/CenterContainer/VBoxContainer"]
unique_name_in_owner = true
layout_mode = 2
text = "다시 시작"
```

> `process_mode = 3`은 `ProcessModeEnum.Always`에 해당. 게임이 일시정지되어도 HUD가 동작해야 재시작 버튼을 누를 수 있음.

---

## 이벤트 흐름 (전체)

```
[적 스폰]
EnemySpawner._PhysicsProcess → 타이머 → TrySpawnEnemy → enemy.tscn 인스턴스 생성

[전투]
Enemy 추격 → 공격 범위 도달 → TryAttack → Player.TakeDamage
Player 공격 → Enemy.TakeDamage → HP 0 → Die → Gold 증가 → QueueFree

[플레이어 사망]
Player HP 0 → Die → EventManager.TriggerPlayerDeath
→ HUD.ShowGameOver → GameOverPanel 표시 + 게임 일시정지

[재시작]
RestartButton 클릭 → Unpause → ReloadCurrentScene
→ GameManager._ExitTree (Instance = null)
→ 새 씬 로드 → GameManager._Ready (Instance = this)
```

## 구현 순서 (권장)

1. **1단계 (적 공격)** 먼저 — 가장 독립적이고, 적이 공격하는지 바로 테스트 가능
2. **3단계 (사망/게임오버)** — 적 공격이 있어야 플레이어가 죽을 수 있으므로
3. **2단계 (스포너)** — 마지막에 추가해도 됨, 기존 수동 Enemy로 1~3단계 테스트 가능

## 검증 방법

1. **적 공격 확인**: 실행 → 적에게 가까이 가면 1초마다 체력 감소, 체력바 반영
2. **사망 확인**: 체력 0 → "GAME OVER" 패널 표시, 게임 정지
3. **재시작 확인**: "다시 시작" 클릭 → 씬 리로드, 체력/골드 초기화
4. **스포너 확인**: 3초마다 적 생성, 최대 5마리 제한
