# 세이브/로드 시스템 구현 계획

> 상태 메모 (2026-04-15)
> 이 문서는 구현 당시의 계획서다. 현재 프로젝트는 `main.tscn` 단일 구조가 아니라 맵 씬이 `Scenes/Maps/*` 아래로 분리되어 있다.
> 문서 안의 `main.tscn` 참조는 현재의 `town.tscn`, `field_1.tscn` 등으로 읽어야 한다.

## Context
현재 플레이어가 죽으면 `ReloadCurrentScene`으로 모든 상태가 초기화됨. 성장형 게임을 위해 플레이어 상태(HP, 골드, 위치)를 저장하고, 사망 시 마지막 저장 시점으로 복귀하는 시스템이 필요함. 자동 저장(적 처치 시)과 수동 저장(세이브 포인트)을 모두 지원.

## 생성/수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scripts/Data/SaveData.cs` | **신규** | 저장 데이터 클래스 |
| `Scripts/Core/SaveManager.cs` | **신규** | 저장/불러오기 로직 (static) |
| `Scripts/Entities/SavePoint/SavePoint.cs` | **신규** | 세이브 포인트 상호작용 |
| `Scenes/Objects/save_point.tscn` | **신규** | 세이브 포인트 씬 |
| `Scripts/Entities/Player/PlayerController.cs` | **수정** | 로드 데이터 적용, 초기 자동저장 |
| `Scripts/Entities/Enemies/EnemyController.cs` | **수정** | 적 처치 시 자동저장 호출 |
| `Scripts/UI/HUD.cs` | **수정** | 사망 시 "불러오기" 동작, 저장 알림 |
| `Scenes/UI/hud.tscn` | **수정** | 저장 알림 라벨 추가, 버튼 텍스트 변경 |
| `main.tscn` | **수정** | 세이브 포인트 배치 |
| `project.godot` | **수정** | "interact" 입력 액션 추가 (E키) |

---

## 1단계: 저장 데이터 구조

### 1-1. SaveData.cs 신규 (`Scripts/Data/SaveData.cs`)

```csharp
namespace FirstGame.Data
{
    public class SaveData
    {
        public float PlayerPosX { get; set; }
        public float PlayerPosY { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerMaxHealth { get; set; }
        public int PlayerGold { get; set; }
        public string Timestamp { get; set; }
    }
}
```

순수 C# 클래스. Godot 의존성 없음. `System.Text.Json`으로 직렬화됨 (.NET 8 내장).

---

## 2단계: SaveManager 구현

### 2-1. SaveManager.cs 신규 (`Scripts/Core/SaveManager.cs`)

```csharp
using Godot;
using System;
using System.IO;
using System.Text.Json;
using FirstGame.Data;
using FirstGame.Entities.Player;

namespace FirstGame.Core
{
    public static class SaveManager
    {
        private const string SaveDir = "user://saves/";
        private const string AutoSaveSlot = "autosave";
        private const string ManualSaveSlot = "manual";

        // 씬 리로드 후 적용할 데이터 (static이라 씬 전환에도 유지됨)
        public static SaveData PendingLoadData { get; set; } = null;

        // HUD에서 구독하여 "Saved!" 알림 표시
        public static event Action OnGameSaved;

        public static void SaveGame(string slot = AutoSaveSlot)
        {
            var tree = (SceneTree)Engine.GetMainLoop();
            var players = tree.GetNodesInGroup("Player");
            if (players.Count == 0) return;

            var player = players[0] as Node2D;
            var playerCtrl = players[0] as PlayerController;
            if (playerCtrl == null) return;

            var data = new SaveData
            {
                PlayerPosX = player.GlobalPosition.X,
                PlayerPosY = player.GlobalPosition.Y,
                PlayerHealth = playerCtrl.Stats.CurrentHealth,
                PlayerMaxHealth = playerCtrl.Stats.MaxHealth,
                PlayerGold = GameManager.Instance.PlayerGold,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 디렉토리 생성
            DirAccess.MakeDirRecursiveAbsolute(
                ProjectSettings.GlobalizePath(SaveDir)
            );

            // JSON 파일 저장
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);

            OnGameSaved?.Invoke();
            GD.Print($"Game saved to slot: {slot}");
        }

        public static void LoadGame(string slot = null)
        {
            // slot이 null이면 최신 저장 파일을 찾음 (수동 > 자동 우선)
            if (slot == null)
            {
                if (HasSave(ManualSaveSlot)) slot = ManualSaveSlot;
                else if (HasSave(AutoSaveSlot)) slot = AutoSaveSlot;
                else
                {
                    GD.Print("No save file found. Restarting fresh.");
                    var t = (SceneTree)Engine.GetMainLoop();
                    t.Paused = false;
                    t.ReloadCurrentScene();
                    return;
                }
            }

            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            string json = File.ReadAllText(path);
            PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);

            var tree = (SceneTree)Engine.GetMainLoop();
            tree.Paused = false;
            tree.ReloadCurrentScene();
        }

        public static bool HasSave(string slot = AutoSaveSlot)
        {
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            return File.Exists(path);
        }
    }
}
```

