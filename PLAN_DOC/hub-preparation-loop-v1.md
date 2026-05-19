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

## Claude 적대적 리뷰 후속 hardening v2 (2026-05-18, 드랍·스킬·계약·세이브)
- **[F1] 사냥 불가 계약 차단**: `outpost_kill_skeleton` 타깃 `Skeleton`(미참조 레거시 리소스만 보유)→`SkeletonWanderer`(field_2 실제 스폰). `validate.py` R10의 Kill 타깃 검증을 **씬에서 실제 참조되는 적 리소스의 EnemyTypeName**만 유효로 강화(`_collect_enemy_type_names`가 Scenes/Maps 참조분으로 한정) — 동류 결함 재발 시 검사 실패.
- **[F2] 영구 진행 불가 보스 계약 차단**: 비반복 스토리 보스(orc_warlord_d1/skeleton_king_d2/ancient_lich_d3)는 1회 처치 후 재스폰 안 됨(`EnemySpawner.TrySpawnBoss` 억제). `HuntingContractManager`에 `RepeatableBossIds`(kraken/glacier_titan/inferno_drake/crystal_lord) 도입, `IsBossContractUnobtainable` = BossKill·비반복·이미 처치. `CanAccept`/`AvailableForRegion` 제외, `RestoreFromSave`는 **미완료**일 때만 폐기(TurnInReady면 보스 재스폰 불필요라 수령 가능 유지). → skeleton_king 계약은 "1회성 현상금" 의미.
- **[F3] 진행도 저장 누락창 차단**: `Inventory.AddItem`이 `OnItemPickedUp`보다 먼저 `OnInventoryChanged`→autosave를 발생시켜 Gather 진행이 다음 저장에서 누락될 수 있던 문제. `Advance()`가 변경 시 `SaveManager.RequestAutoSave()` 호출(GameTransaction 중엔 dirty만→종료 시 flush). Kill/Mining/Boss 진행에도 동일 적용.
- **[F4] Chain Bolt 감전 실제 동작**: `LightningStorm`이 SkillData를 보존하지 않아 `chain_bolt`의 Shock가 무시되던 문제. `ISkillTarget.StartLightningStorm` 시그니처에 element/status/dur/chance 추가, `PlayerController`가 storm 상태로 보존하고 매 틱 `_stormElement`로 데미지 + 확률 `ApplyStatusEffect`. `LightningStormStrategy`가 skill 값 전달, `ChainBoltStrategy`는 위임 상속으로 자동 적용. `lightning_storm`(Status=None)은 불변.
- **[F5] 드랍 희석 → 독립 가산형**: in-table append(weight 0.12)는 `PickDropIndex` 정규화로 기존 드랍 확률을 희석(0.7→0.7/1.12)했음. 60개 .tres의 PossibleDrops/DropWeights를 base(025f313)로 **원복**, `EnemyStats.RegionDrop`/`RegionDropChance`(0.1) 신설 + `EnemyController` 비보스 분기에서 **독립 굴림**(정규화 무관 → 기존 확률 불변, 진짜 가산형). `validate.py` R6에 RegionDropChance 0~1·정합 검사 추가.
- 검증 4종(validate/balance/build/test 20)+git diff --check green.

