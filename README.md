# first_game

Godot 4.6 + C# / .NET 8로 만드는 탑다운 액션 RPG 던전 크롤러 (모바일 출시 목표).

## 개발 환경

- **Engine**: Godot 4.6 (Mono / .NET 8)
- **Language**: C#
- **Target**: Mobile (Android 우선)

## 프로젝트 셋업 (clone 후 최초 1회)

새 PC에서 작업하거나 git clone 직후 다음 절차가 필요합니다.

### 1. .NET Android 워크로드 설치 (C# Android 빌드 필수)

```bash
dotnet workload install android
```

### 2. C# 프로젝트 빌드

```bash
dotnet build
```

### 3. Godot 에디터에서 Android 빌드 템플릿 설치

`/android/` 디렉토리는 Gradle 빌드 템플릿용 작업 폴더이며 git에 포함되지 않습니다 (용량 큼). 따라서 다음 절차로 로컬에 재생성해야 합니다:

1. Godot 에디터 열기
2. **프로젝트 → Android 빌드 템플릿 설치...** 클릭
3. 확인 → 자동으로 `android/build/` 디렉토리 생성됨

> `export_presets.cfg`에 `gradle_build/use_gradle_build=true`로 설정되어 있어, 이 단계 없이는 Android 익스포트가 실패합니다.

### 4. Android SDK 경로 설정

Godot 에디터: **편집기 → 편집기 설정 → 내보내기 → Android**

- **Android Sdk Path**: `~/Library/Android/sdk` (macOS 기본 경로)
- **Java SDK 경로**: 시스템에 설치된 JDK 경로 (예: `~/Library/Java/JavaVirtualMachines/temurin-23.0.2/Contents/Home`)

## Android 빌드

1. **프로젝트 → 내보내기** 열기
2. Android 프리셋 선택
3. **내보낼 경로** 설정 (예: `~/Desktop/first_game.apk`)
4. **프로젝트 내보내기** 클릭

빌드된 APK를 폰에 설치:

```bash
~/Library/Android/sdk/platform-tools/adb install -r ~/Desktop/first_game.apk
```

폰 준비:
- 개발자 옵션 활성화: 설정 → 휴대폰 정보 → 빌드 번호 7번 탭
- USB 디버깅 ON
- 삼성: 설정 → 보안 및 개인정보 보호 → 자동 차단 → "USB 케이블로 명령 차단" OFF

## 프로젝트 구조

```
Scenes/         씬 파일 (.tscn)
  Maps/         맵 (town, field_N, dungeon_N)
  UI/           UI 씬 (HUD, 모바일 컨트롤, 메인 메뉴 등)
  Characters/   캐릭터 씬
Scripts/
  Core/         싱글톤 (GameManager, AudioManager, SceneManager 등)
  Entities/     플레이어, 적
  UI/           UI 스크립트 (BaseUIWindow 상속)
  Data/         Inventory, Stats 등 데이터 클래스
Resources/
  Balance/      game_balance.json (밸런스 수치)
  Generated/    GPT 생성 이미지/아이콘
  Tilesets/     타일맵 에셋
```

## 작업 원칙

- 자동전투 없음 — 직접 조작 전투
- 밸런스 수치는 코드 하드코딩 금지 → `Resources/Balance/game_balance.json` 사용
- `SaveData.CurrentScene` 항상 저장 → `LoadGame()`은 저장된 씬으로 복귀
- Autoloads: `GameManager`, `AudioManager`, `SceneManager` (3개만)
- 새 UI 창 추가 시 `BaseUIWindow` 상속 (자동 일시정지/상호 배제 처리)