**핵심 포인트:**
- `static` 클래스이므로 Autoload 등록 불필요. `static` 필드는 씬 리로드에도 유지됨
- `PendingLoadData`: LoadGame에서 데이터를 저장하고 씬 리로드 → _Ready에서 적용 후 null로 초기화
- `LoadGame(null)`: 수동 저장 → 자동 저장 → 저장 없음 순서로 최신 파일 탐색
- 저장 경로: `user://saves/autosave.json`, `user://saves/manual.json`

---

## 3단계: 세이브 포인트

### 3-1. SavePoint.cs 신규 (`Scripts/Entities/SavePoint/SavePoint.cs`)

```csharp
using Godot;
using FirstGame.Core;

namespace FirstGame.Entities
{
    public partial class SavePoint : Area2D
    {
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
                SaveManager.SaveGame("manual");
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

### 3-2. save_point.tscn 신규 (`Scenes/Objects/save_point.tscn`)

```
SavePoint (Area2D) ← SavePoint.cs, collision_layer/mask 설정
├── CollisionShape2D (CircleShape2D, radius=50)
├── SaveIcon (Label) ← text="[SAVE]", font_size=20, 가운데 정렬
└── PromptLabel (Label) ← text="Press E to Save", font_size=14, position.y=40, Visible=false
```

> Area2D의 collision은 Player의 CharacterBody2D를 감지해야 하므로, `collision_mask`에 Player의 레이어 포함 필요. 기본값(레이어 1)이면 별도 설정 불필요.

> `monitoring = true` (기본값)로 BodyEntered/BodyExited 시그널이 작동함.

---

## 4단계: 기존 파일 수정

### 4-1. PlayerController.cs 수정

**`_Ready` 메서드 변경** - 로드 데이터 적용 + 초기 자동저장:

```csharp
public override void _Ready()
{
    if (Stats == null)
    {
        Stats = new PlayerStats();
    }

    if (SaveManager.PendingLoadData != null)
    {
        // 저장 데이터 적용
        var data = SaveManager.PendingLoadData;
        GlobalPosition = new Vector2(data.PlayerPosX, data.PlayerPosY);
        Stats.MaxHealth = data.PlayerMaxHealth;    // MaxHealth 먼저 설정
        Stats.CurrentHealth = data.PlayerHealth;   // 그 다음 CurrentHealth (Clamp 때문)
        GameManager.Instance.PlayerGold = data.PlayerGold; // 골드 복원
        SaveManager.PendingLoadData = null;        // 적용 후 초기화
    }
    else if (!SaveManager.HasSave())
    {
        // 최초 실행 시 초기 자동저장 생성
        SaveManager.SaveGame();
    }

    _isDead = false; // 부활 시 초기화
    GD.Print("Player Initialized");
}
```

> **중요**: `MaxHealth`를 `CurrentHealth`보다 먼저 설정해야 함. `CharacterStats.CurrentHealth` setter가 `Mathf.Clamp(value, 0, MaxHealth)`를 사용하기 때문.

> **`_isDead = false`**: 씬 리로드 후 새 인스턴스이므로 기본값이 false지만, 명시적으로 초기화하여 안전성 확보.

### 4-2. EnemyController.cs 수정

`Die()` 메서드에 자동저장 추가:

```csharp
private void Die()
{
    GD.Print("Enemy Died!");
    GameManager.Instance.PlayerGold += 10;
    SaveManager.SaveGame(); // 적 처치 후 자동저장
    QueueFree();
}
```

> `using FirstGame.Core;`는 이미 있으므로 추가 using 불필요.

### 4-3. HUD.cs 수정

**추가할 필드:**
```csharp
private Label _saveNotification;
```

**`_Ready`에 추가:**
```csharp
_saveNotification = GetNode<Label>("%SaveNotification");
_saveNotification.Visible = false;