## Claude 적대적 리뷰 후속 hardening v3 (2026-05-18, 내구성·결합·경제·검증·신규메커닉)
- **F3 격상**: Gather 진행은 throttle된 RequestAutoSave가 아니라 **즉시 `SaveManager.SaveGame()`**(완료/포기와 동일). `AddItem`의 OnInventoryChanged→autosave가 OnItemPickedUp보다 앞서 진행이 유실되던 창을 실제로 닫음. Kill/Mining/Boss는 빈번하거나 직후 SaveGame 경로가 따로 있어 RequestAutoSave 유지(=일반 autosave 수준 내구성, 의도적·문서화).
- **F2 결합 제거**: "처치된 비반복 보스 계약 폐기"를 `RestoreFromSave`에서 분리 → 신규 `PruneUnobtainable()`를 **전체 복원 완료 후**(DefeatedBosses 확정) `PlayerController.LoadFromSaveData` 말미에서 1회 호출. 복원 호출 순서에 비의존(향후 reorder 시 조용히 무력화되던 잠재 결함 제거). TurnInReady면 보스 재스폰 불필요라 유지.
- **F5 경제 정합**: RegionDropChance 0.1→**0.05**(60 .tres). 구 in-table 실효율(DropChance 0.3~0.5 × 0.12/1.12 ≈ 3~5%/킬) 대역으로 맞춤 — 독립 굴림이지만 경제 영향이 구 의도와 정합. EnemyStats에 산출 근거 주석.
- **검증공백 해소**: validate.py R10에 `_collect_mining_ore_paths()` 추가 — Mining 계약 `targetOreItemPath`가 **실제 어떤 씬 MiningNode가 캐는 광석**인지 교차검증(Kill 강화와 대칭). 채광 불가 계약 재발 차단.
- **HUD**: throttle 0.15→**0.08s**(~12.5Hz) — 깜빡임 자연스럽고 0.08s↑ 지속 상태이상 칩 최소 1회 가시 보장.
- **LightningStorm 제약 명시**: 스톰 상태는 인스턴스 싱글턴 — lightning_storm↔chain_bolt 교차 시전 시 element/status/지속 덮어쓰기 + 타이머 리셋(동시 2스톰 불가, 의도된 단순화). 코드 주석화.
- **신규 메커닉 스킬 2종(v3, 위임 아님·전용 전략)**: `ChainLightning`(SkillType 35, 마법사 Lv22, mountain) = 적→인접 적 최대 5회 즉발 연쇄·점프마다 20% 감쇠; `LifeDrain`(36, 마법사 Lv19, harbor) = 단일 강타 + 피해 1/3 즉시 흡혈. ISkillTarget 기존 표면(GetNearbyEnemies/HealSelf/TriggerCameraShake)만 사용해 신규 API/회귀 0. 스킬·스킬북 .tres + 거점 스킬상점 배치.
- 잔여 한계(정직): Gather 외 진행도는 여전히 일반 autosave 내구성(하드 크래시 시 throttle 창 존재 — 설계상 게임 전체와 동일). 신규 v3 2종 외 v2 8종은 여전히 "기존 동작 리스킨"(메커닉 다양성 아님). 검증 4종+diff green.

