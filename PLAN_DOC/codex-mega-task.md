# Codex 메가 작업: 맵 4확장 + 보스 패턴 + 사이드 퀘스트 + 장비/소모품 폭증 + UI 개편 + 무게 시스템

> 단일 코덱스 세션으로 진행하는 5~8시간 분량 풀패키지.
> 작업 디렉터리: `/Users/ksj/personal/project/first_game`

## 컨텍스트 (필독)

참조 문서:
- `CLAUDE.md` — 프로젝트 방향성
- `AGENTS.md` — 코덱스 협업 규칙
- `game_story/main_story.md` — 메인 스토리 5챕터 (카엘)
- `PLAN_DOC/1-current-state.md` — 시스템 현황
- `PLAN_DOC/generated-asset-inventory.md` — 자산 인벤토리

엔진: Godot 4.6 / C# / .NET 8 / 모바일.
빌드: `dotnet build` (경고 0/오류 0).

## ⚠ 이미지 생성 금지 정책 (필수)

**이번 작업에서 코덱스는 이미지(PNG)를 신규 생성하지 않는다.** 자산은 사용자가 후속에 GPT로 직접 생성해 전달할 예정.

처리 방식:
- **적/보스 스프라이트**: 기존 적 PNG를 임시 재활용. 예: `field5_yeti`의 `Sprite` 필드에 일단 기존 `Resources/Generated/GPT/Enemies/Field2/zombie_walker.png` 등 적절히 유사한 이미지 path 지정.
- **아이템 아이콘**: 기존 아이콘 재활용. 예: `cutlass`의 `Icon` 필드에 기존 `Resources/Generated/GPT/Icons/Items/iron_sword.png`. 같은 카테고리(검/활/지팡이/갑옷/반지 등) 내에서 가장 가까운 기존 아이콘 매칭.
- **NPC 스프라이트**: 기존 `NPCs/Town/` 8개 중 캐릭터 컨셉에 가까운 것 재활용. 예: 음유시인 이반 → `general_merchant.png` 임시.
- **환경 오브젝트** (부서진 배, 등대, 산호 등): 기존 `Objects/Town/` 또는 `Objects/Village/`에서 가장 가까운 것 임시 사용. 없으면 단순 ColorRect 배경 + 생략 (시각이 빠지지만 동작은 함).
- **텔레그래프**: `Telegraph.cs`가 `_Draw`로 도형을 코드로 그림 — PNG 불필요.
- **던전 입구 sprite** (`dungeon_entrance.png` 같은 신규 자산): 기존 `Objects/Town/shop_sign.png` 등 임시 재활용 또는 ColorRect 단순 마커.

**산출 가이드 파일**:
`PLAN_DOC/asset-replacement-guide.md` 신규 생성 — 사용자가 GPT로 만들어야 할 자산 전체 목록 + 임시 사용 중인 path + 권장 사이즈/스타일 명시. 후속 교체 작업의 체크리스트.

빈 PNG 파일은 만들지 않음 (Godot가 import 에러 낼 수 있음).

## 절대 거부 (방향성 불일치)

- 리니지 라이크 / 자동전투 / 가챠 / 도감 / 절차적 던전 룸 / 펫 동료
- 무한 탑 / 그라인드 시즌 / 복잡 모바일 UI

방향: **Zelda LttP / Cat Quest 1 결의 정통 액션 RPG**.

## 던전 정책 (필수 준수)

- **던전은 이번 작업의 dungeon_4(수중 폐허) 외에 추가 안 함**. 앞으로도 던전 신규 추가는 금지. 필드만 확장.
- **던전 입구는 맵 끝이 아닌 맵 내부 중앙 가까이** 배치. 기존 dungeon_1/2/3 입구도 이번 작업에서 위치 재조정 (필드 중앙 부근으로 이동).
- **광산만 맵 끝**에 배치 가능 (실제 광산처럼 변두리). field_1의 mine_1 portal, mine_2/3는 그대로 끝쪽.
- **필드끼리는 띄엄띄엄 던전 분포** — 모든 필드에 던전 X.

---

## Phase A — 신규 지역: 항구·해안

