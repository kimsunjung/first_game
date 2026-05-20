# World Map Layout v2 (2026-05-20)

## v1 → v2 변경 요약
- v1 (2026-05-20 early): 11건 도착좌표 + 1건 코스몰로지 fix.
- v2 (2026-05-20 late): **데이터 변경 없음**. 검증기 R17 자동화 +
  포탈 75개 전수 감사 결과 통합 + 9건 warning(non-error) 문서화.

## 검증기 자동화 (R17 신규)
`validate.py check_portal_spawn_proximity` 가 75개 포탈을 전수 검사:
- TargetSpawnPosition 이 target 씬의 *비-왕복* 포탈 중심 50px 이내 = **ERROR**.
- 50~150px = **WARNING**(자동 출력, 종료코드 무관, 튜닝 신호).
- 왕복 짝(A↔B 의 도착 좌표 ↔ B 의 PortalToA position) 은 의도된 패턴으로 제외.

**현재 상태**: 0 ERROR / 9 WARNING (validate.py 가 자동 출력).

## 권역 코스몰로지 (확정 — v1 미러 정합 유지)

```
                    [mountain_refuge]
                          │ ↑ ↓
                          │ │ │
                          │ │ │ ← Lv 15~18 권역 정점
                          ▼ │
   [town] ◀── west ── [field_outpost] ── east ──→ 동쪽 사냥터 체인
      │ (Lv 1)            │ (Lv 5)                ├ old_orc_road
      │ ↑ ↓               │ ↑ ↓                   ├ goblin_woods
      ▼                   ▼                       ├ green_meadow
   [field_1]          [field_2]                  └ town_outskirts
      │ Lv4              │ Lv5
      ▼ (mine 1)         ▼ (mine 2 / dungeon 2 / field_3 / dungeon 3)
   [mine_1]           [field_3] ── east ─→ field_5/6 (산악)
                          │
                          └ corner LEFT-BOTTOM → [harbor_village]
                                                  │ Lv 8 (해안 권역)
                                                  ↓
   [town] ──── BOTTOM-RIGHT ──→ [field_4_harbor]
                                  │ TOP → pirate_camp → crab_beach → harbor_outskirts → harbor_village
                                  └ MIDDLE → [dungeon_4_sunken_shrine] (반복 보스: 크라켄)
```

### 거점 4 + 권역
| 거점 | 권역 | 권장 Lv | 주요 사냥터 |
|---|---|---|---|
| town | town_region | 1+ | town_outskirts, green_meadow, goblin_woods, old_orc_road, field_1, mine_1, dungeon_1 |
| field_outpost | outpost_region | 5+ | field_2, ruined_crossroad, field_3, mine_2, dungeon_2, dungeon_3 |
| harbor_village | coast_region | 8+ | field_4_harbor, pirate_camp, crab_beach, harbor_outskirts, dungeon_4 |
| mountain_refuge | mountain_region | 13+ | snowfield_edge, frozen_valley, field_5, volcano_approach, lava_field, field_6, mine_3 |

### 코스몰로지 결정 유지 (outpost-west-of-town)
양쪽 데이터로 일관:
- `town.PortalToOutpost` = LEFT (56,252) → outpost 는 서쪽.
- `field_outpost.PortalToTown` = RIGHT-TOP (600,80) → town 은 동쪽.

이전 v1 P2 hot-fix 에서 outpost.PortalToTown 을 LEFT(40,180) → RIGHT-TOP(600,80) 으로
이동해 미러 정합 확정. v2 에서도 유지.

## R17 비-warning 0건 = 정합 ✓

이전 v1 패스에서 다음 케이스를 모두 잡음(현재 ERROR 0건):
1. `field_5_snowfield/PortalToField3` (640,360) → field_3 dungeon 포탈 위 = **fixed**
2. `field_6_volcano/PortalToField3` (640,360) → field_3 dungeon 포탈 위 = **fixed**
3. `dungeon_4_sunken_shrine/PortalToField4` (640,360) → 같은 좌표의 dungeon4 진입 포탈 — 이건 R17 의 "왕복 짝" 으로 분류되므로 제외 (의도된 패턴: 던전 나가면 던전 진입 포탈 옆에 떨어짐).
4. 나머지 8건 ping-pong 위험 좌표는 v1 에서 안전거리(>=150px)로 이격.

## 잔여 warning 9건 (도착 좌표가 50~150px 근접, ping-pong 무관)

