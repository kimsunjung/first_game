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
| `dungeon_1` | 오크 소굴 — 오크 계열 첫 던전 | orc_warrior + orc_shaman (orc_basic 누락) | **orc_basic + orc_warrior + orc_rogue + orc_shaman** | 좌동 + 상위 오크 정예/맹수 | `boss_orc_king` (유지) | basic 누락 회귀 수정 |
| `field_2` | 묘지 외곽/황폐 필드 — 약한 언데드 중반 초입 | skeleton_base + skeleton_warrior + skeleton_rogue | **skeleton_base + skeleton_warrior + skeleton_rogue** (3종 균등 랜덤 유지) | 좌동 + 좀비 외야(현재 mine_zombie_*과 별개의 야외 좀비) | 없음 | mage는 의도적 제거 → dungeon_2로 이동. **가중치 분포는 EnemySpawner 미지원 — weighted spawn은 별도 후속 PR** |
| `dungeon_2` | 지하 묘지 — 본격 스켈레톤 던전 | skeleton_warrior + skeleton_mage + skeleton_rogue | **유지** (현 구성 적절) | 좌동 + 강화 스켈레톤 가디언 | `boss_skeleton_king` (BossId="skeleton_king_d2", 유지) | 변경 없음 |
| `field_3` | 저주받은 황무지 — 고급 언데드/타락 기사 | skeleton_warrior + skeleton_mage + skeleton_rogue (dungeon_2와 동일) | **skeleton_warrior + skeleton_mage** (rogue 제거, warrior 위주) | 신규: 타락 기사(`fallen_knight`), 망령(`wraith_field`), 강화 좀비 | 없음 | 현재는 d2와의 구분이 약함 — 신규 이미지 시급 |
| `dungeon_3` | 심연 던전 — 최종급 언데드/심연 계열 | skeleton_warrior + skeleton_mage + skeleton_rogue + boss_skeleton_king 재사용 | **skeleton_mage + skeleton_rogue** (warrior 제거, 마법/은신 위주) | 신규: 리치(`ancient_lich`), 심연 망령(`abyssal_wraith`), 그림자 자객 | 임시 `boss_skeleton_king` 재사용 (BossId="skeleton_king_d3"로 d2와 처치 키 분리). **별도 최종 보스 이미지/리소스 필요**: `boss_ancient_lich.tres` | BossId 충돌은 EnemyController가 .tscn 주입값으로 처리. d2/d3 분리 OK |

## 필요 신규 이미지 (GPT 생성 우선순위)

| 순위 | 적 종류 | 사용 지역 | 비고 |
|---|---|---|---|
| 1 | `boss_ancient_lich` | dungeon_3 | 최종 보스 — 현재 d3가 d2 보스 재사용 중. 식별성 즉시 필요 |
| 2 | ~~`slime`, `goblin`, `wolf`~~ | field_1 | **완료** — slime/wild_wolf/goblin_scout/orc_scout/wild_boar 배치, forest_spider/spirit/hobgoblin_guard는 리소스만 |
| 3 | `fallen_knight`, `wraith_field` | field_3 | d2/d3와의 시각 분리 — 이번 임시 배치가 워리어/메이지 재사용 |
| 4 | `zombie_field_basic`, `zombie_field_runner` | field_2 | 묘지 외곽 좀비 — mine_zombie_*과 별개 야외 변종 |
| 5 | `abyssal_wraith`, `shadow_assassin` | dungeon_3 | 일반 몹 차별화 |
| 6 | `skeleton_guardian` | dungeon_2 | 강화 변형 — 우선순위 낮음 |

## 필요 신규 EnemyStats 리소스 (이미지 도착 시 동반 생성)