**씬 2개**:
- `Scenes/Maps/field_4_harbor.tscn` (1280×720, 모래사장 + 부서진 배 + 동굴 입구)
- `Scenes/Maps/dungeon_4_sunken_shrine.tscn` (1280×720, 청록 톤, CanvasModulate(0.12, 0.14, 0.20))

`town.tscn` 남쪽에 portal → field_4. field_4 **중앙 부근**에 dungeon_4 입구 portal.

**적 10종**:
| 파일 | 위치 | ATK | HP | 특징 |
|---|---|---|---|---|
| field4_pirate_grunt | field_4 | 8 | 80 | 일반 |
| field4_pirate_brute | field_4 | 12 | 130 | 광폭 |
| field4_pirate_sniper | field_4 | 15 | 100 | Ranged |
| field4_giant_crab | field_4 | 10 | 180 | Defense 6 |
| field4_seagull_swarm | field_4 | 4 | 40 | 빠름 110 |
| dungeon4_drowned_sailor | dungeon_4 | 14 | 150 | 일반 |
| dungeon4_siren | dungeon_4 | 18 | 130 | Lightning Ranged |
| dungeon4_deep_lurker | dungeon_4 | 16 | 160 | Passive 매복 |
| dungeon4_coral_golem | dungeon_4 | 20 | 350 | Def 10 |
| boss_dungeon4_kraken | dungeon_4 | 35 | 1800 | Lightning 보스 |

**무기 6개**:
| 파일 | 클래스 | 등급 | 옵션 | 입수 |
|---|---|---|---|---|
| cutlass | Warrior | Uncommon | ATK +10 | 상점 |
| harpoon | Warrior | Rare | ATK +14, AtkSpd +0.08 | 상점/드랍 |
| fishermans_bow | Archer | Uncommon | ATK +9, Speed +5 | 상점 |
| coral_bow | Archer | Rare | ATK +16, Crit +0.06 | IsShopBlocked |
| tide_staff | Mage | Rare | ATK +13, MaxMP +25 | 상점/드랍 |
| kraken_trident | Warrior | Legendary | ATK +35, Lifesteal +0.02 | **크라켄 전용** |

---

## Phase I — 신규 지역: 설원 (Snowfield)

**씬**: `Scenes/Maps/field_5_snowfield.tscn` (1280×720, 흰색·청색 톤, 부드러운 눈보라 파티클).
field_3에서 portal로 연결 또는 신규 텔레포트 목적지.
**던전 없음** — 깊은 필드 단독.

**적 8종**:
| 파일 | ATK | HP | 특징 |
|---|---|---|---|
| field5_frost_wolf | 13 | 140 | 빠름 95 |
| field5_yeti | 22 | 320 | Defense 8, 광폭 |
| field5_ice_imp | 11 | 90 | Ranged Ice |
| field5_snow_witch | 17 | 160 | Magic Ice, 텔레포트 |
| field5_polar_bear | 25 | 400 | 광폭, knock 100 |
| field5_frost_archer | 15 | 130 | Ranged |
| field5_icicle_elemental | 18 | 200 | Ice 면역, Lightning 약점 |
| field5_named_glacier_titan | 30 | 800 | 명명 미니보스(Elite 패턴) |

**무기 4개 + 갑옷 6개**:
- **무기**: `frost_blade`(Warrior Rare, Ice 속성), `glacier_axe`(Warrior Epic), `winter_bow`(Archer Rare), `frostfang_staff`(Mage Rare).
- **갑옷**: `frost_armor`(Warrior Rare, Ice 저항), `frost_robe`(Mage Rare), `frost_vest`(Archer Rare), `frost_helm`(공통 Uncommon), `frost_cloak`(Mage Epic, MaxMP +30), `glacier_boots`(공통 Rare, Speed +20).

---

## Phase J — 신규 지역: 화산계곡 (Volcano Valley)

**씬**: `Scenes/Maps/field_6_volcano.tscn` (1280×720, 검붉은 톤, 용암 흐름, ember 파티클).
field_3 또는 field_5에서 연결. **던전 없음**.

