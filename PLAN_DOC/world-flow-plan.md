# World Flow Plan v2 — 오픈엔드 사냥 RPG 권역 구조

> 작성: 2026-05-15 (v1) | 갱신: 2026-05-15 (v2, Regional Hunting World Full Build)
>
> v2 차이: "엔딩형 RPG" 가정 폐기. 미르의전설3/클래식 PC RPG식 **오픈엔드 사냥 RPG**로 재정의.
> v1의 챕터 보스(고대 리치)는 여전히 존재하지만 **메인 엔딩 트리거가 아니라 outpost_region의 정점 보스**일 뿐. 그 뒤에도 coast_region / mountain_region이 펼쳐진다.
>
> 자세한 권역 설계는 `regional-world-map-plan.md` 참조.

## 핵심 게임 사이클

게임은 **하나의 엔딩으로 수렴하지 않는다**. 다음 사이클을 무한 반복하며 캐릭터가 성장한다:

```
거점 보급 → 사냥터 진출 → 적 처치/드랍 → 거점 복귀(강화/장비/스킬 갱신) → 더 강한 사냥터 진출
```

보스 처치는 게임 종료가 아니라 **그 권역의 졸업 신호**다. 다음 권역으로 이동할 준비가 됐다는 뜻.

## 전체 월드 흐름도

```
town (허브 — NPC 다수, 기본 SkillShop)
 ├─ field_1 (입문 사냥터, Lv.1~5, 기존)
 │    ├─ dungeon_1 (오크 워로드 — town_region 보스)
 │    └─ mine_1 (광산, Lv.3~7)
 │
 ├─ town_outskirts (NEW, Lv.1~3) → green_meadow → goblin_woods → old_orc_road → field_outpost
 │
 └─ field_outpost (분기 허브 — 중급 SkillShop)
       ├─ field_2 (graveyard_edge, Lv.10~13)
       │    ├─ dungeon_2 (스켈레톤 킹 — outpost_region 중간 보스)
       │    ├─ mine_2 (Lv.8~12)
       │    └─ ruined_crossroad (NEW, Lv.13~15) → field_3
       │
       ├─ field_3 (저주 폐허, Lv.15~18)
       │    └─ dungeon_3 (고대 리치 — outpost_region 정점 보스, 엔딩 아님)
       │
       ├─ harbor_village (NEW 거점 — 궁수/해양 SkillShop)
       │    └─ harbor_outskirts → crab_beach → pirate_camp → field_4_harbor → dungeon_4_sunken_shrine (크라켄)
       │
       └─ mountain_refuge (NEW 거점 — 고급 SkillShop)
              ├─ snowfield_edge → frozen_valley → field_5_snowfield (글래시어 타이탄)
              └─ volcano_approach → lava_field → field_6_volcano (인페르노 드레이크) → mine_3 (크리스탈 로드)
```

## 권역 구조 (Region)

| 권역 | 거점 Hub | 진행 레벨대 | 종착 보스 (졸업 신호) |
|---|---|---|---|
| town_region | town | 1-10 | 오크 워로드 (dungeon_1) |
| outpost_region | field_outpost | 10-18 | 고대 리치 (dungeon_3) |
| coast_region | harbor_village | 15-22 | 크라켄 (dungeon_4_sunken_shrine) |
| mountain_region | mountain_refuge | 20-38 | 크리스탈 로드 (mine_3) |

**보스 재출현 규칙** (EnemySpawner.RepeatableBoss 플래그):

| 보스 | RepeatableBoss | 의미 |
|---|---|---|
| 오크 워로드 (dungeon_1) | `false` (기본값) | 1회 처치 후 사라짐. 메인 챕터 진행 표지자. |
| 스켈레톤 킹 (dungeon_2) | `false` | 동일 |
| 고대 리치 (dungeon_3) | `false` | 동일 |
| 크라켄 (dungeon_4_sunken_shrine) | `true` | 첫 처치 기록은 GameManager.DefeatedBosses에 영구 남되, BossKillThreshold마다 재출현하여 파밍 가능 |
| 글래시어 타이탄 (field_5_snowfield) | `true` | 동일 |
| 인페르노 드레이크 (field_6_volcano) | `true` | 동일 |
| 크리스탈 로드 (mine_3) | `true` | 동일 |

→ 메인 챕터 보스는 1회 처치 후 봉인. 야외/광산 Named 보스는 first-kill 해금 기록과 별도로 재처치 가능 (재료 파밍 루프).

## 지역별 정의 (요약)

전체 상세 표는 `regional-world-map-plan.md` 참조. 본 문서는 v1에서 정의되어 있던 기존 14지역 + v2 추가 14지역의 통합 상태만 기록.

### town_region

| 지역 | Scene | 권장 Lv | 역할 |
|---|---|---|---|
| 마을 | town.tscn | - | 게임 시작 허브 |
| 마을 외곽 | town_outskirts.tscn | 1-3 | 초보 골드/포션 파밍 |
| 푸른 초원 | green_meadow.tscn | 3-5 | 자연/가죽/나무 |
| 고블린 숲 | goblin_woods.tscn | 5-7 | 도적형/궁수 적 |
| 옛 오크 길 | old_orc_road.tscn | 7-10 | 오크 재료 |
| 필드 1 | field_1.tscn | 1-5 | 입문 사냥 (튜토리얼) |
| 광산 1 | mine_1.tscn | 3-7 | 구리/철광 채광 |
| 던전 1 | dungeon_1.tscn | 5-8 | 오크 워로드 |

### outpost_region

