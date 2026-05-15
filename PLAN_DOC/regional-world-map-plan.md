# Regional World Map — Open-Ended Hunting RPG (v1)

작성일: 2026-05-15
관련 코드 변경: Regional Hunting World Full Build v1

## 1. 게임 방향성

이 게임은 **엔딩 클리어형 RPG가 아니다.** 미르의전설3, 클래식 PC RPG (Diablo 1, Ultima, Crystalis 후반부)처럼 **마을 주변에 여러 사냥터가 펼쳐져 있고, 레벨업·장비 파밍·강화·소모품 준비를 반복하며 더 강한 사냥터로 이동하는 오픈엔드 성장형 사냥 RPG**다.

핵심 루프:
1. 거점(마을/피난처)에서 보급 → 2. 사냥터에서 적 처치/드랍 수집 → 3. 거점에서 강화/장비 갱신 → 4. 더 강한 사냥터로 진출 → 1로

"최종 보스 한 번 잡고 끝" 구조는 없다. 보스 처치는 **그 권역의 졸업 신호**이지 **게임의 종료**가 아니다.

## 2. 권역(Region) 구조

월드는 4개 권역으로 구성된다. 각 권역에는 하나의 **거점(Hub)** 또는 **거점 후보**가 있고, 그 주변에 여러 사냥터가 배치된다.

| 권역 | 거점 | 사냥터 | 던전/광산 |
|---|---|---|---|
| **town_region** | town | town_outskirts, green_meadow, goblin_woods, old_orc_road | mine_1, dungeon_1 |
| **outpost_region** | field_outpost | field_2(graveyard_edge), ruined_crossroad, field_3 | mine_2, dungeon_2, dungeon_3 |
| **coast_region** | harbor_village | harbor_outskirts, crab_beach, pirate_camp, field_4_harbor | dungeon_4_sunken_shrine |
| **mountain_region** | mountain_refuge | snowfield_edge, frozen_valley, field_5_snowfield, volcano_approach, lava_field, field_6_volcano | mine_3 |

## 3. 거점(Hub) 4곳

거점은 **세이브 + 상점 + 스킬상점 + 포탈 허브**를 모두 제공한다. 사냥터에서 죽거나 인벤토리가 가득 차면 거점으로 돌아와 정리 후 재진출.

| 거점 | Scene | 역할 | 특수 판매품 |
|---|---|---|---|
| 마을 (town) | `town.tscn` | 게임 시작 지점. 가장 풍부한 NPC 및 시설 | 기본 포션, 강화재료, 기본 무기/방어구 |
| 전초기지 (field_outpost) | `field_outpost.tscn` | town_region 끝, outpost_region 시작. 중급 진출 분기점 | 여행 키트(hi_potion, swift_potion, antidote, defense_potion, revive_scroll) |
| 항구마을 (harbor_village) | `harbor_village.tscn` | 해양 권역 거점 | tidal_tonic, mana_herb_extract, swift_potion, return_scroll |
| 산악 피난처 (mountain_refuge) | `mountain_refuge.tscn` | 설원/화산 권역 거점 | warming_draught, defrost_potion, fire_resist_elixir, magma_brew, battle_elixir |

## 4. 사냥터별 반복 파밍 목적

각 사냥터는 단순 레벨업이 아니라 **반복 방문 목적**이 있다.

### town_region

| 사냥터 | Scene | 권장 레벨 | 주요 적 | 반복 파밍 목적 |
|---|---|---|---|---|
| 마을 외곽 (town_outskirts) | `town_outskirts.tscn` | 1-3 | 슬라임, 거미, 늑대 | **초보 골드/포션 파밍**, 첫 장비 비용 충당 |
| 푸른 초원 (green_meadow) | `green_meadow.tscn` | 3-5 | 멧돼지, 숲 정령, 고블린 스카우트, 늑대 | **나무/가죽/초급 재료**, 1차 장비 강화 재료 |
| 고블린 숲 (goblin_woods) | `goblin_woods.tscn` | 5-7 | 고블린, 홉고블린, 고블린 트래퍼, 오크 로그 | **궁수/도적형 몹**, 초급 장신구 드랍 기대 |
| 옛 오크 길 (old_orc_road) | `old_orc_road.tscn` | 7-10 | 오크 클럽/도끼/샤먼/로그 | **오크 재료**, 철 장비 강화 |

