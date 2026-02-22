# 사운드 시스템 구현 계획

## Context
현재 시각적 피드백(체력바, 플래시, HUD 알림)은 갖춰져 있지만 청각 피드백이 전혀 없음. AudioManager 싱글톤을 추가하여 전투, UI, 환경 사운드를 재생. 기존 코드에 최소한의 변경(`AudioManager.Instance?.PlaySFX()` 호출 1줄 추가)으로 구현.

## 사운드 파일 준비
사운드 파일(.wav 또는 .ogg)은 별도로 준비해야 함. 무료 사운드: [freesound.org](https://freesound.org), [opengameart.org](https://opengameart.org) 등에서 다운로드.

---

## 생성/수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scripts/Core/AudioManager.cs` | **신규** | 오디오 싱글톤 (SFX 풀 + BGM) |
| `Resources/Audio/SFX/` | **신규** | 효과음 파일 폴더 |
| `Resources/Audio/BGM/` | **신규** | 배경음 파일 폴더 |
| `project.godot` | **수정** | AudioManager autoload 등록 |
| `Scripts/Entities/Player/PlayerController.cs` | **수정** | 공격/피격/사망 사운드 호출 추가 |
| `Scripts/Entities/Enemies/EnemyController.cs` | **수정** | 공격/피격/사망 사운드 호출 추가 |
| `Scripts/Data/Inventory.cs` | **수정** | 아이템 사용/장착 사운드 호출 추가 |
| `Scripts/UI/ShopUI.cs` | **수정** | 구매/판매/실패 사운드 호출 추가 |
| `Scripts/Entities/SavePoint/SavePoint.cs` | **수정** | 저장 사운드 호출 추가 |
| `Scripts/UI/HUD.cs` | **수정** | 게임오버 사운드 호출 추가 |

---

## 1단계: AudioManager 싱글톤 구현

### `Scripts/Core/AudioManager.cs` (신규 생성)

GameManager와 동일한 싱글톤 패턴. SFX는 AudioStreamPlayer 풀(동시 재생 지원), BGM은 별도 AudioStreamPlayer 1개.

```csharp
using Godot;
using System.Collections.Generic;

namespace FirstGame.Core
{
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        private const int SfxPoolSize = 8; // 동시 재생 가능한 효과음 수
        private AudioStreamPlayer _bgmPlayer;
        private List<AudioStreamPlayer> _sfxPool = new();
        private Dictionary<string, AudioStream> _cache = new();

        // 볼륨 설정 (0.0 ~ 1.0)
        [Export] public float SfxVolume { get; set; } = 0.8f;
        [Export] public float BgmVolume { get; set; } = 0.5f;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
                return;
            }

            // BGM 플레이어 생성
            _bgmPlayer = new AudioStreamPlayer();
            _bgmPlayer.Bus = "Master";
            AddChild(_bgmPlayer);

            // SFX 풀 생성
            for (int i = 0; i < SfxPoolSize; i++)
            {
                var player = new AudioStreamPlayer();
                player.Bus = "Master";
                AddChild(player);
                _sfxPool.Add(player);
            }
        }

        /// <summary>
        /// 효과음 재생. path는 "res://Resources/Audio/SFX/" 기준 파일명 (확장자 포함)
        /// 예: PlaySFX("player_attack.wav")
        /// </summary>
        public void PlaySFX(string fileName)
        {
            string path = $"res://Resources/Audio/SFX/{fileName}";
            var stream = LoadAudio(path);
            if (stream == null) return;

            // 유휴 플레이어 찾기
            foreach (var player in _sfxPool)
            {
                if (!player.Playing)
                {
                    player.Stream = stream;
                    player.VolumeDb = Mathf.LinearToDb(SfxVolume);
                    player.Play();
                    return;
                }
            }

            // 모든 플레이어가 사용 중이면 첫 번째 것을 재사용
            _sfxPool[0].Stream = stream;
            _sfxPool[0].VolumeDb = Mathf.LinearToDb(SfxVolume);
            _sfxPool[0].Play();
        }

        /// <summary>
        /// 배경음 재생. 기존 BGM은 중지됨.
        /// 예: PlayBGM("field_theme.ogg")
        /// </summary>
        public void PlayBGM(string fileName)
        {
            string path = $"res://Resources/Audio/BGM/{fileName}";
            var stream = LoadAudio(path);
            if (stream == null) return;

            _bgmPlayer.Stream = stream;
            _bgmPlayer.VolumeDb = Mathf.LinearToDb(BgmVolume);
            _bgmPlayer.Play();
        }

        public void StopBGM()
        {
            _bgmPlayer.Stop();
        }

        private AudioStream LoadAudio(string path)
        {
            if (_cache.TryGetValue(path, out var cached))
                return cached;

            if (!ResourceLoader.Exists(path))
            {
                GD.PrintErr($"AudioManager: 오디오 파일 없음 - {path}");
                return null;
            }

            var stream = GD.Load<AudioStream>(path);
            _cache[path] = stream;
            return stream;
        }

        public override void _ExitTree()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
```

### `project.godot` autoload 섹션 수정

기존:
```ini
[autoload]
GameManager="*res://Scripts/Core/GameManager.cs"
```

변경:
```ini
[autoload]
GameManager="*res://Scripts/Core/GameManager.cs"
AudioManager="*res://Scripts/Core/AudioManager.cs"
```

---

## 2단계: 사운드 파일 폴더 구조

빈 폴더 생성 (`.gdkeep` 파일로 Git 추적):

```
Resources/Audio/
├── SFX/
│   ├── player_attack.wav      ← 플레이어 공격 (칼 휘두르기)
│   ├── player_hit.wav         ← 플레이어 피격
│   ├── player_death.wav       ← 플레이어 사망
│   ├── enemy_hit.wav          ← 적 피격
│   ├── enemy_death.wav        ← 적 사망
│   ├── potion_use.wav         ← 포션 사용
│   ├── equip.wav              ← 장비 장착
│   ├── shop_buy.wav           ← 상점 구매
│   ├── shop_sell.wav          ← 상점 판매
│   ├── shop_fail.wav          ← 구매/판매 실패
│   ├── save.wav               ← 게임 저장
│   └── game_over.wav          ← 게임 오버
└── BGM/
    └── field_theme.ogg        ← 필드 배경음 (선택사항)
```

사운드 파일이 없으면 `PlaySFX`가 에러 로그만 출력하고 크래시하지 않음 (`ResourceLoader.Exists` 체크).

---

## 3단계: 기존 코드에 사운드 호출 추가

각 파일에 `AudioManager.Instance?.PlaySFX()` 한 줄씩 추가. **기존 로직 변경 없음.**

### 3-1. `Scripts/Entities/Player/PlayerController.cs`

`using FirstGame.Core;`는 이미 있음 (line 4).

**Attack() 메서드 (line 146~168)** — 첫 줄에 추가:
```csharp
private void Attack()
{
    AudioManager.Instance?.PlaySFX("player_attack.wav");  // ← 추가
    GD.Print("플레이어 공격! (Player Attacked!)");
    // ... 기존 코드 그대로
}
```

**TakeDamage() 메서드 (line 17~28)** — `Stats.CurrentHealth -= damage;` 다음 줄에 추가:
```csharp
public void TakeDamage(int damage)
{
    if (IsDead) return;
    Stats.CurrentHealth -= damage;
    AudioManager.Instance?.PlaySFX("player_hit.wav");  // ← 추가
    GD.Print($"Player took {damage} damage. HP: {Stats.CurrentHealth}/{Stats.MaxHealth}");
    // ... 기존 코드 그대로
}
```

**Die() 메서드 (line 30~36)** — `IsDead = true;` 다음 줄에 추가:
```csharp
private void Die()
{
    IsDead = true;
    AudioManager.Instance?.PlaySFX("player_death.wav");  // ← 추가
    GD.Print("플레이어 사망! (Player Died!)");
    // ... 기존 코드 그대로
}
```

### 3-2. `Scripts/Entities/Enemies/EnemyController.cs`

`using FirstGame.Core;`는 이미 있음 (line 3).

**TryAttack() 메서드 (line 113~122)** — `target.TakeDamage` 다음 줄에 추가:
```csharp
private void TryAttack()
{
    if (_attackTimer <= 0f && IsInstanceValid(_target) && _target is IDamageable target)
    {
        target.TakeDamage(Stats.BaseDamage);
        _attackTimer = Stats.AttackCooldown;
        AudioManager.Instance?.PlaySFX("enemy_attack.wav");  // ← 추가
        GD.Print($"[Enemy] 공격 수행! 데미지: {Stats.BaseDamage}");
    }
}
```

**TakeDamage() 메서드 (line 135~157)** — `Stats.CurrentHealth -= damage;` 다음 줄에 추가:
```csharp
public void TakeDamage(int damage)
{
    Stats.CurrentHealth -= damage;
    AudioManager.Instance?.PlaySFX("enemy_hit.wav");  // ← 추가
    _healthBar.Value = Stats.CurrentHealth;
    // ... 기존 코드 그대로
}
```

**Die() 메서드 (line 159~184)** — 첫 줄에 추가:
```csharp
private void Die()
{
    AudioManager.Instance?.PlaySFX("enemy_death.wav");  // ← 추가
    GD.Print("적 사망! (Enemy Died!)");
    // ... 기존 코드 그대로
}
```

### 3-3. `Scripts/Data/Inventory.cs`

**`using FirstGame.Core;` 추가 필요** (line 4 뒤에):
```csharp
using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core;          // ← 추가
using FirstGame.Entities.Player;
```

Inventory.cs는 일반 클래스(Node가 아님)이지만 `AudioManager.Instance`는 static이므로 접근 가능.

**UseItem() 메서드 (line 75~91)** — 포션 사용 시 GD.Print 다음에 추가:
```csharp
if (slot.Item.Type == ItemType.Consumable)
{
    player.Stats.CurrentHealth += slot.Item.HealAmount;
    GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount}");
    AudioManager.Instance?.PlaySFX("potion_use.wav");  // ← 추가
    RemoveItem(slotIndex, 1);
}
```

**EquipItem() 메서드 (line 95~128)** — `OnEquipmentChanged?.Invoke();` 다음 줄에 추가:
```csharp
OnEquipmentChanged?.Invoke();
AudioManager.Instance?.PlaySFX("equip.wav");  // ← 추가
GD.Print($"{item.ItemName} 장착! (Equipped {item.ItemName})");
```

### 3-4. `Scripts/UI/ShopUI.cs`

`using FirstGame.Core;`는 이미 있음 (line 3).

**TryBuyItem() 메서드 (line 158~182)** — 3곳에 추가:

골드 부족 시 (line 165):
```csharp
if (GameManager.Instance.PlayerGold < totalCost)
{
    ShowMessage("골드가 부족합니다! (Not enough gold!)");
    AudioManager.Instance?.PlaySFX("shop_fail.wav");  // ← 추가
    return;
}
```

인벤토리 꽉 참 (line 171):
```csharp
if (!canAdd)
{
    ShowMessage("가방이 꽉 찼습니다! (Inventory full!)");
    AudioManager.Instance?.PlaySFX("shop_fail.wav");  // ← 추가
    return;
}
```

구매 성공 (line 178):
```csharp
GameManager.Instance.PlayerGold -= totalCost;
AudioManager.Instance?.PlaySFX("shop_buy.wav");  // ← 추가
ShowMessage($"{item.ItemName} x{quantity} 구매! (-{totalCost}G)");
```

**TrySellItem() 메서드 (line 207~228)** — 판매 성공 시 추가:
```csharp
GameManager.Instance.PlayerGold += sellPrice;
AudioManager.Instance?.PlaySFX("shop_sell.wav");  // ← 추가
_inventory.RemoveItem(slotIndex, 1);
```

### 3-5. `Scripts/Entities/SavePoint/SavePoint.cs`

`using FirstGame.Core;`는 이미 있음 (line 2).

**_Process() 메서드 (line 20~28)** — `SaveManager.SaveGame("manual");` 다음 줄에 추가:
```csharp
if (_playerInRange && Input.IsActionJustPressed("interact"))
{
    SaveManager.SaveGame("manual");
    AudioManager.Instance?.PlaySFX("save.wav");  // ← 추가
}
```

### 3-6. `Scripts/UI/HUD.cs`

`using FirstGame.Core;`는 이미 있음 (line 3).

**ShowGameOver() 메서드 (line 98~102)** — 첫 줄에 추가:
```csharp
private void ShowGameOver()
{
    AudioManager.Instance?.PlaySFX("game_over.wav");  // ← 추가
    _gameOverPanel.Visible = true;
    GetTree().Paused = true;
}
```

---

## 주의사항

1. **`AudioManager.Instance?.PlaySFX()`** — null 조건부 호출(`?.`) 사용으로 AudioManager 초기화 전에도 크래시 방지
2. **사운드 파일 없어도 동작** — `ResourceLoader.Exists()` 체크로 파일 없으면 에러 로그만 출력
3. **Inventory.cs는 Node가 아님** — `AudioManager.Instance`는 static이므로 Node가 아닌 클래스에서도 접근 가능
4. **일시정지 중 사운드** — AudioManager는 autoload이므로 process_mode 기본값(Inherit). 게임오버 시 `GetTree().Paused = true` 이전에 사운드를 재생하므로 문제없음. 만약 일시정지 후에도 UI 사운드가 필요하면 AudioManager의 ProcessMode를 Always로 변경
5. **SFX 풀 크기** — 8개면 충분. 동시에 8개 이상 사운드가 재생되면 가장 오래된 것을 재사용
6. **_ExitTree에서 `Instance = null`** — GameManager와 동일하게 씬 리로드 시 싱글톤 버그 방지

---

## 검증 방법

1. 공격 사운드: Space 키로 공격 → `player_attack.wav` 재생 확인
2. 피격 사운드: 적에게 맞기 → `player_hit.wav` 재생 확인
3. 적 사망 사운드: 적 처치 → `enemy_death.wav` 재생 확인
4. 포션 사용: 인벤토리에서 포션 사용 → `potion_use.wav` 재생 확인
5. 상점 구매/판매: 상점에서 거래 → `shop_buy.wav` / `shop_sell.wav` 재생 확인
6. 실패 사운드: 골드 부족 시 구매 → `shop_fail.wav` 재생 확인
7. 저장 사운드: 세이브 포인트에서 E키 → `save.wav` 재생 확인
8. 게임 오버: 사망 → `game_over.wav` 재생 확인
9. 파일 누락 시: 사운드 파일 없이 실행 → 에러 로그만 출력, 크래시 없음 확인
10. 동시 재생: 여러 적 동시 처치 → 사운드 겹쳐서 재생되는지 확인
