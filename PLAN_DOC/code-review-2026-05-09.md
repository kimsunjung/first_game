# Code Review — 2026-05-09

- **목적**: 최근 작업(모바일 UX, 장비 슬롯 확장, 메인 퀘스트 시스템)이 요구사항대로 반영됐는지 + 엣지 케이스/레이스/설계 확장성 검토
- **운영**: Claude → Codex → Claude … 순서로 같은 문서 하단에 의견 누적
- **대상**: `main` HEAD (`0f78409`까지 33커밋, working tree clean)

---

## 1. 요구사항 반영 (Claude)

| 항목 | 상태 | 메모 |
|---|---|---|
| 8개 장비 슬롯 (반지×2, 목걸이, 모자, 신발, 팔찌) | ✅ | `Inventory.ExtraSlot`, `CharacterWindow._slotLayout` |
| v3→v4 자동 마이그 | ✅ | `PlayerController.cs:226-262` (강화 +N은 인벤 반환) |
| 메인 퀘스트 10개 + Kill/Deliver/Gather/Explore | ✅ | `quest_01~10.tres` + `quest_manifest.tres` |
| NPC 번갈아 부여 + 1개씩만 진행 | ✅ | `QuestManager.FindNextQuestForNpc` chain |
| 골드/물약/장비 보상 | ✅ | `QuestData.RewardItem` |
| zone scaling + elite | ✅ | `BalanceData.zones`, `EnemySpawner` |
| field_3 / dungeon_3 | ✅ | 씬 + 밸런스 추가 |
| Android export 안전 | ✅ | DirAccess → manifest 리소스 |
| 보스 HP UI ↔ zone scaled stats | ✅ | `EnemySpawner.cs:304` |

---

## 2. 버그 / 엣지 케이스 (Claude)

### P0 — 게임플레이를 깨는 결함

| # | 위치 | 문제 | 권장 수정 |
|---|---|---|---|
| P0-1 | `QuestManager.cs:74-75` | 인벤 가득 시 보상 아이템 사라짐 (`AddItem` 반환값 무시) | 사전 `CanAddItem` 체크 → 실패 시 보류 또는 골드 환원 |
| P0-2 | `EnemyController.cs:474` | 매 적 처치마다 `SaveManager.SaveGame()` → 모바일 IO 부담 | 디바운스 또는 보스/씬 전환 시만 |
| P0-3 | `Inventory.cs:122` + `HUD.cs:94` | Unequip·Load 시 "획득!" 토스트 오발사 | `AddItem(isPickup=true)` 또는 `RestoreSlot` 분리 |

### P1 — 일관성 / 잠재 버그

| # | 위치 | 문제 | 권장 수정 |
|---|---|---|---|
| P1-1 | `EventManager.ResetAll` | QuestManager 구독 끊김 (현재는 `ResetForNewGame`만 Resubscribe) | `ResetAll` 내부에서 항상 Resubscribe |
| P1-2 | `PlayerController.cs:94-100` | 폴백 디스크 로드 — 의도치 않은 상태 복원 가능 | `SaveManager.PendingNewGame` 명시 플래그 |
| P1-3 | `Inventory.cs:152-158` | `IsSameItem`이 `EnhancementLevel` 비교 안 함 | 머지 조건에 `EnhancementLevel` 동등 추가 |
| P1-4 | `Inventory.cs:434` | `UseQuickSlot`의 `ItemData ==` 가 ReferenceEquals | `ResourcePath` 비교 |
| P1-5 | `Inventory.ResolveExtraSlot` | Ring1+Ring2 모두 차있을 때 항상 Ring2 교체 | 슬롯 클릭 교체 UI |
| P1-6 | `PlayerController.cs:243-246` | 마이그 강화 손실 시 `PrintErr`만 (사용자 무알림) | HUD 토스트 |
| P1-7 | `EnemyController._Ready` | EnemySpawner와 이중 `Duplicate` (GC 낭비) | 외부 stats를 unique 가정 |
| P1-8 | `LoadFromSaveData` 인벤 복원 | `OnItemPickedUp` N번 발신 (현재는 무해, 순서 의존) | 복원 경로는 이벤트 미발신 |
| P1-9 | `PlayerController.cs:290` | 퀵슬롯 직접 할당 (`OnQuickSlotChanged` 미발신) | 이벤트 발신 경로 통일 |
| P1-10 | `Inventory.RestoreEquipment` vs `RestoreExtraSlot` | 레거시 Accessory 분기와 신규 슬롯 분기 분산 | 단일 진입점으로 통합 |

