# 인수인계 맥락 정리

## 문서 목적
- 다른 PC나 새 세션에서 작업을 이어갈 때, 최근 의사결정의 핵심 맥락을 빠르게 전달한다.
- `PLAN_DOC` 정리 이유, `Franuka` 선택 배경, 앞으로의 작업 우선순위를 한 장으로 요약한다.

작성일: 2026-04-15

---

## 1. 현재 프로젝트 상태
- 프로젝트는 `Godot 4.6 + C# + 2D 탑다운 + 모바일` 구조다.
- 현재 씬은 `main.tscn` 단일 구조가 아니라 `Scenes/Maps/town.tscn`, `field_1.tscn`, `field_2.tscn`, `dungeon_1.tscn`, `dungeon_2.tscn`로 분리되어 있다.
- 맵 씬은 기능적으로는 돌아가지만 비주얼은 아직 플레이스홀더 성격이 강하다.
- 특히 `town.tscn`, `field_1.tscn`이 먼저 손대야 할 핵심 맵이다.

---

## 2. 최근 문서 정리에서 한 일

### 추가한 문서
- `README.md`
- `8-art-direction-guide.md`
- `9-asset-purchase-guide.md`
- `10-asset-shortlist-2026-04-15.md`
- `5-map-art-priority-plan.md`
- `6-town-layout-sketch.md`
- `3-franuka-full-migration-plan.md`
- `4-player-first-replacement-checklist.md`
- `7-town-tile-placement-order.md`
- `1-handover-context.md`

### 갱신한 문서
- `2-source-code-reference.md`
- `combat-loop-plan.md`
- `item-inventory-plan.md`
- `save-load-system-plan.md`
- `shop-system.md`
- `sprite-scale-fix-plan.md`
- `today-work-summary.md`

### 삭제한 문서
- `map-redesign-plan.md`
- `tilemap-field-plan.md`
- `tileset-source-id-mapping.md`

삭제 이유:
- 모두 과거 `main.tscn` 또는 `MapGenerator` 전제를 두고 있어서 현재 구조와 맞지 않았다.

---

## 3. 아트/에셋 방향 결정

### 초기 판단
- 원래는 `Pixel Crawler` 중심 유지가 가장 안전하다고 판단했다.
- 이유는 이미 프로젝트에 반영된 리소스가 많고, 전환 비용이 크기 때문이다.

### 현재 선택
- 사용자 판단 기준으로 `Franuka` 계열을 메인 후보로 보기로 했다.
- 감성적으로 아주 완벽히 마음에 드는 건 아니지만, `팩 종류가 다양하고 전체 스타일을 맞추기 좋다`는 이유로 선택했다.

### 중요 포인트
- `Franuka`로 간다면 부분 혼합보다 `단계적 풀 교체`가 낫다.
- 단, 전체를 한 번에 바꾸면 실패 확률이 높으므로 아래 순서를 따른다.

권장 순서:
1. UI
2. player
3. enemy
4. NPC
5. town
6. field_1
7. dungeon

---

## 4. 왜 player부터 보나
- 캐릭터 프레임 크기
- 애니메이션 방향 수
- 공격 모션 기준점
- 충돌체와 발 위치
- 카메라 줌과 실제 체감 크기

이 다섯 가지 문제가 `player.tscn`에서 가장 먼저 드러난다.
그래서 `4-player-first-replacement-checklist.md`가 실제 전환의 첫 관문이다.

---

## 5. 현재 맵 작업 우선순위
- 맵은 `town -> field_1 -> UI 병행` 순으로 본다.
- `town`은 허브라서 체감 개선이 가장 크다.
- `field_1`은 첫 전투 필드라 구조적 완성도가 중요하다.

관련 문서:
- `5-map-art-priority-plan.md`
- `6-town-layout-sketch.md`
- `7-town-tile-placement-order.md`

---

## 6. 다른 세션에서 먼저 읽을 문서
새 세션이나 다른 PC에서 시작할 때는 아래 순서로 보면 된다.

1. `PLAN_DOC/1-handover-context.md`
2. `PLAN_DOC/README.md`
3. `PLAN_DOC/2-source-code-reference.md`
4. `PLAN_DOC/3-franuka-full-migration-plan.md`
5. `PLAN_DOC/4-player-first-replacement-checklist.md`
6. `PLAN_DOC/5-map-art-priority-plan.md`
7. `PLAN_DOC/6-town-layout-sketch.md`
8. `PLAN_DOC/7-town-tile-placement-order.md`

---

## 7. 다음으로 바로 이어질 작업 후보
- `Franuka RPG UI Pack`을 기준으로 `HUD`, `inventory_ui`, `shop_ui` 목업 교체
- `player.tscn` 1차 교체 테스트
- `town.tscn` 타일 기반 마을 작업 시작

---

## 8. 한 줄 결론
현재 `first_game`은 문서 기준으로는 정리된 상태고, 다음 실작업은 `Franuka` 에셋을 `UI -> player -> town` 순서로 천천히 옮기는 방향이 맞다.
