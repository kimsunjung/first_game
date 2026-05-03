# PLAN_DOC 인덱스

## 목적
- 프로젝트 방향, 작업 우선순위, 실행 플랜을 한 곳에 정리한다.
- 다른 PC 또는 새 세션에서 맥락을 빠르게 복원한다.

작성일: 2026-04-21

---

## 현재 결정 (요약)
- **비주얼 방향**: GPT 이미지 파이프라인으로 아이콘/UI/단일 오브젝트를 보강한다.
- **Franuka 구매 판단**: 타일셋 또는 4방향 캐릭터/몬스터 애니메이션을 본격 교체할 때 재검토한다.
- **실행 순서**: `모바일 베이스라인 검증 → 상호작용 프롬프트 → UI 폴리싱 → 코드 위생/테스트`
- **자세한 내용**: `1-current-state.md`, `mobile-checklist.md`

---

## 다른 PC에서 이어갈 때
새 세션에서는 이렇게 요청하면 된다.
- `PLAN_DOC/README.md 먼저 읽고, 그 다음 1번부터 6번까지 순서대로 확인한 뒤 이어서 작업해줘`

---

## 활성 문서 (7개)

### 방향과 현황
- [`1-current-state.md`](./1-current-state.md)
  - 현재 프로젝트 상태 + 결정된 방향 + 다음 실행 포인트
- [`2-source-code-reference.md`](./2-source-code-reference.md)
  - 씬/스크립트/리소스 파일 위치 맵

### 실행 플랜
- [`mobile-checklist.md`](./mobile-checklist.md)
  - 모바일 실기기/해상도 QA + 핵심 루프 회귀 체크리스트
- [`3-migration-plan.md`](./3-migration-plan.md)
  - Franuka 풀 교체 Phase 0~7 단계별 플랜
- [`4-player-checklist.md`](./4-player-checklist.md)
  - `player.tscn` 1차 교체 체크리스트 (Phase 2용)
- [`5-town-plan.md`](./5-town-plan.md)
  - `town.tscn` 레이아웃 + 타일 배치 순서 (Phase 5용)

### 레퍼런스
- [`6-asset-inventory.md`](./6-asset-inventory.md)
  - 번들 내 팩별 프로젝트 매핑 + 라이선스 + 기술 스펙

---

## 시스템 구현 메모 (유지)
현재 구현된 시스템의 계획서/작업 요약. 코드와 다를 수 있으므로 구조 판단은 `2-source-code-reference.md` 우선.

- `audio-system-plan.md`
- `bugfix-code-review.md`
- `character-animation-plan.md`
- `combat-loop-plan.md`
- `item-inventory-plan.md`
- `save-load-system-plan.md`
- `shop-system.md`
- `sprite-scale-fix-plan.md`
- `today-work-summary.md`

---

## 추가 분석/리뷰 문서 (유지)
- `DATA_SKILLS_ANALYSIS.md`, `claude_handoff_session_2.md`, `code_extensibility_analysis.md`, `code_quality_report.md`, `enhancement_treasure_plan.md`, `feature_recommendations.md`, `feature_roadmap_v2.md`, `feature_ux_content.md`, `final_review_report.md`, `improvement_plan.md`, `master_plan_completion_report.md`, `phase2_structural_refactor.md`, `phase3_remaining.md`, `project_status_report.md`, `qa_feature_impact.md`, `qa_report.md`, `review_architecture.md`, `review_entities.md`, `review_feature_impact.md`, `review_ui_scenes.md`, `sandbox-rpg-master-plan.md`, `session_cleanup_report.md`

분석/리뷰 히스토리. 참고용.

---

## archive (과거 방향/대안 경로)
더 이상 주요 방향이 아니지만 의사결정 이력이나 대안으로 보존.

- `archive/1-handover-context.md` — 과거 인수인계 맥락 (2026-04-15)
- `archive/5-map-art-priority-plan.md` — 맵 작업 우선순위 (3/5번과 내용 중복)
- `archive/8-art-direction-guide.md` — Pixel Crawler 중심 축 (방향 전환 이전)
- `archive/9-asset-purchase-guide.md` — 신중 구매 전략 (번들 결정 이전)
- `archive/10-asset-shortlist-2026-04-15.md` — 에셋 후보 리스트 (번들 결정 이전)

---

## 읽는 순서 추천
1. `1-current-state.md` — 5분 요약
2. `2-source-code-reference.md` — 코드 위치 빠르게 훑기
3. `mobile-checklist.md` — 모바일 기준 회귀 검증 항목
4. `3-migration-plan.md` — 앞으로 할 일 큰 그림
5. `6-asset-inventory.md` — 구매 후 어디에 쓸지
6. 실제 작업 진입 시점에 `4-player-checklist.md` / `5-town-plan.md`
