# Project Instructions

## Environment
- OS: macOS / Shell: zsh
- Engine: Godot 4.6 (C# / .NET 8)
- Target: 모바일(Android/iOS) 출시. 개인 성취 목적, 수익 무관.

## 게임 방향
- 장르: **고전 탑다운 액션 RPG 던전 크롤러** (Zelda LttP / Cat Quest 1 / Crystalis 결)
- 컨셉: **단순·심플** 우선. 명확한 시작-중간-엔딩
- 자동전투 없음 — 직접 조작 전투만

## 거부된 시스템 (제안 금지)
- 리니지 라이크 / MMO식 자동전투
- 도감(Compendium) / 절차적 던전 룸 생성 / 펫 동료
- 무한 탑 / 던전 모디파이어 등 엔드게임 그라인드
- 요즘 모바일 RPG식 복잡한 컨트롤·UI

## 씬 구조
```
town (허브) ─ NPC 6: shop / blacksmith / skill_shop / material_shop / teleport / save_point
 ├─ field_outpost (입문 사냥터)
 ├─ field_1 ──┬─ mine_1, mine_2 (광산)
 │            └─ dungeon_1 (오크 워로드)
 ├─ field_2 ─── dungeon_2 (스켈레톤 킹)
 └─ field_3 ─── dungeon_3 (고대 리치 — 최종 보스)
```

## 메인 스토리 (5챕터)
Prologue → Ch1(오크 워로드) → Ch2(스켈레톤 킹) → Ch3(봉인) → Final(고대 리치) → Ending
- 챕터 플래그는 `SaveData.ChapterFlags`에 누적. `GameManager.CurrentChapter`로 조회
- 자동 트리거: 포탈 진입 / 보스 처치 (`GameManager.RecordBossDefeat`, `RecordSceneVisit`)
- 챕터 대사: `Scripts/Data/ChapterDialogue.cs` (NPC × Chapter → 한 줄). NPC 상호작용 시 자동 토스트

## 클래스 시스템
- `PlayerClass`: Warrior(STR) / Mage(INT) / Archer(DEX). **CON은 공통**
- 신규 게임 시 MainMenu에서 클래스 선택 → 시작 무기 + 시작 스킬 자동 지급
- 스킬은 `SkillData.RequiredClass` + `AvailableToAllClasses`로 분기

## 작업 원칙
- 밸런스 수치 코드 하드코딩 금지 → `Resources/Balance/game_balance.json`
- `SaveData.CurrentScene` 항상 저장 — LoadGame()은 저장된 씬으로 복귀
- Stats(Resource) 스폰 시 반드시 `Stats.Duplicate()` — 원본 보호
- EnemySpawner에서 `enemy.AddToGroup("Enemy")` 필수
- Autoloads 3개만: GameManager, AudioManager, SceneManager
- GameManager._ExitTree에서 `Instance = null` 필수
- tscn 직접 편집 시 UID 충돌 주의

## 핵심 게임루프
적 처치 → 금화/재료/장비 드랍 → 상점·대장간 강화 → 다음 챕터·던전 → 보스 → 엔딩
