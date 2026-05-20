# Mining Loop v2 (2026-05-20)

## 목적
광산 3개(mine_1/2/3) 의 광맥→광석→사용처(제작/계약/판매/재련) 연결을
매트릭스로 가시화. 미연결 광석(orphan ore) 식별 + 향후 보강 방향.

## 광산 구성 (현재 데이터)

### mine_1 (town_region, 권장 Lv 4)
- 광맥 5개: iron×2, silver×1, gold×1, enhance_stone×1
- 진입: field_1.PortalToMine1 (TOP)
- 적: 표준 mob, 보스 없음

### mine_2 (outpost_region, 권장 Lv 8)
- 광맥 6개: copper×2, platinum×1, mithril×1, ruby×1, sapphire×1
- 진입: mine_1.PortalToMine2 (RIGHT)
- 적: 표준 mob, 보스 없음

### mine_3 (mountain_region, 권장 Lv 18)
- 광맥 5개: crystal_ore×2, deep_ore×1, prismatic_crystal×1, enhance_stone×1
- 진입: field_6_volcano.PortalToMine3 (RIGHT)
- 적: 권역 특화 mob 6종 + **RepeatableBoss crystal_lord_m3**

## 광석 사용처 매트릭스 (2026-05-20 audit)

| 광석 | 광산 | 제작재료 | 계약타깃 | 상점진열 | SellPrice | 상태 |
|---|---|---|---|---|---|---|
| iron_ore | mine_1 | ✗ | 2 (town/outpost mining/gather) | ✓ (재료상) | (raw) | ⚠ 제작 미연결 |
| silver_ore | mine_1 | ✗ | 1 (outpost gather) | ✓ | (raw) | ⚠ 제작 미연결 |
| gold_ore | mine_1 | ✗ | 1 (coast gather) | ✓ | (raw) | ⚠ 제작 미연결 |
| enhance_stone | mine_1+mine_3 | ✓ (제작 결과물) | ✓ (1회성 보스 보상) | ✓ | 200 | 게이트 통화(B5/B6) |
| copper_ore | mine_2 | ✗ | ✗ | ✓ | (raw) | ⚠⚠ orphan |
| platinum_ore | mine_2 | ✗ | ✗ | ✓ | (raw) | ⚠⚠ orphan |
| mithril_ore | mine_2 | 1 (warming_draught 등) | ✗ | ✓ | (raw) | OK |
| ruby_ore | mine_2 | ✗ | ✗ | ✓ | (raw) | ⚠⚠ orphan |
| sapphire_ore | mine_2 | ✗ | ✗ | ✓ | (raw) | ⚠⚠ orphan |
| crystal_ore | mine_3 | ✓ (enhance_stone_craft 입력) | 2 (mountain mining/gather) | ✓ | 80 | OK |
| deep_ore | mine_3 | ✗ | ✗ | ✓ | (raw) | ⚠⚠ orphan |
| prismatic_crystal | mine_3 | 1 (prismatic_craft 입력) | ✗ | ✓ | 200 | OK |

**Orphan ores (제작/계약 미연결 5종)**: copper_ore, platinum_ore, ruby_ore,
sapphire_ore, deep_ore. 현재 판매·재료상 통화 외에는 의미 없음.

## 자동 검증 (validate.py R12)
- MiningNode 인스턴스의 NodeName 광산 내 유일.
- OreItem 지정 강제.
- RespawnChanceOnReentry 0~1 범위.
- Mining 계약 targetOreItemPath 가 실제 MiningNode OreItem 에 존재(R10 교차검증).

## v3 보강 방향 (제안 / 미구현)

### O1. orphan ore → 제작 연결 (Priority B)
- `copper_ore × N + iron_ore × N + 골드 → bronze_armor`(전사 중티어 placeholder)
- `ruby_ore × 2 + 골드 → ruby_ring`(이미 ruby_ring.tres 존재 — 보스/드랍 외 제작 경로 신설)
- `sapphire_ore × 2 + 골드 → sapphire_ring`(동일)
- `platinum_ore × 3 + 골드 → enhance_stone × 1`(대체 게이트 통화 경로, R18 검사 통과 필수)
- `deep_ore × 5 + 골드 → deep_essence`(신규 재료 — 산악 고급 제작 입력)

**위험**: 모든 신규 레시피는 R18 (gold ≥ result_sell - mat_sell) 통과해야 함.
ring 류는 SellPrice 200~ 라 mat+gold 비용을 충분히 잡아야 함.

### O2. orphan ore → 계약 연결 (Priority B)
- `coast_gather_copper`(Lv 8, copper_ore 10개 납품, 250G)
- `outpost_gather_sapphire`(Lv 8, sapphire_ore 5개 납품)
- `mountain_mine_deep`(Lv 18, deep_ore 채광 6개)

**위험**: 계약 추가는 R10/R14/B5/B6 통과 필수. Gather A안(완료 시 소모) 의미상
판매가 200 의 sapphire 를 5개 소모는 -1000G 손실 → 보상 골드를 그만큼 보상해야 함.

### O3. 광산 보스 추가 (Priority C — 위험)
- mine_1/mine_2 는 현재 보스 없음. 광산 3 만 crystal_lord_m3.
- mine_1 에 *iron_overseer*(town_region 일회성 보스)나
  mine_2 에 *gem_warden*(outpost_region 일회성 보스) 추가 가능하지만 신규 .tres + 씬 변경.
- v3 분리 필요(이번 패스 범위 밖).

### O4. 채광 속도 / 도구 시스템 (Priority C — 위험)
- 현재 채광은 단순 1-shot. 도구·시간·확률 시스템 도입은 MiningNode 코드 변경.
- 모바일 단순성 가치 위협 — 보류 후보.

## 알려진 제한
- mine_2 의 6/6 광맥 중 5종(copper/platinum/ruby/sapphire/silver — silver 는 계약 1)
  이 사실상 판매전용. 광산 2 의 파밍 동기가 약함.
- 광산 3 의 enhance_stone 직접 광맥은 게이트 통화 직지급 — 채광 게이트(MinPlayerLevel
  18)가 강하긴 하나 무한 파밍 가능. balance.py B5/B6 은 *계약* 경로만 막고
  *광맥 직접 드랍* 은 별도 게이트가 없음. v3 후속에서 RespawnChance/Cooldown
  강화 검토.

## 수동 테스트 체크리스트
- [ ] mine_1 진입 → 5광맥 채광 → 광석 인벤 적재
- [ ] mine_2 진입(Lv 8+) → 6광맥 채광 → orphan 광석 sell 가능 확인
- [ ] mine_3 진입(Lv 18+) → 5광맥 채광 → enhance_stone 광맥 채광
- [ ] crystal_ore × 3 + 200G → enhance_stone × 2 제작 (mountain refuge 제작상)
- [ ] mountain_mine_crystal 계약 수락 → mine_3 에서 crystal_ore 채광 → 완료

## 참고
- 데이터/광산 변경 없음(문서만). 광산 보강은 v3 분리.
- mine_3 enhance_stone 광맥은 의도된 가속 경로(고티어 게이트 통화 직지급)이지만
  무한 파밍 가능성은 v3 검토.
