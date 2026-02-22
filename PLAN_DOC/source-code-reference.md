# 소스코드 참조 가이드

## 기능별 핵심 코드 위치

---

### 1. 레벨/경험치 시스템

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 레벨 필드 정의 | `Scripts/Data/PlayerStats.cs` | `_level`, `AddExp()`, `ExpToNextLevel` |
| 경험치 획득 | `Scripts/Entities/Player/PlayerController.cs` | `GainExp(int amount)` 메서드 |
| 레벨업 이벤트 | `Scripts/Core/EventManager.cs` | `OnLevelUp`, `TriggerLevelUp(int)` |
| HUD 레벨 표시 | `Scripts/UI/HUD.cs` | `OnLevelUp()`, `UpdateExpBar()` |

**학습 포인트**: PlayerStats.AddExp() → 레벨업 조건 체크 → EventManager.TriggerLevelUp() → HUD.OnLevelUp() 콜백 체인

---

### 2. MP 시스템

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| MaxMp, CurrentMp 정의 | `Scripts/Data/CharacterStats.cs` | `MaxMp`, `CurrentMp` 프로퍼티 |
| MP 재생 로직 | `Scripts/Entities/Player/PlayerController.cs` | `_mpRegenTimer`, `_mpRegenInterval` |
| MP 소모 | `PlayerController.cs` | `ActivateSkill()` 내 `Stats.CurrentMp -= skill.MpCost` |
| HUD MP바 | `Scripts/UI/HUD.cs` | `UpdateMpDisplay()`, `OnMpChanged()` |

**학습 포인트**: CharacterStats.CurrentMp setter에서 OnMpChanged 이벤트 발생 → HUD 자동 업데이트

---

### 3. 스킬 시스템

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 스킬 데이터 정의 | `Scripts/Data/SkillData.cs` | `SkillType` enum, `MpCost`, `CooldownSeconds` |
| 스킬 학습 | `Scripts/Data/PlayerStats.cs` | `LearnSkill()`, `HasSkill()`, `LearnedSkills` |
| 스킬 슬롯 배치 | `Scripts/Entities/Player/PlayerController.cs` | `_skillSlots[4]`, `AssignSkillToSlot()` |
| 스킬 활성화 | `PlayerController.cs` | `UseSkillSlot(int idx)`, `ActivateSkill()` |
| 스킬 이펙트 (각 타입) | `PlayerController.cs` | `switch (skill.Type)` 분기 |

**학습 포인트**:
- Q=slot0, W=slot1, E=slot2, R=slot3
- PowerStrike: BaseDamage × BonusDamageMultiplier
- HealSelf: CurrentHp += HealAmount
- Dash: Velocity에 이동 속도 부스트
- FireBolt: 방향으로 원거리 공격

---

### 4. 스킬창 UI

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 스킬창 스크립트 | `Scripts/UI/SkillWindow.cs` | `Toggle()`, `RefreshSlots()` |
| 씬 구조 | `Scenes/UI/skill_window.tscn` | PanelContainer > SkillSlotContainer |
| Tab 키 감지 | `PlayerController.cs` | `Input.IsActionJustPressed("ui_focus_next")` |

---

### 5. 스킬 상점

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 상점 NPC 스크립트 | `Scripts/Entities/Shop/SkillShopNPC.cs` | `OnBodyEntered()`, E키 → `OpenShop()` |
| 상점 UI 스크립트 | `Scripts/UI/SkillShopUI.cs` | `OpenShop()`, `BuildSkillList()`, `OnBuyClicked()` |
| 스킬북 아이템 리소스 | `Resources/Items/skillbook_*.tres` | LearnedSkill 참조 |
| 스킬 데이터 리소스 | `Resources/Skills/*.tres` | RequiredLevel, MpCost, CooldownSeconds |

---

