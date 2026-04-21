# Franuka 번들 에셋 매핑

## 문서 목적
- `Complete Fantasy RPG Bundle` 구매 후 각 팩을 프로젝트 어디에 쓸지 명확히 매핑한다.
- 라이선스 조건과 적용 순서를 한 곳에 정리한다.

작성일: 2026-04-21

---

## 1. 번들 정보

- 작가: Franuka
- 링크: https://itch.io/s/161328/complete-fantasy-rpg-bundle
- 가격: `US$39.99`
- 구매 시점: PC 변경 후 (2026-05월경)

### 번들 추정 구성 (문서 3/10 기준)
- `RPG UI Pack`
- `RPG Icon Pack`
- `Fantasy RPG Heroes Pack`
- `Fantasy RPG Monster Pack`
- `RPG Townsfolk Pack`
- `RPG Asset Pack` (필드/타운 타일셋)
- `Dungeon Asset Pack`
- `Fantasy RPG Interior Pack`

**※ 실제 구성은 구매 시점에 itch.io 번들 페이지에서 최종 확인.**

---

## 2. 팩별 프로젝트 매핑

### RPG UI Pack
- 적용 대상:
  - `Scenes/UI/hud.tscn`
  - `Scenes/UI/inventory_ui.tscn`
  - `Scenes/UI/shop_ui.tscn`
  - `Scenes/UI/skill_shop_ui.tscn`
  - `Scenes/UI/skill_window.tscn`
  - `Scenes/UI/character_window.tscn`
  - `Scenes/UI/enhance_ui.tscn`
  - `Scenes/UI/settings_ui.tscn`
  - `Scenes/UI/boss_health_bar.tscn`
- Phase: **1** (가장 먼저)
- 에셋 위치: `Resources/Franuka/UI/`

### RPG Icon Pack
- 적용 대상:
  - `Resources/Items/*.tres` 의 `Icon` 필드
  - `Resources/Skills/*.tres` 의 스킬 아이콘
  - HUD 퀵슬롯 아이콘
- Phase: **1** (UI와 함께)
- 에셋 위치: `Resources/Franuka/Icons/`

### Fantasy RPG Heroes Pack
- 적용 대상:
  - `Scenes/Characters/player.tscn` 의 `AnimatedSprite2D`
  - `PlayerController.Animation.cs` 의 프레임 세트
- 필수 애니메이션 확인: idle / run / attack / hit / death (4방향)
- Phase: **2**
- 에셋 위치: `Resources/Franuka/Heroes/`
- 체크리스트: `4-player-checklist.md`

### Fantasy RPG Monster Pack
- 적용 대상:
  - `Scenes/Characters/enemy.tscn`
  - `Resources/Enemies/*.tres` (orc_basic, orc_warrior, orc_rogue, skeleton_*, 보스)
- Phase: **3**
- 에셋 위치: `Resources/Franuka/Monsters/`

### RPG Townsfolk Pack
- 적용 대상:
  - `Scenes/Objects/shop_npc.tscn`
  - `Scenes/Objects/skill_shop_npc.tscn`
  - `Scenes/Objects/blacksmith_npc.tscn`
- 역할별 실루엣 차이 확보 (상인/마법사/대장장이)
- Phase: **4**
- 에셋 위치: `Resources/Franuka/Townsfolk/`

### RPG Asset Pack (타일셋)
- 적용 대상:
  - `Scenes/Maps/town.tscn` (기존 ColorRect → 타일 기반)
  - `Scenes/Maps/field_1.tscn` (기존 MapGenerator 타일셋 교체)
  - `Scenes/Maps/field_2.tscn`
  - `Resources/Tilesets/` 아래 신규 타일셋 리소스 생성
- Phase: **5** (town), **6** (field_1, field_2)
- 에셋 위치: `Resources/Franuka/Tiles/`
- 참고 문서: `5-town-plan.md`

### Dungeon Asset Pack
- 적용 대상:
  - `Scenes/Maps/dungeon_1.tscn`
  - `Scenes/Maps/dungeon_2.tscn`
- Phase: **7**
- 에셋 위치: `Resources/Franuka/Dungeon/`

### Fantasy RPG Interior Pack
- 적용 대상:
  - 향후 실내 씬 (가게 안, 여관, 대장간 내부 등 신규 콘텐츠)
  - 현재 기획에는 없지만 번들에 포함되면 보관
- Phase: **후순위 / 선택**
- 에셋 위치: `Resources/Franuka/Interior/`

---

## 3. 라이선스 조건

### 허용
- ✅ 상업적 사용
- ✅ 비상업적 사용
- ✅ 구글 플레이 / 앱 스토어 출시
- ✅ 에셋 편집 (색 변경, 리사이즈, 수정)

### 금지
- ❌ 에셋 자체 재판매
- ❌ 에셋 재배포 (다른 에셋 팩으로 재구성해서 배포)

### 권장 (필수 아님)
- 게임 크레딧에 itch 페이지 링크 표시
- 가능하면 X/Instagram 크레딧

### 출처
- https://franuka.itch.io/rpg-asset-pack (라이선스 명시)

---

## 4. 기술 스펙

### 해상도
- 기본 픽셀 크기: **16x16**
- 실제 배치 그리드: **48x48** (공격 모션, 무기, 액세서리 수용)
- 제공 크기: **100%, 200%, 300%** (다운로드 시 선택 가능)

### 형식
- `.png` 개별 이미지
- 스프라이트 시트 (.png)

### Godot 4.6 적용
- `AtlasTexture` 또는 `SpriteFrames` 리소스로 임포트
- 현재 카메라 `zoom = (3, 3)` 유지 가능
- 또는 300% 에셋을 zoom=(1,1)로 사용 가능 (모바일 해상도 대응)

---

## 5. 폴더 구조 (구매 후)

```
Resources/
├── Franuka/
│   ├── UI/
│   ├── Icons/
│   ├── Heroes/
│   ├── Monsters/
│   ├── Townsfolk/
│   ├── Tiles/
│   ├── Dungeon/
│   └── Interior/
├── Pixel Crawler - Free Pack/   ← 안정화 전까지 삭제 금지
└── Kenney/                       ← 안정화 전까지 삭제 금지
```

**기존 에셋 보존 원칙:**
- Phase 7까지 완료 + 플레이 테스트 통과 시점까지 삭제 금지
- Phase별 커밋을 유지해 롤백 가능하게

---

## 6. 임포트 체크리스트

### 구매 직후 (Phase 0)
- [ ] 번들 다운로드 (itch.io)
- [ ] 압축 해제
- [ ] 원본 폴더 구조 보존
- [ ] `Resources/Franuka/` 루트에 팩별 하위 폴더로 배치
- [ ] Godot 에디터에서 임포트 오류 확인
- [ ] 각 팩 README/라이선스 파일 보존

### 최초 1시간 체크
- [ ] UI 팩에 9-slice 버튼/패널 있는지
- [ ] Heroes 팩에 4방향 idle/run/attack/hit/death 있는지
- [ ] Monster 팩에 오크/스켈레톤류 또는 대체 가능 몬스터 있는지
- [ ] 아이콘 수량 확인 (인벤토리/스킬 충분한지)

---

## 7. 한 줄 결론
`Franuka Complete Fantasy RPG Bundle` 은 프로젝트의 UI/Player/Monster/NPC/World/Dungeon/Interior/Icon 을 단일 작가 일관성으로 커버하며, 상업 출시에 문제가 없고 16x16 기반으로 현재 구조와 호환된다.