**적 8종**:
| 파일 | ATK | HP | 특징 |
|---|---|---|---|
| field6_lava_slime | 9 | 80 | Fire 면역 |
| field6_fire_imp | 13 | 100 | Ranged Fire |
| field6_salamander | 16 | 180 | 빠름 100 |
| field6_magma_golem | 24 | 450 | Def 12, Fire 면역 |
| field6_phoenix_chick | 19 | 140 | 죽으면 1회 부활 |
| field6_ember_archer | 16 | 130 | Ranged Fire |
| field6_lava_serpent | 21 | 260 | 광폭 |
| field6_named_inferno_drake | 32 | 900 | 명명 미니보스 Fire 강력 |

**무기 4개 + 갑옷 6개**:
- **무기**: `flame_sword`(Warrior Rare Fire), `magma_hammer`(Warrior Epic), `phoenix_bow`(Archer Rare Fire), `inferno_staff`(Mage Epic Fire).
- **갑옷**: `flame_armor`(Warrior Rare, Fire 저항), `crimson_robe`(Mage Rare), `phoenix_vest`(Archer Rare), `flame_helm`(공통 Uncommon), `ember_cloak`(Mage Epic, BonusDamage +8), `magma_boots`(공통 Rare).

---

## Phase F — mine_3 (광산 확장)

**씬**: `Scenes/Maps/mine_3.tscn` (1280×720, 짙은 어둠 CanvasModulate(0.08, 0.10, 0.12)).
mine_2의 **맵 끝**에서 portal로 연결 (광산은 끝쪽 정책 OK).

**적 5종**:
- `mine_crystal_grunt` ATK 12, HP 140
- `mine_crystal_archer` Ranged Ice ATK 16
- `mine_crystal_warlock` Magic ATK 18
- `mine_crystal_brute` ATK 22, HP 280
- `mine_corrupted_miner` ATK 14, HP 180

**보스**: `boss_mine3_crystal_lord` "결정 군주". HP 1500. 패턴: Summon(crystal_grunt) + BeamSweep(Ice) + AoeBurst (3개).

**광물 4종**: `crystal_ore`, `deep_ore`, `prismatic_crystal`, `corrupted_stone`.

---

## Phase C — 보스 패턴 시스템

**신규 코드** (`Scripts/Entities/Enemies/`):
- `BossPattern.cs` — enum (ChargeAttack, AoeBurst, SummonMinions, BeamSweep, ProjectileVolley, Teleport)
- `BossPatternData.cs` — Resource (PatternType, TelegraphDuration, CastDuration, Cooldown, DamageMul, Radius)
- `IBossPattern.cs` + `BossPatternStrategies.cs` (6 strategies)
- `BossController.cs` — EnemyController 상속, 패턴 큐 + 페이즈 전환
- `Telegraph.cs` — Node2D + _Draw 빨강 도형(원/사각/선/콘) + 알파 페이드

**텔레그래프**: `Telegraph.cs`의 `_Draw`로 빨강 도형(원/콘/선/링)을 코드로 직접 그림. PNG 불필요.

**8보스 패턴 매핑** (기존 4 + 신규 4):
| 보스 | 패턴 |
|---|---|
| 오크 워로드 | Charge + AoeBurst |
| 오크 킹 | Charge + Summon + AoeBurst |
| 스켈레톤 킹 | ProjectileVolley + Summon + Teleport |
| 고대 리치 | BeamSweep + ProjectileVolley + Teleport + Summon |
| 크라켄 (Phase A) | BeamSweep(전기) + AoeBurst(파도) + Summon |
| 결정 군주 (Phase F) | Summon + BeamSweep(Ice) + AoeBurst |
| **빙하 거인** (Phase I 명명) | Charge + AoeBurst(Ice) |
| **인페르노 드레이크** (Phase J 명명) | BeamSweep(Fire) + ProjectileVolley(불꽃) + AoeBurst |

각 보스 .tres에 `[Export] BossPatternData[] Patterns`. `IsBoss=true` 또는 명명 미니보스도 BossController 활용.