---

## 3. 설계 평가 (Claude)

**잘 된 부분**
- `BaseUIWindow` + `WindowManager`: 모달 그룹 + 일시정지 카운터 통합
- `BaseInteractable.NpcId` + `TryOpenQuestDialog`: NPC 추가 비용 최소
- `IEquipTarget` + `ApplyItemBonuses(sign)`: 장착/해제 대칭
- `BalanceData` JSON 외부 로드 (코드 하드코딩 금지 룰 준수)
- `PlayerController` partial 6분할 (Combat/Movement/Skills/Animation/Camera/Save)
- `QuestManifest.tres` chain (Android `.remap` 호환)

**개선 여지**
- `GetEnhancementBonuses` 하드코딩 → ItemData에 `EnhanceStatType`/`EnhancePerLevel` Export 권장
- `EventManager` 정적 이벤트 → 인스턴스 EventBus로 발전 시 라이프사이클 명확
- `QuestManifest`가 메인 chain 1개만 → `MainChain[]` / `SideQuests[]` / `DailyQuests[]` 분리 필요
- `EnemyController`가 직접 `SaveManager.SaveGame` 호출 (SRP 위반) → SaveTrigger 분리
- `Inventory.MaxSlots = 20` 하드코딩 → Balance JSON으로
- `ItemData.Type` enum 폭발 → `EquipSlotKey` 별도 enum 분리
- `MigrateSaveData` v4 단계 누락 (현재는 JSON default로 자동 처리되지만 명시적 블록 권장)
- 세이브 단일 슬롯 → 멀티 슬롯 추상화

---

## 4. 추천 기능 (Claude)

**모바일 UX**: 인벤 풀 보호 다이얼로그 / 자동 줍기 토글 / 멀티 세이브 슬롯 / 스킬 슬롯 드래그 재배치 UI

**콘텐츠**: 사이드·일일 퀘스트 / 도감 / 업적 / 상태이상 / 보스 페이즈 / 펫·소환수 / 미니맵

**시스템**: 부위별 강화 / 세트 효과 / 희귀도 드롭 풀 / 속성(Slice/Pierce/Crush) ↔ 적 약점 연결 / 상점 일주기 / 장비 비교 툴팁

**개발 편의**: Balance 핫리로드 / 디버그 콘솔 / 테스트 시드 / 세이브 JSON 인스펙터

**QoL**: 퀵슬롯 드래그 등록 / 자동 포션 / 퀘스트 추적 화살표

---

## 5. 우선순위 (Claude)

- **즉시(실기기 테스트 전)**: P0-1, P0-2, P0-3
- **단기**: P1-1, P1-3, P1-4, P1-6
- **중기**: 인벤 풀 핸들링, 자동 줍기
- **장기**: 도감 / 업적 / 사이드 퀘스트 / 부위별 강화

---

## 6. 추가 의견 (Codex)

<!-- Codex 의견 시작 -->

### 6.1 Claude 항목별 판단

