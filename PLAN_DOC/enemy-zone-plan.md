# 지역별 적 배치 계획 (Enemy Zone Plan)

작성: 2026-05-13
배경: field/dungeon 6개 맵이 오크/스켈레톤 위주로 겹쳐 지역별 정체성이 약함. 새 이미지 없이 우선 현재 EnemyStats 리소스로 임시 배치를 정리하고, 후속 GPT 이미지 작업이 가리킬 신규 적을 표로 명시한다.

난이도 순서: `field_1 < mine_1 < dungeon_1 < mine_2 < field_2 < dungeon_2 < field_3 < dungeon_3`

mine_2는 mine_1보다 깊은 광산이며 보스가 없고 일반 몹 가중치가 dungeon_1보다 살짝 높게 배치된다(`game_balance.json` zones: hpMul 2.0 / atkMul 1.6 / expMul 1.7). mine_1은 광산 적 등록 작업 PR이 EnemySpawner 가중치를 별도로 조정 중이라 이 문서의 임시 배치 컬럼은 비워둔다.

## 지역별 표

| 지역 | 컨셉 | 변경 전 (Before) | 임시 배치 (현 PR) | 목표 배치 (필요 신규 이미지 포함) | 보스 | 비고 |
|---|---|---|---|---|---|---|
| `field_1` | 초원/초반 사냥터 — 가장 쉬운 야외 | orc_basic, orc_warrior, orc_rogue (오크 3종) | **field1_slime + wild_wolf + goblin_scout + orc_scout + wild_boar** (5종 균등) | 좌동 + forest_spider/spirit/hobgoblin_guard (가중치 시스템 도착 시) | 없음 | orc_basic은 dungeon_1로 이동. forest_spider/spirit/hobgoblin_guard는 리소스+PNG 등록 완료, 맵 미배치 (균등 랜덤이라 강한몹/희귀몹 분포 곤란) |
| `mine_1` | 광산 입구 — Iron/Silver/Gold 채광 | zombie_basic | (이번 PR 변경 X — 광산 적 작업 PR이 다룸) | mine_zombie_basic/fast/armored 위주 + skeleton_miner 일부 | 없음 | 광산 적 8종(mine_*.tres) 등록 작업과 충돌 방지 |
| `dungeon_1` | 오크 소굴 — 오크 계열 첫 던전 | orc_warrior + orc_shaman (orc_basic 누락) | **dungeon1_orc_club + orc_axe_warrior + orc_rogue + orc_shaman** (4종 균등, 신규 PNG) | 좌동 + orc_brute/goblin_trapper/orc_captain 정예 (가중치 시스템 도착 시) | **`boss_orc_warlord_boss`** (BossId="orc_warlord_d1"로 교체) — 기존 `boss_orc_king` 처치 기록 폐기(개발 중 결정) | 기존 orc_basic/warrior/rogue/shaman/boss_orc_king은 보존(미배치). orc_brute/goblin_trapper/orc_captain은 리소스+PNG만 등록 |
| `field_2` | 묘지 외곽/황폐 필드 — 약한 언데드 중반 초입 | skeleton_base + skeleton_warrior + skeleton_rogue | **skeleton_wanderer + skeleton_archer + zombie_walker + grave_slime + cursed_wolf** (5종 균등, 신규 PNG) | 좌동 + ghoul/bone_soldier/grave_wraith 정예 (가중치 시스템 도착 시) | 없음 | 기존 skeleton_base/warrior/rogue 보존(미배치). ghoul/bone_soldier/grave_wraith는 리소스+PNG만 등록 |
| `dungeon_2` | 지하 묘지 — 본격 스켈레톤 던전 | skeleton_warrior + skeleton_mage + skeleton_rogue | **dungeon2_skeleton_warrior + skeleton_mage + skeleton_rogue + bone_archer + ghoul_brute** (5종 균등, 신규 PNG) | 좌동 + bone_knight/crypt_wraith/skeleton_champion 정예 (가중치 시스템 도착 시) | `boss_skeleton_king` (BossId="skeleton_king_d2", 유지) | 기존 skeleton_warrior/mage/rogue 보존(미배치). skeleton_champion은 네임드/보스 후보 보관 |
| `field_3` | 저주받은 황무지 — 고급 언데드/타락 기사 | skeleton_warrior + skeleton_mage + skeleton_rogue (dungeon_2와 동일) | **field3_cursed_soldier + dark_wolf + shadow_bat + bone_hound + plague_ghoul** (5종 균등, 신규 PNG) | 좌동 + fallen_knight/ruin_golem/cursed_banner_wraith 정예 (가중치 시스템 도착 시) | 없음 | 기존 skeleton_warrior/mage 보존(미배치). fallen_knight/ruin_golem/cursed_banner_wraith는 리소스+PNG만 등록 |
| `dungeon_3` | 심연 던전 — 최종급 언데드/심연 계열 | skeleton_warrior + skeleton_mage + skeleton_rogue + boss_skeleton_king 재사용 | **abyss_wraith + shadow_assassin + cursed_warlock + abyss_hound** (4종 균등) | 좌동 + death_knight/bone_golem/ancient_lich(엘리트)/dungeon_guardian 가중치 시스템 도착 후 추가 | **`boss_dungeon3_ancient_lich`** (BossId="ancient_lich_d3"로 교체) | 보스 교체로 기존 `skeleton_king_d3` 처치 기록 폐기(개발 중 결정) — 이전에 d3 보스 잡았던 세이브는 ancient_lich를 다시 잡아야 함 |

