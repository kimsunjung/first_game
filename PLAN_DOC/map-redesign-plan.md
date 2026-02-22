# 맵 리디자인 설계 - 구역 기반 맵 생성

## 목표
1. 스타듀밸리 스타일 직선 길 생성
2. 호수/물 지형 추가
3. 마을 구역 + 상점/세이브포인트 배치

## 현재 맵 구조 (80x60 타일, 16x16px)

```
┌──────────────────────────────────┐
│ 테두리(벽 2타일)                   │
│                                  │
│   노이즈 기반 랜덤 장애물           │
│   Drunkard's Walk 흙길            │
│                                  │
└──────────────────────────────────┘
```

## 새 맵 구조 (구역 기반)

```
80x60 타일 맵
┌──────────────────────────────────────────────┐
│ 벽(2)                                         │
│  ┌─────────┐    ┌──────┐                      │
│  │ 마을구역  │    │ 호수  │    숲 (노이즈)       │
│  │ 상점,세이브│────│      │                      │
│  │ 돌바닥    │    └──────┘                      │
│  └────┬─────┘                                  │
│       │ (직선 길)                                │
│       │                                        │
│  ─────┼──────────── (직선 길)                    │
│       │                                        │
│    플레이어                                     │
│    스폰 지점         숲 (노이즈)                  │
│                                               │
└──────────────────────────────────────────────┘
```

## 사용 타일 리소스

### Source 0: Floors_Tiles.png (바닥)
- 풀: (3,10), (4,10), (5,10), (6,10), (7,10) — 기존 GrassTiles
- 흙: (7,4), (8,4), (9,4), (6,4) — 기존 DirtTiles
- 돌바닥: (16,0), (17,0), (18,0) — 마을 바닥용 (Stone/Brick 영역)

### Source 2: Wall_Tiles.png (장애물)
- 기존 TreeTiles 유지 — 테두리 + 숲

### Source 4: Water_tiles.png (물) — 신규
- 물 내부: (1,1), (2,1) — 깊은 물
- 물 가장자리: (0,1), (3,1), (0,2), (3,2) 등 — 오토타일 패턴
- ObstacleLayer에 배치 (통과 불가)

## 구현 상세

### Phase 1: 구역(Zone) 시스템

```csharp
// 새 Export 프로퍼티
[ExportGroup("Village")]
[Export] public Vector2I VillageCenter { get; set; } = new(15, 12); // 타일 좌표
[Export] public int VillageWidth { get; set; } = 14;   // 타일 단위
[Export] public int VillageHeight { get; set; } = 10;

[ExportGroup("Lake")]
[Export] public Vector2I LakeCenter { get; set; } = new(50, 15);
[Export] public int LakeRadius { get; set; } = 6;

[ExportGroup("Water Tiles (Source 4)")]
[Export] public int WaterSourceId { get; set; } = 4;
```

마을 구역 판정:
```csharp
private bool IsInVillage(int x, int y)
{
    return x >= VillageCenter.X - VillageWidth/2
        && x < VillageCenter.X + VillageWidth/2
        && y >= VillageCenter.Y - VillageHeight/2
        && y < VillageCenter.Y + VillageHeight/2;
}
```

### Phase 2: 직선 길 생성 (Drunkard's Walk 교체)

스타듀밸리 스타일: **L자형 직선 경로**

```csharp
private void DrawStraightPath(Vector2I from, Vector2I to)
{
    // 1단계: from에서 수평으로 이동
    int midX = to.X;
    for (int x = Min(from.X, midX); x <= Max(from.X, midX); x++)
        DrawPathTile(x, from.Y);  // 3타일 폭 직선

    // 2단계: 도착 X좌표에서 수직으로 이동
    for (int y = Min(from.Y, to.Y); y <= Max(from.Y, to.Y); y++)
        DrawPathTile(midX, y);
}
```

길 폭: 3타일 (현재와 동일, DrawPathTile의 -1~+1 루프)

### Phase 3: 호수 생성

```csharp
private void GenerateLake()
{
    // 타원형 호수 (노이즈로 가장자리 불규칙하게)
    var lakeNoise = new FastNoiseLite();
    lakeNoise.Seed = usedSeed + 100;
    lakeNoise.Frequency = 0.1f;

    for (int x = LakeCenter.X - LakeRadius - 2; x <= LakeCenter.X + LakeRadius + 2; x++)
    {
        for (int y = LakeCenter.Y - LakeRadius - 2; y <= LakeCenter.Y + LakeRadius + 2; y++)
        {
            float dx = (x - LakeCenter.X) / (float)LakeRadius;
            float dy = (y - LakeCenter.Y) / (float)(LakeRadius * 0.7f); // 타원
            float dist = dx*dx + dy*dy;
            float edgeNoise = lakeNoise.GetNoise2D(x, y) * 0.3f;

            if (dist + edgeNoise < 1.0f)
            {
                // 물 타일 배치 (ObstacleLayer → 통과 불가)
                _obstacleLayer.SetCell(pos, WaterSourceId, waterAtlas);
                // GroundLayer도 물로 교체 (깊은 물 효과)
                _groundLayer.SetCell(pos, WaterSourceId, deepWaterAtlas);
            }
        }
    }
}
```

### Phase 4: 마을 구역 생성

```csharp
private void GenerateVillage()
{
    // 1. 마을 바닥 → 돌바닥 타일로 교체
    for (각 마을 영역 타일)
    {
        _groundLayer.SetCell(pos, GroundSourceId, stoneTile);
        _obstacleLayer.EraseCell(pos); // 장애물 제거
    }

    // 2. 마을 테두리 → 울타리/벽 배치 (입구 2곳 열어둠)
    // 남쪽, 동쪽에 3타일 폭 입구
}
```

### Phase 5: main.tscn에서 상점/세이브포인트 위치 이동

마을 구역 중심으로 배치:
```
ShopNPC: VillageCenter 기준 좌측 (예: 15*16-32, 12*16)
SavePoint: VillageCenter 기준 우측 (예: 15*16+32, 12*16)
Player 스폰: 맵 중앙 하단 (40*16, 45*16)
EnemySpawner: 마을 밖 영역
```

## GenerateMap() 새 흐름

```
1. 바닥 채우기 (풀)
2. 테두리 벽
3. 마을 구역 생성 (돌바닥 + 울타리)
4. 호수 생성 (타원형 물 타일)
5. 숲 생성 (노이즈 기반, 마을/호수/길 제외)
6. 직선 길 연결 (마을 → 스폰지점, 마을 → 호수 근처)
7. 장식 배치
```

## 수정 파일

| 파일 | 작업 |
|------|------|
| `Scripts/Maps/MapGenerator.cs` | 구역 시스템, 직선길, 호수, 마을 생성 추가 |
| `main.tscn` | ShopNPC, SavePoint, Player 위치를 마을 기준으로 이동 |

## 검증

1. Regenerate 후 마을 구역이 돌바닥으로 표시되는지
2. 직선 길이 마을에서 스폰지점까지 연결되는지
3. 호수가 타원형으로 생성되고 통과 불가인지
4. 상점/세이브포인트가 마을 안에 배치되는지
5. 적이 마을/호수에 스폰되지 않는지
