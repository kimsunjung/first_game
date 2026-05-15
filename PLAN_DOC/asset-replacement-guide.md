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

시트: `Resources/Generated/GPT/SourceSheets/Enemies/source_enemies_mine3_crystal_final_2026_05_15.png` (2×2, 1254×1254). 슬라이서: `slice_enemies_mine3_crystal_final_2026_05_15.py`. 본 시트는 4종(crystal_archer/warlock/brute, corrupted_miner)만 포함 — `mine_crystal_grunt`, `boss_mine3_crystal_lord`는 본 시트에 미포함.

| 신규 적 | 임시 path | 교체 후 path | 비고 |
|---|---|---|---|
| mine_crystal_grunt |  | `Mine/crystal_grunt.png` | 미완료 — 본 시트에 미포함, 후속 시트 필요 |
| mine_crystal_archer |  | `Mine/crystal_archer.png` | ✅ 완료 (2026-05-15) — `Resources/Enemies/mine_crystal_archer.tres` Sprite 연결 |
| mine_crystal_warlock |  | `Mine/crystal_warlock.png` | ✅ 완료 (2026-05-15) — `Resources/Enemies/mine_crystal_warlock.tres` Sprite 연결 |
| mine_crystal_brute |  | `Mine/crystal_brute.png` | ✅ 완료 (2026-05-15) — `Resources/Enemies/mine_crystal_brute.tres` Sprite 연결 |
| mine_corrupted_miner |  | `Mine/corrupted_miner.png` | ✅ 완료 (2026-05-15) — `Resources/Enemies/mine_corrupted_miner.tres` Sprite 연결 |
| boss_mine3_crystal_lord |  | `NamedBosses/crystal_lord.png` | 미완료 — 본 시트에 미포함, 후속 시트 필요 |

### 무기 아이콘 (~18개 신규)

항구 6 + 설원 4 + 화산 4 + (필요 시) 추가. 예시:
| 신규 무기 | 임시 path | 교체 후 path |
|---|---|---|
| cutlass |  | `Icons/Items/cutlass.png` |
| harpoon |  | `Icons/Items/harpoon.png` |
| ... |  | ... |

#### 화산/보스 보상/고급 무기 8종 (2026-05-15 완료)

시트: `Resources/Generated/GPT/SourceSheets/Items/source_weapon_icons_fire_boss_dark_2026_05_15.png` (4×2)
슬라이서: `Resources/Generated/GPT/SourceSheets/Items/slice_weapon_icons_fire_boss_dark_2026_05_15.py`

| 무기 | `.tres` | 이전 path (재사용 중이었음) | 교체 후 path | 비고 |
|---|---|---|---|---|
| kraken_trident | `Resources/Items/kraken_trident.tres` | `Icons/Items/halberd.png` | `Icons/Items/kraken_trident.png` | ✅ 완료 |
| flame_sword | `Resources/Items/flame_sword.tres` | `Icons/Items/golden_sword.png` | `Icons/Items/flame_sword.png` | ✅ 완료 |
| phoenix_bow | `Resources/Items/phoenix_bow.tres` | `Icons/Items/elven_bow.png` | `Icons/Items/phoenix_bow.png` | ✅ 완료 |
| magma_hammer | `Resources/Items/magma_hammer.tres` | `Icons/Items/arcane_hammer.png` | `Icons/Items/magma_hammer.png` | ✅ 완료 |
| inferno_staff | `Resources/Items/inferno_staff.tres` | `Icons/Items/shadow_staff.png` | `Icons/Items/inferno_staff.png` | ✅ 완료 |
| crystal_staff | `Resources/Items/crystal_staff.tres` | `Icons/Items/crystal_staff.png` | `Icons/Items/crystal_staff_v2.png` | ✅ 완료 — `starter_staff.tres`/`tide_staff.tres`는 기존 `crystal_staff.png` 그대로 사용 |
| dread_blade | `Resources/Items/dread_blade.tres` | `Icons/Items/dread_blade.png` | `Icons/Items/dread_blade_v2.png` | ✅ 완료 |
| void_scepter | `Resources/Items/void_scepter.tres` | `Icons/Items/void_scepter.png` | `Icons/Items/void_scepter_v2.png` | ✅ 완료 |

### 갑옷·망토·벨트·장갑·투구·신발 (~60개)

테마 세트(화염/냉기/폭풍/암흑) × 8부위 + 기존 카테고리 확장. 코덱스가 등록 시 항목 추가.

#### 화염/불사조 장비 8종 (2026-05-15 완료)