## 7-에이전트 적대적 리뷰 후속 hardening v4 (2026-05-19, 경제 익스플로잇 봉쇄)
Opus 적대적 리뷰어 7명 병렬 + Codex 리뷰 종합 반영. **ship-blocker 2건(무한 골드/재료) 봉쇄.**
- **[치명·A안] Gather = 무한 골드/EXP 분수 봉쇄**: 비소모+무한재수락+구매도 픽업카운트+채광 이중카운트 → 무한 머니. **A안 채택** — `NotifyItemAcquired`가 *현재 인벤 보유 수량* 기반으로 진행/TurnInReady 산정(누적 폐기), `Complete()`가 Gather일 때 `HasItems` 재검증 후 트랜잭션 내 `ConsumeItems(Goal)` 소모(보유 부족이면 완료 실패+진행 재계산). 구매/판매/채광 이중카운트·수량 미반영 익스플로잇 원천 차단. desc도 "납품(소모)"으로 정정.
- **[치명·경제] enhance_stone faucet 차단**: 게이트 통화(Price=0)가 반복 계약으로 무한 누수. 반복 계약(town_mine_iron/coast_boss_kraken[반복보스]/mountain_mine_crystal)에서 enhance_stone 제거(골드 보전), 1회성 보스(outpost_boss_skeletonking)만 qty 1 유지. `balance.py` B5 신설 — 반복 계약 enhance_stone 지급 시 **에러**(영구 재발 차단).
- **[높음·세이브] SaveGame 트랜잭션 안전**: `SaveManager.SaveGame()`이 `_autoSaveSuspendCount>0`이면 dirty만(RequestAutoSave와 동일). 보스 트랜잭션 내 Gather 픽업→즉시 SaveGame이 RecordBossDefeat 전에 디스크 박는 부분상태 차단(직후 명시 SaveGame은 Dispose 이후라 정상 기록).
- **[P3·Codex] 권역 플래그 멱등화**: `BackfillRegionFlagsV13`를 버전 게이트 밖에서 무조건 호출 — 수정 전 만든 v13 로컬 세이브도 DefeatedBosses에서 누락 마커 보정(중복 추가 없음).
- **[P3·Codex] Accept 즉시 SaveGame**: Abandon/Complete와 일관, 수락 직후 하드크래시 보존.
- **[P1·Codex] 신규 스킬 4파일 스테이징**: chain_lightning/life_drain + 스킬북 2 git add(클린 체크아웃에서 스킬상점 깨짐 방지).
- **[중간·밸런스] RegionDropChance 티어링**: 일률 0.05 → 일반재 0.05 / sapphire 0.03 / 상위 강화·재련재(drake_scale·glacier_shard·crystal_ore) **0.02**. EnemyStats 주석을 "DropChance 게이트 없는 faucet이라 등급별 차등" 으로 정정(이전 "구 실효율 정합" 표현 수정).
- **[중간·스킬] ChainLightning 풀 확대**: 플레이어중심 `GetNearbyEnemies(300)` → `range+maxJumps*jumpRange(1150)` 수집(먼 연쇄 끊김 해소).
- **[중간·UX] 계약 보드 3허브 중앙축 이격**: field_outpost(520,150)/harbor_village·mountain_refuge(420,140) — player→portal y=180 통로·중앙 x 회피.
- **[중간·UX] HUD**: 팝업 외부탭 dismiss(전체화면 캡처 ColorRect) + 활성 시그니처 변경 시에만 행 재생성(80ms마다 QueueFree 폐기), PendingReward 보류 시 정직한 토스트. 미니맵은 단일화면 허브(≤760×440)에서 자동 숨김.
- **[검증] validate.py**: R13(스킬 .tres Type 전역 유일 + chain_lightning=35/life_drain=36 — Type충돌 재발 차단), R14(RepeatableBossIds ↔ 씬 RepeatableBoss=true 정확일치 — 하드코딩 드리프트 차단), R6+/R10+(RegionDrop·계약 보상/타깃이 ItemData인지 script_class 검사).
- **v4 재리뷰 후속(2026-05-19, Codex 적대 2건)**:
  - [높음] `SaveManager.SaveGame()` suspend 가드가 `TryClaimPendingRewards`의 "큐 제거 직후 즉시 저장"을 dirty로 격하 → 크래시 시 펜딩 보상 중복 위험. `GameTransaction.Dispose` 순서를 **ResumeAutoSave → ResumePendingRewardClaims**로 교체(autosave 먼저 풀어 claim의 SaveGame이 실제 기록). 본문은 이미 종료라 중간상태 위험 없음.
  - [중간] Gather 진행도가 "현재 보유" 모델인데 재계산 경로가 `NotifyItemAcquired` 뿐 → 수락 전부터 재료를 보유했거나 로드 후엔 영영 완료 불가. 단일 재계산 `RecomputeGatherProgress()` 신설, **Accept 직후 / 복원 완료 후(PlayerController) / 획득 시** 모두 호출. `Complete()`의 인벤 재검증을 `TurnInReady` 게이트 *앞*으로 이동(현재 보유로 완료 가능 판정).
- **v4 재재리뷰 후속(2026-05-19, Codex P2×2/P3×1)**:
  - [P2] 납품형 Gather UI stale — 재료를 창고/상점으로 뺀 뒤에도 보드에 "완료" 잔존(데이터는 안전, UX만 혼란). `ContractBoardUI.Rebuild()` 진입 시 `RecomputeGatherProgress()` 호출 추가 → 보드 표시와 완료 가능 여부 항상 일치.
  - [P2] LifeDrain이 방어·저항·오버킬 적용 *전* 피해 기준으로 흡혈 → 과다 회복. `IDamageable.TakeDamageReporting(dmg, elem)` 신설(기본 구현=입력값 반환), `EnemyController`가 속성보정·Defense·잔여HP 클램프 후 **실제 적용 피해** 반환. `TakeDamage(int,Element)`는 이 메서드에 위임(동작 불변). LifeDrain이 반환값/3로 회복.
  - [P3] 계약 보드 Gather 라벨 "수집(획득 누적)" → "납품(완료 시 소모)"로 교체(실제 A안 설계와 일치).
- 잔여 한계(정직): Gather 외(Kill/Boss/Mining) 진행도는 여전히 게임 전체와 동일한 throttled autosave 내구성. v2 8종은 리스킨. 헤드리스 검증만 — Godot 런타임 시연 미수행(보드 stale/LifeDrain 체감/미니맵 터치는 에디터 실밟기 권장). 검증 4종+diff green, 신규 스킬 4파일 staged.

## 사용 흐름
- 창고: town 우하단 [창고] NPC → 가방/창고 목록 → 보관/꺼내기.
- 제작: town 대장간 인근 [제작] → 레시피 선택 → 제작.
- 재련: town 대장간 좌측 [재련] → 미장착 장신구 선택 → 재련(affix 재생성).
