# 스프라이트 크기/카메라 근본 해결 설계

> 상태 메모 (2026-04-15)
> 이 문서는 스프라이트 비율과 카메라를 조정하던 당시의 설계 메모다.
> 현재 `Player`의 `Camera2D zoom=(3,3)`은 적용되어 있지만, 문서 안의 `MapGenerator`, `field_tileset.tres` 관련 부분은 현재 씬 구조에 직접 연결되지 않는다.
> 이 문서는 `플레이어/적 스프라이트 비율`, `카메라 줌`, `애니메이션 패딩` 참고용으로만 유지한다.

## 현재 문제 분석

### 1. 플레이어 스프라이트 (Body_A)
- 프레임 크기: **64x64** (모든 애니메이션 동일)
- Idle 실제 콘텐츠: **16x30px** (프레임의 25% 영역만 사용)
- Attack(Slice) 실제 콘텐츠: **최대 62x64px** (거의 전체 프레임 사용)
- **의도된 설계**: 공격 모션(칼 휘두르기)을 위해 여유 공간이 필요하므로 64x64가 맞음
- 결론: 프레임 크기 자체는 정상. 트리밍 불가 (공격 애니메이션이 잘림)

### 2. 적(오크) 스프라이트
- **Idle**: 128x32 시트, **프레임 32x32** (콘텐츠 20x30px)
- **Run**: 384x64 시트, **프레임 64x64** (콘텐츠 24x30px)
- **Death**: 384x64 시트, **프레임 64x64**
- **문제**: 애니메이션마다 프레임 크기가 다름 (32 ↔ 64)
- idle→run 전환 시 스프라이트 크기가 갑자기 변함 (시각적 점프)

### 3. 타일 크기
- 타일: 16x16px
- 플레이어 idle 콘텐츠(16x30) = 타일 1x2개 크기 → **비율 자체는 정상**
- 문제는 크기가 아니라 **화면 해상도 대비 표시 크기**

### 4. 카메라/화면
- 기본 윈도우: 1152x648
- zoom 없이: 72x40 타일 표시 → 16px 캐릭터가 너무 작게 보임
- **16x16 픽셀아트 게임은 zoom 3~4x가 표준** (Stardew Valley, Celeste 등)

---

## 해결 방안

### Phase 1: 적(오크) 프레임 크기 통일

**문제**: Idle(32x32) vs Run/Death(64x64) 프레임 크기 불일치
**해결**: 모든 오크 스프라이트 시트를 64x64 기준으로 통일

#### 방법: Python 스크립트로 Idle 시트 리사이즈
- `Idle-Sheet.png` (128x32, 4프레임 32x32)
- → 각 32x32 프레임을 64x64 캔버스 중앙 하단에 배치
- → 새 시트: 256x64 (4프레임 64x64)
- 원본 파일 보존, `_padded` 접미사로 새 파일 생성

```
Scripts/Tools/SpritePadder.py (신규)
```

```python
from PIL import Image
import os

# 오크 Idle-Sheet를 32x32 → 64x64 프레임으로 패딩
src = "Resources/Tilesets/Pixel Crawler - Free Pack/Entities/Mobs/Orc Crew/Orc/Idle/Idle-Sheet.png"
img = Image.open(src)
old_fw, old_fh = 32, 32
new_fw, new_fh = 64, 64
frame_count = img.width // old_fw

new_img = Image.new("RGBA", (new_fw * frame_count, new_fh), (0, 0, 0, 0))
for i in range(frame_count):
    frame = img.crop((i * old_fw, 0, (i + 1) * old_fw, old_fh))
    # 중앙 하단 배치
    x_offset = (new_fw - old_fw) // 2
    y_offset = new_fh - old_fh
    new_img.paste(frame, (i * new_fw + x_offset, y_offset))

new_img.save(src.replace(".png", "_padded.png"))
```