**페이즈** (HP %별): >60% 기본 / 30~60% 빈도 1.3 텔레그래프 0.8 / <30% 빈도 1.6 텔레그래프 0.6 + modulate 빨강.

---

## Phase D — 사이드 퀘스트 + 시민 NPC 6명

**신규 NPC** (`town.tscn`):
1. 퀘스트 게시판 (`quest_board.tscn`)
2. 여관 주인 마르타 — 광장 동쪽
3. 노부 광부 한센 — 광산 근처
4. 꽃 가게 소녀 릴리 (Gather)
5. 순찰병 가렛 (Kill)
6. 음유시인 이반 (BGM 토글 + 회상)

기존 미사용 sprite 활용: `quest_elder.png`, `storage_keeper.png`.

**사이드 퀘스트 30개** (`Resources/Quests/side/`):
- 메인 5챕터 진행과 별개로 반복형 콘텐츠
- Kill 12 / Gather 10 / Deliver 4 / Explore 4
- 설원·화산·항구·mine_3 적/재료 활용한 퀘스트 포함
- `QuestData`에 `IsRepeatable` bool 추가

**QuestManager 확장**:
- `ActiveSideQuest` 슬롯 별도
- Kill/Gather 이벤트 시 메인+사이드 양쪽 진행도 체크
- 사이드 보드 UI (`quest_board_ui.tscn`)

---

## Phase G — 장비 카테고리 + 슬롯 + 아이템 폭증

### ItemType 확장
```csharp
public enum ItemType
{
    Consumable, Weapon, Armor, SkillBook, Material, Accessory,
    Helmet, Boots, Necklace, Ring, Bracelet,
    Cloak,    // 신규 — 망토
    Belt,     // 신규 — 벨트
    Gloves    // 신규 — 장갑
}
```

### SaveData v10 → v11
- `EquippedCloakPath` / `Affixes`
- `EquippedBeltPath` / `Affixes`
- `EquippedGlovesPath` / `Affixes`
- BackfillV11 — 신규 필드는 빈 문자열 폴백

### CharacterWindow 슬롯 레이아웃
사람 모양 슬롯 8개 → **11개**:
- 기존: Helmet, Necklace, Ring1, Weapon, Armor, Bracelet, Ring2, Boots
- 신규: **Cloak (등)**, **Belt (허리)**, **Gloves (양손)**

### 신규 아이템 총 ~80개 분포

기존 활/로브 8 + 갑옷 3 + 망토/벨트/장갑/투구 등 신규 풀세트. **테마별 세트 도입**.

**테마 세트** (전 클래스 적용, 각 테마 5~8 부위):
- **화염 세트** (Fire 저항, 화상 적용 부여):
  - `flame_armor`(전사), `crimson_robe`(마법사), `phoenix_vest`(궁수), `flame_helm`, `flame_cloak`, `flame_belt`, `flame_gloves`, `flame_boots`
- **냉기 세트** (Ice 저항, 빙결 적용):
  - `frost_armor`(전사), `frost_robe`(마법사), `frost_vest`(궁수), `frost_helm`, `frost_cloak`, `frost_belt`, `frost_gloves`, `glacier_boots`
- **폭풍 세트** (Lightning 저항):
  - `storm_armor`(전사), `storm_robe`(마법사), `storm_vest`(궁수), `storm_helm`, `storm_cloak`, `storm_belt`, `storm_gloves`, `storm_boots`
- **암흑 세트** (Dark 속성, 보스 드랍 전용):
  - `dark_armor`, `dark_robe`, `dark_vest`, `dark_helm`, `dark_cloak`, `dark_belt`, `dark_gloves`, `dark_boots`

**기존 카테고리 확장**:
- 망토 8개 (각 세트 + 일반 Common~Legendary 분포)
- 벨트 8개 (대부분 AvailableToAllClasses=true)
- 장갑 8개 (각 세트 + 클래스 분기)
- 마법사 갑옷 4개 추가 (mystic_robe + apprentice + archmage + void + 신규 3)
- 궁수 갑옷 4개 추가 (leather + ranger_vest + 신규)
- 신규 무기 (설원/화산 4 + 4 = 8개, 이미 Phase I/J에서)