## 필요 신규 이미지 (GPT 생성 우선순위)

| 순위 | 적 종류 | 사용 지역 | 비고 |
|---|---|---|---|
| 1 | ~~`boss_ancient_lich`~~ | dungeon_3 | **완료** — `boss_dungeon3_ancient_lich.tres` 등록 + dungeon_3 BossStatVariant 교체 |
| 2 | ~~`slime`, `goblin`, `wolf`~~ | field_1 | **완료** — slime/wild_wolf/goblin_scout/orc_scout/wild_boar 배치, forest_spider/spirit/hobgoblin_guard는 리소스만 |
| 3 | ~~`fallen_knight`, `wraith_field`~~ | field_3 | **완료** — cursed_soldier/dark_wolf/shadow_bat/bone_hound/plague_ghoul 배치, fallen_knight/ruin_golem/cursed_banner_wraith는 리소스만 |
| 4 | ~~`zombie_field_basic`, `zombie_field_runner`~~ | field_2 | **완료** — skeleton_wanderer/skeleton_archer/zombie_walker/grave_slime/cursed_wolf 배치, ghoul/bone_soldier/grave_wraith는 리소스만 |
| 5 | ~~`abyssal_wraith`, `shadow_assassin`~~ | dungeon_3 | **완료** — abyss_wraith/shadow_assassin/cursed_warlock/abyss_hound 배치, death_knight/bone_golem/ancient_lich(엘리트)/dungeon_guardian 리소스만 |
| 6 | ~~`skeleton_guardian`~~ | dungeon_2 | **완료** — dungeon2_skeleton_warrior/mage/rogue/bone_archer/ghoul_brute 배치, bone_knight/crypt_wraith/skeleton_champion은 리소스만 |

## 필요 신규 EnemyStats 리소스 (이미지 도착 시 동반 생성)