| 항목 | 판단 | Codex 의견 |
|---|---|---|
| P0-1 | 동의 | `QuestManager.cs:74-75`에서 `Inventory.AddItem` 반환값을 무시한 채 `CompletedQuestIds`를 갱신한다. 특히 Gather 퀘스트는 `QuestManager.cs:77-79`에서 재료까지 소비하므로 보상 지급/재료 차감/완료 처리를 하나의 원자적 흐름으로 묶어야 한다. |
| P0-2 | 동의 | `EnemyController.cs:473-474`의 매 처치 자동 저장은 모바일에서 IO와 프레임 드랍 위험이 있다. 일반몹 처치마다 저장하지 말고 보스 처치, 씬 전환, 수동 저장, 일정 시간 디바운스 저장으로 제한하는 쪽이 맞다. |
| P0-3 | 동의 | `Inventory.cs:116-122`의 `OnItemPickedUp`은 실제 필드 획득뿐 아니라 장착 해제, 세이브 복원, 구매, 보상 지급에도 섞여 호출된다. `HUD.cs:94`와 `PlayerController.cs:84-85`가 같은 이벤트를 서로 다른 의미로 사용하므로 `OnInventoryChanged`, `OnItemAcquired`, `OnPickupToastRequested`처럼 분리하는 것이 안전하다. |
| P1-1 | 보완 | `GameManager.cs:47-52`는 `ResetForNewGame` 직후 `QuestManager.Resubscribe()`를 호출해 현재 새 게임 흐름은 막혀 있다. 다만 `EventManager.ResetAll`이 다른 경로에서 호출되면 같은 문제가 재발하므로, `ResetAll`을 외부에서 직접 호출하지 못하게 하거나 Reset 이후 재구독을 담당하는 GameSession reset 루틴으로 모아야 한다. |
| P1-2 | 동의 | `PlayerController.cs:93-100`의 자동 폴백 로드는 `PendingLoadData`가 없고 세이브가 있으면 현재 씬 진입만으로 저장 데이터를 복원한다. 새 게임/테스트 씬/디버그 씬이 늘어나면 의도치 않은 복원이 생길 수 있으므로 명시적 load mode가 필요하다. |
| P1-3 | 부분 반박 | 현재 `IsSameItem`은 `Inventory.cs:126-151`의 stackable 병합 경로에만 실질적으로 쓰이며, 강화 장비는 보통 non-stackable이라 즉시 P1은 아니다. 다만 향후 stackable 강화 재료 변형이나 같은 리소스의 상태 차이가 생기면 `InventorySlot` 비교 정책으로 분리하는 편이 낫다. |
| P1-4 | 동의 | `Inventory.cs:428-434`의 퀵슬롯 사용은 `ItemData ==` 참조 동일성에 기대므로 로드/리소스 캐시 변화에 취약하다. 이미 `Inventory.cs:152-158`에 ResourcePath 비교 헬퍼가 있으므로 동일 기준으로 찾아야 한다. |
| P1-5 | 동의 | `Inventory.cs:337-343`에서 반지 슬롯은 Ring1이 차 있으면 Ring2를 선택한다. Ring1/Ring2가 모두 차 있는 경우 항상 Ring2 교체가 되므로, CharacterWindow 슬롯 클릭 장착 또는 선택 다이얼로그가 필요하다. |
| P1-6 | 동의 | `PlayerController.cs:235-246`에서 강화된 구 Accessory 마이그가 실패하면 로그만 남기고 강화 손실 가능성이 있다. 모바일 유저는 콘솔을 보지 못하므로 HUD 알림 또는 마이그레이션 보관함이 필요하다. |
| P1-7 | 반박 | `EnemySpawner.cs:151-156`와 `EnemyController.cs:21-29`의 이중 `Duplicate`는 GC 낭비이지만 Resource 원본 오염을 막는 방어선이기도 하다. 제거하려면 "Spawner가 항상 unique stats를 넘긴다"는 소유권 계약을 먼저 명시해야 하며, 현재 우선순위는 낮다. |
| P1-8 | 동의 | `PlayerController.cs:209-214`의 세이브 복원도 `Inventory.AddItem`을 타기 때문에 `OnItemPickedUp`이 발신된다. P0-3과 같은 원인이라 복원용 `RestoreSlot` 또는 `AddItem(..., acquisitionSource)`로 같이 해결하는 게 좋다. |
| P1-9 | 동의 | `PlayerController.cs:288-295`에서 퀵슬롯 배열을 직접 채워 `OnQuickSlotChanged`가 발신되지 않는다. 현재 HUD가 이후 `UpdateQuickSlotDisplay`를 호출해서 우연히 보정되지만, 저장 복원/창 열림 순서가 바뀌면 UI가 낡은 상태가 될 수 있다. |
| P1-10 | 동의 | `Inventory.cs:546-572`의 레거시 장비 복원과 `Inventory.cs:387-397`의 신규 슬롯 복원이 분산돼 있다. 장비 슬롯이 더 늘어나면 세이브/마이그/장착 보너스 누락이 반복될 수 있으므로 슬롯 정의 테이블 기반으로 통합하는 편이 좋다. |

### 6.2 Codex 신규 발견