### 장신구 신규 8개
- 화염의 반지(Fire 데미지 +N%), 냉기의 반지, 폭풍의 반지, 야경의 반지
- 흡혈 목걸이(BonusLifesteal +0.015), 마력 목걸이(MaxMP +30)
- 신속의 팔찌(BonusMoveSpeed +0.04, AttackSpeed +0.05), 수호의 팔찌(Defense +8, MaxHP +30)

### 가이드라인
- Common 무료/저렴, Uncommon 200~500G, Rare 700~1500G, Epic 2000~4500G, Legendary 5000G+
- Rare 이상은 적 드랍 우선 (IsShopBlocked=true 일부 적용)
- Legendary는 보스 또는 명명 미니보스 전용

---

## Phase K — 소모품 확장 + 순간이동 시스템

### 소모품 신규 (`Resources/Items/`)

**물약**:
- `attack_potion` — 60초간 BaseDamage +20% (Buff)
- `defense_potion` — 60초간 Defense +10 (Buff)
- `crit_potion` — 30초간 CritRate +20% (Buff)
- `mega_health_potion` — HP 200 즉시 회복
- `mega_mana_potion` — MP 100 즉시 회복
- `revive_scroll` — 죽었을 때 즉시 부활 (인벤에 있으면 자동 사용, HP 50% 회복)

**순간이동 시스템 (텔포 주문서)**:
- 신규 `ItemUseEffect.Teleport` enum 추가
- `ItemData.TeleportTargetScene` (string), `TeleportTargetPos` (Vector2) 필드
- 사용 시 `SceneManager.ChangeScene` 호출

**텔포 주문서 7종**:
- `scroll_town` — 마을 광장 즉시 이동
- `scroll_field_1` — field_1 입구
- `scroll_field_2` — 묘지 입구
- `scroll_field_3` — 폐허 입구
- `scroll_field_4` — 항구 입구
- `scroll_field_5` — 설원 입구
- `scroll_field_6` — 화산 입구
- 모두 1회 소비. 방문한 적 있는 씬만 가능 (VisitedScenes 검증). 안 방문 시 GD.Print + 소비 안 됨.

**일반 소모품**:
- `antidote_plus` — 중독 + 빙결 동시 해제
- `holy_water` — 언데드 적 한정 일시 데미지 +50% (15초)
- `bait` — 던지면 적 어그로 끌기 (광역 어그로 끌기, 게임 흐름 보조)

총 신규 소모품 ~13개.

---

## Phase L — UI 개편

### 1. HP/MP 바 정돈 (`Scenes/UI/hud.tscn` + `HUD.cs`)

**현재 문제**: HP가 MP보다 크고 사이즈 안 맞음.
**개선**:
- HP 바: width 120, height 14
- MP 바: width 120, height 14 (HP와 동일)
- 두 바 세로 배치 (HP 위, MP 아래), 같은 column
- 좌측에 HP/MP 라벨 (24×14 박스)
- 폰트 12, 색상 HP=빨강 그라데이션 / MP=파랑 그라데이션
- 모서리 라운드 3, 외곽선 진한 회색 (가독성)

### 2. 하단 퀵슬롯 (`hud.tscn` 갱신)

**현재**: 슬롯 4개, 40×40, separation 10, 화면 하단 -55 위치.
**개선**:
- 슬롯 사이즈 **40 → 32**
- separation **10 → 4**
- 슬롯 수 **4 → 6** (소비 아이템 추가)
- Inventory.QuickSlots ItemData[4] → [6]
- HUD/PlayerController에 6슬롯 처리. 키 입력 Key1~Key6 매핑.
- SaveData.QuickSlotPaths가 List<string>이라 자동 확장.

### 3. 스킬 슬롯 (`Scenes/UI/skill_window.tscn` 또는 HUD의 스킬바)

**현재**: Q/W/E/R 4슬롯 + 사이즈 큼.
**개선**:
- 스킬 슬롯 사이즈 줄이고 separation 좁힘
- 스킬 슬롯 **4 → 6** (액티브 스킬 보강 후)
- 모바일 컨트롤(`mobile_controls.tscn`)에도 동일 적용
- 키 입력: 기존 Q/W/E/R + T/Y (또는 별도 매핑)

