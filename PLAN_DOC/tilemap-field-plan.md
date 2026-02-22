# 타일맵 기반 필드맵 구현 계획

## Context
현재 메인 씬(main.tscn)은 빈 공간에 플레이어, 적 스포너, 상점NPC, 세이브포인트가 배치되어 있을 뿐 실제 지형이 없음. TileMap을 사용해 풀/흙/물/나무 등으로 구성된 단일 필드맵을 만들어 게임다운 환경을 구현.

## 타일셋 에셋
**Anokolisa - Pixel Crawler** (16x16px, 무료)
- URL: https://anokolisa.itch.io/free-pixel-art-asset-pack-topdown-tileset-rpg-16x16-sprites
- 숲/던전/동굴 환경, 500+ 스프라이트, 캐릭터/적/무기 포함

## 생성/수정 파일 요약

| 파일 | 작업 | 설명 |
|------|------|------|
| `Resources/Tilesets/` | **신규** | 타일셋 이미지 폴더 |
| `Scenes/Maps/field.tscn` | **신규** | 필드맵 씬 (TileMapLayer + 충돌) |
| `Scenes/main.tscn` | **수정** | field.tscn 인스턴스 추가, 오브젝트 위치 재배치 |
| `Scripts/Entities/Player/PlayerController.cs` | **수정** | 카메라 제한(limit) 설정 추가 |

---

## 1단계: 타일셋 준비

1. Anokolisa 에셋 다운로드 후 `Resources/Tilesets/` 에 PNG 배치
2. Godot 에디터에서 TileSet 리소스 생성 (`Resources/Tilesets/field_tileset.tres`)
   - 타일 크기: 16x16
   - 물리 레이어 설정: 벽/물 타일에 충돌 추가

### 타일 종류 및 충돌 설정

| 타일 | 충돌 | 용도 |
|------|------|------|
| 풀 (grass) | 없음 | 이동 가능 바닥 |
| 흙길 (dirt path) | 없음 | 이동 가능 길 |
| 물 (water) | **있음** | 이동 불가 |
| 나무 (tree) | **있음** | 이동 불가 장애물 |
| 바위 (rock) | **있음** | 이동 불가 장애물 |
| 꽃/풀 장식 | 없음 | 장식용 (위에 걸을 수 있음) |

---

## 2단계: 필드맵 씬 구성

### `Scenes/Maps/field.tscn` (신규)

Godot 4.6에서는 TileMapLayer 노드를 사용 (기존 TileMap은 deprecated).

```
Field (Node2D)
├── GroundLayer (TileMapLayer)    ← 바닥 (풀, 흙)
├── DecorationLayer (TileMapLayer) ← 꽃, 풀 장식 (바닥 위)
└── ObstacleLayer (TileMapLayer)   ← 나무, 바위, 물 (충돌 있음)
```

- **GroundLayer**: 전체 맵을 풀로 채우고, 길/흙 타일 배치
- **DecorationLayer**: 바닥 장식 (플레이어 아래에 렌더링)
- **ObstacleLayer**: 충돌이 있는 장애물, z_index를 높게 설정

### 맵 크기
- 약 **80x60 타일** (1280x960 픽셀) — 화면 몇 배 크기의 탐험 가능 영역
- 맵 가장자리는 나무/물로 막아서 자연스러운 경계 형성

### 맵 레이아웃 (개략)
```
[나무벽 경계]
[  풀밭 영역 - 적 스포너 구역  ]
[  흙길 → 세이브포인트        ]
[  흙길 → 상점NPC 구역        ]
[  풀밭 + 물웅덩이 + 바위     ]
[나무벽 경계]
```

---

## 3단계: 메인 씬 통합

### `Scenes/main.tscn` 수정

1. `field.tscn`을 Main 노드의 **첫 번째 자식**으로 인스턴스 추가 (다른 노드보다 뒤에 렌더링되지 않도록)
2. 기존 오브젝트 위치를 맵에 맞게 재배치:
   - Player: 맵 중앙 부근
   - EnemySpawner: 풀밭 구역 (SpawnRadius를 맵 크기에 맞게 조정)
   - SavePoint: 흙길 위
   - ShopNPC: 흙길 위
3. EnemySpawner의 SpawnRadius 확인 — 장애물 위에 적이 스폰되지 않도록 주의 필요

### 현재 main.tscn 노드 구조 (참고)
```
Main (Node2D)
├── EnemySpawner @ (400, 300)
├── SavePoint @ (500, 200)
├── Player @ (689, 335)
├── HUD (CanvasLayer, process_mode=Always)
├── InventoryUI
├── ShopNPC @ (600, 200)
└── ShopUI
```

---

## 4단계: 카메라 제한

### `Scripts/Entities/Player/PlayerController.cs` 수정

Player의 Camera2D에 맵 경계 제한을 추가하여 카메라가 맵 밖을 보여주지 않도록 함.

```csharp
// _Ready()에서 Camera2D limit 설정
var camera = GetNode<Camera2D>("Camera2D");
camera.LimitLeft = 0;
camera.LimitTop = 0;
camera.LimitRight = 1280;  // 맵 너비 (80 * 16)
camera.LimitBottom = 960;  // 맵 높이 (60 * 16)
```

실제 값은 맵 크기에 맞게 조정. Export 변수로 만들어 에디터에서 설정 가능하게 해도 됨.

---

## 5단계: 충돌 레이어 정리

현재 충돌 레이어:
- Layer 1: 플레이어
- Layer 2: 적

타일맵 충돌 추가:
- **Layer 3: 지형 장애물** (나무, 바위, 물)

플레이어(Layer 1)와 적(Layer 2) 모두 Layer 3과 충돌하도록 mask 설정:
- Player (`player.tscn`): collision mask에 Layer 3 추가
- Enemy (`enemy.tscn`): collision mask에 Layer 3 추가
- ObstacleLayer의 물리 레이어: Layer 3에 설정

---

## 주의사항

1. **타일맵 작업은 Godot 에디터에서 수행** — tscn 파일 직접 편집 X (UID 충돌 위험)
2. **Godot 4.6은 TileMapLayer 사용** — 구 TileMap 노드가 아닌 TileMapLayer 노드 사용
3. **적 스폰 위치 검증** — 장애물 위에 스폰되면 적이 끼일 수 있음. 추후 스폰 위치 유효성 검사 추가 고려
4. **16px 타일 + 현재 캐릭터 크기** — 현재 플레이어 충돌체가 57x47, 적이 64x66으로 16px 타일 대비 큼. 충돌체 크기 조정이나 타일 스케일 조정 필요할 수 있음

---

## 검증 방법

1. 맵 렌더링: 풀/흙/물/나무 타일이 올바르게 표시되는지 확인
2. 충돌: 플레이어가 나무/바위/물을 통과하지 못하는지 확인
3. 적 충돌: 적도 장애물에 막히는지 확인
4. 카메라: 맵 경계 밖이 보이지 않는지 확인
5. 기존 기능: 전투, 상점, 세이브, 인벤토리가 정상 작동하는지 확인
