# 허브 준비 루프 + 반복 파밍 루프 v1 (2026-05-18)

목표: 늘어난 맵/적/아이템/재료를 "허브 준비 → 사냥/채광/보스 → 드랍/재료 → 창고/제작/재련 → 더 강한 지역" 루프로 묶는다. 새 맵·새 PNG 없음. 확률형 제작/뽑기 없음(확정 제작·확정 기능).

## 구현 완료 (이번 v1)

### A. 공유 창고/보관함 ✅
- `SaveData.StorageItems`(v12) — 강화수치/affix/수량 보존. 기존 세이브는 빈 창고로 안전 로드(`MigrateSaveData` v11→v12 backfill + 초기화자 `new()`).
- `GameManager._storage` + `RestoreStorage/AddStorageSlot/RemoveStorageAt` (PendingRewards와 동일 보관소 패턴). `SaveManager.BuildSaveData`가 직렬화, `PlayerController.LoadFromSaveData`가 복원.
- `Scripts/UI/StorageUI.cs` + `Scenes/UI/storage_ui.tscn` — 가방/창고 2열, 슬롯 전체(수량+강화+affix) 이동.
  - 입고/출고 모두 `GameTransaction.Begin()` + 종료 후 명시 `SaveManager.SaveGame()`(ShopUI 판매와 동일 안전 패턴).
  - **장착(IsEquipped) 슬롯은 목록 제외 + Deposit 거부 + RemoveItem 자체 거부(3중)** → 유령/복제 차단.
  - 꺼내기 전 `CanAddItem` 사전 검사 — 공간 부족 시 실패하고 창고 데이터 보존.
- `Scripts/Entities/StorageNPC.cs` + `Scenes/Objects/storage_npc.tscn`(storage_keeper.png) — town `(560,300)` 배치.

### B. 제작 시스템 ✅
- `Resources/Recipes/recipes.json` — 레시피 10개(전부 기존 아이템 결과/재료). `Scripts/Core/CraftingData.cs` 로더(BalanceData와 동일 FileAccess+JsonDocument).
- `Scripts/UI/CraftingUI.cs` + `Scenes/UI/crafting_ui.tscn` — 레시피 목록/충족여부/비용/결과/제작 버튼.
  - 사전 검증(재료 전부 + 골드 + 결과 공간) 후 `GameTransaction`{ ConsumeItems × N + 골드 차감 + 결과 지급 } + 명시 SaveGame. 실패 시 차감 없음. 확률/파괴 없음.
- `Scripts/Entities/CraftingNPC.cs` + `Scenes/Objects/crafting_npc.tscn`(anvil_workstation.png) — town `(190,310)` 대장간 인근.

### C. 장신구/affix 재련 ✅
- `Scripts/UI/ReforgeUI.cs` + `Scenes/UI/reforge_ui.tscn` — **미장착** 장신구(`AffixGenerator.IsJewelry`)만 대상(장착 중 제외 = 스탯 재계산 위험 차단).
  - 비용: rarity별 gold + `enhance_stone`(Common 60/1 … Legendary 600/4).
  - `GameTransaction`{ ConsumeItems(enhance_stone) + 골드 차감 + `slot.Affixes = AffixGenerator.GenerateForJewelry(rarity)` } + SaveGame. ResourcePath/수량/강화수치 유지, affix만 재생성. SaveData 스키마 변경 없음(인벤 직렬화로 자동 영속).
- `Scripts/Entities/ReforgeNPC.cs` + `Scenes/Objects/reforge_npc.tscn`(blacksmith_forge.png) — town `(70,310)`.

## TODO (v1 범위 축소 — 후속)

저장 안정성·메인 퀘스트 안전 우선으로 이번 패스에서 **D/E는 설계만 확정하고 구현 보류**(프롬프트의 우선순위 A>B>C>D>E + 축소 규칙 준수).

