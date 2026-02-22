# 캐릭터 애니메이션 시스템 설계

## 개요
플레이어의 Godot 아이콘(icon.svg)을 Pixel Crawler 에셋의 애니메이션 스프라이트로 교체.
Sprite2D → AnimatedSprite2D로 변경하고, 상태 기반 애니메이션 시스템 구현.

## 에셋 경로
`res://Resources/Tilesets/Pixel Crawler - Free Pack/Entities/Characters/Body_A/Animations/`

## 스프라이트 시트 정보

각 시트는 **가로 스트립** (좌→우 프레임 순서). 프레임 크기는 **32x32px** 추정 (확인 필요).

| 애니메이션 | 프레임 수 | 방향 | 파일명 패턴 |
|-----------|----------|------|------------|
| Idle | 4 | Down/Side/Up | `Idle_Base/Idle_{Dir}-Sheet.png` |
| Walk | 6 | Down/Side/Up | `Walk_Base/Walk_{Dir}-Sheet.png` |
| Run | 6 | Down/Side/Up | `Run_Base/Run_{Dir}-Sheet.png` |
| Slice (공격) | 8 | Down/Side/Up | `Slice_Base/Slice_{Dir}-Sheet.png` |
| Hit (피격) | 4 | Down/Side/Up | `Hit_Base/Hit_{Dir}-Sheet.png` |
| Death (사망) | 8 | Down/Side/Up | `Death_Base/Death_{Dir}-Sheet.png` |

**방향 규칙:**
- Down = 아래 (기본)
- Up = 위
- Side = 오른쪽 (왼쪽은 `FlipH = true`로 반전)

## 수정 파일

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scenes/Characters/player.tscn` | **수정** | Sprite2D → AnimatedSprite2D, scale 조정 |
| `Scripts/Entities/Player/PlayerController.cs` | **수정** | 애니메이션 상태 관리 추가 |

## 1단계: player.tscn 수정

### 노드 변경
```
Player (CharacterBody2D)
├── AnimatedSprite2D   ← 기존 Sprite2D 삭제, 이것으로 교체
├── CollisionShape2D
└── Camera2D
```

### 설정 값
- **Player 노드**: `scale = Vector2(1, 1)` (기존 0.3에서 변경)
- **CollisionShape2D**: RectangleShape2D `size = Vector2(12, 10)` (32x32 스프라이트에 맞게 축소)
- **AnimatedSprite2D**: SpriteFrames는 코드에서 동적 생성 (아래 2단계 참조)

> **주의**: scale을 1.0으로 변경하면 CollisionShape2D 크기도 반드시 재조정해야 함.
> 현재: scale 0.3 × shape 57×47 = 실효 17×14px
> 변경 후: scale 1.0 × shape 12×10 = 실효 12×10px (비슷한 크기 유지)

## 2단계: PlayerController.cs - 애니메이션 시스템

### 2-1. 애니메이션 로딩 (_Ready에서 실행)

SpriteFrames를 코드에서 동적으로 생성하여 AnimatedSprite2D에 할당.

```csharp
// 필드
private AnimatedSprite2D _animSprite;

// 애니메이션 기본 경로
private const string ANIM_BASE_PATH = "res://Resources/Tilesets/Pixel Crawler - Free Pack/Entities/Characters/Body_A/Animations/";

// _Ready()에서 호출
private void SetupAnimations()
{
    _animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    var frames = new SpriteFrames();

    // 기본 "default" 애니메이션 제거
    if (frames.HasAnimation("default"))
        frames.RemoveAnimation("default");

    // 각 애니메이션 등록
    string[] dirs = { "Down", "Side", "Up" };

    // Idle (4프레임)
    foreach (var dir in dirs)
        AddSheetAnimation(frames, $"idle_{dir.ToLower()}", $"Idle_Base/Idle_{dir}-Sheet.png", 4);

    // Walk (6프레임)
    foreach (var dir in dirs)
        AddSheetAnimation(frames, $"walk_{dir.ToLower()}", $"Walk_Base/Walk_{dir}-Sheet.png", 6);

    // Slice/Attack (8프레임)
    foreach (var dir in dirs)
        AddSheetAnimation(frames, $"attack_{dir.ToLower()}", $"Slice_Base/Slice_{dir}-Sheet.png", 8);

    // Hit (4프레임)
    foreach (var dir in dirs)
        AddSheetAnimation(frames, $"hit_{dir.ToLower()}", $"Hit_Base/Hit_{dir}-Sheet.png", 4);

    // Death (8프레임)
    foreach (var dir in dirs)
        AddSheetAnimation(frames, $"death_{dir.ToLower()}", $"Death_Base/Death_{dir}-Sheet.png", 8);

    _animSprite.SpriteFrames = frames;
    _animSprite.Play("idle_down"); // 초기 애니메이션
}