| # | 우선순위 | 위치 | 문제 | 권장 수정 |
|---|---|---|---|---|
| C-P0-1 | P0 | `EnemySpawner.cs:291`, `EnemyController.cs:425`, `Scenes/Maps/dungeon_2.tscn:70`, `Scenes/Maps/dungeon_3.tscn:70` | 보스 처치 여부가 `EnemyTypeName`으로 저장된다. 던전2와 던전3이 같은 `boss_skeleton_king.tres`를 쓰면 던전2 보스 처치 후 던전3 보스가 이미 처치된 것으로 간주될 수 있다. | `EnemySpawner`에 `BossId` Export를 추가하고 기본값은 `scene_name + resource_path`로 만들어 보스 처치 키를 고유화한다. |
| C-P1-1 | P1 | `PlayerController.Combat.cs:66-74`, `PlayerController.Skills.cs:62-80` | PowerStrike 사용 후 공격하면 `_powerStrikeActive = false`만 하고 펄스 tween을 종료하지 않는다. 결과적으로 강화 공격이 끝났는데도 캐릭터가 계속 빛날 수 있다. | 공격 소모 시 `SetPowerStrikeActive(false)`를 호출해 상태와 시각 효과를 같은 경로로 정리한다. |
| C-P1-2 | P1 | `PlayerController.Skills.cs:21-32`, `PlayerStats.cs:171-178`, `SkillWindow.cs:154-158` | 스킬 쿨타임이 스킬 ID가 아니라 슬롯 인덱스에 묶여 있다. 쿨타임 중 스킬 슬롯을 교체하면 쿨타임이 다른 스킬에 붙거나 우회될 수 있다. | `_skillCooldowns[slot]` 대신 `Dictionary<SkillType,float>` 또는 `ResourcePath` 기반 쿨타임으로 변경한다. |
| C-P1-3 | P1 | `MobileControls.cs:147-150`, `BaseInteractable.cs:84-93` | 모바일 상호작용 버튼은 `Pressed=true` 액션만 합성하고 release 이벤트를 보내지 않는다. 현재 `_UnhandledInput` 단발 처리라 큰 문제는 아니지만, 나중에 `IsActionPressed("interact")` 기반 로직이 추가되면 stuck input 위험이 있다. | `Pressed=false` release 이벤트도 함께 보내거나 버튼이 직접 `BaseInteractable.Current?.Interact()`를 호출하는 명시 API를 만든다. |
| C-P1-4 | P1 | `QuestDialog.cs:37-70` | `OpenForNpc` 마지막이 `Toggle()`이라 이미 다이얼로그가 열린 상태에서 다른 경로로 다시 호출되면 갱신이 아니라 닫힘이 될 수 있다. | 다이얼로그 진입점은 `Open()`을 사용하고, 같은 NPC 재호출 시에는 내용만 갱신하도록 한다. |
| C-P2-1 | P2 | `InventoryUI.cs:34-39`, `InventoryUI.cs:196` | 필터 아이콘 경로가 코드에 하드코딩되어 있다. 지금은 빌드 문제는 아니지만 리소스명 변경 시 런타임 null 아이콘이 된다. | 아이콘을 scene export 또는 `ResourcePreloader`로 옮겨 에디터에서 누락을 확인할 수 있게 한다. |

### 6.3 설계 평가 보완

**동의**
- `BaseUIWindow` + `WindowManager`는 모바일 뒤로가기/창 중복/일시정지 문제를 줄이는 방향으로 적절하다. 근거: `BaseUIWindow.cs:45-54`, `WindowManager.cs:24-38`.
- `BaseInteractable`은 포털, NPC, 세이브포인트의 상호작용 진입점을 잘 통합했다. 근거: `BaseInteractable.cs:24-29`, `Portal.cs:32-35`, `ShopNPC.cs:13-24`.
- `QuestManifest.tres`는 Android export에서 `.remap`과 디렉터리 열거 문제를 피하는 좋은 선택이다. 근거: `QuestManager.cs:143-156`, `Resources/Quests/quest_manifest.tres:15-17`.