시트: `Resources/Generated/GPT/SourceSheets/Items/source_equipment_icons_flame_phoenix_2026_05_15.png` (4×2)
슬라이서: `Resources/Generated/GPT/SourceSheets/Items/slice_equipment_icons_flame_phoenix_2026_05_15.py`

| 장비 | `.tres` | 이전 path (재사용 중이었음) | 교체 후 path | 비고 |
|---|---|---|---|---|
| flame_armor | `Resources/Items/flame_armor.tres` | `Icons/Items/iron_armor.png` | `Icons/Equipment/flame_armor.png` | ✅ 완료 |
| flame_helm | `Resources/Items/flame_helm.tres` | `Icons/Items/helmet.png` | `Icons/Equipment/flame_helm.png` | ✅ 완료 |
| flame_gloves | `Resources/Items/flame_gloves.tres` | `Icons/Items/bracelet.png` | `Icons/Equipment/flame_gloves.png` | ✅ 완료 |
| flame_boots | `Resources/Items/flame_boots.tres` | `Icons/Items/boots.png` | `Icons/Equipment/flame_boots.png` | ✅ 완료 |
| flame_belt | `Resources/Items/flame_belt.tres` | `Icons/Items/leather_pouch.png` | `Icons/Equipment/flame_belt.png` | ✅ 완료 |
| flame_cloak | `Resources/Items/flame_cloak.tres` | `Icons/Items/iron_armor.png` | `Icons/Equipment/flame_cloak.png` | ✅ 완료 |
| phoenix_vest | `Resources/Items/phoenix_vest.tres` | `Icons/Equipment/ranger_vest.png` | `Icons/Equipment/phoenix_vest.png` | ✅ 완료 |
| ember_cloak | `Resources/Items/ember_cloak.tres` | `Icons/Items/iron_armor.png` | `Icons/Equipment/ember_cloak.png` | ✅ 완료 |

#### 서리/빙하 장비 8종 (2026-05-15 완료)

시트: `Resources/Generated/GPT/SourceSheets/Items/source_equipment_icons_frost_glacier_2026_05_15.png` (4×2)
슬라이서: `Resources/Generated/GPT/SourceSheets/Items/slice_equipment_icons_frost_glacier_2026_05_15.py`

| 장비 | `.tres` | 이전 path (재사용 중이었음) | 교체 후 path | 비고 |
|---|---|---|---|---|
| frost_armor | `Resources/Items/frost_armor.tres` | `Icons/Equipment/chainmail_armor.png` | `Icons/Equipment/frost_armor.png` | ✅ 완료 |
| frost_helm | `Resources/Items/frost_helm.tres` | `Icons/Items/dark_helm.png` | `Icons/Equipment/frost_helm.png` | ✅ 완료 |
| frost_gloves | `Resources/Items/frost_gloves.tres` | `Icons/Items/bracelet.png` | `Icons/Equipment/frost_gloves.png` | ✅ 완료 |
| glacier_boots | `Resources/Items/glacier_boots.tres` | `Icons/Items/iron_boots.png` | `Icons/Equipment/glacier_boots.png` | ✅ 완료 |
| frost_belt | `Resources/Items/frost_belt.tres` | `Icons/Items/leather_pouch.png` | `Icons/Equipment/frost_belt.png` | ✅ 완료 |
| frost_cloak | `Resources/Items/frost_cloak.tres` | `Icons/Items/frost_staff.png` | `Icons/Equipment/frost_cloak.png` | ✅ 완료 |
| frost_vest | `Resources/Items/frost_vest.tres` | `Icons/Equipment/ranger_vest.png` | `Icons/Equipment/frost_vest.png` | ✅ 완료 |
| frost_robe | `Resources/Items/frost_robe.tres` | `Icons/Equipment/mystic_robe.png` | `Icons/Equipment/frost_robe.png` | ✅ 완료 |

#### 폭풍/번개 장비 8종 (2026-05-15 완료)

시트: `Resources/Generated/GPT/SourceSheets/Items/source_equipment_icons_storm_2026_05_15.png` (4×2)
슬라이서: `Resources/Generated/GPT/SourceSheets/Items/slice_equipment_icons_storm_2026_05_15.py`

