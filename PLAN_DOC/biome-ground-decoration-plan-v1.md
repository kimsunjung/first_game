# 바이옴 지면 장식 패스 계획 v1 (2026-05-19)

작업 12 요청: 모든 주요 맵에 바이옴에 맞는 **지면 식생/장식**(풀·잡초·꽃·이끼·
눈풀·해초·탄 풀 등)을 콘셉트별로 차등 배치. Collision 없음, z 캐릭터·아이템
뒤, 포탈/NPC/SavePoint/상점/계약보드/채광노드/스폰/플레이어 시작 비가림,
모바일 성능 고려, `Decorations` Node2D 하위.

## ⚠ 결정: 에셋 부재로 씬 주입은 보류, 스펙/TODO를 산출

`Resources/Generated/**` 전수 조사 결과 **지면 식생 전용 PNG가 0개**다.
가장 가까운 것도 prop 스케일(barrel_group, lava_pool, snow_drift,
coral_outcrop, ember_rock, flower_planter 등) — 미묘한 지면 풀/이끼가
아니라 큰 오브젝트다. 이것들을 "풀"로 깔면 (a) 시각적으로 틀리고 (b) 큰
스프라이트라 z=-2여도 동선 인지 방해 위험이 있다.

프로젝트 규칙(작업 12 본문 + CLAUDE.md): **새 generated asset을 직접
만들지 말고 필요 에셋 목록 + 이미지 프롬프트만 TODO로 남긴다.** 또한
민감 영역 최소 변경 / 대규모 비검증 .tscn 변경 회피 원칙상, 식생 PNG가
없는 상태에서 ~29개 씬에 Sprite2D를 일괄 주입하면 (헤드리스로 시각
검증 불가 + 잘못된 prop 재활용) 회귀 위험만 크다.

→ 따라서 이번 패스의 산출물은 **(1) 필요 에셋 스펙·프롬프트(아래 +
`generated-asset-inventory.md`)** 와 **(2) 결정적 배치 알고리즘 스펙**.
PNG가 도착하면 그 스펙대로 한 번에 안전 주입한다(별도 패스).

## 현재 Decorations 노드 상태 (검증)

| 맵군 | Decorations | 비고 |
|---|---|---|
| town / harbor_village / mountain_refuge | ✓ (10/3/2) | 기존 prop 재사용 |
| dungeon_1~4 | ✓ (각 3) | 기존 prop |
| field_4/5/6 | ✓ (2~3) | 기존 prop |
| field_outpost / field_1/2/3 / green_meadow / 일부 mine | ✗ 또는 직접 Sprite2D | 미구성 |

기존 ✓ 맵은 과거 패스에서 **prop**(텐트·배럴 등)을 코너에 ≥165px
이격해 z=-2로 배치한 것 — 진짜 "지면 식생"은 아직 어느 맵에도 없다.

## 필요 에셋 TODO (직접 생성 금지 — 이미지 프롬프트만)

티셋 PNG 권장: 64×64 또는 96×96, 투명 배경, 약한 그림자, 톱다운,
픽셀/세미픽셀, 채도 낮춤(지면이라 캐릭터보다 안 튀게). 바이옴당 3종.

