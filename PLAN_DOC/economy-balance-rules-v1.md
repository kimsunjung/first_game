# Economy & Balance Rules v1 (2026-05-20)

## 목적
이 게임은 오픈엔드 사냥 RPG. 골드/재료/게이트 통화가 자동 누적되어 **무한
구매·무한 강화**로 흘러가지 않도록 검증기·문서 기준을 명문화한다.

## 핵심 통화 분류

| 통화 | 분류 | 획득 경로 | 사용처 | 게이트? |
|---|---|---|---|---|
| 골드 | 일반 | 적 처치, 광맥, 아이템 판매, 계약 보상 | 상점 구매, 제작비, 재련비 | ✗ |
| 일반 광석 (iron/copper/silver…) | 일반 | 광맥 | 일부 제작, 일부 계약, 판매 | ✗ |
| 권역 재료 (orc_leather, sea_kelp, lava_stone …) | 일반 | 적 RegionDrop, 보스 드랍 | 권역 특화 제작 | ✗ |
| 보스 토큰 (titan_core, drake_eye, kraken_ink, prismatic_crystal) | 희소 | 보스 드랍 (DropChance 0.05~0.3) | 권역 고급 제작 | ✗ (희귀로 자체 게이트) |
| **enhance_stone** | **게이트** | 1회성 보스 1회, 광맥(mine_1/mine_3), 제작(2 stones per craft) | 장신구 affix 재련, 장비 강화 | **✓ (B5 보호)** |

## 가격 기준 (ItemData)

### Price (상점 구매가, 0 = 비매품)
- Common: 10~80G
- Uncommon: 80~250G
- Rare: 250~800G
- Epic: 800~2000G
- Legendary: 2000+G

### SellPrice (상점 판매가, 0 = 판매 불가)
- 권장: Price × 0.3 ~ 0.4 (장비)
- 권장: Price × 0.5 ~ 0.6 (소모품)
- 광석/재료: 자유 (현재 0~200G)
- 0 (sell-only-impossible): 드랍/퀘스트 전용, 인벤 적재만

### 자동 검증 (validate.py)
- **R16**: ShopItems 진열 품목은 `IsShopBlocked=false` AND `Price>0` 필수.
  Price 라인 누락 시 ItemData.cs 기본값 0 으로 간주 → false negative 봉쇄.
- **R18** (신규 2026-05-20): 제작 레시피 차익 금지.
  `result.SellPrice × qty ≤ gold + Σ(material.SellPrice × qty)` 필수.
  즉시 되팔아 골드 발생 시 에러.

## 제작 규칙 (recipes.json)

### 명시 규칙 (`_rule` 키)
```json
"_rule": "제작 비용 규칙(R18 자동검증): result.SellPrice × qty ≤
         gold + Σ(material.SellPrice × qty). 즉시 되팔아 차익이 나면 골드
         faucet — gold 비용 또는 재료 SellPrice 로 조정."
```

### 안전 재료 타입 (validate.py R9)
- Type=0 (Consumable) 및 Type=4 (Material) 만 재료로 허용.
- 장비(Type=1/2/3) 를 재료로 쓰면 Inventory.ConsumeItems 가 IsEquipped/affix
  무시하고 path 만으로 세므로 **장착 중인 장비가 제작에 소모됨** → 금지.

### enhance_stone 제작 (게이트 통화)
- `enhance_stone_craft`: 3× crystal_ore (SellPrice 80) + 200G → 2× enhance_stone (SellPrice 200)
- result_sell = 400, cost = 200 + 240 = 440 → R18 통과 (차익 -40G).
- 의도: 게이트 통화는 **사용** 동기로 제작. 즉시 되팔아 골드 환수 금지.

## 계약 규칙 (contracts.json)

### 보상 캡 (자동 검증)
- **B5**: 반복 계약(반복 보스 BossKill 포함) 의 rewardItemPath 가 enhance_stone
  으로 끝나면 에러. 게이트 통화 무한 faucet 봉쇄.
