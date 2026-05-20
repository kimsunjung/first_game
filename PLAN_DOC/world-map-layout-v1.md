# World Map Layout v1 (2026-05-20)

## 목적
- 포탈 방향 ↔ 월드 지리 감각 정렬 (왼쪽 포탈 = 왼쪽 지역, 오른쪽 포탈 = 오른쪽 지역 ...).
- 도착 좌표가 다른 포탈 위에 떨어지는 ping-pong 버그 방지.
- 이전 맵으로 즉시 되돌아가는 round-trip 버그 방지.
- 큰 월드 = "허브 + 주변 사냥터 + 광산/던전/보스 지역" 구조의 ASCII 다이어그램화.

본 문서는 **2026-05-20 포탈 감사 결과**(75 포탈 / 29 맵) + 직후 적용한
10건의 도착 좌표 수정을 반영한 v1 baseline. 추가 변경은 v2로 분리.

## 현재 월드 코스몰로지 (실제 데이터 기준)

```
                    [mountain_refuge] ← (北)
                          │ ↑
                          │ │
   (북서)                 │ │ (북동)
                          ▼ │
[town] ◀── west ── [field_outpost] ── east ──→ [old_orc_road] ─→ [goblin_woods]
   │ right=east        │  │  │                              ─→ [green_meadow]
   │ down=south        │  │  │                              ─→ [town_outskirts]
   ▼                   ▼  ▼  ▼
[field_1]          [field_2]                         (town 동측 사냥터는
   │ down=south       │ down                          field_outpost 거치지 않고
   ▼                  ▼                               town 직접 접속)
[mine_1]           [field_3] ─── east ──→ [ruined_crossroad]
   │ east            │ │ │
   ▼                 │ │ └─── BOTTOM-LEFT → [harbor_village] ← (南西)
[mine_2]             │ │
   │ east            │ └──── TOP-RIGHT  → [field_5_snowfield] (북)
   ▼                 │                       │ TOP → [frozen_valley]
[mine_3]             │                       │ BOTTOM → [volcano_approach]
   ▲                 │                       │ TOP-RIGHT → [mountain_refuge]
   │                 │
   └─ TOP ────── [field_6_volcano] ←── BOTTOM-RIGHT ── [field_3]
                  │ TOP → [lava_field]
                  │ RIGHT → [mine_3]

[town] ─ BOTTOM-RIGHT ─→ [field_4_harbor]      (해안 지름길)
                            │ TOP → [pirate_camp] → [crab_beach] → [harbor_outskirts] → [harbor_village]
                            │ MIDDLE → [dungeon_4_sunken_shrine]
                            │ RIGHT → [harbor_village]
```

**중요 결정(코스몰로지 v1, 2026-05-20 P2 hot-fix 후 확정)**:
`field_outpost`는 `town`의 *서쪽*에 위치한다. 이 코스몰로지는 **양쪽 데이터로
일관됨**:
- town 입장: `PortalToOutpost`가 LEFT(56,252) → outpost는 서쪽.
- outpost 입장: `PortalToTown`이 **RIGHT-TOP(600,80)** → town은 동쪽.

이전 상태(outpost.PortalToTown이 LEFT(40,180))는 outpost 입장에서도 town을
서쪽으로 가리켜 양 데이터가 모순됐던 문제(코덱스 리뷰 P2). 본 패스에서
outpost.PortalToTown만 RIGHT-TOP으로 옮겨 미러 정합.

사용자 원래 권장은 "outpost는 town 동쪽"이었지만 그러려면 town 4 포탈
(Field1·TownOutskirts·Field4Harbor가 점유 중인 RIGHT 3슬롯)을 재배치해야
하므로 회귀 위험 큼 → v1은 **outpost-west-of-town**으로 확정. 권장안 전환은
v2 단독 패스 후보로 보류.

## 포탈 방향 정합성 OK/NG (v1 적용 후)

### ✅ 정합 (대부분의 좌·우 사이드 사냥터 체인)
- town↔field_1/town_outskirts/field_4_harbor → right exit / left entry
- town_outskirts→green_meadow→goblin_woods→old_orc_road : 모두 right/left 일관
- mine_1→mine_2→mine_3 : 모두 right/left 일관
- harbor_outskirts→crab_beach→pirate_camp→field_4_harbor : 모두 right/left 일관
- snowfield_edge→frozen_valley : right/left 일관
- volcano_approach→lava_field : right/left 일관
- 던전 진입(field_X 중앙 MIDDLE → dungeon LEFT) : 의도된 패턴(던전 입구는 항상 dungeon 좌측)

### ⚠ 축 불일치(north exit ↔ east entry) — 디자인 의도라 유지
- field_5_snowfield TOP ↔ frozen_valley RIGHT
- field_6_volcano TOP ↔ lava_field RIGHT
- field_6_volcano RIGHT ↔ mine_3 TOP
- field_1 TOP ↔ mine_1 LEFT
- field_4_harbor TOP ↔ pirate_camp RIGHT

축 불일치는 의도적 디자인(mine/lava는 별도 사이드맵)이므로 v1에서는 변경하지
않음. 수동 테스트에서 헷갈리지 않으면 유지.