- ~~`Resources/Enemies/slime.tres`~~ → 등록 완료 (`field1_slime.tres`)
- ~~`Resources/Enemies/wolf.tres`~~ → 등록 완료 (`field1_wild_wolf.tres`)
- ~~`Resources/Enemies/goblin.tres`~~ → 등록 완료 (`field1_goblin_scout.tres`)
- 추가 등록 완료: `field1_orc_scout.tres`, `field1_wild_boar.tres`, `field1_forest_spider.tres`, `field1_forest_spirit.tres`, `field1_hobgoblin_guard.tres` (뒤 3종은 맵 미배치)
- ~~`Resources/Enemies/fallen_knight.tres`~~ → 등록 완료 (`field3_fallen_knight.tres`, 정예, 맵 미배치)
- ~~`Resources/Enemies/wraith_field.tres`~~ → field_3는 `field3_cursed_banner_wraith.tres`로 대체 (정예, 맵 미배치)
- 추가 등록 완료 (field_3): `field3_cursed_soldier.tres`, `field3_dark_wolf.tres`, `field3_shadow_bat.tres`, `field3_bone_hound.tres`, `field3_plague_ghoul.tres`, `field3_ruin_golem.tres` (ruin_golem은 맵 미배치)
- ~~`Resources/Enemies/zombie_field_basic.tres`~~ → field_2는 field2_zombie_walker.tres로 대체
- ~~`Resources/Enemies/zombie_field_runner.tres`~~ → field_2는 field2_cursed_wolf 등으로 대체
- 추가 등록 완료 (field_2): `field2_skeleton_wanderer.tres`, `field2_skeleton_archer.tres`, `field2_zombie_walker.tres`, `field2_grave_slime.tres`, `field2_cursed_wolf.tres`, `field2_ghoul.tres`, `field2_bone_soldier.tres`, `field2_grave_wraith.tres` (뒤 3종은 맵 미배치)
- ~~`Resources/Enemies/abyssal_wraith.tres`~~ → 등록 완료 (`dungeon3_abyss_wraith.tres`)
- ~~`Resources/Enemies/shadow_assassin.tres`~~ → 등록 완료 (`dungeon3_shadow_assassin.tres`)
- ~~`Resources/Enemies/ancient_lich.tres`~~ → 등록 완료 (`dungeon3_ancient_lich.tres`, 엘리트 일반급, 맵 미배치)
- ~~`Resources/Enemies/boss_ancient_lich.tres`~~ → 등록 완료 (`boss_dungeon3_ancient_lich.tres`, dungeon_3 보스 교체)
- 추가 등록 완료: `dungeon3_cursed_warlock.tres`, `dungeon3_abyss_hound.tres`, `dungeon3_death_knight.tres`, `dungeon3_bone_golem.tres`, `dungeon3_dungeon_guardian.tres` (death_knight/bone_golem/dungeon_guardian은 맵 미배치)

## 보스 처치 ID 충돌 처리

EnemyController는 `BossId` 필드를 .tscn EnemySpawner에서 주입받아 `GameManager.RecordBossDefeat(bossKey)` 호출 시 사용한다. 같은 `boss_skeleton_king.tres`를 d2/d3가 공유해도 BossId가 각각 `"skeleton_king_d2"` / `"skeleton_king_d3"`로 분리되므로 처치 기록이 충돌하지 않는다 — 현 코드에 이미 반영됨.

신규 `boss_ancient_lich.tres` 도입 시 d3의 BossId를 `"ancient_lich_d3"` 등으로 갱신하면 기존 d3 진행 세이브에서 보스 재처치 가능(BossId가 바뀌면 새 키이므로). 호환성 위해 마이그레이션 시 d3 정복 기록 보존 여부는 별도 결정.

**적용 완료 (2026-05-13)**: dungeon_3 보스 `boss_skeleton_king`(BossId `"skeleton_king_d3"`) → `boss_dungeon3_ancient_lich`(BossId `"ancient_lich_d3"`)로 교체. 개발 중이라 처치 기록 마이그레이션은 생략 — `skeleton_king_d3` 처치 기록은 폐기되고 새 보스를 다시 잡아야 한다. `dungeon_2`의 `boss_skeleton_king`/`"skeleton_king_d2"`는 그대로 유지.

## 변경 전 → 변경 후 한눈 요약