### outpost_region

| 사냥터 | Scene | 권장 레벨 | 주요 적 | 반복 파밍 목적 |
|---|---|---|---|---|
| 묘지 변경 (field_2 = graveyard_edge) | `field_2.tscn` | 10-13 | 스켈레톤, 좀비, 무덤 와이트 | **뼈가루/저주 재료**, 언데드 특화 |
| 폐허 교차로 (ruined_crossroad) | `ruined_crossroad.tscn` | 13-15 | 타락 기사, 저주받은 병사, 폐허 골렘 | **중급 장비/고급 포션 재료** |
| 필드 3 (field_3) | `field_3.tscn` | 15-18 | 다크 울프, 역병 구울, 그림자 박쥐 | 상위 권역 진출 직전 마지막 정비 |

### coast_region

| 사냥터 | Scene | 권장 레벨 | 주요 적 | 반복 파밍 목적 |
|---|---|---|---|---|
| 항구 외곽 (harbor_outskirts) | `harbor_outskirts.tscn` | 15-17 | 갈매기 떼, 거대 게, 해적 그런트 | **해초/게 재료** |
| 게 해변 (crab_beach) | `crab_beach.tscn` | 16-18 | 거대 게, 갈매기, 해적 그런트 | **해양 소모품 재료**, sea_kelp |
| 해적 야영지 (pirate_camp) | `pirate_camp.tscn` | 18-20 | 해적 브루트/스나이퍼/그런트 | **해적 장비**, 궁수 스킬북 후보 드랍 |
| 항구 (field_4_harbor) | `field_4_harbor.tscn` | 18-22 | 위 모두 + 보스 진입 | dungeon_4 진입 직전 정비 |

### mountain_region

| 사냥터 | Scene | 권장 레벨 | 주요 적 | 반복 파밍 목적 |
|---|---|---|---|---|
| 설원 변경 (snowfield_edge) | `snowfield_edge.tscn` | 20-23 | 아이스 임프, 프로스트 울프/아처 | 빙하 지대 적응, **glacier_shard** 초기 수집 |
| 빙결 계곡 (frozen_valley) | `frozen_valley.tscn` | 22-25 | 아이시클 엘리멘탈, 스노우 위치 | **빙하 파편**, defrost_potion 필요 권장 |
| 설원 (field_5_snowfield) | `field_5_snowfield.tscn` | 25-28 | 위 + 예티, 폴라베어 + 글래시어 타이탄 보스 | **titan_scale, titan_core** 파밍 |
| 화산 진입로 (volcano_approach) | `volcano_approach.tscn` | 26-29 | 파이어 임프, 라바 슬라임, 엠버 아처 | fire_resist_elixir 필요 권장 |
| 용암 평원 (lava_field) | `lava_field.tscn` | 28-32 | 라바 서펜트, 살라만더, 마그마 골렘 | **용암석(lava_stone), drake 재료 진입 전 정비** |
| 화산 (field_6_volcano) | `field_6_volcano.tscn` | 30-35 | 위 + 인페르노 드레이크 보스 | **drake_scale, drake_eye** 파밍 |
| 광산 3 (mine_3) | `mine_3.tscn` | 32-38 | 수정 그런트/아처/워록/브루트 + 크리스탈 로드 보스 | **mithril_ore, prismatic_crystal, crystal_ore** 파밍 |

## 5. 포탈 연결망 (실제 .tscn 포탈로 구현)

