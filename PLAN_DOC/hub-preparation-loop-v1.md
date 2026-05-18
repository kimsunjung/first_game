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

## D/E 구현 완료 (2026-05-18 사냥 계약 v1 패스)

### D. 사냥 계약 시스템 ✅ (구현 완료)
- `Scripts/Core/HuntingContractManager.cs` — GameManager 영구 인스턴스(`ContractManager`). 메인 `QuestManager` **미확장**(완전 독립). `EventManager.ResetAll()`이 `Resubscribe()` 자동 호출.
- `Scripts/Data/ContractData.cs` — `ContractType{Kill,Gather,BossKill,Mining}` + `ContractData`(런타임) + `ContractProgress`(세이브 직렬화 get/set).
- `Resources/Contracts/contracts.json` + `Scripts/Core/ContractsData.cs` 로더(CraftingData 동일 패턴). 계약 16개(권역별 4 / Kill8·Gather4·Mining2·BossKill2).
- SaveData **v13**: `ActiveContracts: List<ContractProgress>` 추가, `MigrateSaveData` v12→v13 backfill, `SaveManager.BuildSaveData`/`PlayerController.LoadFromSaveData` 직렬화·복원, `ResetForNewGame`에서 `ContractManager.RestoreFromSave(null)`.
- 진행 이벤트: Kill=`OnEnemyKilledTyped`(EliteAffixUtil.StripElitePrefix 정규화), BossKill=신설 `EventManager.OnBossKilled`(bossId, EnemyController가 RecordBossDefeat 직후 발신), Mining=신설 `OnOreMined`(MiningNode가 트랜잭션 종료 후 SaveGame 전 발신), Gather=`Inventory.OnItemPickedUp`(PlayerController에서 누적형, 완료 시 차감 없음).
- 동시 3개 / 같은 id 중복 수락 금지 / 완료 후 active 제거→재수락 가능 / 포기는 보상 없이 제거+즉시 SaveGame. 완료 보상은 골드·EXP 항상 + 아이템은 인벤 부족 시 `AddPendingReward`(손실 없음). 완료/보상은 `GameTransaction`+종료 후 명시 `SaveManager.SaveGame()`(QuestManager.CompleteQuest 동일 격리).
- UI `Scripts/UI/ContractBoardUI.cs`/`Scenes/UI/contract_board_ui.tscn`(진행 중/수락 가능 2섹션, autowrap 폭 고정). NPC `Scripts/Entities/ContractBoardNPC.cs`/`Scenes/Objects/contract_board_npc.tscn`(CircleShape r36, UI 자식 포함). 허브 4곳 배치 + Region Export 오버라이드(town(440,160)=town_region / field_outpost(320,180)=outpost_region / harbor_village(320,180)=coast_region / mountain_refuge(330,180)=mountain_region). 스프라이트 `Objects/World/quest_notice_board.png`.

### E. 사냥터 보강 ✅ 부분 / ⏸ 부분
- ✅ `mine_3.tscn`에 MiningNode 5개 추가(CrystalOreNode1/2=crystal_ore, DeepOreNode1=deep_ore, PrismaticCrystalNode1=prismatic_crystal(리스폰 0.15), EnhanceStoneNodeM3=enhance_stone(리스폰 0)). 포탈/스폰 반경(640,360 r340) 회피. mine_1/mine_2는 기존 노드 유지(NodeName=저장 키라 개명 시 MinedNodes 깨짐 → 비변경).
- ✅ FieldItem 희귀도 Loot glow: 코드 Sprite2D 1개(Common 없음 / Uncommon 초록 / Rare 파랑 / Epic 보라 / Legendary 금색), 알파·스케일 사인 펄스. 새 PNG·Light2D·Particle 없음(모바일 성능). Quantity/affix/강화 로직 미변경.
- ✅ **환경 장식 배치 완료(2026-05-18 후속)**: 18개 사냥터 .tscn(coast 5/snow 3/volcano 3/dungeon 3/mine 2/field 3)에 `Decorations` Node2D 하위 Sprite2D 2~3개. **Collision 없음 + z_index=-2** 이므로 동선/포탈/스폰 차단 물리적으로 불가(시각 전용). 배치 좌표는 스크립트가 씬 내 모든 기존 노드 position에서 ≥165px 떨어진 1280×720 코너 후보만 선택(포탈/스폰/플레이어/광맥 회피). mine_3는 광맥 5개가 코너 점유라 0개(스킵). 검증 4종 + git diff --check green 유지.

