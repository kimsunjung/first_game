# 자산 교체 가이드

> 코덱스 메가 작업(`PLAN_DOC/codex-mega-task.md`) 진행 시 신규 .tres가 임시로 참조한 PNG path 목록.
> 사용자가 GPT로 신규 이미지 생성 후 이 가이드대로 각 .tres의 Icon/Sprite path를 교체.

## 사용 방법

1. 아래 표의 "교체 후 path"에 새 PNG 저장
2. 해당 .tres 파일 열어 Icon 또는 Sprite ext_resource path 변경
3. Godot 에디터에서 import 자동 처리 (또는 강제 reimport)

## 권장 사이즈 / 스타일

| 카테고리 | 사이즈 | 스타일 |
|---|---|---|
| 아이템 아이콘 | 128×128 | Pixel Crawler 톤, 어두운 배경 + 단일 라이트 |
| 적 스프라이트 | 256×256 | 톱뷰 단일 프레임, 짙은 그림자 |
| 보스 스프라이트 | 512×512 | 정면, 큼직한 실루엣 |
| NPC 스프라이트 | 128×128 | 4방향 idle 단일 프레임 |
| 환경 오브젝트 | 256×256 | 위에서 본 시점, 그림자 포함 |
| 텔레그래프 | (PNG 불필요) | Telegraph.cs가 _Draw로 도형 직접 그림 |

## 교체 대상 (코덱스가 등록하면서 채움)

### 적 스프라이트 — 항구·해안 (10개)

| 신규 적 | 임시 path | 교체 후 path |
|---|---|---|
| field4_pirate_grunt | (코덱스가 입력) | `Resources/Generated/GPT/Enemies/Field4/pirate_grunt.png` |
| field4_pirate_brute |  | `Field4/pirate_brute.png` |
| field4_pirate_sniper |  | `Field4/pirate_sniper.png` |
| field4_giant_crab |  | `Field4/giant_crab.png` |
| field4_seagull_swarm |  | `Field4/seagull_swarm.png` |
| dungeon4_drowned_sailor |  | `Enemies/Dungeon4/drowned_sailor.png` |
| dungeon4_siren |  | `Dungeon4/siren.png` |
| dungeon4_deep_lurker |  | `Dungeon4/deep_lurker.png` |
| dungeon4_coral_golem |  | `Dungeon4/coral_golem.png` |
| boss_dungeon4_kraken |  | `Enemies/NamedBosses/kraken.png` |

### 적 스프라이트 — 설원 (8개)

| 신규 적 | 임시 path | 교체 후 path |
|---|---|---|
| field5_frost_wolf |  | `Field5/frost_wolf.png` |
| field5_yeti |  | `Field5/yeti.png` |
| field5_ice_imp |  | `Field5/ice_imp.png` |
| field5_snow_witch |  | `Field5/snow_witch.png` |
| field5_polar_bear |  | `Field5/polar_bear.png` |
| field5_frost_archer |  | `Field5/frost_archer.png` |
| field5_icicle_elemental |  | `Field5/icicle_elemental.png` |
| field5_named_glacier_titan |  | `NamedBosses/glacier_titan.png` |

### 적 스프라이트 — 화산 (8개)

| 신규 적 | 임시 path | 교체 후 path |
|---|---|---|
| field6_lava_slime |  | `Field6/lava_slime.png` |
| field6_fire_imp |  | `Field6/fire_imp.png` |
| field6_salamander |  | `Field6/salamander.png` |
| field6_magma_golem |  | `Field6/magma_golem.png` |
| field6_phoenix_chick |  | `Field6/phoenix_chick.png` |
| field6_ember_archer |  | `Field6/ember_archer.png` |
| field6_lava_serpent |  | `Field6/lava_serpent.png` |
| field6_named_inferno_drake |  | `NamedBosses/inferno_drake.png` |

### 적 스프라이트 — mine_3 (6개)

| 신규 적 | 임시 path | 교체 후 path |
|---|---|---|
| mine_crystal_grunt |  | `Mine/crystal_grunt.png` |
| mine_crystal_archer |  | `Mine/crystal_archer.png` |
| mine_crystal_warlock |  | `Mine/crystal_warlock.png` |
| mine_crystal_brute |  | `Mine/crystal_brute.png` |
| mine_corrupted_miner |  | `Mine/corrupted_miner.png` |
| boss_mine3_crystal_lord |  | `NamedBosses/crystal_lord.png` |

### 무기 아이콘 (~18개 신규)

항구 6 + 설원 4 + 화산 4 + (필요 시) 추가. 예시:
| 신규 무기 | 임시 path | 교체 후 path |
|---|---|---|
| cutlass |  | `Icons/Items/cutlass.png` |
| harpoon |  | `Icons/Items/harpoon.png` |
| ... |  | ... |

### 갑옷·망토·벨트·장갑·투구·신발 (~60개)

테마 세트(화염/냉기/폭풍/암흑) × 8부위 + 기존 카테고리 확장. 코덱스가 등록 시 항목 추가.

### 소모품 (13개)

attack_potion, defense_potion, crit_potion, mega_health_potion, mega_mana_potion, revive_scroll, scroll_town, scroll_field_1~6, antidote_plus, holy_water, bait.

### NPC 스프라이트 (6명 + 환경)

| 신규 NPC | 임시 path | 교체 후 path |
|---|---|---|
| marta_innkeeper |  | `NPCs/Town/marta_innkeeper.png` |
| hansen_miner |  | `NPCs/Town/hansen_miner.png` |
| lily_florist |  | `NPCs/Town/lily_florist.png` |
| garret_guard |  | `NPCs/Town/garret_guard.png` |
| ivan_bard |  | `NPCs/Town/ivan_bard.png` |
| quest_board |  | `Objects/Town/quest_board.png` |

### 환경 오브젝트

| 신규 자산 | 임시 path | 교체 후 path |
|---|---|---|
| broken_ship |  | `Objects/Coast/broken_ship.png` |
| lighthouse |  | `Objects/Coast/lighthouse.png` |
| coral_outcrop |  | `Objects/Coast/coral_outcrop.png` |
| snow_drift |  | `Objects/Snowfield/snow_drift.png` |
| ice_pillar |  | `Objects/Snowfield/ice_pillar.png` |
| lava_pool |  | `Objects/Volcano/lava_pool.png` |
| ember_rock |  | `Objects/Volcano/ember_rock.png` |
| dungeon_entrance |  | `Objects/Dungeon/dungeon_entrance.png` |
| crystal_vein |  | `Objects/Mine/crystal_vein.png` |

## 자산 작업 우선순위 추천

1. **보스 5종** (kraken / glacier_titan / inferno_drake / crystal_lord / 그 외) — 가장 임팩트 큼
2. **던전 입구 sprite** — 게임 흐름 표지
3. **신규 적 ~32종** — 그룹별(필드별)로 일괄 생성
4. **무기 아이콘 ~18개** — UI에서 자주 노출
5. **테마 갑옷 세트** — 4 × 8 = 32개
6. **NPC + 환경 오브젝트** — 마을 분위기

## 교체 검증

PNG 교체 후 Godot 에디터에서 실행하면 자동 import. 단일 PNG 교체 시:
1. 새 PNG를 정확한 path에 저장
2. Godot에서 우클릭 → "리임포트"
3. 게임 실행 후 해당 적/아이템 시각 확인
