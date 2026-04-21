# 소스코드 참조 가이드

## 문서 목적
- 현재 `first_game` 리포의 실제 구조를 빠르게 찾기 위한 참조 문서다.
- 예전 문서에 남아 있는 `main.tscn` 중심 설명 대신, 현재 맵 분리 구조를 기준으로 정리한다.

작성일: 2026-04-21

---

## 1. 현재 프로젝트 구조

### 진입점과 공통 설정
- `project.godot`
  - 메인 씬: `res://Scenes/UI/main_menu.tscn`
  - Autoload: `GameManager`, `AudioManager`, `SceneManager`
- `first_game.csproj`
  - Godot C# 프로젝트 설정

### 씬 구조
- `Scenes/Characters`
  - `player.tscn`
  - `enemy.tscn`
- `Scenes/Maps`
  - `town.tscn`
  - `field_1.tscn`
  - `field_2.tscn`
  - `dungeon_1.tscn`
  - `dungeon_2.tscn`
- `Scenes/Objects`
  - `portal.tscn`
  - `save_point.tscn`
  - `shop_npc.tscn`
  - `skill_shop_npc.tscn`
  - `blacksmith_npc.tscn`
  - `field_item.tscn`
- `Scenes/UI`
  - `hud.tscn`
  - `inventory_ui.tscn`
  - `character_window.tscn`
  - `shop_ui.tscn`
  - `skill_shop_ui.tscn`
  - `skill_window.tscn`
  - `settings_ui.tscn`
  - `mobile_controls.tscn`
  - `enhance_ui.tscn`
  - `boss_health_bar.tscn`

### 코드 구조
- `Scripts/Core`
  - 전역 매니저, 이벤트, 세이브, 밸런스, 카메라/연출 헬퍼
- `Scripts/Data`
  - 스탯, 인벤토리, 아이템, 스킬, 저장 데이터
- `Scripts/Entities`
  - 플레이어, 적, 상점 NPC, 세이브 포인트
- `Scripts/Objects`
  - 포털, 필드 아이템
- `Scripts/Maps`
  - `MapGenerator.cs` (필드 맵 런타임 타일 생성기)
- `Scripts/UI`
  - HUD, 인벤토리, 상점, 설정, 스킬창

---

## 2. 핵심 시스템별 진입 파일

### 플레이어
- 메인 클래스: `Scripts/Entities/Player/PlayerController.cs`
- 분리된 partial 클래스:
  - `PlayerController.Movement.cs`
  - `PlayerController.Combat.cs`
  - `PlayerController.Skills.cs`
  - `PlayerController.Animation.cs`
  - `PlayerController.Camera.cs`
  - `PlayerController.Save.cs`
- 플레이어 씬: `Scenes/Characters/player.tscn`

### 적과 전투
- 적 컨트롤러: `Scripts/Entities/Enemies/EnemyController.cs`
- 원거리 투사체: `Scripts/Entities/Enemies/EnemyProjectile.cs`
- 적 스포너: `Scripts/Entities/Enemies/EnemySpawner.cs`
- 적 씬: `Scenes/Characters/enemy.tscn`
- 적 데이터: `Scripts/Data/EnemyStats.cs`
- 적 리소스: `Resources/Enemies/*.tres`

### 게임 전역 상태
- 전역 상태: `Scripts/Core/GameManager.cs`
- 씬 전환: `Scripts/Core/SceneManager.cs`
- 오디오: `Scripts/Core/AudioManager.cs`
- 이벤트 버스: `Scripts/Core/EventManager.cs`
- 밸런스 설정: `Scripts/Core/BalanceData.cs`

### 세이브/로드
- 세이브 매니저: `Scripts/Core/SaveManager.cs`
- 저장 데이터: `Scripts/Data/SaveData.cs`
- 세이브 포인트: `Scripts/Entities/SavePoint/SavePoint.cs`
- 세이브 포인트 씬: `Scenes/Objects/save_point.tscn`

### 인벤토리와 아이템
- 인벤토리 로직: `Scripts/Data/Inventory.cs`
- 아이템 데이터: `Scripts/Data/ItemData.cs`
- 필드 드롭 오브젝트: `Scripts/Objects/FieldItem.cs`
- 아이템 리소스: `Resources/Items/*.tres`

### 스킬
- 스킬 데이터: `Scripts/Data/SkillData.cs`
- 전략 인터페이스: `Scripts/Data/Skills/ISkillStrategy.cs`
- 전략 구현: `Scripts/Data/Skills/SkillStrategies.cs`
- 스킬 리소스: `Resources/Skills/*.tres`
- 스킬 UI: `Scripts/UI/SkillWindow.cs`, `Scenes/UI/skill_window.tscn`
- 스킬 상점 UI: `Scripts/UI/SkillShopUI.cs`, `Scenes/UI/skill_shop_ui.tscn`