### 허브 확장 ✅ (2026-05-18 후속)
- 창고/제작/재련 NPC를 town 외 **나머지 3개 거점**(field_outpost·harbor_village·mountain_refuge)에도 배치. 기존 `storage_npc.tscn`/`crafting_npc.tscn`/`reforge_npc.tscn` + 각 UI 씬 인스턴스 재사용(NpcId=storage_keeper/crafting_station/reforge_station 동일 — 기존 ChapterDialogue 등록 재사용, 새 NpcId 미도입). 좌표는 거점별 하단/측면 빈 공간에 서비스 NPC r40 기준 상호 ≥80px·포탈/보드/상점과 비겹침으로 배치. 이로써 4개 거점 모두에서 "허브 준비 루프"(보관·제작·재련·계약·상점·스킬·세이브)가 완결.

### 검증 추가 (D)
- `validate.py` R10(contracts.json: id 유일/type 유효/goal>0/reward≥0/타입별 타깃 필수+enemy·boss 타깃 실존), R11(허브 4곳 ContractBoardNPC 배치), R12(MiningNode 노드명 유일·OreItem 지정·Respawn 0~1·Quantity>0).
- xUnit `ContractTests`(ContractType 값 고정 / ContractProgress 기본값 / JSON 라운드트립). SaveData v13 상수는 Godot 의존(ItemAffix→Godot)이라 단위테스트 미포함 — 마이그레이션 코드+R10/R11로 커버.

## (구) TODO 설계 메모 — 구현 완료, 히스토리 보존

### D. 지역 사냥 계약/허브 의뢰 (구 설계)
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

## Claude 리뷰 후속 hardening (2026-05-18, 계약·스킬·HUD)
- **[P1] 신규 스킬 SkillType 충돌 해소**: 신규 8스킬이 기존 Type(FireBolt 등) 재사용 → 학습중복/쿨다운/상점 게이팅이 모두 Type 기준이라 "이미 배움" 처리되던 결함. `SkillType` 27~34 신설 + `SkillStrategies.cs`에 기존 전략을 **위임 상속**하는 서브클래스 8개(`VenomShotStrategy : ArrowShotStrategy` 등, 각자 `[SkillStrategy]` — 어트리뷰트 Inherited=false라 별도 등록). 8개 skill `.tres` Type 갱신. 동작 동일·회귀 0, 이제 별개 스킬로 병존.
- **[P1] 채광 계약 달성 가능화**: `town_mine_iron`/`mountain_mine_crystal` goal 4→**2**. 해당 광맥(iron 0.7 / crystal 0.6 respawn) 1회 방문에 2노드 채광으로 완료, 재수락으로 반복.
- **[P2] 권역 플래그 마이그 구멍**: `BackfillChapterFlagsV10`이 v<10에서만 실행돼 v10~12 세이브가 kraken 등 새 플래그 누락. `MigrateSaveData` v<13 블록에 `BackfillRegionFlagsV13`(DefeatedBosses→4개 신규 마커) 추가. CurrentChapter 불변.
- **[P2] 계약 수락 영속화**: `HuntingContractManager.Accept`에 `SaveManager.RequestAutoSave()`(dirty 표시, throttle 경유) 추가 — 수락 직후 강제종료 유실 차단. 포기/완료는 기존대로 즉시 SaveGame.
- **[P2] HUD throttle**: `HUD._Process`가 매 프레임 `RefreshEffects`/미니맵 갱신(리스트·해시셋·Label 재생성) → 0.15s 간격 throttle로 전투 중 GC hitch 완화.
- **[P3] stale 계약 id 폐기**: `RestoreFromSave`가 `ContractsData.EnsureLoaded` 후 `Find()==null` id 폐기 — contracts.json id 변경/삭제 시 보이지 않는 계약이 3칸 한도 점유하던 결함 차단(목록 비었으면 방어적으로 미필터).
- **[수동지적] 계약 보드 UI**: 420×370 → **420×330**(offset ±165, Scroll min 280→240)로 640×360 모바일 세로 넘침 해소.

## 사용 흐름
- 창고: town 우하단 [창고] NPC → 가방/창고 목록 → 보관/꺼내기.
- 제작: town 대장간 인근 [제작] → 레시피 선택 → 제작.
- 재련: town 대장간 좌측 [재련] → 미장착 장신구 선택 → 재련(affix 재생성).
