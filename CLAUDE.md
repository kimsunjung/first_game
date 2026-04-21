# Project Instructions

## Environment
- OS: Windows 11
- Shell: PowerShell (bash/unix 명령어 사용 금지)
- Engine: Godot 4.6 (C# / .NET 8)

## 게임 방향
- 장르: **탑다운 액션 RPG 던전 크롤러** (모바일 출시 목표)
- 수익 무관, 개인 성취(스토어 출시)가 목적
- 디자인보다 **기능 먼저** 구현, 디자인은 나중에

## 씬 구조 (목표)
```
마을(town) ← 허브, 상점/대장간/스킬샵/세이브
  └─ 필드(field_N) ← 탐험+전투+파밍+던전 입구
       └─ 던전(dungeon_N) ← 선형 진행+보스
```

## 핵심 게임루프
적 처치 → 금화/재료 드랍 → 상점/대장간 강화 → 더 깊은 곳 탐험

## 작업 원칙
- 자동전투 없음 — 직접 조작 전투만
- 밸런스 수치는 코드 하드코딩 금지 → `Resources/Balance/game_balance.json` 사용
- SaveData.CurrentScene 항상 저장 → LoadGame()은 저장된 씬으로 복귀
- Autoloads: GameManager, AudioManager, SceneManager (3개만)
- Stats는 Resource. 스폰 시 반드시 `Stats.Duplicate()` 호출
- EnemySpawner에서 `enemy.AddToGroup("Enemy")` 필수

## 주의사항
- PowerShell 문법 사용 (bash/unix 명령어 금지)
- tscn 직접 편집 시 UID 충돌 주의
- GameManager._ExitTree에서 `Instance = null` 필수