```
field_1   orc_basic + warrior + rogue          →  field1_slime + wild_wolf + goblin_scout + orc_scout + wild_boar (5종 균등)
mine_1    (변경 없음 — 별도 PR)
dungeon_1 orc_warrior + shaman                 →  dungeon1_orc_club + axe_warrior + rogue + shaman (4종 균등, 신규 PNG)
field_2   skel_base + warrior + rogue          →  skeleton_wanderer + archer + zombie_walker + grave_slime + cursed_wolf (5종 균등, 신규 PNG)
dungeon_2 skel_warrior + mage + rogue          →  dungeon2_skeleton_warrior + mage + rogue + bone_archer + ghoul_brute (5종 균등, 신규 PNG)
field_3   skel_warrior + mage + rogue (==d2)   →  field3_cursed_soldier + dark_wolf + shadow_bat + bone_hound + plague_ghoul (5종 균등, 신규 PNG)
dungeon_3 skel_warrior + mage + rogue (==d2)   →  abyss_wraith + shadow_assassin + cursed_warlock + abyss_hound (4종 균등) + boss_dungeon3_ancient_lich (보스 교체)
```

## 네임드/보스 후보 리소스 (2026-05-13 등록, 맵 미배치)

`Resources/Generated/GPT/Enemies/NamedBosses/`에 8개 PNG + 8개 `EnemyStats.tres`를 등록했다. 일반 EnemyVariants에 배치하지 않은 상태이며, 맵별 보스 교체 시 후보로 사용 — 기존 `boss_orc_king`/`boss_skeleton_king`/`boss_orc_warlord_boss`/`boss_dungeon3_ancient_lich`는 그대로 유지.

| 리소스 | 종류 | 후보 지역 | HP/Dmg/Def | 비고 |
|---|---|---|---|---|
| `field1_forest_alpha_wolf_named.tres` | 네임드 | field_1 | 220/8/1 | 빠른 늑대 정예 |
| `dungeon1_orc_warlord_boss.tres` | 보스 | dungeon_1 | 1100/17/4 | 기존 `boss_orc_warlord_boss.tres`와 별개 변형 |
| `field2_graveyard_wight_named.tres` | 네임드 | field_2 | 280/11/3 | 무덤 영혼 정예 |
| `dungeon2_skeleton_king_boss.tres` | 보스 | dungeon_2 | 1200/18/5 | 기존 `boss_skeleton_king.tres` 교체 후보 |
| `field3_plague_brute_named.tres` | 네임드 | field_3 | 380/14/4 | 역병 거인 정예 |
| `dungeon3_ancient_lich_boss.tres` | 보스(Ranged) | dungeon_3 | 1500/22/5 | 기존 `boss_dungeon3_ancient_lich.tres`와 별개 변형 |
| `mine_golem_named.tres` | 네임드 | mine_2 | 420/13/8 | 광산 강자 |
| `mine_crystal_guardian_boss.tres` | 보스 | mine_2/mine_3 | 1400/20/6 | 광산 보스 후보 |

**맵 변경 없음.** 이번 작업으로 `Scenes/Maps/*.tscn`의 `EnemyVariants`/`BossStatVariant`/`BossId`는 전혀 건드리지 않았다. 보스 교체는 별도 작업으로 추후 진행.

## 알려진 제약

