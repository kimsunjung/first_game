# first_game 에셋 후보 shortlist

## 문서 목적
- `first_game`에 지금 바로 적용 가능한 구매 후보를 좁힌다.
- `현재 스타일 유지`와 `스타일 전체 피벗`을 분리해서 판단한다.
- 무작정 많이 사는 대신, 가장 적은 비용으로 가장 큰 체감 효과를 내는 순서를 만든다.

작성일: 2026-04-15
가격/상태는 2026-04-15 기준으로 확인했고 이후 바뀔 수 있다.

---

## 1. 결론 먼저
### 추천 결론
현재 `first_game`에는 아래 순서가 가장 안전하다.
1. 무료 UI 리소스로 HUD/패널 가독성 먼저 보강
2. 유료 구매를 한다면 `UI 팩 1개`만 먼저 구매
3. 환경팩은 `스타일 유지`가 가능한 후보가 명확할 때만 구매
4. 스타일 전체를 바꿀 생각이 있을 때만 `에셋 계열 전체 교체`를 검토

### 이유
현재 프로젝트는 이미 `Pixel Crawler` 중심 리소스와 Godot 씬 구조가 꽤 진행돼 있다.
따라서 지금 가장 큰 체감 개선은 `환경 전체 교체`보다 `UI 정리`와 `맵 1개 완성`에서 나온다.

---

## 2. 구매 경로 요약
### 경로 A: 현재 프로젝트 유지
가장 추천.
- 기존 `Pixel Crawler` 월드 유지
- UI만 먼저 보강
- 부족한 아이콘, 패널, 바를 정리
- 맵 1개를 완성한 뒤 환경팩 필요 여부를 다시 판단

### 경로 B: 저비용 스타일 피벗
조건부 추천.
- 지금 월드/캐릭터 톤을 통째로 `Cute Fantasy` 계열로 갈아탈 생각이 있을 때만 고려
- 저렴하지만, 현재 리포의 기존 리소스를 많이 포기해야 할 수 있음

### 경로 C: 중상비용 일관형 패키지 피벗
비추천에 가깝지만 명확한 대안.
- 좀 더 통일된 상용 스타일을 한 번에 얻는 방식
- 현재 리포에 이미 있는 비주얼 구조를 많이 바꿔야 함

---

## 3. 경로 A: 현재 프로젝트 유지용 후보
### A-1. 무료 UI 스타터
#### 1) Kenney `Fantasy UI Borders`
- 가격: 무료 또는 자율 지불
- 성격: 9-slice 기반 판타지 UI 프레임
- 추천도: 높음
- 용도: `HUD`, `인벤토리`, `상점`, `설정창` 프레임 교체
- 장점: 무료, 가볍고, 최종용이 아니라도 빠르게 품질을 올리기 좋음
- 주의: 아이콘 양은 제한적이므로 별도 아이콘 소스가 필요할 수 있음
- 링크: https://kenney-assets.itch.io/fantasy-ui-borders

#### 2) Free Game Assets `Free Basic Pixel User Interface for Fantasy Game`
- 가격: 무료 또는 자율 지불
- 성격: 기본 메뉴, 버튼, 아이콘, HUD 요소 포함
- 추천도: 높음
- 용도: `HUD`, `퀵슬롯`, `상점`, `장비창`, `버튼` 프로토타입과 중간 단계 완성본
- 장점: 무료인데 구성 범위가 넓음
- 주의: 스타일이 다소 범용적이라 월드 아트와 완전 일치하지 않을 수 있음
- 링크: https://free-game-assets.itch.io/free-basic-pixel-art-ui-for-rpg

#### 3) Franuka `RPG UI pack (demo)`
- 가격: 무료 또는 자율 지불
- 성격: 유료 UI 팩의 무료 데모
- 추천도: 중간
- 용도: 스타일 확인용 샘플
- 장점: 이후 유료 버전으로 확장 판단이 쉬움
- 링크: https://franuka.itch.io/rpg-ui-pack-demo

### A-2. 유료 UI 후보
#### 1) Franuka `RPG UI pack`
- 가격: `US$3.99` 이상
- 추천도: 가장 높음
- 용도: `HUD`, `상점`, `스킬창`, `대화창`, `입력 버튼`, `아이콘 슬롯`
- 장점: 16x16/32x32/48x48 버전 포함, 픽셀 판타지 RPG 문법이 분명함
- 판단: 현재 프로젝트에 `가장 먼저 사도 되는 유료 후보`
- 링크: https://franuka.itch.io/rpg-ui-pack

#### 2) itchabop `Stonebase UI pack`
- 가격: `US$10.00` 이상
- 추천도: 중간
- 용도: 돌/나무 느낌의 모듈형 패널 제작
- 장점: 유연성이 좋고 9-slice UI 작업에 مناسب
- 주의: 화면 톤이 다소 무거워서 `밝은 판타지` 기준에는 조정이 필요함
- 링크: https://itchabop.itch.io/stonebase-ui-pack

### A-3. 현재 프로젝트에서 환경팩을 바로 안 사는 이유
아직 `town`과 `field_1`이 완성되지 않은 상태라, 지금 환경팩을 사면 높은 확률로 아래 문제가 생긴다.
- 어떤 타입이 부족한지 모른 채 구매함
- 현재 스타일과 안 맞을 수 있음
- 배경은 늘어났는데 UI와 캐릭터가 더 어색해짐

즉, 현재 유지 경로에서는 `환경팩보다 UI 팩 우선`이 맞다.