```
town
 ├─→ field_1 (직접 진입, 기존)
 ├─→ town_outskirts (NEW)
 │     ├─→ green_meadow
 │     │     ├─→ goblin_woods
 │     │     │     ├─→ old_orc_road
 │     │     │     │     └─→ field_outpost
 ├─→ field_outpost (직접 진입, 기존)
 │     ├─→ town (귀환)
 │     ├─→ old_orc_road (귀환, NEW)
 │     ├─→ field_2 (NEW)
 │     │     ├─→ ruined_crossroad (NEW)
 │     │     │     └─→ field_3
 │     │     │           ├─→ harbor_village (NEW)
 │     │     │           └─→ mountain_refuge (간접: harbor_village 경유)
 │     │     └─→ dungeon_2 (기존)
 │     ├─→ harbor_village (직통, NEW)
 │     └─→ mountain_refuge (직통, NEW)
 └─→ field_4_harbor (직접 진입, 기존)

harbor_village
 ├─→ field_outpost (귀환)
 ├─→ harbor_outskirts
 │     ├─→ crab_beach
 │     │     ├─→ pirate_camp
 │     │     │     └─→ field_4_harbor
 │     │     │           └─→ dungeon_4_sunken_shrine
 └─→ mountain_refuge (직통)

mountain_refuge
 ├─→ harbor_village (귀환)
 └─→ snowfield_edge
       └─→ frozen_valley
             └─→ field_5_snowfield
                   ├─→ volcano_approach
                   │     └─→ lava_field
                   │           └─→ field_6_volcano
                   │                 └─→ mine_3
                   └─→ mountain_refuge (귀환 직통)
```

귀환은 모든 사냥터에서 이전 씬으로 가는 포탈을 양방향으로 배치했다. 그 외 town의 TeleportNPC가 scroll_field_* 아이템으로 fast-travel을 지원한다.

## 6. 스킬북 권역별 분산

기존에 town의 SkillShopUI에 26종 스킬북이 모두 모여 있어 진행감이 사라졌다. v1에서는 4개 거점에 분산:

| 거점 | 스킬북 수 | 카테고리 | 목록 |
|---|---|---|---|
| town | 10 | 기본/입문 | heal, dash, power_strike, fire_bolt, ice_shard, hp_regen, speed_boost, iron_stance, multi_shot, crit_boost |
| field_outpost | 7 | 중급/전투 | cleave, whirlwind, ground_slam, battle_cry, lightning_storm, precise_aim, lifesteal |
| harbor_village | 5 | 궁수/해양 | piercing_shot, backstep_shot, rain_of_arrows, hunter_focus, frost_nova |
| mountain_refuge | 4 | 고급/마법 | flame_wave, mana_shield, arcane_missile, execute |

총 26개. 권역 진출 = 새 스킬 카드 풀 해금.

## 7. game_balance.json zones 신규 등록

`Resources/Balance/game_balance.json` zones 섹션에 14개 zone 키 추가:

```json
"town_outskirts":    { "hpMul": 0.9,  "atkMul": 0.9,  "expMul": 1.0 },
"green_meadow":      { "hpMul": 1.05, "atkMul": 1.0,  "expMul": 1.2 },
"goblin_woods":      { "hpMul": 1.2,  "atkMul": 1.1,  "expMul": 1.3 },
"old_orc_road":      { "hpMul": 1.4,  "atkMul": 1.25, "expMul": 1.5 },
"ruined_crossroad":  { "hpMul": 2.2,  "atkMul": 1.7,  "expMul": 2.2 },
"harbor_outskirts":  { "hpMul": 3.0,  "atkMul": 2.2,  "expMul": 2.8 },
"crab_beach":        { "hpMul": 3.2,  "atkMul": 2.3,  "expMul": 3.0 },
"pirate_camp":       { "hpMul": 3.8,  "atkMul": 2.6,  "expMul": 3.5 },
"snowfield_edge":    { "hpMul": 5.0,  "atkMul": 3.2,  "expMul": 4.5 },
"frozen_valley":     { "hpMul": 6.5,  "atkMul": 4.0,  "expMul": 6.0 },
"volcano_approach":  { "hpMul": 7.0,  "atkMul": 4.5,  "expMul": 6.5 },
"lava_field":        { "hpMul": 8.5,  "atkMul": 5.2,  "expMul": 7.5 },
"harbor_village":    { "hpMul": 1.0,  "atkMul": 1.0,  "expMul": 1.0 },
"mountain_refuge":   { "hpMul": 1.0,  "atkMul": 1.0,  "expMul": 1.0 }
```