### 4. 인벤토리 UI 깔끔화 (`inventory_ui.tscn` + `InventoryUI.cs`)

**현재 문제**: 슬롯 등록 부분이 비어 보임.
**개선**:
- 슬롯 그리드 5×4 → 6×5 (총 20슬롯 유지 그대로) 또는 가시 영역 보강:
  - 슬롯 패널에 진한 외곽선 + 라운드 4
  - 빈 슬롯에 흐릿한 카테고리 아이콘 또는 슬롯 번호
  - 그리드 padding 균일화
- 우측에 상세 정보 패널 (선택한 아이템의 옵션 풀 표시)
- 인벤토리 헤더에 무게 표시 `[ 무게: 23 / 50 ]`
- 아이템 정렬 버튼 (이름/희귀도/타입)

### 5. 장비 클릭 동작 변경 (`CharacterWindow.cs`)

**현재**: 장착 슬롯 클릭 시 즉시 해제.
**개선**:
- 1번째 클릭: 슬롯 강조 + 우측에 능력치 상세 표시 (Tooltip 패널)
- 2번째 클릭 (동일 슬롯): 해제 실행
- 다른 슬롯 클릭 시 강조 이동 (해제 안 됨)
- 강조 표시: 노란색 외곽선 + 살짝 확대 modulate

### 6. 인벤토리에 장착 아이템 유지 (`Inventory.cs` + UI)

**현재**: 장비 장착 시 인벤 슬롯에서 제거.
**개선**:
- 장착해도 인벤 슬롯에 그대로 유지 (Slot에 `IsEquipped` bool 또는 Inventory.EquippedSlotIndex 매핑)
- 장착 중인 아이템 슬롯에 "장착중" 라벨 또는 녹색 외곽선
- 인벤 슬롯에서 그 아이템 다시 클릭 시 → 해제(2클릭 패턴 따라 동일하게 또는 직접 해제)
- 두 아이템 교체 시: 이전 장착 자동 해제 → 새 아이템 장착 (인벤 슬롯에 둘 다 유지)
- **MaxSlots 의미**: 인벤 슬롯 수는 그대로. 단지 장착 아이템도 한 슬롯 차지.
- **세이브**: EquippedWeaponPath 등 기존 필드는 유지, 인벤 슬롯과 매칭됨을 보장 (중복 저장이지만 호환성)

### 7. 모바일 한 손 친화 (선택)

조이스틱·공격·스킬 버튼 위치는 그대로 유지. 슬롯/스킬 사이즈 축소만.

---

## Phase M — 무게 시스템

### 기본 정책
- 각 아이템에 `Weight` 필드 (float, 기본 1.0)
- 인벤토리 총 무게 = 슬롯 아이템 무게 × 수량 합
- `MaxCarryWeight` = 50 (기본) + Strength 스탯 영향 시 추후 확장
- 초과 시:
  - 0~80%: 정상
  - 80~100%: 이동속도 -10% (모든 클래스)
  - 100% 초과: 이동속도 -30% + 추가 아이템 획득 거부

### ItemData 확장
```csharp
[Export] public float Weight { get; set; } = 1.0f;
```

기본 가이드:
- 소비/포션: 0.1
- 재료/광물: 0.5
- 장신구: 0.3
- 무기: 2.0~5.0 (등급/타입별)
- 갑옷: 3.0~8.0
- 망토/벨트/장갑/투구/신발: 1.5~3.0

### PlayerStats / Inventory
- `Inventory.CurrentWeight` property (합산)
- `Inventory.GetMaxWeight()` → 50 + STR 보너스
- `PlayerStats.WeightPenaltyMultiplier` (1.0/0.9/0.7)
- PlayerController.GetInput()에서 MoveSpeed 적용 시 `Stats.MoveSpeed * WeightPenaltyMultiplier`

### UI
- 인벤토리 헤더 `[ 무게: {cur:0.0} / {max:0.0} ]`
- 80% 이상 시 빨강 표시 + 토스트 "과적!" (한 번만)

