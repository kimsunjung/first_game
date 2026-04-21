# Franuka 풀 교체 마이그레이션 계획서

## 문서 목적
- 현재 `first_game` 프로젝트를 `Franuka` 에셋 계열로 단계적으로 교체하는 계획을 정리한다.
- 무리하게 전체를 한 번에 바꾸지 않고, 실패 비용이 낮은 순서로 진행한다.

작성일: 2026-04-15

---

## 1. 현재 전제
- 현재 프로젝트는 `Godot 4.6 + C# + 2D 탑다운 + 모바일` 구조다.
- 씬 구조는 `Scenes/Maps/*`로 분리되어 있다.
- 현재 월드와 캐릭터 리소스는 `Pixel Crawler`, `Kenney` 계열이 일부 섞여 있다.
- 문서 기준으로 `Franuka` 계열로의 풀 교체를 검토 중이다.

---

## 2. 목표
- 플레이어, 적, NPC, 월드 타일, 던전, 실내, UI, 아이콘을 한 작가 계열로 맞춘다.
- 최종적으로 `같은 게임에서 나온 것처럼 보이는 일관성`을 얻는다.
- 교체 중에도 기존 게임이 완전히 깨지지 않도록 단계적으로 이동한다.

---

## 3. 대상 팩

### 교체 대상 팩
- 플레이어: `Fantasy RPG Heroes Pack`
- 적/몬스터: `Fantasy RPG Monster Pack`
- NPC: `RPG Townsfolk Pack`
- 필드/타운 타일셋: `RPG Asset Pack`
- 던전 타일셋: `Dungeon Asset Pack`
- 실내: `Fantasy RPG Interior Pack`
- UI: `RPG UI Pack`
- 아이콘: `RPG Icon Pack`

### 구매 판단
- 개별 구매보다 `Complete Fantasy RPG Bundle`이 유리하다.
- 단, 번들을 사더라도 실제 적용은 단계적으로 한다.

---

## 4. 기본 원칙

### 원칙 1. UI부터 시작
이유:
- 기존 게임 구조를 덜 건드린다.
- 체감 품질 향상이 빠르다.
- 실패해도 롤백 비용이 낮다.

### 원칙 2. 플레이어를 가장 먼저 교체
이유:
- 프레임 크기, 애니메이션 수, 카메라 줌, 충돌체 문제가 가장 먼저 드러난다.
- 이후 적/NPC/월드 교체의 기준점이 된다.

### 원칙 3. 한 번에 한 축만 바꾼다
권장 순서:
1. UI
2. 플레이어
3. 적
4. NPC
5. town
6. field_1
7. dungeon_1
8. 나머지 맵

### 원칙 4. 기존 씬 경로를 유지하는 쪽을 우선 검토
가능하면 아래를 우선한다.
- 새 에셋 파일을 추가하고
- 기존 씬/스크립트에서 참조만 바꾼다.

이유:
- 로직 코드 수정량이 줄어든다.
- 리소스 경로 추적이 쉽다.

---

## 5. 단계별 계획

### Phase 0. 구매 및 자산 정리
- `Franuka` 번들 다운로드
- 팩별 원본 폴더 구조 보존
- 프로젝트 내부엔 아래처럼 별도 루트로 가져온다.

예시:
- `Resources/Franuka/Heroes/`
- `Resources/Franuka/Monsters/`
- `Resources/Franuka/Townsfolk/`
- `Resources/Franuka/Tiles/`
- `Resources/Franuka/Dungeon/`
- `Resources/Franuka/Interior/`
- `Resources/Franuka/UI/`
- `Resources/Franuka/Icons/`

중요:
- 기존 `Pixel Crawler`, `Kenney` 리소스는 바로 삭제하지 않는다.
- 최소 한 사이클 테스트가 끝날 때까지 보존한다.

### Phase 1. UI 교체
대상:
- `Scenes/UI/hud.tscn`
- `Scenes/UI/inventory_ui.tscn`
- `Scenes/UI/shop_ui.tscn`
- `Scenes/UI/skill_shop_ui.tscn`
- `Scenes/UI/skill_window.tscn`
- `Scenes/UI/character_window.tscn`

핵심 작업:
- 프레임/패널 스타일 교체
- 버튼, 슬롯, 바, 탭 스타일 교체
- 아이콘 슬롯과 기본 버튼 크기 통일

완료 기준:
- HUD와 상점/인벤토리 UI가 `Franuka` 톤으로 보임
- 텍스트 가독성이 유지됨
- 모바일 조작 UI와 크게 충돌하지 않음

