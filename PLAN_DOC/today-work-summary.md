# 오늘 작업 요약 (제미나이 검증용)

> 상태 메모 (2026-04-15)
> 이 문서는 `2026-02-22` 시점의 기능 구현 요약이다. 현재 프로젝트 구조를 설명하는 최신 인덱스는 `PLAN_DOC/README.md`와 `PLAN_DOC/source-code-reference.md`를 우선 참고한다.

## 날짜: 2026-02-22

---

## 구현된 기능 목록

### 1. 레벨 / 경험치 시스템
- **파일**: `Scripts/Data/PlayerStats.cs`
- 레벨 1~50, 레벨업 시 HP+10, BaseDamage+2, MaxMp+5
- 경험치 공식: `ExpToNextLevel = (int)(100 * Math.Pow(level, 1.5))`
- 적 처치 → `PlayerController.GainExp()` → `PlayerStats.AddExp()` → 레벨업 시 EventManager.TriggerLevelUp 발생

### 2. MP 시스템
- **파일**: `Scripts/Data/CharacterStats.cs`, `Scripts/Entities/Player/PlayerController.cs`
- MaxMp, CurrentMp 필드 추가
- 플레이어: 2MP/초 자동 재생
- 스킬 사용 시 MP 소모

### 3. 스킬 시스템 (Q/W/E/R 슬롯)
- **파일**: `Scripts/Data/SkillData.cs` (신규), `Scripts/Entities/Player/PlayerController.cs`
- SkillType 4종: PowerStrike / HealSelf / Dash / FireBolt
- RequiredLevel로 습득 가능 레벨 제한
- 스킬북 구매 → 인벤토리 사용 → LearnSkill() → Q/W/E/R 슬롯에 배치

### 4. 스킬창 UI (Tab 키)
- **파일**: `Scripts/UI/SkillWindow.cs` (신규), `Scenes/UI/skill_window.tscn` (신규)
- Tab 키 토글, 스킬 슬롯(Q/W/E/R) 및 사용 힌트 표시

### 5. 스킬 상점
- **파일**: `Scripts/UI/SkillShopUI.cs` (신규), `Scripts/Entities/Shop/SkillShopNPC.cs` (신규)
- **씬**: `Scenes/UI/skill_shop_ui.tscn` (신규), `Scenes/Objects/skill_shop_npc.tscn` (신규)
- NPC 범위 접근 → E키 → 스킬북 목록 표시 → 구매 시 인벤토리에 추가
- 레벨 미달 / 이미 학습한 경우 구매 불가

### 6. 다양한 적 (8종)
- **파일**: `Scripts/Data/EnemyStats.cs`, `Scripts/Entities/Enemies/EnemySpawner.cs`
- **리소스**: `Resources/Enemies/` (8개 .tres 파일)
  - Orc 4종: orc_basic, orc_warrior, orc_rogue, orc_shaman
  - Skeleton 4종: skeleton_base, skeleton_warrior, skeleton_rogue, skeleton_mage
- EnemySpawner가 StatVariants 배열에서 랜덤 선택하여 스폰

### 7. 아이템 다양화
- **리소스**: `Resources/Items/`
- 무기 추가: steel_sword(+10), battle_axe(+18, Crush), iron_spear(+13, Pierce)
- 방어구 추가: leather_armor(HP+15/Speed+20), steel_armor(HP+40)
- 포션 추가: hi_potion(HP+80), mega_potion(HP+999)
- 스킬북 4종: skillbook_power_strike/heal/dash/fire_bolt

### 8. 무기 타입별 공격 애니메이션
- **파일**: `Scripts/Data/ItemData.cs`, `Scripts/Entities/Player/PlayerController.cs`
- WeaponAttackType: Slice(기본) / Pierce / Crush
- 공격 시 장착 무기 타입에 따라 다른 애니메이션 재생

### 9. HUD 강화
- **파일**: `Scripts/UI/HUD.cs`, `Scenes/UI/hud.tscn`
- MP바 (파란색) + "X/X" 숫자 표시
- 레벨 표시 (LevelLabel)
- 경험치바 (노란색)
- 레벨업 알림 메시지 (LevelUpLabel)

### 10. 장비 스탯 복원 버그 수정
- **파일**: `Scripts/Data/Inventory.cs`
- `RestoreEquipment()`: 로드 시 장착 아이템의 스탯 보너스가 적용되지 않던 버그 수정

### 11. 이동 키 변경 (WASD → 방향키)
- **파일**: `project.godot`
- move_left: Left(4194319), move_right: Right(4194321), move_up: Up(4194320), move_down: Down(4194322)
- 기존 W/A/S/D가 스킬 단축키와 겹치는 문제 해결

### 12. 세이브/로드 확장
- **파일**: `Scripts/Core/SaveManager.cs`, `Scripts/Data/SaveData.cs`
- PlayerMp, PlayerLevel, PlayerExp 저장
- 학습한 스킬 (ResourcePath 배열)로 저장/복원

---

## 검증 체크리스트 (제미나이에게)

1. **방향키 이동**: 방향키로 플레이어 이동 가능한지
2. **스킬창**: Tab 키 누르면 스킬창 열리는지
3. **스킬 사용**: Q/W/E/R 키로 스킬 슬롯 사용 가능한지
4. **MP 재생**: 시간이 지남에 따라 MP가 자동 회복되는지
5. **레벨업**: 적 처치 → 경험치 → 레벨업 → HUD에 표시되는지
6. **스킬 상점 NPC**: [SKILL] NPC 접근 → E키 → 스킬북 구매 가능한지
7. **적 다양성**: 적 스포너에서 8종 중 랜덤으로 스폰되는지
8. **공격 애니메이션**: 무기 타입(Slash/Pierce/Crush)에 따라 다른 애니메이션 재생되는지
9. **장비 스탯**: 로드 후 장비 스탯 보너스 적용되는지
10. **빌드**: 0 errors, 0 warnings 확인 ✅ (확인됨)