- 임시 배치는 현재 5종 스켈레톤·4종 오크 리소스 내에서 가능한 최대 차별화. 진짜 정체성 회복은 신규 이미지/리소스 도착 후.
- field_3 / dungeon_3 모두 스켈레톤 계열에 의존 — zone scaling으로 체감 난이도는 분리되지만 시각적 단조로움은 남는다.
- mine_1 작업 PR이 mine_* 적 8종을 mine_1 EnemySpawner에 등록할 예정. 이 문서는 그 PR의 결과를 mine_1 행에 반영하지 않은 상태.
- ~~`EnemySpawner.StatVariants`는 균등 랜덤 — 가중치 미지원.~~ **(해결됨, v3 2026-05-16)** `EnemySpawner.StatWeights : float[]` 가중치 스폰 도입. `StatWeights`가 `StatVariants`와 길이가 같고 합>0이면 룰렛 선택, 아니면 균등 fallback. 18개 사냥터에 신규 테마 적이 희귀 가중치(0.25~0.4)로 배치됨. 상세 `PLAN_DOC/regional-weather-hunting-v3.md`. validate.py R3가 길이·합 검사.
- mine_* 적 8종 중 mine_2에 실제 배치된 건 3종(skeleton_miner / cave_bat / rock_golem). 나머지 5종(zombie_basic/fast/armored/brute, mine_wraith)은 리소스 + PNG만 등록되어 있고 맵 EnemySpawner에는 미배치 상태. 광산 적 등록 PR이 mine_1 / mine_2에 가중치를 분배할 때 함께 처리한다.
- EnemyStats에 `SpriteScale` + `CollisionScale` Export가 추가됐다. 시각은 `_animSprite.Scale`로, 충돌은 `CollisionShape2D.Scale`로 각각 적용 — 둘 다 root `CharacterBody2D.Scale` 비점유라 EnemySpawner elite scale(1.25)과 독립적으로 작동. mine_* 8종에 시각의 ~50% 비례로 CollisionScale 설정(예: rock_golem SpriteScale 0.55 / CollisionScale 2.5). 게임 디자인상 정확한 매칭 값은 플레이테스트 후 미세 조정 필요.

## 권역별 파밍 목적 (실제 데이터 기준, 2026-05-19)

각 권역 사냥터의 **파밍 동기**. 수치는 실제 `Resources/Enemies/*.tres`의
`RegionDrop` ExtResource + `RegionDropChance`를 직접 파싱해 확정(문서-데이터
정합 통과). RegionDrop은 PossibleDrops와 **독립 굴림**이라 핵심 드랍 확률을
희석하지 않는다(가산형) — EnemyController 비보스 분기에서 별도 `GD.Randf()`.

| 권역 | 사냥터 | 테마 재료(RegionDrop) | 확률 | 부가 목적 |
|---|---|---|---|---|
| town_region | field_1, dungeon_1 | **orc_leather** | 0.06 | 초반 골드/EXP, iron_ore 채광(town_mine 계약) |
| outpost_region | field_2/3, dungeon_2/3 | **bone_dust** | 0.04 | 중반 성장, silver_ore 채광(outpost 계약), 스토리 보스 1회 현상금 |
| coast_region | field_4_harbor, dungeon_4 | **sapphire_ore** | 0.03 | 해안 장신구 재료, 크라켄 반복 보스 파밍 |
| mountain_region | field_5_snowfield | **glacier_shard** | 0.02 | 빙결 장비 재료, 글래시어 타이탄 반복 |
| mountain_region | field_6_volcano | **drake_scale** | 0.02 | 화염 장비 재료, 인페르노 드레이크 반복 |
| (광산) | mine_1/2 | iron_ore·silver_ore | 0.05 | 저층 채광·금속 |
| (광산) | mine_3 | **crystal_ore** | 0.02 | 최상위 강화/재련 재료, 크리스탈 로드 반복 |

- **확률 차등화 근거**: 4개 권역이 단조 그라데이션(0.06 > 0.04 > 0.03 >
  0.02) — 이른 권역일수록 테마 재료가 흔하고 후반 권역일수록 희소.
  이전엔 town/outpost가 둘 다 0.05라 "권역이 같은 느낌"이었던 것을
  교정(작업 7). coast/mountain/mine은 기존 티어 유지.
- **문서↔데이터 정합 메모**: 기존 기획 위시리스트의 "해안 kelp" 표현과
  달리 **coast RegionDrop은 sapphire_ore**가 실제 데이터다. `sea_kelp.tres`는
  존재하지만 일부 해안 적의 일반 드랍 테이블 재료이지 RegionDrop이 아니다
  (field4_reef_raider / field4_storm_siren의 RegionDrop id=rgd1 →
  sapphire_ore로 확인). 문서를 실제 데이터에 맞춰 기술함.