| 장비 | `.tres` | 이전 path (재사용 중이었음) | 교체 후 path | 비고 |
|---|---|---|---|---|
| storm_armor | `Resources/Items/storm_armor.tres` | `Icons/Equipment/knight_plate_armor.png` | `Icons/Equipment/storm_armor.png` | ✅ 완료 |
| storm_helm | `Resources/Items/storm_helm.tres` | `Icons/Equipment/steel_helm.png` | `Icons/Equipment/storm_helm.png` | ✅ 완료 |
| storm_gloves | `Resources/Items/storm_gloves.tres` | `Icons/Items/bracelet.png` | `Icons/Equipment/storm_gloves.png` | ✅ 완료 |
| storm_boots | `Resources/Items/storm_boots.tres` | `Icons/Equipment/knight_boots.png` | `Icons/Equipment/storm_boots.png` | ✅ 완료 |
| storm_belt | `Resources/Items/storm_belt.tres` | `Icons/Items/leather_pouch.png` | `Icons/Equipment/storm_belt.png` | ✅ 완료 |
| storm_cloak | `Resources/Items/storm_cloak.tres` | `Icons/Items/sapphire_ring.png` | `Icons/Equipment/storm_cloak.png` | ✅ 완료 |
| storm_vest | `Resources/Items/storm_vest.tres` | `Icons/Equipment/ranger_vest.png` | `Icons/Equipment/storm_vest.png` | ✅ 완료 |
| storm_robe | `Resources/Items/storm_robe.tres` | `Icons/Equipment/mystic_robe.png` | `Icons/Equipment/storm_robe.png` | ✅ 완료 |

#### 암흑/공허 장비 8종 (2026-05-15 완료)

시트: `Resources/Generated/GPT/SourceSheets/Items/source_equipment_icons_dark_void_2026_05_15.png` (4×2)
슬라이서: `Resources/Generated/GPT/SourceSheets/Items/slice_equipment_icons_dark_void_2026_05_15.py`

기존 `Icons/Items/dark_helm.png`는 의도적으로 유지(삭제·덮어쓰기 금지). 신규 `Icons/Equipment/dark_helm.png`를 별도 생성해 `dark_helm.tres`에 연결. `frost_helm.tres`는 본 작업과 무관(이미 별도 path 사용 중).

| 장비 | `.tres` | 이전 path (재사용 중이었음) | 교체 후 path | 비고 |
|---|---|---|---|---|
| dark_armor | `Resources/Items/dark_armor.tres` | `Icons/Items/iron_armor.png` | `Icons/Equipment/dark_armor.png` | ✅ 완료 |
| dark_helm | `Resources/Items/dark_helm.tres` | `Icons/Items/dark_helm.png` | `Icons/Equipment/dark_helm.png` | ✅ 완료 — 기존 `Icons/Items/dark_helm.png`는 보존 |
| dark_gloves | `Resources/Items/dark_gloves.tres` | `Icons/Items/shadow_ring.png` | `Icons/Equipment/dark_gloves.png` | ✅ 완료 |
| dark_boots | `Resources/Items/dark_boots.tres` | `Icons/Equipment/shadow_boots.png` | `Icons/Equipment/dark_boots.png` | ✅ 완료 |
| dark_belt | `Resources/Items/dark_belt.tres` | `Icons/Items/leather_pouch.png` | `Icons/Equipment/dark_belt.png` | ✅ 완료 |
| dark_cloak | `Resources/Items/dark_cloak.tres` | `Icons/Items/shadow_ring.png` | `Icons/Equipment/dark_cloak.png` | ✅ 완료 |
| dark_vest | `Resources/Items/dark_vest.tres` | `Icons/Equipment/ranger_vest.png` | `Icons/Equipment/dark_vest.png` | ✅ 완료 |
| dark_robe | `Resources/Items/dark_robe.tres` | `Icons/Equipment/mystic_robe.png` | `Icons/Equipment/dark_robe.png` | ✅ 완료 |

### 소모품 (13개)

attack_potion, defense_potion, crit_potion, mega_health_potion, mega_mana_potion, revive_scroll, scroll_town, scroll_field_1~6, antidote_plus, holy_water, bait.

#### 전투/유틸 소모품 8종 (2026-05-15 완료)

시트: `Resources/Generated/GPT/SourceSheets/Items/source_consumable_icons_combat_utility_2026_05_15.png` (4×2, Codex 생성)
슬라이서: `Resources/Generated/GPT/SourceSheets/Items/slice_consumable_icons_combat_utility_2026_05_15.py`