#### EnemyController.cs 수정
```csharp
// 변경 전
AddSheetAnimation(frames, "idle", "Idle/Idle-Sheet.png", 4, 6, true);

// 변경 후 (패딩된 시트 사용)
AddSheetAnimation(frames, "idle", "Idle/Idle-Sheet_padded.png", 4, 6, true);
```

### Phase 2: 카메라 설정 표준화

**기준**: 16x16 픽셀아트 → zoom 3x (24x14 타일 표시)

#### player.tscn
```
Camera2D:
  zoom = Vector2(3, 3)
  position_smoothing_enabled = true
```

#### PlayerController.cs 카메라 제한
```csharp
camera.LimitLeft = 0;
camera.LimitTop = 0;
camera.LimitRight = 1280;  // 80 * 16
camera.LimitBottom = 960;  // 60 * 16
```

### Phase 3: 스프라이트 스케일 밸런싱

zoom 3x 기준 화면 표시 크기:

| 대상 | 프레임 | idle 콘텐츠 | 화면 표시(zoom 3x) | scale |
|------|--------|------------|-------------------|-------|
| 플레이어 | 64x64 | 16x30 | 48x90px | 1.0 |
| 오크 | 64x64 (통일 후) | 20x30 | 60x90px | 0.8 |

- 플레이어: scale **1.0** (idle 콘텐츠 16px → 화면 48px)
- 오크: scale **0.8** (idle 콘텐츠 20px*0.8=16px → 화면 48px, 플레이어와 동일)

#### player.tscn
```
AnimatedSprite2D: scale = Vector2(1, 1)
```

#### enemy.tscn
```
AnimatedSprite2D: scale = Vector2(0.8, 0.8)
```

### Phase 4: 타일셋 정리

현재 field_tileset.tres에 93개 Source 등록. 대부분 16x16 타일 그리드가 아닌 개별 스프라이트.

#### 맵 생성에 사용 가능한 Source (16x16 타일 그리드)
| Source | 이미지 | 용도 |
|--------|--------|------|
| 0 | Floors_Tiles.png (400x416) | 바닥 |
| 1 | Dungeon_Tiles.png (400x400) | 던전 바닥 |
| 2 | Wall_Tiles.png (400x400) | 벽/장애물 |
| 3 | Wall_Variations.png (256x480) | 벽 변형 |
| 4 | Water_tiles.png (400x400) | 물 |

#### 맵 생성에 사용 불가 (개별 스프라이트)
- Source 5~19: 나무 (개별 이미지, 16x16 그리드 아님)
- Source 20~92: 가구/식생/바위/구조물 등 (개별 스프라이트)

#### 조치
- MapGenerator.cs의 TreeTiles: Source 2 (Wall_Tiles) 타일만 사용 (현재 적용 완료)
- DecorationTiles: 비움 (현재 적용 완료)
- 타일셋에서 Source 5~92 제거는 선택사항 (향후 수동 배치에 활용 가능)

---

## 수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Scripts/Tools/SpritePadder.py` | **신규** | 오크 Idle 시트 패딩 스크립트 |
| `Scripts/Entities/Enemies/EnemyController.cs` | 수정 | 패딩된 Idle 시트 경로 변경 |
| `Scenes/Characters/player.tscn` | 수정 | Camera2D zoom=(3,3), sprite scale=(1,1) |
| `Scenes/Characters/enemy.tscn` | 수정 | sprite scale=(0.8, 0.8), healthbar 위치 조정 |

## 검증

1. 오크 idle→run 전환 시 크기 점프 없는지 확인
2. 플레이어와 적의 크기 비율이 자연스러운지 확인
3. zoom 3x에서 맵 탐험이 답답하지 않은지 확인 (24x14 타일 표시)
4. 기존 기능 (전투, HUD, 인벤토리, 상점) 정상 작동 확인
5. zoom이 너무 크면 3→2.5로, 작으면 3→3.5로 미세 조정