### 6. 다양한 적 (EnemyStats)

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 적 스탯 데이터 | `Scripts/Data/EnemyStats.cs` | AnimBasePath, ExperienceReward, new MoveSpeed |
| 스폰 다양화 | `Scripts/Entities/Enemies/EnemySpawner.cs` | `StatVariants[]`, 랜덤 선택 |
| 적 스탯 리소스 | `Resources/Enemies/*.tres` | 8종 각각 HP/SPD/ATK/EXP 설정 |
| EXP 지급 | `Scripts/Entities/Enemies/EnemyController.cs` | `Die()` → `player.GainExp()` |

**중요**: EnemySpawner에서 `Stats = (EnemyStats)variant.Duplicate()` — Duplicate() 없으면 공유 버그 발생

---

### 7. 무기 타입별 애니메이션

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| WeaponAttackType 정의 | `Scripts/Data/ItemData.cs` | `WeaponAttackType` enum (Slice/Pierce/Crush) |
| 애니메이션 분기 | `PlayerController.cs` | `GetAttackAnimPrefix()` 메서드 |

---

### 8. 아이템 / 인벤토리

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 아이템 타입 | `Scripts/Data/ItemData.cs` | `ItemType` enum (Consumable/Weapon/Armor/SkillBook) |
| 스킬북 사용 처리 | `Scripts/Data/Inventory.cs` | `UseItem()` → `ItemType.SkillBook` 분기 |
| 장비 복원 버그 수정 | `Inventory.cs` | `RestoreEquipment(ItemData weapon, ItemData armor, PlayerController player)` |

---

### 9. 이벤트 시스템

| 이벤트 | 파일 | 설명 |
|--------|------|------|
| OnHealthChanged | `CharacterStats.cs` | HP 변경 시 |
| OnMpChanged | `CharacterStats.cs` | MP 변경 시 |
| OnGoldChanged | `EventManager.cs` | 골드 변경 시 |
| OnPlayerDeath | `EventManager.cs` | 플레이어 사망 시 |
| OnGameSaved | `EventManager.cs` | 세이브 완료 시 |
| OnLevelUp | `EventManager.cs` | 레벨업 시 (int newLevel) |

---

### 10. 세이브/로드

| 항목 | 파일 | 주요 라인 |
|------|------|----------|
| 세이브 데이터 구조 | `Scripts/Data/SaveData.cs` | PlayerLevel, PlayerExp, PlayerMp, LearnedSkillPaths |
| 저장 로직 | `Scripts/Core/SaveManager.cs` | `SaveGame()` |
| 로드 로직 | `SaveManager.cs` | `LoadGame()`, `PendingLoadData` static |
| 씬 로드 후 적용 | `PlayerController.cs` | `_Ready()` → PendingLoadData 체크 |

---

### 11. 이동 키 설정

- **파일**: `project.godot` → `[input]` 섹션
- move_left: physical_keycode=4194319 (Left)
- move_right: physical_keycode=4194321 (Right)
- move_up: physical_keycode=4194320 (Up)
- move_down: physical_keycode=4194322 (Down)
- attack: physical_keycode=32 (Space)
- interact: physical_keycode=69 (E)
- inventory: physical_keycode=73 (I)
- 스킬 슬롯: Q(81)/W(87)/E(69)/R(82) — PlayerController.cs 내 Input.IsActionJustPressed 직접 처리

---

### 아키텍처 흐름 요약

```
플레이어 입력
    ↓
PlayerController._Process()
    ├── 이동: move_left/right/up/down
    ├── 공격: attack (Space)
    ├── 스킬: Q/W/E/R → UseSkillSlot(idx)
    └── 인벤토리/스킬창: I / Tab

스킬 사용
    ↓
ActivateSkill(SkillData)
    ├── MP 검사 → 소모
    ├── 쿨타임 설정
    └── 스킬 효과 적용

적 처치
    ↓
EnemyController.Die()
    └── PlayerController.GainExp()
            └── PlayerStats.AddExp()
                    └── 레벨업 → EventManager.TriggerLevelUp()
                                    └── HUD.OnLevelUp()

세이브
    ↓
SaveManager.SaveGame()
    └── JSON 파일 (user://saves/save.json)
```