### Phase 2. 플레이어 교체
대상:
- `Scenes/Characters/player.tscn`
- `Scripts/Entities/Player/PlayerController*.cs`

핵심 작업:
- `AnimatedSprite2D`의 프레임 세트 재구성
- 방향별 idle/run/attack/hit/death 유무 확인
- 프레임 크기와 기준점 재조정
- 카메라 줌과 충돌체 확인

완료 기준:
- 이동, 공격, 피격, 죽음, 스킬 사용 중 시각 점프가 없음
- 기존 충돌과 카메라 구도가 유지됨

### Phase 3. 적 교체
대상:
- `Scenes/Characters/enemy.tscn`
- `Scripts/Entities/Enemies/EnemyController.cs`
- `Resources/Enemies/*.tres`

핵심 작업:
- 기본 적 1종 먼저 교체
- 스폰된 적이 정상적으로 idle/run/attack/death를 수행하는지 확인
- 체력바 위치와 공격 연출 위치 수정

완료 기준:
- `field_1`에서 적 1종이 시각적으로 안정적으로 동작함

### Phase 4. NPC 교체
대상:
- `Scenes/Objects/shop_npc.tscn`
- `Scenes/Objects/skill_shop_npc.tscn`
- `Scenes/Objects/blacksmith_npc.tscn`

핵심 작업:
- 역할별 NPC 실루엣 차이 확보
- 배경 소품과 함께 역할 구분 강화

### Phase 5. town 교체
대상:
- `Scenes/Maps/town.tscn`

핵심 작업:
- `RPG Asset Pack`과 `RPG Townsfolk Pack` 기준으로 허브 마을 구성
- `SavePoint`, 상점가, 출구, 중앙 광장 재디자인
- `6-town-layout-sketch.md`, `7-town-tile-placement-order.md` 기준으로 작업

완료 기준:
- 마을 허브가 실제 게임 공간처럼 보임
- UI와 월드 톤이 맞음

### Phase 6. field_1 교체
대상:
- `Scenes/Maps/field_1.tscn`

핵심 작업:
- 바깥 필드 타일과 장애물, 길, 출구 랜드마크 구성
- 적 스포너 영역 시각 정리

### Phase 7. dungeon와 interior 교체
대상:
- `Scenes/Maps/dungeon_1.tscn`
- `Scenes/Maps/dungeon_2.tscn`
- 이후 필요 시 실내 씬 추가

핵심 작업:
- 던전 팩, 실내 팩을 늦게 적용
- 허브와 필드 스타일이 안정된 뒤 확장

---

## 6. 실제 작업 순서
1. `RPG UI Pack` 적용
2. `player.tscn` 교체
3. `enemy.tscn` 교체
4. `RPG Townsfolk Pack`으로 NPC 교체
5. `town.tscn` 타일 기반 재구성
6. `field_1.tscn` 재구성
7. `Dungeon Asset Pack` 적용
8. `Fantasy RPG Interior Pack` 적용

---

## 7. 리스크

### 애니메이션 리스크
- 프레임 수가 현재 코드 가정과 다를 수 있음
- 방향 수가 다를 수 있음
- 공격 애니메이션 기준점이 달라 무기 연출이 어색해질 수 있음

### 스케일 리스크
- 현재 `zoom=(3,3)` 기준에서 캐릭터가 너무 크게 또는 작게 보일 수 있음
- HUD, 버튼, 아이콘 크기 재조정이 필요할 수 있음

### 맵 리스크
- 기존 플레이스홀더 맵보다 타일 기반 구조가 복잡해져 작업 시간이 늘 수 있음
- `town`, `field_1`을 동시에 바꾸면 품질과 일정이 흔들릴 가능성이 큼

---

## 8. 롤백 전략
- 각 Phase별로 별도 커밋
- 한 Phase가 끝날 때마다 플레이 테스트
- 다음 Phase로 넘어가기 전에 화면 캡처와 체크리스트 기록
- `Pixel Crawler`, `Kenney` 리소스는 최종 안정화 전까지 삭제 금지

---

## 9. 권장 첫 실행
가장 먼저 할 일:
1. `RPG UI Pack` 확인
2. `player.tscn`에 맞는 주인공 후보 선택
3. `4-player-first-replacement-checklist.md` 기준으로 1차 교체 테스트

---

## 10. 한 줄 결론
`Franuka`로의 풀 교체는 가능하지만, 성공 확률을 높이려면 `UI -> player -> enemy -> town -> field -> dungeon` 순으로 천천히 옮겨야 한다.