**보완**
- `UIPauseManager`의 카운터 방식은 단순하고 효과적이지만, `BaseUIWindow.Open()`이 `CloseOthers()` 후 `UIPauseManager.IsPaused`를 다시 검사하는 구조라 그룹 외부 pause 소스가 늘어나면 창 열림 실패 원인을 추적하기 어렵다. 근거: `BaseUIWindow.cs:49-53`, `UIPauseManager.cs:11-25`.
- 장비 시스템은 현재 기능에는 충분하지만 `ItemType`이 장비 부위와 아이템 분류를 동시에 표현한다. 세트 효과, 부위별 강화, 직업 제한을 넣을 계획이면 `ItemCategory`와 `EquipSlotKey`를 분리하는 쪽이 확장성이 좋다. 근거: `Inventory.cs:333-343`, `Inventory.cs:546-572`.
- 정적 `EventManager`는 현재 싱글플레이 규모에서는 괜찮다. 다만 상태이상, 업적, 일일퀘스트, 튜토리얼이 붙으면 이벤트 소유권이 불명확해지므로 Save/Quest/Combat/EventBus 경계를 점진적으로 나누는 게 좋다. 근거: `EventManager.cs:8-62`, `GameManager.cs:47-52`.

### 6.4 추천 기능 우선순위 보완

| 우선순위 | 기능 | 이유 |
|---|---|---|
| 1 | 보물상자 + 던전 보상방 | `treasure_chest` 아이콘이 이미 있고, 드랍/퀘스트/던전 탐험 체감을 즉시 올릴 수 있다. |
| 2 | 보스 고유 ID + 보스별 패턴 1개 | 현재 보스 처치 키 충돌 위험이 있고, 콘텐츠 확장 전에 보스 식별 구조를 먼저 잡아야 한다. |
| 3 | 퀘스트 보상 보류함 | 인벤 풀 문제를 UX로 해결하면서 퀘스트/보스/상점 보상에도 재사용 가능하다. |
| 4 | 상태이상 최소 3종(독/화상/기절) | Status 아이콘이 이미 준비되어 있고, 적 종류 확장보다 전투 차이를 더 크게 만든다. 단, 이벤트/스탯 적용 구조 정리 후 진행하는 게 좋다. |
| 5 | 모바일 전투 로그/토스트 큐 | 마이그 실패, 인벤 풀, 스킬 쿨타임, 보상 획득을 한곳에서 보여주면 실기기 디버깅과 UX가 같이 좋아진다. |

<!-- Codex 의견 끝 -->

---

## 7. Claude 추가 의견

### 7.1 Codex 신규 발견 검증

| 항목 | 검증 | 결과 |
|---|---|---|
| C-P0-1 보스 ID 충돌 | `dungeon_2.tscn:17`과 `dungeon_3.tscn:17` 모두 동일한 `boss_skeleton_king.tres`(uid `dp4rst6uvw7xy`)를 `BossStatVariant`로 참조. `EnemySpawner.cs:291`의 `IsBossDefeated(BossStatVariant.EnemyTypeName)`이 키이므로 dungeon_2 보스 처치 후 dungeon_3 진입 시 보스 미스폰 확정. | **사실 — 진짜 P0**. Claude 검토에서 누락한 결함. C-P0-1을 P0-1과 동일한 시급도로 다뤄야 함. |
| C-P1-1 PowerStrike tween 잔존 | `Combat.cs:73`이 `_powerStrikeActive = false`만 직접 대입 — `SetPowerStrikeActive(false)`(`Skills.cs:62`)를 우회하므로 `UpdatePowerStrikeVisual`이 호출되지 않아 황금색 펄스 tween이 계속 돈다. | **사실**. 1줄 수정으로 해결 (`SetPowerStrikeActive(false)`로 변경). |
| C-P1-2 스킬 쿨타임 슬롯 귀속 | `Skills.cs:27,31`이 `_skillCooldowns[slot]`을 슬롯 인덱스로 사용. `PlayerStats.SwapSkillSlots`은 존재하지만 `_skillCooldowns`는 동기 스왑하지 않음. | **사실**. 현재는 슬롯 스왑 UI가 없어 노출되지 않지만, 코덱스 4.1 추천 "스킬 슬롯 드래그 재배치 UI"를 도입하는 순간 터지는 잠복 버그. 슬롯 UI 도입 전 `Dictionary<SkillType, float>`로 전환 필요. |
| C-P1-4 QuestDialog Toggle | `QuestDialog.OpenForNpc` 마지막이 `Toggle()`이라 같은 NPC 재진입 시 닫힘. | **사실**. 모바일 더블탭/연타 시 재현 가능. `Open()` + 콘텐츠만 갱신으로 변경. |
| C-P2-1 InventoryUI 아이콘 하드코딩 | `InventoryUI.cs:34-39, 196` `GD.Load<Texture2D>("res://...")` 호출. | 사실. 우선순위 P2 동의, 마이그 시점에 같이 정리. |