### 🔒 일방통행(One-way) 지름길 — 의도 유지
- field_3 → harbor_village (역방향 없음, 돌아갈 땐 outpost 경유)
- field_4_harbor → harbor_village (역방향 없음, outpost 경유)
- field_5_snowfield → mountain_refuge (역방향 없음, snowfield_edge 경유)

세 지름길은 *진행 후 빠른 귀환* 목적이라 의도적으로 일방. **단**, 도착 좌표는
귀환 포탈 옆이 아닌 안전한 거리로 조정 완료(v1).

## v1 수정 적용 목록 (2026-05-20)

| # | 출발 | 포탈 | 변경 전 spawn | 변경 후 spawn | 사유 |
|---|---|---|---|---|---|
| 1 | field_5_snowfield | PortalToField3 | (640,360) | (1150,200) | **field_3/PortalToDungeon3 위에 정확히 떨어지는 크리티컬 버그** |
| 2 | field_6_volcano | PortalToField3 | (640,360) | (1150,520) | **동일 — dungeon_3 포탈 위 spawn** |
| 3 | old_orc_road | PortalToFieldOutpost | (80,180) | (560,180) | 방향 정합(LEFT exit→RIGHT entry) + outpost/PortalToTown(40,180) 옆 ping-pong 방지 |
| 4 | field_3 | PortalToHarborVillage | (80,180) | (320,200) | harbor/PortalToOutpost(40,180) 옆 ping-pong 방지 |
| 5 | field_outpost | PortalToMountainRefuge | (80,180) | (320,60) | mountain/PortalToHarborVillage(40,180) 옆 ping-pong 방지, TOP 진입은 mountain 상단 spawn |
| 6 | field_5_snowfield | PortalToMountainRefuge | (560,180) | (320,200) | mountain/PortalToSnowfieldEdge(600,180) 옆 ping-pong 방지 |
| 7 | lava_field | PortalToField6Volcano | (80,360) | (200,360) | field_6/PortalToField3(50,360) 옆 ping-pong 방지 |
| 8 | frozen_valley | PortalToField5Snowfield | (80,360) | (200,360) | field_5/PortalToField3(50,360) 옆 ping-pong 방지 |
| 9 | field_6_volcano | PortalToMine3 | (80,360) | (200,360) | mine_3/PortalToMine2(50,360) 옆 ping-pong 방지 |
| 10 | pirate_camp | PortalToField4Harbor | (80,360) | (200,360) | field_4/PortalToTown(50,360) 옆 ping-pong 방지 |
| 11a | field_outpost | PortalToTown (position) | (40,180) LEFT | (600,80) RIGHT-TOP | **코스몰로지 일관(outpost-west-of-town): outpost→town은 동쪽 출구** |
| 11b | town | PortalToOutpost (spawn) | (100,180) | (520,120) | town LEFT 출구→outpost RIGHT 안쪽 도착(미러) |

모든 변경은 `TargetSpawnPosition` 값만 조정 — 포탈 위치(`position`)·
TargetScenePath·DestinationName·SceneManager 로직 무변경. SaveData 무영향.

## 수동 테스트 체크리스트

- [ ] field_5_snowfield/PortalToField3 → field_3 (1150,200) 도착, 즉시 dungeon_3 진입 안 됨
- [ ] field_6_volcano/PortalToField3 → field_3 (1150,520) 도착, 즉시 dungeon_3 진입 안 됨
- [ ] old_orc_road→field_outpost 도착 후 즉시 town 포탈에 빨려들지 않음
- [ ] field_3→harbor_village 도착 후 즉시 outpost 포탈에 빨려들지 않음
- [ ] field_outpost→mountain_refuge 도착 후 즉시 harbor 포탈에 빨려들지 않음
- [ ] field_5_snowfield→mountain_refuge 도착 후 즉시 snowfield_edge로 되돌아가지 않음
- [ ] lava_field→field_6, frozen_valley→field_5, field_6→mine_3, pirate→field_4 모두 도착 직후 인접 포탈 미발동
- [ ] 모든 양방향 포탈 왕복 시(좌→우→좌) 출발 위치로 자연스럽게 돌아옴
- [ ] 모바일 터치 환경에서 도착 직후 발동 안 됨(touch radius 고려)

## 보류·후속 (v2 후보)
- **town↔field_outpost 권장안(outpost-east-of-town) 재설계**: v1에서는 양쪽
  데이터를 outpost-west-of-town으로 미러 정합 완료(P2 닫음). 권장안 전환을
  굳이 하려면 town 4 포탈(현재 LEFT 1 + RIGHT 3슬롯) 전부 + 인바운드 도착 좌표
  4건 재배치 필요. 회귀 위험 큼 → 단독 패스로 분리.
- **field_3 4방향 포탈 corner mounting 정리**: 현재 TOP-LEFT/TOP-RIGHT/BOTTOM-
  LEFT/BOTTOM-RIGHT corner 4개 + 중앙 dungeon. corner mounting은 모바일 터치
  타겟 작아 휴리스틱상 살짝 모호 → 별도 UX 패스에서 검토.
- **포탈 방향성 자동 검사**: validate.py에 (포탈 position 사분면) ↔ (target scene의 인바운드 포탈 position 사분면) 반대 매핑 규칙 추가는 가능하지만 corner/MIDDLE 분류 휴리스틱이 복잡 → v2 검토.