---

## 4. 경로 B: 저비용 스타일 피벗 후보
이 경로는 `현재 Pixel Crawler 기반을 유지하지 않겠다`는 의사결정이 있을 때만 의미가 있다.

### Kenmi `Cute Fantasy` 계열
#### 1) `Cute Fantasy RPG - 16x16 top down pixel art asset pack`
- 가격: 무료 기본 버전, 프리미엄은 `US$2.99` 이상
- 장점: 월드, 캐릭터, 적, 건물, 소품까지 저비용으로 시작 가능
- 링크: https://kenmi-art.itch.io/cute-fantasy-rpg

#### 2) `Cute Fantasy Dungeon - 16x16 top down asset pack`
- 가격: `US$2.99` 이상
- 장점: 던전 계열 확장에 적합
- 링크: https://kenmi-art.itch.io/cute-fantasy-dungeon-16x16-top-down

#### 3) `Cute Fantasy UI asset pack`
- 가격: `US$2.99` 이상
- 장점: 같은 계열 UI로 톤 통일이 쉬움
- 링크: https://kenmi-art.itch.io/cute-fantasy-ui

### 이 경로의 장단점
- 장점: 저렴하고 한 계열로 맞추기 쉬움
- 단점: 현재 프로젝트의 월드/캐릭터 분위기를 꽤 많이 바꾸게 됨
- 총평: `새 프로젝트`에는 매우 좋지만, 현재 `first_game`에는 조건부

---

## 5. 경로 C: 중상비용 일관형 패키지 후보
현재 프로젝트에 가장 자연스럽게 맞는 것은 아니지만, `한 번에 상용 퀄리티 계열로 정렬`하고 싶을 때의 대안이다.

### Franuka 계열
#### 1) `RPG asset pack`
- 가격: `US$9.50` 이상
- 구성: grass tileset, roads, houses, NPC, monsters 등
- 링크: https://franuka.itch.io/rpg-asset-pack

#### 2) `Fantasy RPG Monster pack`
- 가격: `US$12.50` 이상
- 구성: 40+ animated top-down monsters, projectiles, portraits 등
- 링크: https://franuka.itch.io/fantasy-rpg-monster-pack

#### 3) `Complete Fantasy RPG bundle`
- 가격: `US$39.99`
- 구성: tiles, characters, icons, UI, monsters 등 다수 포함
- 링크: https://itch.io/s/161328/complete-fantasy-rpg-bundle

### 이 경로의 장단점
- 장점: 에셋 일관성이 높고 확장성이 좋음
- 단점: 현재 프로젝트에선 사실상 비주얼 재출발에 가깝다
- 총평: `새로 갈아엎을 각오`가 없으면 과투자일 수 있다

---

## 6. 지금 바로 추천하는 실제 구매 순서
### 0원 단계
1. `Fantasy UI Borders` 적용 테스트
2. `Free Basic Pixel User Interface for Fantasy Game`로 `HUD`와 `상점 UI`에 목업 적용
3. `town` 또는 `field_1` 한 맵 완성

### 첫 유료 구매 단계
1. `Franuka RPG UI pack` 1개 구매

### 두 번째 구매 판단 시점
아래 중 하나가 명확할 때만 다음 구매로 넘어간다.
- 맵이 단조로워 환경팩이 꼭 필요함
- UI는 정리됐고 적/NPC 다양성 부족이 명확함
- 스타일 전체 피벗을 하기로 결정함

---

## 7. 추천하지 않는 구매 패턴
- Unity Asset Store에서 엔진 의존성이 있는 템플릿을 사서 Godot로 억지 이식하는 것
- 무료/유료 캐릭터 팩을 여러 작가 스타일로 섞는 것
- 월드 기준을 정하기 전에 환경팩을 여러 개 사는 것
- `에셋을 사면 디자인 문제가 해결될 것`이라고 기대하는 것

---

## 8. 최종 추천
현재 `first_game`에 가장 맞는 한 줄 결론은 이렇다.

`지금은 환경 전체를 새로 사기보다, 무료 UI 리소스로 먼저 정리하고, 첫 유료 구매는 UI 팩 1개만 선택하는 것이 가장 안전하다.`

그 다음 단계에서만 `스타일 유지` 또는 `스타일 피벗`을 다시 판단한다.

---

## 9. 참고 링크
- Kenney Fantasy UI Borders: https://kenney-assets.itch.io/fantasy-ui-borders
- Free Basic Pixel User Interface for Fantasy Game: https://free-game-assets.itch.io/free-basic-pixel-art-ui-for-rpg
- Franuka RPG UI pack (demo): https://franuka.itch.io/rpg-ui-pack-demo
- Franuka RPG UI pack: https://franuka.itch.io/rpg-ui-pack
- Stonebase UI pack: https://itchabop.itch.io/stonebase-ui-pack
- Kenmi Cute Fantasy RPG: https://kenmi-art.itch.io/cute-fantasy-rpg
- Kenmi Cute Fantasy Dungeon: https://kenmi-art.itch.io/cute-fantasy-dungeon-16x16-top-down
- Kenmi Cute Fantasy UI: https://kenmi-art.itch.io/cute-fantasy-ui
- Franuka RPG asset pack: https://franuka.itch.io/rpg-asset-pack
- Franuka Fantasy RPG Monster pack: https://franuka.itch.io/fantasy-rpg-monster-pack
- Franuka Complete Fantasy RPG bundle: https://itch.io/s/161328/complete-fantasy-rpg-bundle