- **B5**: 반복 계약 goldReward / recommendedLevel > 60 = 경고 (튜닝 신호).
- **B6** (신규 2026-05-20): 반복 계약 총 보상가치(goldReward +
  rewardItem SellPrice × **rewardItemQuantity**) / recommendedLevel:
  - \> 100 = 에러 (faucet 위험)
  - \> 70  = 경고 (튜닝 신호)
  - rewardItemPath 없는 골드 단독 계약도 검사(total = gold).
  - JSON 키는 런타임 로더(`HuntingContractManager`) 와 동일한
    `rewardItemQuantity` 사용(`rewardItemQty` 아님 — 2026-05-20 hot-fix).

### Gather 계약 (A안 정합)
- 완료 시 ConsumeItems 로 재료 소모(turn-in 형).
- 설명에 "소모" 또는 "납품" 단어 필수 (validate.py R10, xUnit ContractsJsonTests).
- 보유 기반 진행도(=ProgressFromInventory) — 무한 골드/재료 익스플로잇 봉쇄.

### 권역 분포 가이드 (계약 v2 2026-05-20)
- 모든 권역 Kill 계약 최소 1개 (xUnit `EveryRegion_HasAtLeastOneKillContract`).
- 모든 RepeatableBoss 최소 1개 BossKill 계약 (xUnit `EveryRepeatableBoss_HasContract`).
- town_region 은 BossKill 의도적 없음(초반 단순화).

### 보상 v2 분포 (현재 19개 계약)
| Region | 계약 수 | 평균 goldReward / Lv |
|---|---|---|
| town_region | 4 | ~37 (Lv2-4, 90-160G) |
| outpost_region | 4 | ~36 (Lv6-9, 150-400G) |
| coast_region | 4 | ~40 (Lv11-14, 210-650G) |
| mountain_region | 7 | ~40 (Lv15-18, 260-850G) |

모두 B5 60G/Lv 경고 임계 이하. 골드는 권역 진행 따라 자연 증가.

## 상점 규칙

### ShopItems 진열 정합 (validate.py R16)
- `IsShopBlocked=true` 품목은 ShopUI.RefreshBuyTab 가 사일런트 제외 → 데이터에
  넣어도 안 보임 → 데이터 단계에서 차단.
- `Price<=0` (Price 누락 포함) 품목은 구매 의미 없음 (드랍/퀘스트 전용) → 차단.

### 직업 필터 (ShopUI.cs)
- `AvailableToAllClasses=true` (기본): 모든 직업에 노출.
- false: `RequiredClass` 일치 직업만 노출. 무기/방어구/장신구 공통 적용
  (Codex P2 fix 후).

### 4 허브 분포 (지역 상점 재분배 v2)
| 허브 | 상점 종류 | 가격대 |
|---|---|---|
| town | 기본 장비 상점 (14종 Common~Uncommon) + 소모품 | 초반 가격대 |
| field_outpost | 전초기지 무구상 (중티어) | 중급 |
| harbor_village | 해안 특화 (composite/storm/tide) | 중급 |
| mountain_refuge | 설원·화산 고티어 (frost/flame) | 고급 |

## 강화·재련 게이트

### enhance_stone 소비 경로 (`PlayerStats.SpendEnhanceStones`)
- 장신구 affix 재련: 1 stone / reroll
- 장비 강화: 변동 (장비 강화 시스템에 따라)

### 게이트 강도
- 1회성 보스 (orc_warlord_d1/skeleton_king_d2/ancient_lich_d3) 처치 시 1개씩 = 3개 total.
- 광맥 (mine_1/mine_3 각 1광맥) = 반복 채광 가능 (현재 cooldown 무관).
- 제작 (crystal_ore × 3 → 2 stones) = 광석 광맥 의존.

**잠재 누수**: mine_1/mine_3 의 enhance_stone 광맥 + crystal_ore 제작 경로가
열려있어 사실상 채광 무한 파밍 가능. v3 후속에서 RespawnChance 조정 또는
cooldown 추가 검토.

## 종합 검증 게이트 (v1)

다음 명령 모두 0 종료코드 유지 필수:

```sh
python3 tools/validate/validate.py     # R1~R18 (R17/R18 신규)
python3 tools/validate/balance.py      # B1~B6 (B6 신규)
dotnet build                            # net8 빌드
dotnet test tools/Tests/FirstGame.Tests.csproj  # 44개 통과
git diff --check                        # whitespace
```

## 변경 이력
- v1 (2026-05-20): 초안. R17/R18/B6 신규 검증 + recipes._rule 명시 + xUnit 3 신규.