- **named/boss 예외**: field5 글래시어 타이탄 → titan_core, field6
  인페르노 드레이크 → drake_eye 등 네임드는 전용 재료(RegionDropChance
  미설정 — 보스 드랍 테이블로 별도 지급).

---

## v3 진행 게이트 (2026-05-19, Regional Hunting Identity v3 작업)

초반 정보량·난이도 폭주 완화. 모두 `PlayerStats.Level` 기준 — **SaveData 변경 없음**.

### 기능 해금 게이트 (`Scripts/Data/FeatureGates.cs`, NPC 진입 단계 차단 + HUD 토스트)
| 기능 | 게이트 | 근거 |
|---|---|---|
| 공유 창고 | Lv.1 (실질 무게이트) | 파밍 게임 기본 편의 |
| 사냥 계약 보드 | Lv.3 | 사냥 동선 핵심이라 낮게 |
| 제작대 | Lv.5 | 재료 개념 첫 복잡 기능 |
| 장신구 재련 | Lv.10 | affix 등 정보량 최대 후반 준비 |
| 재료 상점(town) | Lv.5 | `ShopNPC.MinPlayerLevel`(material_shop_npc.tscn). 광물=제작과 동일 티어 |
| 스킬 상점 town | Lv.0 | 기본기/초급 |
| 스킬 상점 field_outpost | Lv.8 | `.tscn` SkillShopNPC.MinPlayerLevel |
| 스킬 상점 harbor_village | Lv.8 | 〃 |
| 스킬 상점 mountain_refuge | Lv.12 | 고급/후반 스킬 |

차단 메시지: `"{기능}은(는) Lv.{N}부터 이용할 수 있습니다. (현재 Lv.{cur})"`.
구현: `BaseInteractable.CheckLevelGate()` 공용 헬퍼 — 각 NPC `OnInteract()`가
`TryOpenQuestDialog()` 뒤·UI open 앞에서 호출(퀘스트 turn-in과 비충돌).

### 엘리트/희귀 변종 등장 게이트 (`EnemySpawner.CanSpawnElite()`)
- 게이트 = `max(elite.minPlayerLevel, MapLevels.Get(zone))`.
- `game_balance.json elite.minPlayerLevel = 5` (글로벌 플로어, 중앙화).
- **희귀 변종(v3 테마 적)도 동일 게이트로 차단**: `EnemyStats.IsRareVariant`
  표식(테마 .tres 12종 = field1_mist_spider/grove_spirit, field2_fog_wraith,
  field3_ruin_sentinel, field4_reef_raider/storm_siren, field5_blizzard_witch/
  frostbound_bear, field6_ash_imp/cinder_salamander, mine3_crystal_shocker,
  dungeon4_tide_lurker). 게이트 미통과 시 `PickVariantIndex(allowRare=false)`가
  해당 슬롯을 룰렛에서 제외(가중치 0 취급), 후보가 전부 희귀면 소프트락 방지
  폴백으로 전체 선택. 이전엔 엘리트 affix만 막혀 town_outskirts(StatWeights
  0.4 mist_spider)가 Lv.1부터 등장하던 갭 교정(Codex P1).
- 효과: town_outskirts/field_1/green_meadow 등 초반(zone 1~2)은 **플레이어
  Lv.5 전 엘리트 0%**. outpost↑는 zone 권장레벨이 플로어를 넘어 자연 상승
  (field_2=6, field_3=10, coast=11~14, mountain=15~18).
- **보스 무관**: `BossStatVariant`/`BossId`/`RepeatableBoss` 흐름은 불변.
  네임드 보스는 던전/보스 씬 도달 자체가 포탈 권장레벨 게이트 역할.
- xUnit `FeatureGateTests` 13종이 게이트 단조성·zone별 계산을 회귀 검증.