| 지역 | Scene | 권장 Lv | 역할 |
|---|---|---|---|
| 전초기지 | field_outpost.tscn | - | 분기 허브 |
| 묘지 변경 | field_2.tscn | 10-13 | 언데드/뼈가루 |
| 폐허 교차로 | ruined_crossroad.tscn | 13-15 | 중급 장비/포션 재료 |
| 필드 3 | field_3.tscn | 15-18 | 정점 보스 직전 정비 |
| 광산 2 | mine_2.tscn | 8-12 | 은광/심해광 |
| 던전 2 | dungeon_2.tscn | 10-14 | 스켈레톤 킹 |
| 던전 3 | dungeon_3.tscn | 16-22 | 고대 리치 (outpost 정점) |

### coast_region

| 지역 | Scene | 권장 Lv | 역할 |
|---|---|---|---|
| 항구마을 | harbor_village.tscn | - | 해양 권역 거점 |
| 항구 외곽 | harbor_outskirts.tscn | 15-17 | 갈매기/게/해적 |
| 게 해변 | crab_beach.tscn | 16-18 | sea_kelp 파밍 |
| 해적 야영지 | pirate_camp.tscn | 18-20 | 해적 장비 |
| 항구 | field_4_harbor.tscn | 18-22 | dungeon_4 진입 |
| 수몰 신전 | dungeon_4_sunken_shrine.tscn | 18-24 | 크라켄 |

### mountain_region

| 지역 | Scene | 권장 Lv | 역할 |
|---|---|---|---|
| 산악 피난처 | mountain_refuge.tscn | - | 산악 권역 거점 |
| 설원 변경 | snowfield_edge.tscn | 20-23 | 빙하 적응 |
| 빙결 계곡 | frozen_valley.tscn | 22-25 | glacier_shard |
| 설원 | field_5_snowfield.tscn | 25-28 | 빙하 타이탄 |
| 화산 진입로 | volcano_approach.tscn | 26-29 | 화염 적응 |
| 용암 평원 | lava_field.tscn | 28-32 | lava_stone |
| 화산 | field_6_volcano.tscn | 30-35 | 인페르노 드레이크 |
| 광산 3 | mine_3.tscn | 32-38 | 크리스탈 로드 |

## 포탈 연결 (실제 구현)

권역 간 포탈은 **양방향**으로 배치된다. 거점에서 사냥터로, 사냥터에서 거점으로 자유 이동.

| 출발 | 목적지 | 방향 |
|---|---|---|
| town | field_1 / town_outskirts / field_outpost / field_4_harbor | 다방향 |
| town_outskirts ↔ green_meadow ↔ goblin_woods ↔ old_orc_road ↔ field_outpost | 양방향 체인 |
| field_outpost | town / old_orc_road / field_2 / harbor_village / mountain_refuge | 다방향 |
| field_2 ↔ ruined_crossroad ↔ field_3 | 양방향 |
| field_3 → harbor_village / mountain_refuge | 깊은 진입 분기 |
| harbor_village ↔ harbor_outskirts ↔ crab_beach ↔ pirate_camp ↔ field_4_harbor ↔ dungeon_4_sunken_shrine | 양방향 체인 |
| mountain_refuge ↔ snowfield_edge ↔ frozen_valley ↔ field_5_snowfield | 양방향 체인 |
| field_5_snowfield ↔ volcano_approach ↔ lava_field ↔ field_6_volcano ↔ mine_3 | 양방향 체인 |
| town의 TeleportNPC | 모든 핵심 지역 (scroll 아이템 기반 fast travel) | 별도 |

## 스킬샵 분산 (v2 변경)

v1에서는 모든 스킬북(26종)이 town SkillShopUI에 모여 있어 진행감이 없었다. v2에서는 거점별로 분산:

| 거점 | 스킬북 카테고리 | 개수 |
|---|---|---|
| town | 기본/입문 | 10 |
| field_outpost | 중급/전투 | 7 |
| harbor_village | 궁수/해양 | 5 |
| mountain_refuge | 고급/마법 | 4 |

목록 상세는 `regional-world-map-plan.md` §6 참조.

## 마을 상점 단계별 해금 (현재 미구현 — v3 후보)

ShopNPC.ShopItems는 현재 정적 배열이다. 챕터 플래그 기반 동적 필터링은 후속 작업.

| 진행 단계 | 새로 해금되는 아이템 |
|-----------|---------------------|
| 게임 시작 | health_potion, mana_potion, antidote, return_scroll |
| dungeon_1 완료 | hi_potion, swift_potion, defense_potion |
| field_outpost 방문 | guard_potion, revive_scroll, antidote_plus |
| dungeon_2 완료 | mega_health_potion, mega_mana_potion, crit_potion |
| harbor_village 방문 | tidal_tonic, mana_herb_extract |
| mountain_refuge 방문 | warming_draught, defrost_potion, fire_resist_elixir, magma_brew, battle_elixir, curse_water |

## "엔딩" 표현 정리 (v1 → v2 마이그레이션 기록)

| v1 표현 | v2 표현 |
|---|---|
| "최종 보스" | "권역 정점 보스" 또는 단순히 "보스" |
| "메인 엔딩 트리거" | "outpost_region 졸업 신호" |
| "메인 엔딩 시퀀스 시작" | "추가 권역 해금 / 보상 연출" |
| "dungeon_X 클리어" | "dungeon_X 완료" 또는 "dungeon_X 보스 처치" |
| "챕터3/최종 보스 던전" | "outpost_region 정점 던전" |

`Scripts/Data/ChapterDialogue.cs`의 챕터 플래그 시스템은 그대로 유지하되, 챕터는 **스토리 진행 표지자**이지 **게임 종료 트리거**가 아닌 것으로 의미 변경. ChapterFlag.Lich_Killed 후에도 캐릭터 성장과 상위 권역 콘텐츠가 계속된다.