private void AddSheetAnimation(SpriteFrames frames, string animName, string sheetPath, int frameCount)
{
    frames.AddAnimation(animName);
    frames.SetAnimationSpeed(animName, 10); // 10 FPS (조정 가능)

    var texture = GD.Load<Texture2D>(ANIM_BASE_PATH + sheetPath);
    if (texture == null)
    {
        GD.PrintErr($"Failed to load: {sheetPath}");
        return;
    }

    int frameWidth = texture.GetWidth() / frameCount;
    int frameHeight = texture.GetHeight();

    for (int i = 0; i < frameCount; i++)
    {
        var atlas = new AtlasTexture();
        atlas.Atlas = texture;
        atlas.Region = new Rect2(i * frameWidth, 0, frameWidth, frameHeight);
        frames.AddFrame(animName, atlas);
    }
}
```

### 2-2. 애니메이션 상태 관리

```csharp
// 애니메이션 상태
private enum AnimState { Idle, Walk, Attack, Hit, Death }
private AnimState _currentAnimState = AnimState.Idle;
private bool _isAnimLocked = false; // 공격/피격/사망 중 다른 애니메이션 전환 방지

// 방향 → 애니메이션 접미사 변환
private string GetDirectionSuffix()
{
    float absX = Mathf.Abs(_facingDirection.X);
    float absY = Mathf.Abs(_facingDirection.Y);

    if (absY >= absX)
    {
        return _facingDirection.Y >= 0 ? "down" : "up";
    }
    else
    {
        // Side: 좌우 반전 처리
        _animSprite.FlipH = _facingDirection.X < 0;
        return "side";
    }
}
```

### 2-3. 애니메이션 전환 로직

```csharp
private void UpdateAnimation()
{
    if (_isAnimLocked) return; // 공격/피격/사망 애니메이션 재생 중

    string dir = GetDirectionSuffix();

    if (Velocity != Vector2.Zero)
    {
        PlayAnim($"walk_{dir}");
        _currentAnimState = AnimState.Walk;
    }
    else
    {
        PlayAnim($"idle_{dir}");
        _currentAnimState = AnimState.Idle;
    }
}

private void PlayAnim(string animName)
{
    if (_animSprite.Animation != animName)
        _animSprite.Play(animName);
}

// 공격 시 호출 (Attack() 메서드 내부에서)
private void PlayAttackAnimation()
{
    _isAnimLocked = true;
    string dir = GetDirectionSuffix();
    _animSprite.Play($"attack_{dir}");
    // 애니메이션 끝나면 잠금 해제 (AnimationFinished 시그널)
}

// 피격 시 호출 (TakeDamage() 메서드 내부에서)
private void PlayHitAnimation()
{
    _isAnimLocked = true;
    string dir = GetDirectionSuffix();
    _animSprite.Play($"hit_{dir}");
}

// 사망 시 호출 (Die() 메서드 내부에서)
private void PlayDeathAnimation()
{
    _isAnimLocked = true;
    string dir = GetDirectionSuffix();
    _animSprite.Play($"death_{dir}");
}
```

### 2-4. AnimationFinished 시그널 연결

```csharp
// _Ready()에서 시그널 연결
_animSprite.AnimationFinished += OnAnimationFinished;