EnemySpawner는 씬 파일명(확장자 제거)을 zone 키로 사용해 `BalanceData.GetZone()`을 조회한다.

## 8. 기존 EnemySpawner API 통일

v1 작업 중 `field_4_harbor`, `field_5_snowfield`, `field_6_volcano`, `mine_3`, `dungeon_4_sunken_shrine` 5개 씬이 존재하지 않는 export 속성(`EnemyStatsList`, `SpawnCount`, `SpawnAreaSize`, `SpawnAreaOffset`)을 쓰고 있어 사실상 적이 스폰되지 않는 버그를 발견. 모두 표준 API(`StatVariants`, `SpawnInterval`, `MaxEnemies`, `SpawnRadius`, `BossStatVariant`, `BossId`)로 마이그레이션 완료.

## 8.1 Repeatable Boss 시스템

`EnemySpawner.RepeatableBoss` 플래그(2026-05-15 추가)로 first-kill 기록과 재출현 조건을 분리:

- **메인 챕터 보스 (1회용)**: dungeon_1(오크 워로드), dungeon_2(스켈레톤 킹), dungeon_3(고대 리치). RepeatableBoss=false. 1회 처치 후 GameManager.IsBossDefeated가 true가 되어 재스폰 차단.
- **반복 가능 야외/광산 보스**: dungeon_4(크라켄), field_5(글래시어 타이탄), field_6(인페르노 드레이크), mine_3(크리스탈 로드). RepeatableBoss=true. 첫 처치 기록은 GameManager.DefeatedBosses에 영구 저장되지만 BossKillThreshold마다 재출현하여 titan_scale/drake_eye 등 재료 파밍 루프 성립.

코드: `Scripts/Entities/Enemies/EnemySpawner.cs::TrySpawnBoss()`의 조건문 `if (!RepeatableBoss && GameManager.Instance?.IsBossDefeated(bossKey) == true) return;`.

## 8.2 mountain_refuge 귀환 경로

mountain_refuge 거점의 포탈:
- → harbor_village (귀환, 해양 권역으로 이동)
- → snowfield_edge (진출, 빙하 권역 시작)
- → field_outpost (직통 귀환, 거점 간 fast travel)

field_outpost 직통 포탈을 추가하여 산악 권역 깊은 곳에서 작업 중일 때 마을·전초기지로 빠르게 복귀 가능. harbor_village 경유는 여전히 유효 (긴 우회 경로지만 해양 권역에 들를 일이 있을 때 자연스러움).

## 9. 후속 작업 (아직 안 한 것)

- [ ] 신규 사냥터에 PNG 배경 자산 (현재 ColorRect + CanvasModulate + Label 플레이스홀더)
- [ ] 신규 사냥터에 광맥 노드(MiningNode) 배치
- [ ] 사냥터별 고유 드랍 테이블 정밀 튜닝 (현재는 기존 적의 DropChance/PossibleDrops 그대로)
- [ ] 챕터 시스템과 권역의 관계 재정리 (현재 챕터 플래그는 dungeon_1/2/3 보스 처치로만 트리거되므로 신규 권역은 챕터에 영향 없음)
- [ ] 사냥터별 한정 NPC(상인 외) 추가

## 10. 거부 사항 (이 시스템에 들어가지 않음)

- 자동전투 / 자동 사냥
- 가챠 / 시즌 패스 / BM
- 도감(Compendium) 시스템
- 절차적 던전 룸 생성
- 펫 동료
- 무한 탑 / 던전 모디파이어
- 일일 숙제 / 주간 토벌