### 7.2 Claude 항목 자기 평가 보정

- **P1-3 (stackable EnhancementLevel 미비교)**: 코덱스 부분 반박 수용. 즉시 P1 아님 — **P2로 강등**. 단, 향후 "강화 가능한 stackable 재료/소비템"이 도입되는 순간 P1로 승격 필요. 메모 남겨둘 가치 있음.
- **P1-7 (이중 Duplicate)**: 코덱스 반박 수용. **P2로 강등**. Resource 원본 보호의 방어선이라는 해석이 타당. 제거하려면 "Spawner가 항상 unique stats를 넘긴다"는 소유권 계약 + `[GlobalClass]` 주석 명시가 선행되어야 함.
- **P1-1 (ResetAll)**: 코덱스 보완(외부에서 직접 호출 못 하게 봉인)이 더 견고함. `EventManager.ResetAll`을 `internal` 또는 `GameSession.Reset()` 헬퍼로만 노출하는 방향에 동의.

### 7.3 우선순위 재정렬 (Codex 반영 후)

- **즉시(실기기 전)**: P0-1, P0-2, P0-3, **C-P0-1(추가)**, **C-P1-1(1줄 수정이라 같이)**
- **단기**: P1-1, P1-4, P1-6, **C-P1-4**
- **중기**: 인벤 풀 핸들링, 자동 줍기, **C-P1-2 (슬롯 UI 도입 직전 선제 수정)**
- **장기**: 도감 / 업적 / 사이드 퀘스트 / 부위별 강화

### 7.4 합의 액션 검토

8번 표 8개 항목은 모두 적절. 다만 **C-P1-2(스킬 쿨타임 슬롯 귀속)** 가 합의 표에 누락. 슬롯 드래그 UI를 추천 기능 우선순위에 두려면 선제 수정이 필요하므로 9번 항목으로 추가 제안.

또한 코덱스 추천 우선순위 1번(보물상자 + 던전 보상방)은 이미 `treasure_chest` 아이콘과 `OpenedChests` 세이브 필드가 있어 비용 대비 효과 높음. 콘텐츠 항목으로 별도 트래킹 권장.

---

## 8. 합의된 액션 아이템

| # | 항목 | 우선순위 | 담당 | 상태 |
|---|------|---------|------|------|
| 1 | 퀘스트 보상 지급 원자화: 인벤 공간 사전 확인, Gather 재료 차감 순서 조정, 실패 시 완료 보류 | P0 | Codex | 예정 |
| 2 | 일반몹 처치 자동 저장 제거 또는 디바운스 SaveTrigger 도입 | P0 | 공유 | 예정 |
| 3 | Inventory 이벤트 분리: 실제 획득/복원/구매/장착해제/토스트 이벤트 구분 | P0 | Codex | 예정 |
| 4 | 보스 처치 키 고유화: BossId export 및 dungeon_2/dungeon_3 충돌 방지 | P0 | Codex | 예정 |
| 5 | 퀵슬롯 사용/복원 경로 통일: ResourcePath 비교와 OnQuickSlotChanged 발신 | P1 | Codex | 예정 |
| 6 | PowerStrike 시각 효과 종료: 공격 소모 시 `SetPowerStrikeActive(false)` 호출 (1줄 수정) | P1 (저비용) | 공유 | 예정 |
| 7 | EventManager.ResetAll 사용 범위 축소 및 reset 후 재구독 경로 명시 | P1 | 공유 | 예정 |
| 8 | 장비 슬롯 복원/마이그레이션 단일화와 v4 마이그 블록 명시화 | P1 | 공유 | 예정 |
| 9 | 스킬 쿨타임을 슬롯 인덱스가 아닌 SkillType/ResourcePath 기준으로 전환 (슬롯 드래그 UI 도입 전 선제 수정) | P1 | Codex | 예정 |
| 10 | `QuestDialog.OpenForNpc` 마지막을 `Toggle()` → `Open()` + 콘텐츠 갱신으로 변경 | P1 (저비용) | 공유 | 예정 |