---

## Phase N — 던전 입구 위치 정책 적용 + 기존 던전 마이그

**현재 던전 portal 위치**:
- field_1 → dungeon_1: field_1.tscn 좌측 끝 (예 X=80)
- field_2 → dungeon_2: 끝
- field_3 → dungeon_3: 끝

**변경**:
- 각 필드 .tscn의 dungeon portal 위치를 **맵 중앙 부근으로 이동** (예: X=640 정도, 중앙에서 약간 안쪽)
- 시각적으로 던전 입구임을 알리는 오브젝트 추가 (`dungeon_entrance.png` — 어두운 문/계단 스프라이트). field 중앙 부근에 sprite 배치.
- 광산은 그대로 끝 유지 (mine_1, mine_2/3 portal 위치 변경 X)
- field_4/5/6도 동일 정책 — field_4의 dungeon_4 입구는 field_4 **중앙 부근**.

---

## Phase Z — 통합 검증 + 커밋

### 빌드
`dotnet build` 경고 0 / 오류 0

### 자체 점검 체크리스트

- [ ] 새 씬 5개(field_4, dungeon_4, field_5, field_6, mine_3) 모두 Player + HUD + UI 인스턴스
- [ ] town → field_4, field_3 → field_5/6 portal 연결, mine_2 끝 → mine_3 연결
- [ ] dungeon_1/2/3/4 입구가 필드 **중앙 부근**, 광산은 끝 유지
- [ ] 8보스(기존 4 + 신규 4 미니보스) 텔레그래프 + 패턴 실행 + 페이즈 전환
- [ ] 신규 적 ~40종 적별 동작 정상 (Ranged/Magic/Passive 분기)
- [ ] 퀘스트 보드 UI 정상, 사이드 30개 시작/완료
- [ ] 시민 NPC 6명 챕터별 대사 분기
- [ ] 장비 슬롯 3개(Cloak/Belt/Gloves) 장착·해제 정상, 클래스 가드 작동
- [ ] 신규 아이템 ~80개 인벤에서 정상 표시, 무게 합산 정확
- [ ] 무게 80% 이상 시 이동속도 페널티 시각 확인
- [ ] HP/MP 바 동일 크기, 깔끔 정렬
- [ ] 퀵슬롯 6개 키 입력 정상
- [ ] 스킬 슬롯 6개 사이즈 축소 + Q/W/E/R/T/Y 매핑
- [ ] 인벤토리 슬롯 패널 외곽선 + 빈 슬롯 일관성
- [ ] 장비 클릭: 1클릭 = 상세 정보, 2클릭 = 해제
- [ ] 장착 아이템 인벤 슬롯에 유지 + 장착중 라벨/외곽선
- [ ] 텔포 주문서 7종 — 방문한 씬만 가능, 미방문 시 거부
- [ ] revive_scroll 자동 발동 (사망 시 인벤에 있으면 소비 후 부활)
- [ ] SaveData v10 → v11 마이그 (Backfill: 신규 필드 빈값 폴백)
- [ ] 기존 v10 세이브 로드 시 정상 동작
- [ ] 모든 신규 .tres가 기존 PNG path를 임시로 참조 (Icon/Sprite null 없음 — 게임 시각 동작)
- [ ] `PLAN_DOC/asset-replacement-guide.md` 작성 완료 (모든 신규 자산 항목 + 임시 path + 권장 사이즈)

### 커밋 (Phase별 1커밋, 총 ~12커밋)