### D. 지역 사냥 계약/허브 의뢰 (보류)
- 권장 설계: 메인 `QuestManager`(ActiveQuest 1개) 건드리지 말고 **별도 `HuntingContractManager`** 신설.
  - `SaveData`에 `ActiveContracts: List<ContractProgress>` 추가(v13) — 별도 버전 bump + backfill.
  - `ContractData`(JSON manifest `Resources/Contracts/contracts.json`): id/name/desc/region/type(Kill·Gather·BossKill)/target/goal/reward(gold·exp·item). 시간제한·일일리셋 없음(상시 반복).
  - EventManager.OnEnemyKilledTyped / 아이템 획득 / 씬 진입 훅에 진행도 연결. 보상은 인벤 부족 시 `GameManager.AddPendingReward` 경유(손실 방지).
  - `ContractBoardUI` + 보드 NPC: town(town_region)·field_outpost(outpost)·harbor_village(coast)·mountain_refuge(mountain). 계약 12개(권역별 3).
  - 보스 계약은 `RepeatableBoss`/first-kill semantics 비파괴 — kill 카운트 기반만 사용.

### E. 사냥터 오브젝트/광맥/드랍 피드백 (보류)
- coast/snowfield/volcano/dungeon/mine 환경 PNG를 Sprite2D 장식으로 배치(Collision 생략, 동선/포탈/스폰 회피).
- `mine_3` 등에 MiningNode 추가(NodeName stable = 저장 키).
- `FieldItem`에 rarity 기반 glow/sparkle(기존 Loot PNG 또는 코드 Modulate).
- 연결 시 `generated-asset-inventory.md` 상태 갱신.

## 안전성 요약
- 민감 영역 변경은 **SaveData v12 단일 필드 추가(StorageItems) + backfill** 뿐. SceneManager/GameTransaction/Inventory 코어 로직 미변경(기존 API 재사용).
- 모든 자원/골드 이동은 GameTransaction + 종료 후 명시 SaveGame(기존 ShopUI/EnhanceUI 패턴 동일).
- 새 PNG 0개. 기존 GPT 자산만 사용.

## Codex 리뷰 후속 hardening (2026-05-18)
- **P1 새 게임 창고 이월 차단**: `GameManager.ResetForNewGame()`에 `_storage.Clear()` 추가(`_pendingRewards.Clear()`와 동일 의도). 새 게임이 이전 세션 창고 아이템을 물려받지 않음.
- **P2 제작 트랜잭션 안전화**: `CraftingUI.Craft()`가 재료를 ResourcePath 기준 합산 후 검증/소비(중복 기재 줄도 총량 처리). 결과 지급(AddItem)을 트랜잭션 내 가장 먼저 수행해 실패 시 재료/골드 무차감으로 안전 종료. GameTransaction이 메모리 rollback이 아님을 주석에 명시.
- **검증 R9**: `validate.py`가 recipes.json 검사 — id 유일, material/result 경로 실존, qty>0·gold≥0, 레시피 내 material path 중복 금지, 재료는 Consumable(0)/Material(4) 타입만(장비가 ConsumeItems로 소모되는 위험 차단).
- **town 배치 + 상호작용 범위**: 상호작용형 9개 NPC(shop/skill_shop/blacksmith/storage/crafting/reforge/material_shop/teleport/potion_shop)의 CircleShape **r56→r40**(villager는 30×30 박스라 r56이 과대). `StorageNPC` (560,300)→(470,320)로 PortalToField4Harbor와 이격, `CraftingNPC` (190,310)→(300,340)로 IvanBard(200,280)·TeleportNPC(320,240)와 분리(각 ≥100px). ReforgeNPC(70,310)는 r40 적용만으로 충분해 위치 유지. 결과적으로 town 내 모든 상호작용 영역이 이웃 NPC/포탈과 겹치지 않음(상점↔상점 ≥80px, 상점↔villager ≥55px).
- **StorageUI 행 폭**: 420px 패널 2열 구조에서 행 `CustomMinimumSize` 폭 250→185로 축소(2×185+구분 < 420). 모바일 해상도에서 행/버튼이 패널 밖으로 삐져나가지 않게 함.

## 사용 흐름
- 창고: town 우하단 [창고] NPC → 가방/창고 목록 → 보관/꺼내기.
- 제작: town 대장간 인근 [제작] → 레시피 선택 → 제작.
- 재련: town 대장간 좌측 [재련] → 미장착 장신구 선택 → 재련(affix 재생성).
