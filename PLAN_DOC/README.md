# PLAN_DOC 인덱스

## 목적
- `PLAN_DOC` 안의 문서를 현재 프로젝트 구조 기준으로 정리한 인덱스다.
- 어떤 문서가 아직 유효한지, 어떤 문서는 과거 구현 메모인지 빠르게 판단할 수 있게 한다.

작성일: 2026-04-15

---

## 현재 구조 기준 핵심 문서
- `2-source-code-reference.md`
  - 현재 코드/씬/리소스 구조를 보는 기본 문서
- `1-handover-context.md`
  - 최근 문서 정리 이유와 `Franuka` 선택 맥락을 담은 인수인계 문서
- `8-art-direction-guide.md`
  - 현재 프로젝트의 비주얼 방향 기준
- `9-asset-purchase-guide.md`
  - 에셋 구매 원칙과 우선순위
- `10-asset-shortlist-2026-04-15.md`
  - 실제 구매 후보 링크 정리
- `3-franuka-full-migration-plan.md`
  - `Franuka` 풀 교체를 전제로 한 단계별 마이그레이션 계획
- `4-player-first-replacement-checklist.md`
  - `player.tscn`부터 1차 교체할 때 확인할 체크리스트
- `5-map-art-priority-plan.md`
  - `town -> field_1 -> UI` 순서의 실제 작업 우선순위
- `6-town-layout-sketch.md`
  - `town.tscn` 기준 레이아웃 스케치
- `7-town-tile-placement-order.md`
  - `town.tscn`을 실제 타일 기반 마을로 바꿀 때의 작업 순서 문서

---

## 유지 문서

### 시스템 구현/구현 메모
- `audio-system-plan.md`
- `bugfix-code-review.md`
- `character-animation-plan.md`
- `combat-loop-plan.md`
- `item-inventory-plan.md`
- `save-load-system-plan.md`
- `shop-system.md`
- `sprite-scale-fix-plan.md`
- `today-work-summary.md`

이 문서들은 구현 당시의 계획서 또는 작업 요약이다.
현재 구조와 다른 부분이 있을 수 있으므로, 파일 경로와 씬 구조는 `2-source-code-reference.md`를 우선 기준으로 삼는다.

---

## 이번 정리에서 삭제된 문서
- `map-redesign-plan.md`
  - `Scripts/Maps/MapGenerator.cs`와 `main.tscn` 기반 설계여서 현재 구조와 맞지 않음
- `tilemap-field-plan.md`
  - 단일 `main.tscn` + `field.tscn` 전제를 두고 있어 현재 구조와 맞지 않음
- `tileset-source-id-mapping.md`
  - 현재 맵 씬이 직접 타일셋 매핑을 사용하지 않아 독립 문서 가치가 낮음

---

## 읽는 순서 추천
1. `1-handover-context.md`
2. `2-source-code-reference.md`
3. `3-franuka-full-migration-plan.md`
4. `4-player-first-replacement-checklist.md`
5. `5-map-art-priority-plan.md`
6. `6-town-layout-sketch.md`
7. `7-town-tile-placement-order.md`
8. `8-art-direction-guide.md`
9. `9-asset-purchase-guide.md`
10. 필요 시 각 시스템 구현 문서