1. `Phase G — 장비 카테고리 3종(Cloak/Belt/Gloves) + ItemType v11 + SaveData 마이그`
2. `Phase A — 신규 지역 항구·해안 풀세트`
3. `Phase I — 신규 지역 설원 + 적 8 + 무기 4 + 갑옷 6`
4. `Phase J — 신규 지역 화산 + 적 8 + 무기 4 + 갑옷 6`
5. `Phase F — mine_3 + 결정 군주 + 광물 4`
6. `Phase C — 보스 패턴 시스템 + Telegraph + 8보스 매핑`
7. `Phase D — 사이드 퀘스트 보드 + 시민 NPC 6 + 사이드 퀘스트 30`
8. `Phase K — 소모품 확장 + 순간이동 주문서 7 + ItemUseEffect.Teleport`
9. `Phase L — HUD HP/MP 정돈 + 퀵슬롯 6 + 스킬 슬롯 6 + 인벤토리 UI 개편`
10. `Phase L — 장비 클릭 동작(1=정보/2=해제) + 인벤 장착 유지`
11. `Phase M — 무게 시스템 + 페널티`
12. `Phase N — 던전 입구 중앙 이동 + 입구 sprite 배치`
13. `asset-replacement-guide.md — 사용자 GPT 이미지 교체 체크리스트` (혹은 다른 커밋에 함께 포함)

### 푸시

`git push origin main`

---

## 작업 순서 권장

1. **`PLAN_DOC/asset-replacement-guide.md` 작성 시작** — 자산 교체 가이드 파일을 미리 생성하고 각 Phase 진행 중 새 .tres 등록할 때마다 가이드에 항목 추가. 임시 path도 기록.
2. **Phase G (장비 슬롯/SaveData v11)** — 다른 Phase의 신규 아이템이 활용 가능.
3. **Phase L (UI 개편 일부 — HP/MP/슬롯)** — 다른 Phase가 UI에 의존 안 함.
4. **Phase C (보스 패턴)** — 시스템 기반.
5. **Phase A (항구) / I (설원) / J (화산) / F (mine_3)** — 보스 패턴 활용.
6. **Phase K (소모품/텔포)** — VisitedScenes 활용.
7. **Phase D (사이드 퀘스트)** — 모든 신규 적/재료 활용.
8. **Phase M (무게 시스템)** — 모든 아이템 등록 후 일괄.
9. **Phase L (인벤/장비 클릭)** — UI 마무리.
10. **Phase N (던전 입구 마이그)** — 기존 던전 portal 위치 조정.
11. **Phase Z (검증/커밋/푸시)**.

> 모든 .tres 등록 시 Icon/Sprite path는 기존 자산에서 가장 가까운 것 임시 매칭. 후속 사용자가 GPT 이미지 받아 일괄 교체.

## 산출물 명세

- 신규 .cs: 12~15개 (BossPattern 6 + Telegraph + BossController + QuestBoardUI + WeightSystem + 기타)
- 신규 .tres: ~250개 (적 40 + 무기 14 + 갑옷·망토·벨트·장갑·투구·신발 60 + 소모품 13 + 텔포 주문서 7 + 사이드 퀘스트 30 + 보스패턴데이터 15 + 광물 4 + 장신구 8 + NPC관련 10 + 기타)
- 신규 .tscn: ~12개 (field_4, dungeon_4, field_5, field_6, mine_3, quest_board, quest_board_ui, 6 NPC, town.tscn 갱신)
- 신규 PNG: **0개** (모든 자산은 기존 .png 임시 재활용, 후속 사용자가 GPT로 교체)
- 신규 가이드: `PLAN_DOC/asset-replacement-guide.md` (자산 교체 체크리스트)
- 수정 기존 파일: town.tscn, field_1/2/3.tscn (던전 입구 이동), hud.tscn, inventory_ui.tscn, character_window.tscn, mobile_controls.tscn, skill_window.tscn, QuestManager.cs, EnemyController.cs, Inventory.cs, SaveData.cs(v11), CharacterStats.cs, PlayerStats.cs, PlayerController.cs, ItemData.cs, CLAUDE.md, AGENTS.md(필요 시)

## 안전 원칙

- destructive 명령 금지 (`git reset --hard`, `git push --force`, `rm -rf` 등)
- 단계별로 빌드 검증 — 실패하면 다음 Phase 진행 금지
- 막힐 경우 커밋·푸시 보류 후 사용자에게 보고
- v10 → v11 마이그 시 기존 세이브 데이터 안전성 최우선
- 자산 PNG의 .import 파일은 Godot이 자동 생성 — 강제로 미리 만들지 않음 (또는 정확한 형식 보장 시에만)

소요 예상: **5~8시간**.