| 소모품 | `.tres` | 이전 path (재사용 중이었음) | 교체 후 path | 비고 |
|---|---|---|---|---|
| attack_potion | `Resources/Items/attack_potion.tres` | `Icons/Items/guard_potion.png` | `Icons/Items/attack_potion.png` | ✅ 완료 |
| defense_potion | `Resources/Items/defense_potion.tres` | `Icons/Items/guard_potion.png` | `Icons/Items/defense_potion.png` | ✅ 완료 |
| crit_potion | `Resources/Items/crit_potion.tres` | `Icons/Items/hi_potion.png` | `Icons/Items/crit_potion.png` | ✅ 완료 |
| mega_health_potion | `Resources/Items/mega_health_potion.tres` | `Icons/Items/mega_potion.png` | `Icons/Items/mega_health_potion.png` | ✅ 완료 |
| mega_mana_potion | `Resources/Items/mega_mana_potion.tres` | `Icons/Items/mana_potion.png` | `Icons/Items/mega_mana_potion.png` | ✅ 완료 |
| revive_scroll | `Resources/Items/revive_scroll.tres` | `Icons/Items/return_scroll.png` | `Icons/Items/revive_scroll.png` | ✅ 완료 |
| antidote_plus | `Resources/Items/antidote_plus.tres` | `Icons/Items/antidote.png` | `Icons/Items/antidote_plus.png` | ✅ 완료 |
| holy_water | `Resources/Items/holy_water.tres` | `Icons/Items/hi_potion.png` | `Icons/Items/holy_water.png` | ✅ 완료 |

미완료(본 시트 미포함): `scroll_town`, `scroll_field_1~6`, `bait` — 후속 시트 필요.

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

확장 지역 환경 오브젝트 8종은 2026-05-15에 Codex가 생성한 시트(`Resources/Generated/GPT/SourceSheets/Objects/source_environment_objects_expansion_2026_05_15.png`, 4×2)를 슬라이싱해 등록 완료. 슬라이서: `slice_environment_objects_expansion_2026_05_15.py`. **PNG 리소스만 등록**, `.tscn` 배치·CollisionShape2D·Area2D·포털/퀘스트/상호작용 로직 미추가 — 상태 `리소스만 등록`. `crystal_vein`은 본 시트에 없음 (미완료).

| 신규 자산 | 임시 path | 교체 후 path | 비고 |
|---|---|---|---|
| broken_ship |  | `Objects/Coast/broken_ship.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| lighthouse |  | `Objects/Coast/lighthouse.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| coral_outcrop |  | `Objects/Coast/coral_outcrop.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| snow_drift |  | `Objects/Snowfield/snow_drift.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| ice_pillar |  | `Objects/Snowfield/ice_pillar.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| lava_pool |  | `Objects/Volcano/lava_pool.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| ember_rock |  | `Objects/Volcano/ember_rock.png` | ✅ 리소스 등록 완료 (2026-05-15) |
| dungeon_entrance |  | `Objects/Dungeon/dungeon_entrance.png` | ✅ 리소스 등록 완료 (2026-05-15) — 기존 `Icons/Interaction/dungeon_entrance.png` 및 `Objects/World/dungeon_entrance_gate.png`와 별도 자산 |
| crystal_vein |  | `Objects/Mine/crystal_vein.png` | ✅ 리소스 등록 완료 (2026-05-15, 후속 시트). 맵 배치/채집 로직 미추가 — 상태 `리소스만 등록`. 기존 `Objects/World/mining_ore_vein.png`(런타임 OreItem.Icon 사용)와 별도 신규 자산. |

#### 광산/던전/필드 환경 오브젝트 후속 7종 (2026-05-15 리소스 등록)

시트: `Resources/Generated/GPT/SourceSheets/Objects/source_environment_objects_mine_dungeon_field_2026_05_15.png` (4×2, Codex 생성). 슬라이서: `slice_environment_objects_mine_dungeon_field_2026_05_15.py`. **PNG 리소스만 등록**, `.tscn` 배치·CollisionShape2D·Area2D·채집·상호작용·포털·퀘스트 로직 미추가 — 모두 상태 `리소스만 등록`.

| 신규 자산 | 카테고리 | 교체 후 path | 상태 | 후속 배치 후보 |
|---|---|---|---|---|
| mine_cart | Mine | `Objects/Mine/mine_cart.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | `mine_1`/`mine_2.tscn` 장식 |
| ore_crates | Mine | `Objects/Mine/ore_crates.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | 광산 입구·저장소 장식 |
| dungeon_brazier | Dungeon | `Objects/Dungeon/dungeon_brazier.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | `dungeon_1~3.tscn` 통로/보스 챔버 장식 |
| broken_pillar | Dungeon | `Objects/Dungeon/broken_pillar.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | 던전 폐허 분위기 장식 |
| rune_stone | Dungeon | `Objects/Dungeon/rune_stone.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | 보스 봉인/챕터 플래그 표식 — 상호작용 시스템 미추가 |
| field_camp_tent | Field | `Objects/Field/field_camp_tent.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | `field_outpost`/`field_1~3` 휴식 포인트 |
| signpost_waypoint | Field | `Objects/Field/signpost_waypoint.png` | ✅ 리소스 등록 완료 (맵 배치 보류) | 필드 분기점 표지 — 텍스트/툴팁 시스템 별도 |

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