### 상점과 강화
- 일반 상점 NPC: `Scripts/Entities/Shop/ShopNPC.cs`
- 스킬 상점 NPC: `Scripts/Entities/Shop/SkillShopNPC.cs`
- 대장간 NPC: `Scripts/Entities/Shop/BlacksmithNPC.cs`
- 상점 UI: `Scripts/UI/ShopUI.cs`
- 강화 UI: `Scripts/UI/EnhanceUI.cs`

### UI
- HUD 컨트롤러: `Scripts/UI/HUD.cs`
- 인벤토리 UI: `Scripts/UI/InventoryUI.cs`
- 캐릭터 창: `Scripts/UI/CharacterWindow.cs`
- 설정창: `Scripts/UI/SettingsUI.cs`
- 모바일 조작: `Scripts/UI/MobileControls.cs`, `Scripts/UI/VirtualInput.cs`, `Scripts/UI/VirtualJoystick.cs`

---

## 3. 맵별 역할

### `Scenes/Maps/town.tscn`
- 허브 맵
- 상점 NPC, 스킬 상점 NPC, 대장간 NPC, 세이브 포인트 배치
- `field_1.tscn`으로 이동하는 포털 포함

### `Scenes/Maps/field_1.tscn`
- 첫 야외 필드
- 적 스포너 포함
- **MapGenerator** 로 런타임 타일 생성 (잔디 + 흙 패치 + 호수 + 나무 + 테두리 벽)
- `town.tscn`, `dungeon_1.tscn`, `field_2.tscn`으로 이어지는 포털 포함

### `Scenes/Maps/field_2.tscn`
- 후속 야외 필드

### `Scenes/Maps/dungeon_1.tscn`, `dungeon_2.tscn`
- 던전 계열 맵

---

## 4. 데이터와 리소스 위치

### 아이템
- `Resources/Items/health_potion.tres`
- `Resources/Items/iron_sword.tres`
- `Resources/Items/iron_armor.tres`
- `Resources/Items/skillbook_*.tres`
- 그 외 드롭/장비 리소스

### 스킬
- `Resources/Skills/power_strike.tres`
- `Resources/Skills/heal_self.tres`
- `Resources/Skills/dash.tres`
- `Resources/Skills/fire_bolt.tres`

### 적
- `Resources/Enemies/orc_basic.tres`
- `Resources/Enemies/orc_warrior.tres`
- `Resources/Enemies/orc_rogue.tres`
- `Resources/Enemies/skeleton_*.tres`

### 타일셋 및 비주얼 에셋
- `Resources/Tilesets/Pixel Crawler - Free Pack/...`
- `Resources/Kenney/...`

참고:
- `Resources/Tilesets/field_tileset.tres`는 `field_1.tscn`의 MapGenerator가 참조한다.
- `town.tscn`, `field_2.tscn`, `dungeon_1/2.tscn`은 여전히 `ColorRect` 기반 플레이스홀더 상태다.
- 맵 비주얼의 Franuka 전환은 `3-migration-plan.md`의 Phase 5~7 참고.

---

## 5. 우선적으로 보면 좋은 파일

### 전투 밸런스 파악
1. `Scripts/Data/PlayerStats.cs`
2. `Scripts/Data/EnemyStats.cs`
3. `Scripts/Entities/Player/PlayerController.Combat.cs`
4. `Scripts/Entities/Enemies/EnemyController.cs`
5. `Scripts/Core/BalanceData.cs`

### 저장 흐름 파악
1. `Scripts/Core/SaveManager.cs`
2. `Scripts/Data/SaveData.cs`
3. `Scripts/Entities/Player/PlayerController.Save.cs`
4. `Scripts/Entities/SavePoint/SavePoint.cs`

### 인벤토리/상점 파악
1. `Scripts/Data/Inventory.cs`
2. `Scripts/Data/ItemData.cs`
3. `Scripts/UI/InventoryUI.cs`
4. `Scripts/UI/ShopUI.cs`
5. `Scripts/Entities/Shop/*.cs`

### UI 전체 흐름 파악
1. `Scenes/UI/hud.tscn`
2. `Scripts/UI/HUD.cs`
3. `Scenes/UI/inventory_ui.tscn`
4. `Scenes/UI/shop_ui.tscn`
5. `Scenes/UI/skill_window.tscn`

---

## 6. 문서 해석 주의사항
- 예전 계획 문서 일부에는 `main.tscn` 중심 설명이 남아 있다 (archive 참조).
- 현재 실제 구조는 `Scenes/Maps/*` 분리 구조다.
- 맵 관련 의사결정은 `PLAN_DOC/5-town-plan.md`를 우선 본다.
- 아트 방향과 에셋 판단은 아래 문서를 우선 본다.
  - `PLAN_DOC/1-current-state.md` (확정된 방향)
  - `PLAN_DOC/3-migration-plan.md` (단계별 플랜)
  - `PLAN_DOC/6-asset-inventory.md` (Franuka 번들 매핑)
- 과거 대안 경로 (Pixel Crawler 유지, 무료 UI 먼저 등)는 `archive/` 참조.
