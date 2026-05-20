# Boss Farming v2 (2026-05-20)

## 목적
반복 보스가 *한 번 잡고 끝*이 아니라 권역 파밍의 **장기 목표**가 되도록
보상/연결/안전성을 정리. 코드 변경 없이 *데이터 + 계약 + 문서* 기반.

## 보스 분류

| BossId | Scene | RepeatableBoss | BossStatVariant | First-kill 챕터 |
|---|---|---|---|---|
| `orc_warlord_d1` | dungeon_1 | false (1회성) | boss_orc_warlord_boss | OrcWarlordKilled |
| `skeleton_king_d2` | dungeon_2 | false (1회성) | boss_skeleton_king_boss | SkeletonKingKilled |
| `ancient_lich_d3` | dungeon_3 | false (1회성) | boss_ancient_lich_boss | LichKilled |
| **`kraken_d4`** | dungeon_4_sunken_shrine | **true** | boss_dungeon4_kraken | (마커만) |
| **`glacier_titan_f5`** | field_5_snowfield | **true** | field5_named_glacier_titan | (마커만) |
| **`inferno_drake_f6`** | field_6_volcano | **true** | field6_named_inferno_drake | (마커만) |
| **`crystal_lord_m3`** | mine_3 | **true** | boss_mine3_crystal_lord | (마커만) |

**1회성 3종**: town_region / outpost_region 진행 게이트. 처치 시 `ChapterFlags`
영구 기록 + 챕터 진행. 보상은 강력하지만 1회성 한정.

**반복 4종**: coast/mountain_region 정점. 처치해도 게임 종료 아님(엔딩
없음). first-kill 마커는 권역 진행 마커이지 게임 진행 게이트 아님.
계약/드랍 파밍 가능.

## 검증 안전망 (자동)
- `validate.py R14` — `HuntingContractManager.RepeatableBossIds` ↔ 씬
  `RepeatableBoss=true BossId` 일치 강제. 드리프트 시 에러.
- `balance.py B5` — 반복 계약(반복 보스 BossKill 포함) 의 enhance_stone
  보상 금지. 게이트 통화 무한 faucet 봉쇄.
- `balance.py B6` (신규 2026-05-20) — 반복 계약의 *총 보상가치*
  (gold + rewardItem SellPrice × qty) / 레벨이 100 초과 = 에러,
  70 초과 = 경고. 우회 item faucet 차단.

## 권역별 보상 목적 (현재 데이터 기준)

### kraken_d4 (해안 / dungeon_4_sunken_shrine, RepeatableBoss=true)
**현재 PossibleDrops**: health_potion, trident, drake_armor(임시), kraken_ink
- **권역 목적**: 해안 마법/저주/해양 재료. `kraken_ink` 는 storm_ward
  제작 핵심 재료(coast 저항 부적).
- **계약 연결**: `coast_boss_kraken` (BossKill, repeatable, gold/exp만)
- **반복 동기**: kraken_ink 가 coast 권역 storm_ward (Shock 저항) 재료라
  반복 처치로 부적 다중 제작 가능. 폭풍 부적은 무게이트 소모품이라
  과파밍 리스크 없음.

### glacier_titan_f5 (산악 / field_5_snowfield, RepeatableBoss=true)
**현재 PossibleDrops**: health_potion, glacier_axe, frost_cloak, titan_scale, titan_core
- **권역 목적**: 빙결 방어 / 저항. titan_scale·titan_core 는 미래 빙결
  방어구 제작 입력 후보(현재는 sell-only 재료).
- **계약 연결**: `mountain_boss_glacier` (BossKill, repeatable, gold 700G/exp 340)
- **반복 동기**: glacier_axe 는 전사 빙결 무기(드랍 시 affix 부착 가능).
  반복 처치는 affix 강한 axe 굴리는 용도.

### inferno_drake_f6 (산악 / field_6_volcano, RepeatableBoss=true)
**현재 PossibleDrops**: health_potion, inferno_staff, ember_cloak, drake_scale, drake_eye
- **권역 목적**: 화염 공격 / 저항. drake_scale·drake_eye 는 fire_resist
  엘릭서 재료(현재 fire_resist_craft 레시피에 drake_scale 사용).
- **계약 연결**: `mountain_boss_inferno` (BossKill, repeatable, gold 780G/exp 380)
- **반복 동기**: inferno_staff 는 법사 화염 무기. drake_scale 은 화염
  저항 엘릭서 핵심 재료라 반복 처치로 엘릭서 비축 가능.

### crystal_lord_m3 (산악 / mine_3, RepeatableBoss=true)
**현재 PossibleDrops**: health_potion, prismatic_ring, dwarf_helm
- **권역 목적**: 수정 재련 / 고급 제작. prismatic_ring 은 prismatic_craft
  의 결과물(역으로 보스 드랍과 제작 양쪽 경로).
- **계약 연결**: `mountain_boss_crystal` (BossKill, repeatable, gold 850G/exp 420)
- **반복 동기**: prismatic_ring 은 고급 장신구. 반복 처치는 affix 굴림용.

## first-kill ↔ repeat 충돌 안전성

| 항목 | 동작 |
|---|---|
| `GameManager.RecordBossDefeat(bossId)` | first-kill 시 ChapterFlags 영구 마커 set. 이미 마커 set 상태면 no-op. |
| `EventManager.OnBossKilled` | 매 처치마다 발사(반복 가능). 계약 진행에 사용. |
| 반복 계약 완료 → `ContractManager.Complete()` | 정상 보상 지급. 보상 후 계약 status=완료, 보드에서 재수락 가능. |
| 반복 처치 vs first-kill | 독립 경로. ChapterFlags 한 번 set 되면 다시 안 set, 계약은 매번 set. |

## 알려진 제한 (v2 보류 후보)

1. **kraken/titan/drake 의 RareDrops 분리**: 현재 PossibleDrops 가 DropWeights
   균등으로 모든 드랍이 같은 확률. 4종 모두 `prismatic_ring`·`titan_core` 같은
   고티어 드랍을 분리해 희귀하게 굴리는 RareDrop 슬롯이 없음. SaveData 영향
   없는 형태로 추가 가능하지만 회귀 위험 — v2 후속.
2. **반복 보스 affix 슬롯**: 보스 무기/방어구 드랍에 elite affix 가 자동 부착되지
   않음. EnemyStats.AffixGuaranteed 같은 옵트인이 필요. 현재는 일반 affix 굴림.
3. **dungeon_4 / mine_3 깊이 부재**: 보스만 있고 사냥터로서 mob 다양성 약함.
   필드형 사냥터(field_5/field_6)와 달리 보스방 단일 구조. 신규 적/공간 추가
   없이 데이터로만 보강 어렵 — 별도 패스 후보.

## 수동 테스트 체크리스트
- [ ] kraken_d4 처치 → first-kill 챕터 대사 출력 → 다시 처치 시 챕터 변경 없음
- [ ] coast_boss_kraken 계약 수락 → kraken 처치 → 보상 수령 → 보드에서 재수락 가능
- [ ] glacier_titan_f5 / inferno_drake_f6 / crystal_lord_m3 모두 동일 흐름
- [ ] 계약 완료 보상이 enhance_stone 미포함 (B5/B6 검증)
- [ ] kraken_ink/drake_scale 등 권역 재료가 storm_ward/fire_resist 제작에 사용됨

## 참고
- 데이터 변경 없음(이번 v2 패스는 문서/검증만). 실제 보스/드랍/계약
  데이터는 v1 + 이전 패스에서 이미 배치 완료.
- 향후 보상 변경 시 R14/B5/B6 통과 유지 의무.
