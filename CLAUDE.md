# Project Instructions

## Environment
- OS: macOS / Shell: zsh
- Engine: Godot 4.6 (C# / .NET 8)
- Target: 모바일(Android/iOS) 출시. 개인 성취 목적, 수익 무관.

## 게임 방향 (v2 — 2026-05-15 재정의)
- 장르: **클래식 PC식 오픈엔드 사냥터 성장 RPG** (미르의전설3 / Diablo 1 / Ultima IV / Crystalis 후반부 결)
- 컨셉: **단순·심플 컨트롤** 유지. 그러나 진행은 **권역형 오픈엔드 성장 루프**.
- 자동전투 없음 — 직접 조작 전투만.
- **"엔딩형 RPG가 아니다."** 고대 리치 같은 메인 보스 처치는 게임 종료가 아니라 그 권역의 졸업 표지자. 그 뒤에도 상위 권역에서 캐릭터 성장이 계속된다.
- 챕터/스토리는 지역 소개 + 진행 표지자 역할만. 메인 보스 처치로 게임이 끝나는 시퀀스를 만들지 말 것.

## 핵심 게임 루프 (v2)
```
거점 보급 → 사냥터/광산/보스 파밍 → 드랍/골드/재료 회수 → 장비 강화/스킬 성장/소모품 정비 → 더 강한 사냥터 진입 → 반복
```
보스 처치 = 그 권역의 졸업 신호이지 게임 종료가 아니다. 야외/광산 Named 보스는 `EnemySpawner.RepeatableBoss=true`로 first-kill 기록과 별도로 재파밍 가능.

## 거부된 시스템 (제안 금지, v2 갱신)
- 리니지 라이크 BM (가챠 / 시즌 패스 / VIP / 자동전투 / 일일숙제 / 주간 토벌)
- 도감(Compendium) / 절차적 던전 룸 생성 / 펫 동료
- 무한 탑 / 던전 모디파이어 등 시즌 그라인드 콘텐츠
- 요즘 모바일 RPG식 복잡한 컨트롤·UI

**단, 클래식 PC RPG식 사냥터 반복 파밍 루프는 의도된 디자인이다.** 이건 BM식 그라인드와 다르다 — 사냥터 자체가 끝(엔딩) 없는 성장의 무대고 플레이어가 자기 페이스로 진행한다.

## 월드 구조 (v2 — 권역형)
4개 권역, 각 권역은 거점 1곳 + 사냥터 여러 곳 + 광산/던전/보스 지역으로 구성.

| 권역 | 거점 | 주요 사냥터 | 던전/광산/보스 |
|---|---|---|---|
| town_region | town | town_outskirts, green_meadow, goblin_woods, old_orc_road, field_1 | mine_1, dungeon_1 (오크 워로드) |
| outpost_region | field_outpost | field_2(graveyard_edge), ruined_crossroad, field_3 | mine_2, dungeon_2 (스켈레톤 킹), dungeon_3 (고대 리치) |
| coast_region | harbor_village | harbor_outskirts, crab_beach, pirate_camp, field_4_harbor | dungeon_4_sunken_shrine (크라켄, 반복) |
| mountain_region | mountain_refuge | snowfield_edge, frozen_valley, field_5_snowfield, volcano_approach, lava_field, field_6_volcano | mine_3 (크리스탈 로드, 반복) + 글래시어 타이탄 + 인페르노 드레이크 (모두 반복) |

주요 거점 4곳: **town, field_outpost, harbor_village, mountain_refuge**. 각 거점에 SavePoint + Shop + SkillShop + 다방향 포탈. 스킬북 26종은 거점별로 분산 (town 10권 / outpost 7권 / harbor 5권 / mountain 4권).

상세는 `PLAN_DOC/regional-world-map-plan.md`, `PLAN_DOC/world-flow-plan.md` 참조.

## 클래스 시스템
- `PlayerClass`: Warrior(STR) / Mage(INT) / Archer(DEX). **CON은 공통**
- 신규 게임 시 MainMenu에서 클래스 선택 → 시작 무기 + 시작 스킬 자동 지급
- 스킬은 `SkillData.RequiredClass` + `AvailableToAllClasses`로 분기

## 챕터 플래그 (의미 v2)
챕터는 여전히 `SaveData.ChapterFlags` / `GameManager.CurrentChapter`로 추적되지만, **"엔딩 트리거"가 아니라 "권역 졸업 표지자"**다.
- `ChapterFlags.OrcWarlordKilled` = town_region 졸업
- `ChapterFlags.SkeletonKingKilled` = outpost_region 중간 졸업
- `ChapterFlags.LichKilled` = outpost_region 정점 졸업 (게임 종료 아님!)

자동 트리거: 포탈 진입 / 보스 처치 (`GameManager.RecordBossDefeat`, `RecordSceneVisit`). 챕터 대사: `Scripts/Data/ChapterDialogue.cs`.

## 작업 원칙
- 밸런스 수치 코드 하드코딩 금지 → `Resources/Balance/game_balance.json`
- `SaveData.CurrentScene` 항상 저장 — LoadGame()은 저장된 씬으로 복귀
- Stats(Resource) 스폰 시 반드시 `Stats.Duplicate()` — 원본 보호
- EnemySpawner에서 `enemy.AddToGroup("Enemy")` 필수
- EnemySpawner export는 **`StatVariants` / `SpawnInterval` / `MaxEnemies` / `SpawnRadius` / `BossStatVariant` / `BossId` / `RepeatableBoss`**. (과거 `EnemyStatsList` / `SpawnCount` / `SpawnAreaSize` / `SpawnAreaOffset` 표기는 무효 — 이전 씬에 남아 있으면 적이 스폰되지 않음)
- Autoloads 3개만: GameManager, AudioManager, SceneManager
- GameManager._ExitTree에서 `Instance = null` 필수
- tscn 직접 편집 시 UID 충돌 주의

## 검증·테스트 인프라 (2026-05-16 신설 — Godot 런타임 불필요)
큰 변경 후 아래 3개 게이트를 **모두 green** 으로 유지할 것. CI(`.github/workflows/ci.yml`)가 push/PR마다 동일하게 검증.
- `python3 tools/validate/validate.py` — 리소스 무결성 (res:// 해결, 헤더 uid 유일성, EnemySpawner 표준 API/폐기 API, balance zones↔씬, 스킬북↔스킬, 드랍 weight 개수, teleport ScenePath)
- `python3 tools/validate/balance.py` — 밸런스 정합성 (스폰 적 빈 드랍 금지, 난이도(hp×atk)↔expMul 강한 역전 금지, 씬 참조 보스의 BossId 누락 금지). 경고(완만한 역전·사장 보스 리소스)는 종료코드 무관.
- `dotnet test tools/Tests/FirstGame.Tests.csproj` — Godot 미의존 순수 로직 단위 테스트(11개: ChapterDialogue 등록NPC 전챕터 완비 + 사전↔씬 NpcId 정합 + enum/세이브키 안정성). 게임과 동일 **net8.0**(RollForward=Major라 net8 런타임 없는 환경에서도 실행). `tools/Tests` 는 독립 프로젝트라 `first_game.sln`/csproj에 추가 금지(게임 빌드는 `tools/**` 제외 처리됨).
- 새 사냥터/적/zone 추가 시 위 검사를 통과시키는 게 완료 기준. 적은 반드시 비어있지 않은 PossibleDrops + DropChance>0.

## Regional Weather & Hunting v3 (2026-05-16)
신규 맵 없이 기존 사냥터 정체성 강화. 상세 `PLAN_DOC/regional-weather-hunting-v3.md`.
- **가중치 스폰**: `EnemySpawner.StatWeights : float[]`. null/길이불일치/합≤0이면 균등 fallback. 씬 .tscn `StatWeights = PackedFloat32Array(...)`. validate.py R3가 길이·합 검사.
- **날씨**: `Scripts/Maps/BiomeWeatherController.cs` — 씬 루트 Node2D, 코드로 오버레이/파티클 + 저확률 hazard(`Stats.ApplyStatus`). 새 PNG·세이브 상태 없음. 허브는 hazard 없음. 28맵 배치 완료.
- **상태저항 배선**: `CharacterStats.StatusResist`는 **raw 누적값**(장비+버프 합산, 저장값 자체는 clamp 안 함). `PlayerStats.ModifyStatusResist`는 `+= delta`만. 면역 상한(0~0.85)은 **사용 시점** `CharacterStats.ApplyStatus`에서만 clamp(버프 만료가 장비 저항을 깎지 않게). `ItemData.BonusStatusResist`(장비, Inventory equip/clear/restore), `BuffStatusResist`(소모품 Buff, `ApplyBuffEx` 선택 인자+만료 차감). 저장 미수정 — 로드 후 장비 재적용으로 복구.
- **신규**: 날씨 테마 적 12종 + 소모품 6종. **전부 기존 PNG/아이콘 임시 재사용**(v3 문서에만 기록, generated-asset-inventory 미수정). 18 사냥터에 테마 적을 희귀 가중치로 추가, field_1/보스던전 구조 유지.
- 검증 4종(validate/balance/build/test)+ git diff --check 모두 green 유지가 완료 기준.

## 허브 루프 + 사냥 계약 v1 (2026-05-18)
- 허브 준비 루프: 공유 창고(SaveData v12) / 확정 제작(recipes.json) / 미장착 장신구 affix 재련. **4개 거점(town·field_outpost·harbor_village·mountain_refuge) 전부**에 창고/제작/재련 NPC 배치 완료.
- **사냥 계약**: 메인 `QuestManager`와 독립된 별도 `Scripts/Core/HuntingContractManager.cs`(GameManager.ContractManager). **메인 QuestManager를 계약용으로 확장 금지.** `Resources/Contracts/contracts.json` 16개(권역별 4 / Kill·Gather·Mining·BossKill). SaveData **v13** `ActiveContracts`(backfill). 동시 3개·완료 후 재수락·일일/시간제한 없음(반복 파밍 보조). 보상은 골드/EXP 항상 + 아이템 PendingReward 폴백(손실 금지). 완료/포기는 GameTransaction + 명시 SaveGame. 보드 NPC 4거점 배치(Region Export).
- 진행 이벤트: Kill=`OnEnemyKilledTyped`, BossKill=신설 `EventManager.OnBossKilled`, Mining=신설 `OnOreMined`, Gather=`Inventory.OnItemPickedUp` 누적형(완료 시 차감 없음). `EventManager.ResetAll`이 `ContractManager.Resubscribe` 자동 호출.
- 사냥터 보강: mine_3 광맥 5개 추가, FieldItem 희귀도 Loot glow(코드 Sprite2D), 18개 사냥터 환경 장식 Sprite2D(Collision 없음·z=-2 → 동선 차단 불가). 상세 `PLAN_DOC/hub-preparation-loop-v1.md`.
- 검증: validate.py R10(contracts)/R11(보드)/R12(MiningNode), xUnit `ContractTests`. 4종+diff green 유지.
- **UX/스케일 후속**: HUD 상태이상 칩 전용 아이콘(`Icons/Status/*`, Curse 색상 폴백) / HUD 좌측하단 맵 이름(`Scripts/Data/MapNames.cs`) / 캐릭터 시각 크기 −20%(`Scripts/Core/GameScale.cs` `CharacterVisual=0.8`, 적·플레이어·NPC 스프라이트만 — 콜리전/상호작용/사거리 불변이라 밸런스 무영향).
- **HUD/진행 후속2**: 상단 이펙트 바(상태+버프 아이콘·클릭 시 목록+남은시간 팝업·잔여<3s 깜빡임, `PlayerStats.GetActiveBuffs()`) / 미니맵 `Scripts/UI/MinimapView.cs`(플레이어·적·포탈) / coast·mountain 보스 ChapterFlags 누적 마커(**Chapter enum·CurrentChapter 불변, 세이브 버전 미bump — 엔딩형 아님**). 권역 특화 드랍 테이블은 회귀 위험으로 **단독 검증 패스로 분리**(미진행, 문서화됨).
- **적대적 리뷰 hardening v3**: Gather 진행 즉시 SaveGame 격상 / 보스계약 폐기 `PruneUnobtainable()`를 복원 완료 후 호출(순서 비의존) / validate R10에 Mining↔MiningNode 교차검증 / HUD throttle 0.08 / LightningStorm 싱글턴 제약 주석. **신규 *전용 전략* 메커닉 스킬**: `ChainLightning`(연쇄 점프), `LifeDrain`(흡혈 폭딜).
- **7-에이전트 적대적 리뷰 hardening v4(2026-05-19)**: **Gather 계약 = A안(turn-in 시 `ConsumeItems` 소모 + 보유기반 진행)** — 무한 골드/재료 익스플로잇 봉쇄. enhance_stone(게이트 통화)은 반복 계약 금지(1회성 보스만), `balance.py` B5 에러로 영구 차단. `SaveManager.SaveGame()`도 트랜잭션 중 deferred. `BackfillRegionFlagsV13` 버전무관 멱등. RegionDropChance 등급 티어링(0.05/0.03/0.02). validate R13(스킬Type 유일성)·R14(RepeatableBossIds↔씬). HUD 팝업 dismiss/시그니처 재생성, 미니맵 허브 숨김. 상세 `PLAN_DOC/hub-preparation-loop-v1.md` v4.
- **권역 드랍 v1 / 스킬 확장 v2**: 60개 비보스 적에 권역 테마 재료 — `EnemyStats.RegionDrop`/`RegionDropChance`(기본 0.1)로 **드랍 테이블과 독립 굴림**(가중치 정규화에 안 끼므로 기존 드랍 확률 불변, 진짜 가산형). 신규 능동 스킬 8종 — 고유 `SkillType`(27~34) + 기존 전략 위임상속(`VenomShotStrategy : ArrowShotStrategy` 등) + Element/Status/파라미터 차별 + 스킬북 8 + 4거점 분산. 검증 4종 green.
- **허브 루프 완성도 패스 v1(2026-05-19, 15항목)**: ContractBoardUI 모바일 정보밀도↑(Gather 보유/필요·소모 명시, pending 메시지 정확화) / xUnit +8(`ContractsJsonTests` 매니페스트 무결성·A안 Gather 설명·반복보스 id, `MapCoverageTests` 맵 한글명 누락) → **28개** / validate **R15**(스킬북↔스킬 RequiredClass·AvailableToAll 정합), R10 권역 유효성+Gather 소모설명 / 스킬 밸런스 1차(`PLAN_DOC/skill-balance-pass-v1.md`, **ChainLightning 첫 타깃 300→240**만 동작변경) / 스킬북 11개 메타를 스킬값 정렬(상점은 스킬 쪽 읽음·동작불변) / 드랍 체감: RegionDropChance **권역 단조 차등**(town 0.06>outpost 0.04>coast 0.03>mtn/mine 0.02, 23파일) / `enemy-zone-plan.md` 권역 파밍목적+데이터정합(coast=sapphire_ore, sea_kelp는 일반드랍) / MinimapView 적 점 1.6→2.2px(허브 숨김 검증) / HUD 아이콘 경로 정적 캐시(0.08s ResourceLoader.Exists 반복 제거) / 4거점 NPcId 중복0·기능NPC 무대사 정상(ChapterDialogue 빈문자 graceful) / **지면 식생 장식: 전용 PNG 0개라 생성 금지 규칙대로 미생성 — `biome-ground-decoration-plan-v1.md`에 15종 에셋 프롬프트+결정적 배치 알고리즘 스펙, 씬 주입은 에셋 도착 후로 보류**. town ShopSign 노드 중복은 별도 정리작업 분리. 검증 4종+diff green, **미커밋**.

- **UX 버그픽스 패스(2026-05-19, 미커밋)**: (A) 소모품 상점 `scroll_town` 이름 충돌 → "마을 순간이동 주문서"로 분리(아이콘은 PNG 1종뿐이라 TODO). (B) **포탈 왕복 버그**: town↔field_outpost 도착 좌표가 반대편 포탈 옆이라 "다시 타면 옛 오크길" → 양쪽 `TargetSpawnPosition` 교정. (C) 퀘스트보드 NPC 0.192→0.4, `quest_board_ui` 800×600→480×380. (D) `Scripts/Data/MapLevels.cs`(Godot-free) 신설 → 포탈 프롬프트 "(권장 Lv.N+)". (E) **town 균일 ×1.4 확대**(640×360→896×504): 전 노드 위치·데코콜라이더·벽 sub_resource·배경 스케일, 벽/엣지 오프셋은 새 치수 재계산, 마을행 인바운드 포탈 4개(field_1·field_4_harbor·field_outpost·town_outskirts) `TargetSpawnPosition` ×1.4 보정. **부수효과: town 폭 896>760이라 MinimapView 허브 숨김 임계 초과 → 이제 마을에서 미니맵 표시됨**(의도된 수용). 검증 4종+diff green.

## 문서·메모리 동기화 정책 (필수)
다음 변경이 발생할 때마다 **CLAUDE.md와 메모리 디렉토리(`~/.claude/projects/.../memory/`)를 함께 최신화**할 것:
- 큰 기능 변경 (스킬/전투/세이브/상점 시스템 등)
- 월드 구조 변경 (권역/거점/포탈 네트워크 추가·재배치)
- 시스템 방향 전환 (장르·루프·BM 정책 등)
- 자산 파이프라인 변경 (PNG/타일셋/사운드 경로 규약 등)
- 리뷰 후 수정 방향 변경

업데이트할 항목:
1. CLAUDE.md — 게임 방향, 거부 시스템, 월드 구조, 작업 원칙, 핵심 루프
2. `~/.claude/projects/.../memory/MEMORY.md` 인덱스 — 새 메모리 항목 추가/이름 갱신
3. 관련 project_* / feedback_* 메모리 파일 — 현재 상태, 완료 시스템, 알려진 위험, 다음 작업 후보

문서를 최신화하지 않은 채 큰 변경을 끝내지 말 것. 다음 세션이 잘못된 가정으로 시작된다.