- ~~`Resources/Enemies/slime.tres`~~ → 등록 완료 (`field1_slime.tres`)
- ~~`Resources/Enemies/wolf.tres`~~ → 등록 완료 (`field1_wild_wolf.tres`)
- ~~`Resources/Enemies/goblin.tres`~~ → 등록 완료 (`field1_goblin_scout.tres`)
- 추가 등록 완료: `field1_orc_scout.tres`, `field1_wild_boar.tres`, `field1_forest_spider.tres`, `field1_forest_spirit.tres`, `field1_hobgoblin_guard.tres` (뒤 3종은 맵 미배치)
- `Resources/Enemies/fallen_knight.tres`
- `Resources/Enemies/wraith_field.tres`
- `Resources/Enemies/zombie_field_basic.tres`
- `Resources/Enemies/zombie_field_runner.tres`
- `Resources/Enemies/abyssal_wraith.tres`
- `Resources/Enemies/shadow_assassin.tres`
- `Resources/Enemies/ancient_lich.tres` (일반급 변형 검토)
- `Resources/Enemies/boss_ancient_lich.tres` (보스급)

## 보스 처치 ID 충돌 처리

EnemyController는 `BossId` 필드를 .tscn EnemySpawner에서 주입받아 `GameManager.RecordBossDefeat(bossKey)` 호출 시 사용한다. 같은 `boss_skeleton_king.tres`를 d2/d3가 공유해도 BossId가 각각 `"skeleton_king_d2"` / `"skeleton_king_d3"`로 분리되므로 처치 기록이 충돌하지 않는다 — 현 코드에 이미 반영됨.

신규 `boss_ancient_lich.tres` 도입 시 d3의 BossId를 `"ancient_lich_d3"` 등으로 갱신하면 기존 d3 진행 세이브에서 보스 재처치 가능(BossId가 바뀌면 새 키이므로). 호환성 위해 마이그레이션 시 d3 정복 기록 보존 여부는 별도 결정.

## 변경 전 → 변경 후 한눈 요약

```
field_1   orc_basic + warrior + rogue          →  field1_slime + wild_wolf + goblin_scout + orc_scout + wild_boar (5종 균등)
mine_1    (변경 없음 — 별도 PR)
dungeon_1 orc_warrior + shaman                 →  orc_basic + warrior + rogue + shaman
field_2   skel_base + warrior + rogue          →  유지 (3종 균등, mage 없음 — dungeon_2로 분리)
dungeon_2 skel_warrior + mage + rogue          →  유지
field_3   skel_warrior + mage + rogue (==d2)   →  skel_warrior + mage (rogue 제거)
dungeon_3 skel_warrior + mage + rogue (==d2)   →  skel_mage + rogue (warrior 제거)
```

## 알려진 제약

- 임시 배치는 현재 5종 스켈레톤·4종 오크 리소스 내에서 가능한 최대 차별화. 진짜 정체성 회복은 신규 이미지/리소스 도착 후.
- field_3 / dungeon_3 모두 스켈레톤 계열에 의존 — zone scaling으로 체감 난이도는 분리되지만 시각적 단조로움은 남는다.
- mine_1 작업 PR이 mine_* 적 8종을 mine_1 EnemySpawner에 등록할 예정. 이 문서는 그 PR의 결과를 mine_1 행에 반영하지 않은 상태.
- `EnemySpawner.StatVariants`는 **균등 랜덤** — 가중치 미지원. 같은 지역의 적 비율을 차등하려면 weighted spawn 시스템을 별도 PR로 도입해야 한다. 그 전까지는 모든 임시 배치가 균등 분포다.
- mine_* 적 8종 중 mine_2에 실제 배치된 건 3종(skeleton_miner / cave_bat / rock_golem). 나머지 5종(zombie_basic/fast/armored/brute, mine_wraith)은 리소스 + PNG만 등록되어 있고 맵 EnemySpawner에는 미배치 상태. 광산 적 등록 PR이 mine_1 / mine_2에 가중치를 분배할 때 함께 처리한다.
- EnemyStats에 `SpriteScale` + `CollisionScale` Export가 추가됐다. 시각은 `_animSprite.Scale`로, 충돌은 `CollisionShape2D.Scale`로 각각 적용 — 둘 다 root `CharacterBody2D.Scale` 비점유라 EnemySpawner elite scale(1.25)과 독립적으로 작동. mine_* 8종에 시각의 ~50% 비례로 CollisionScale 설정(예: rock_golem SpriteScale 0.55 / CollisionScale 2.5). 게임 디자인상 정확한 매칭 값은 플레이테스트 후 미세 조정 필요.