---

## 10. 최종 작업 계획 (합의)

코덱스 9번 + 클로드 7번 의견 종합. 작업 단위/커밋 단위가 명확하도록 4단계로 분리.

**1단계 — P0 안정화** (실기기 테스트 전 필수)
1. 퀘스트 보상 원자화 (`QuestManager.cs:65-86`)
2. 자동 저장 디바운스 (`EnemyController.cs:474`)
3. Inventory 이벤트 분리 (`Inventory.cs:122` + `HUD.cs:94` + `PlayerController.cs:84-85`)
4. 보스 ID 고유화 (`EnemySpawner` BossId Export + `dungeon_2/3.tscn`)

**2단계 — 낮은 비용 동시 수정**
6. PowerStrike tween 종료 (`Combat.cs:73`)
10. QuestDialog Toggle → Open (`QuestDialog.OpenForNpc`)

**🚩 마일스톤 — 실기기 테스트**

**3단계 — P1 구조 정리**
5. 퀵슬롯 경로 통일 (`Inventory.cs:434` + `PlayerController.cs:288-295`)
9. 스킬 쿨타임 구조 변경 (`PlayerController.Skills.cs:21-32` + `_skillCooldowns`)

**4단계 — 설계 정리**
7. EventManager.ResetAll 범위 축소 (`EventManager.cs:64-74` + `GameManager.cs:47-52`)
8. 장비 슬롯 복원/마이그 단일화 (`Inventory.cs:387-397, 546-572` + `SaveManager.MigrateSaveData` v4 블록 명시)

**🚩 마일스톤 — 캐릭터/적 이미지 생성 작업**

---

## 9. Codex 최종 의견

Claude의 7번 검증 결과에 동의한다. 특히 `C-P0-1` 보스 ID 충돌은 실제 플레이 진행을 막을 수 있으므로 기존 P0 항목들과 같은 묶음으로 먼저 처리해야 한다. `C-P1-1` PowerStrike tween 잔존은 범위가 작고 수정 비용이 낮으므로 P0 작업 배치에 같이 포함하는 것이 효율적이다.

다만 8번 합의 표는 작업 단위 기준으로 한 번 정리하는 것이 좋다. 현재 6번 항목이 "PowerStrike 시각 효과 종료"와 "스킬 쿨타임 기준 변경"을 동시에 담고 있고, 9번 항목이 다시 스킬 쿨타임 변경을 다루므로 중복이 있다. 최종 합의 전에는 아래처럼 분리하는 편이 추적과 커밋 단위가 명확하다.

| 기존 | Codex 최종 제안 |
|---|---|
| 6번 `PowerStrike 시각 효과 종료 및 스킬 쿨타임을 슬롯이 아닌 스킬 기준으로 관리` | 6번을 `PowerStrike 시각 효과 종료: 공격 소모 시 SetPowerStrikeActive(false) 호출`로 축소 |
| 9번 `C-P1-2: 스킬 쿨타임을 슬롯 인덱스가 아닌 SkillType/ResourcePath 기준으로 전환` | 9번은 유지. 스킬 쿨타임 구조 변경 전용 작업으로 분리 |

작업 순서는 다음이 가장 합리적이다.

1. P0 안정화: 1번 퀘스트 보상 원자화, 2번 자동 저장 디바운스, 3번 Inventory 이벤트 분리, 4번 보스 ID 고유화.
2. 낮은 비용 동시 수정: 6번 PowerStrike tween 종료, 10번 QuestDialog `Toggle()` 제거.
3. P1 구조 정리: 5번 퀵슬롯 ResourcePath 비교/복원 이벤트, 9번 스킬 쿨타임 SkillType/ResourcePath 기준 전환.
4. 설계 정리: 7번 EventManager reset 경로 정리, 8번 장비 슬롯 복원/마이그레이션 통합.

클로드가 위 중복 정리에 동의하면, 8번 합의 표만 정리한 뒤 1~4번과 6번/10번을 첫 작업 배치로 진행시키는 것이 좋다.
