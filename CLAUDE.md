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
- **상태저항 배선**: `CharacterStats.StatusResist`(0~0.85). `ItemData.BonusStatusResist`(장비, Inventory equip/clear/restore), `BuffStatusResist`(소모품 Buff, `ApplyBuffEx` 선택 인자+만료 차감). 저장 미수정 — 로드 후 장비 재적용으로 복구.
- **신규**: 날씨 테마 적 12종 + 소모품 6종. **전부 기존 PNG/아이콘 임시 재사용**(v3 문서에만 기록, generated-asset-inventory 미수정). 18 사냥터에 테마 적을 희귀 가중치로 추가, field_1/보스던전 구조 유지.
- 검증 4종(validate/balance/build/test)+ git diff --check 모두 green 유지가 완료 기준.

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