SaveManager.OnGameSaved += ShowSaveNotification;
```

**`_ExitTree`에 추가:**
```csharp
SaveManager.OnGameSaved -= ShowSaveNotification;
```

**`OnRestartPressed` 변경:**
```csharp
private void OnRestartPressed()
{
    SaveManager.LoadGame(); // ReloadCurrentScene 대신 저장 데이터 불러오기
}
```

> `SaveManager.LoadGame()`가 내부에서 `tree.Paused = false` + `ReloadCurrentScene()`를 처리하므로, 기존의 Unpause + Reload 코드를 대체.

> 저장 파일이 없으면 `LoadGame` 내부에서 자동으로 `ReloadCurrentScene` (새 게임 시작).

**새 메서드 추가:**
```csharp
private async void ShowSaveNotification()
{
    _saveNotification.Visible = true;
    await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
    if (IsInstanceValid(this))
        _saveNotification.Visible = false;
}
```

### 4-4. hud.tscn 수정

**SaveNotification 라벨 추가** (MarginContainer 내부 하단 또는 상단):

```
MarginContainer
└── VBoxContainer
    ├── HBoxContainer (기존 체력바 + 골드)
    │   └── ...
    └── Control (스페이서, v_size_flags: Expand+Fill)

(HUD 직속 자식으로 추가 - 화면 하단 중앙)
SaveNotification (Label) ← Unique Name, Visible=false
```

tscn 노드 추가:
```
[node name="SaveNotification" type="Label" parent="."]
unique_name_in_owner = true
visible = false
anchors_preset = 7
anchor_left = 0.5
anchor_top = 1.0
anchor_right = 0.5
anchor_bottom = 1.0
offset_left = -50.0
offset_top = -50.0
offset_right = 50.0
grow_horizontal = 2
grow_vertical = 0
theme_override_font_sizes/font_size = 20
text = "Saved!"
horizontal_alignment = 1
```

**RestartButton 텍스트 변경:**
```
기존: text = "Restart Game"
변경: text = "Load Save"
```

### 4-5. project.godot 수정

`[input]` 섹션에 `interact` 액션 추가 (E키, physical_keycode=69):

```
interact={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":69,"key_label":0,"unicode":69,"location":0,"echo":false,"script":null)
]
}
```

### 4-6. main.tscn 수정

세이브 포인트 배치:

```
Main (Node2D)
├── EnemySpawner
├── SavePoint ← save_point.tscn 인스턴스, position=(500, 200) 등 적절한 위치
├── Player
└── HUD
```

---

## 데이터 흐름

```
[자동 저장]
적 처치 → EnemyController.Die() → SaveManager.SaveGame("autosave")
→ Player 위치/HP/Gold 수집 → JSON 파일 저장 → OnGameSaved → HUD "Saved!" 표시

[수동 저장]
Player가 SavePoint 범위 진입 → "Press E to Save" 표시
→ E키 입력 → SaveManager.SaveGame("manual") → JSON 파일 저장

[사망 → 불러오기]
Player HP 0 → Die → GameOver 패널 표시
→ "Load Save" 클릭 → SaveManager.LoadGame()
→ manual.json or autosave.json 로드 → PendingLoadData에 저장
→ ReloadCurrentScene → PlayerController._Ready
→ PendingLoadData 적용 (위치, HP, Gold 복원) → PendingLoadData = null

[최초 실행]
PlayerController._Ready → 저장 파일 없음 → SaveManager.SaveGame() → 초기 상태 저장
```

## 구현 순서 (권장)

1. **SaveData.cs + SaveManager.cs** — 핵심 인프라, 다른 모든 것이 이에 의존
2. **PlayerController.cs 수정** — 로드 데이터 적용 로직
3. **EnemyController.cs 수정** — 자동저장 호출 (1줄 추가)
4. **HUD.cs + hud.tscn 수정** — 불러오기 동작 변경 + 알림
5. **SavePoint.cs + save_point.tscn** — 수동 저장
6. **project.godot + main.tscn** — interact 입력 + 세이브 포인트 배치

## 검증 방법

1. **최초 실행**: 게임 시작 → `user://saves/autosave.json` 파일 생성 확인
2. **자동 저장**: 적 처치 → "Saved!" 알림 + JSON 파일 갱신 확인
3. **수동 저장**: 세이브 포인트 접근 → "Press E to Save" 표시 → E키 → `manual.json` 생성
4. **사망 후 불러오기**: 죽기 전 적 처치(골드 획득) → 사망 → "Load Save" 클릭 → 골드/위치 유지 확인
5. **저장 없이 사망**: autosave.json 삭제 후 실행 → 사망 → 새 게임으로 시작 확인