private void OnAnimationFinished()
{
    var anim = _animSprite.Animation;

    if (anim.ToString().StartsWith("attack_") || anim.ToString().StartsWith("hit_"))
    {
        _isAnimLocked = false;
        // idle로 복귀
        UpdateAnimation();
    }
    // death 애니메이션은 잠금 해제하지 않음 (마지막 프레임에서 정지)
}
```

### 2-5. 기존 메서드 수정 요약

| 메서드 | 추가할 코드 |
|--------|-----------|
| `_Ready()` | `SetupAnimations()` 호출 |
| `_PhysicsProcess()` | `UpdateAnimation()` 호출 (GetInput + MoveAndSlide 뒤) |
| `Attack()` | `PlayAttackAnimation()` 호출 |
| `TakeDamage()` | `PlayHitAnimation()` 호출 |
| `Die()` | `PlayDeathAnimation()` 호출 |

### 2-6. Attack 애니메이션 중 이동 제한 (선택사항)

공격 모션 중 이동을 막으려면 GetInput()에 체크 추가:
```csharp
private void GetInput()
{
    if (IsDead || _isAnimLocked) return; // 애니메이션 잠금 중 입력 무시
    // ... 기존 코드
}
```

> **참고**: 이동 중 공격 허용 여부는 게임 느낌에 따라 결정.
> 이동 허용 시 `_isAnimLocked`를 공격 시에만 `_isAttacking`으로 분리.

## 3단계: player.tscn 에디터 작업

1. player.tscn 열기
2. **Sprite2D 노드 삭제**
3. Player 노드 아래에 **AnimatedSprite2D** 노드 추가 (이름: `AnimatedSprite2D`)
4. Player 노드의 **Scale** → `(1, 1)` 로 변경
5. CollisionShape2D의 RectangleShape2D **size** → `(12, 10)` 으로 변경 (테스트 후 조정)
6. 저장 후 빌드 (Alt+B)

> SpriteFrames 리소스는 코드에서 자동 생성되므로 에디터에서 별도 설정 불필요.

## 공격 애니메이션 FPS 조정

| 애니메이션 | 추천 FPS | 사유 |
|-----------|---------|------|
| idle | 6 | 느긋한 호흡 느낌 |
| walk | 10 | 자연스러운 보행 |
| attack (slice) | 15 | 빠른 베기 느낌 |
| hit | 12 | 짧고 빠른 반응 |
| death | 8 | 천천히 쓰러짐 |

`AddSheetAnimation`에서 `animName`에 따라 FPS를 다르게 설정하면 됨.

## 주의사항

1. **기존 흰색 플래시**: EnemyController의 `sprite.Modulate = Colors.White` 피격 효과는 AnimatedSprite2D에서도 동일하게 작동함. PlayerController에서도 TakeDamage에 추가 고려.
2. **Side 방향 FlipH**: `GetDirectionSuffix()`에서 FlipH를 설정하는데, Down/Up 방향일 때 FlipH를 false로 리셋해야 함:
```csharp
private string GetDirectionSuffix()
{
    float absX = Mathf.Abs(_facingDirection.X);
    float absY = Mathf.Abs(_facingDirection.Y);

    if (absY >= absX)
    {
        _animSprite.FlipH = false; // 상하 방향일 때 반전 해제
        return _facingDirection.Y >= 0 ? "down" : "up";
    }
    else
    {
        _animSprite.FlipH = _facingDirection.X < 0;
        return "side";
    }
}
```
3. **scale 변경 영향**: Player scale을 0.3→1.0으로 변경하면:
   - CollisionShape2D 크기 재조정 필수
   - Camera2D zoom이나 position은 영향 없음 (Camera2D는 자체 좌표계)
   - `Stats.AttackRange`, `Stats.DetectionRange` 등 거리값은 GlobalPosition 기준이라 영향 없음
4. **비공격 애니메이션 루프**: idle, walk는 `SetAnimationLoop(animName, true)` 설정 필요. attack, hit, death는 `false`.
```csharp
// AddSheetAnimation에서 루프 설정 추가
bool shouldLoop = animName.StartsWith("idle_") || animName.StartsWith("walk_");
frames.SetAnimationLoop(animName, shouldLoop);
```