| # | 출발 | 도착 | 거리 | 근접 포탈 | 평가 |
|---|---|---|---|---|---|
| 1 | field_outpost/PortalToField2 | field_2 (640,80) | 55px | PortalToField1 (640,25) | 유지 — 진입 후 자연스럽게 아래로 이동, 위쪽 field_1 포탈 거리는 모바일에서 분간 가능 |
| 2 | town_outskirts/PortalToTown | town (784,252) | 56px | PortalToField1 (840,252) | 유지 — town RIGHT 중앙 도착, 다음 사냥터 진입 자연스러움 |
| 3 | dungeon_2/PortalToField2 | field_2 (1150,360) | 70px | PortalToRuinedCrossroad (1220,360) | 유지 — dungeon 1/3 과 동일 패턴(1150 우측 도착) |
| 4 | town/PortalToOutpost | field_outpost (520,120) | 100px | PortalToOldOrcRoad (600,180) | 안전 |
| 5 | field_2/PortalToOutpost | field_outpost (560,280) | 108px | PortalToOldOrcRoad (600,180) | 안전 |
| 6 | old_orc_road/PortalToFieldOutpost | field_outpost (560,180) | 108px | PortalToTown (600,80), PortalToField2 (600,280) | 안전 |
| 7 | field_3/PortalToHarborVillage | harbor_village (320,200) | 130px | PortalToMountainRefuge (320,330) | 안전 — 상하 방향이라 시각적 분리 |
| 8 | field_5_snowfield/PortalToMountainRefuge | mountain_refuge (320,200) | 130px | PortalToFieldOutpost (320,330) | 안전 — 동일 |

**총평**: 9건 모두 모바일 화면(480×720)에서 시각적 분간 가능한 거리.
프롬프트 라벨 폰트 9pt + 캐릭터 콜라이더 16px 고려해 50px 이상이면 동시
프롬프트 표시 안 됨. 좌표 추가 수정 없음.

## 포탈 발동 메커니즘 (Important)
`Portal.OnInteract()` 는 **[F] 키 / 모바일 인터랙트 버튼 입력으로만** 발동.
자동 트리거 아님 (`BaseInteractable._UnhandledInput` → IsActionPressed("interact")).
- 따라서 좌표 근접 = 자동 ping-pong 아님.
- 실제 위험은 *플레이어가 도착 직후 인터랙트를 누르며 어느 포탈인지 분간 못 함*.
- 분간 가능 거리(>= 50px) 보장이 R17 의 핵심 가치.

## 4 허브 동선 점검

### town (896×504)
- 포탈 4: LEFT(outpost), RIGHT-TOP(town_outskirts), RIGHT(field_1), BOTTOM-RIGHT(field_4_harbor)
- NPC: SavePoint(100,100), PotionShopNPC(574,112), QuestBoardNpc(364,196),
  MartaInnkeeperNpc(588,196), 기타 ShopNPC/SkillShop/StorageNPC 등
- 도착 좌표 4: 모두 80~140 의 안전 영역
- 미니맵: town(896 wide) > MinimapView 허브 숨김 임계 → 마을 미니맵 표시
- 동선 OK

### field_outpost (640×360)
- 포탈 5: RIGHT-TOP(town), RIGHT(old_orc_road), BOTTOM-RIGHT(field_2),
  BOTTOM(harbor_village), TOP(mountain_refuge)
- 도착 좌표: 80,180 ~ 560,280 — 모두 NPC/포탈에서 50px 이상 이격
- 동선 OK

### harbor_village (640×360)
- 포탈 3: LEFT(field_outpost), RIGHT(harbor_outskirts), BOTTOM(mountain_refuge)
- 도착 좌표: 80~560 — OK
- 동선 OK

### mountain_refuge (640×360)
- 포탈 3: LEFT(harbor_village), RIGHT(snowfield_edge), BOTTOM(field_outpost)
- 도착 좌표: 80~560 — OK
- 동선 OK

## v3 후속 후보 (보류)

- **outpost-east-of-town 권장안 전환** (큰 회귀 위험 — v3 분리)
- **field_3 4 corner 포탈 정리** (모바일 터치 타겟 작아 식별 약함)
- **dungeon 4 / mine_3 사냥 깊이** (보스만 있고 mob 다양성 약함)
- **포탈 방향성 자동 휴리스틱 검사** (corner 분류 모호 — R17 의 거리 기반이 더 안전)

## 참고
- v1 표(11건 fix) 는 `world-map-layout-v1.md` 에 보존 — 변경 이력 추적용.
- 좌표 변경 없음 — v2 패스는 검증기·문서·xUnit 강화에 집중.