| 키 | 경로(제안) | 바이옴/적용 맵 | 이미지 프롬프트(영문 권장) |
|---|---|---|---|
| `grass_tuft` | `Objects/Ground/grass_tuft.png` | 초원: town_outskirts, green_meadow, goblin_woods, field_1 | "small top-down tuft of green meadow grass, transparent bg, soft shadow, low saturation, game decal, 64px" |
| `meadow_flowers` | `Objects/Ground/meadow_flowers.png` | 초원 동일 | "tiny cluster of wild meadow flowers (white/yellow), top-down, transparent bg, subtle, 64px" |
| `weed_clump` | `Objects/Ground/weed_clump.png` | 초원/공용 | "small clump of green weeds, top-down decal, transparent bg, 64px" |
| `dry_grass` | `Objects/Ground/dry_grass.png` | 길: old_orc_road, ruined_crossroad, field_outpost | "dry brown roadside grass tuft, top-down, transparent bg, muted, 64px" |
| `broken_roadweed` | `Objects/Ground/broken_roadweed.png` | 길 동일 | "sparse weeds breaking through cracked dirt road, top-down decal, 64px" |
| `coastal_reed` | `Objects/Ground/coastal_reed.png` | 해안: harbor_outskirts, crab_beach, pirate_camp, field_4_harbor | "coastal reed/beach grass tuft, sandy base, top-down, transparent bg, 64px" |
| `beach_kelp` | `Objects/Ground/beach_kelp.png` | 해안 동일 | "small washed-up kelp/seaweed strand on sand, top-down decal, 64px" |
| `snow_grass` | `Objects/Ground/snow_grass.png` | 설원: snowfield_edge, frozen_valley, field_5_snowfield | "frosted grass tuft poking through snow, pale, top-down, transparent bg, 64px" |
| `frozen_bush` | `Objects/Ground/frozen_bush.png` | 설원 동일 | "small leafless bush encased in frost, top-down, transparent bg, 64px" |
| `burnt_grass` | `Objects/Ground/burnt_grass.png` | 화산: volcano_approach, lava_field, field_6_volcano | "charred blackened grass tuft, ash, top-down decal, transparent bg, 64px" |
| `black_shrub` | `Objects/Ground/black_shrub.png` | 화산 동일 | "small burnt black shrub near ashy ground, top-down, transparent bg, 64px" |
| `cave_moss` | `Objects/Ground/cave_moss.png` | 광산: mine_1/2/3 | "patch of damp green cave moss on rock, top-down decal, transparent bg, 64px" |
| `small_mushrooms` | `Objects/Ground/small_mushrooms.png` | 광산 동일 | "tiny cluster of pale cave mushrooms, top-down, transparent bg, 64px" |
| `dungeon_lichen` | `Objects/Ground/dungeon_lichen.png` | 던전: dungeon_1~4 | "creeping lichen / fungal patch on dungeon stone floor, top-down decal, 64px" |
| `broken_vines` | `Objects/Ground/broken_vines.png` | 던전 동일 | "torn dead vines on damp dungeon ground, top-down decal, transparent bg, 64px" |

(.import 사이드카는 Godot 4.6.2 mono headless import로 생성 — 에셋
도착 시 별도 처리. 생성 금지 규칙상 본 작업에서는 PNG/.import 미생성.)

## 결정적 배치 알고리즘 스펙 (에셋 도착 후 적용)

1. 대상 씬: `Scenes/Maps/*.tscn` 중 사냥터/거점/던전/광산. 거점 4곳은
   기존 prop 데코 유지 + 식생 소량만 보강.
2. 각 씬 루트에 `Decorations` Node2D 없으면 생성(일관 이름). z_index = -2.
3. 바이옴 매핑은 위 표의 "적용 맵" 기준. 맵당 식생 키 2~3종만.
4. **금지 반경(클리어런스)**: 모든 Portal/NPC(*Npc*/*NPC*)/SavePoint/
   Shop*/SkillShop*/ContractBoard/MiningNode/EnemySpawner/PlayerStart
   노드 position에서 **반경 ≥140px** 밖에만 배치. (기존 패스는 165px;
   식생은 작아 140px로 완화 가능하나 더 보수적이면 165px 유지)
5. **밀도 캡(모바일)**: 맵(1280×720 기준)당 식생 Sprite2D **최대 14개**.
   화면당 환산 6개 이하가 되도록 균등 분산(겹침 방지: 신규 식생끼리도
   ≥90px). 반복 패턴 회피 위해 키·스케일(0.7~1.1)·flip_h 랜덤.
6. Collision 노드 절대 추가 금지(Sprite2D만). `texture` ext_resource
   path는 실제 존재 검증(validate.py가 res:// 해결로 자동 검사).
7. 적용 후 게이트: `validate.py`(uid·ext_resource), `git diff --check`,
   `dotnet build` 0. 시각 겹침은 Godot 에디터 수동 확인 항목(체크리스트).

## 산출/보류 요약

- ✅ 산출: 본 계획 + 필요 에셋 15종 스펙/프롬프트 +
  `generated-asset-inventory.md` TODO 섹션.
- ⏸ 보류(의도적): ~29개 .tscn Sprite2D 식생 주입 — 전용 PNG 부재 +
  생성 금지 규칙 + 헤드리스 시각 검증 불가. 에셋 도착 시 위 알고리즘으로
  단일 패스 진행. 기존 prop 데코는 그대로 유지(회귀 0).
