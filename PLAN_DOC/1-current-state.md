# 현재 상태 + 결정된 방향

## 문서 목적
- 다른 PC 또는 새 세션에서 작업을 이어갈 때, 현재 프로젝트 상태와 결정된 방향을 한 장으로 파악하게 한다.
- 아트/에셋 의사결정 맥락, 작업 우선순위, 다음 실행 포인트를 요약한다.

작성일: 2026-05-03 (갱신: 2026-05-18)

## 2026-05-18 현행 요약
- 월드: Scenes/Maps 29개 / 4권역(town·outpost·coast·mountain). 적 ~120, 아이템 ~244, 스킬 27, 퀘스트 41(side 30), 보스패턴 10.
- 구현 완료 시스템: QuestBoardUI·반복 사이드퀘스트, BiomeWeatherController(v3)·StatWeights·StatusResist, EliteAffix, BossController/BossPattern, 무게 시스템, MiningNode, 검증망(validate/balance/xUnit/CI).
- **허브 루프 v1(2026-05-18)**: 공유 창고(SaveData v12) / 확정 제작(recipes.json) / 미장착 장신구 affix 재련 — town 배치 완료. 상세 `hub-preparation-loop-v1.md`.
- **사냥 계약 v1(2026-05-18)**: 별도 `HuntingContractManager`(메인 QuestManager 미확장) + `contracts.json` 16개(권역별 4, Kill8/Gather4/Mining2/BossKill2). SaveData **v13** `ActiveContracts` 추가(backfill). 동시 3개, 완료 후 재수락, 일일/시간제한 없음. 보드 NPC를 허브 4곳 배치. 보상은 골드/EXP 항상+아이템은 PendingReward 폴백(손실 없음).
- **사냥터 보강 v1(2026-05-18)**: mine_3 광맥 5개(crystal/deep/prismatic/enhance_stone) 추가. FieldItem 희귀도 Loot glow(코드 Sprite2D, 새 PNG 없음). 18개 사냥터에 환경 장식 Sprite2D(Collision 없음, z=-2) 2~3개씩 배치 완료.
- **허브 확장(2026-05-18)**: 창고/제작/재련 NPC를 field_outpost·harbor_village·mountain_refuge에도 배치 — 4개 거점 전부에서 허브 준비 루프 완결.
- **UX/스케일(2026-05-18)**: HUD 상태이상 칩에 전용 아이콘(poison/freeze/burn/shock; Curse는 색상 폴백) 적용. HUD 좌측하단 현재 맵 이름 표시(`MapNames` 테이블, 29맵 한글). 플레이어/적/NPC **시각 크기 일괄 −20%**(`GameScale.CharacterVisual=0.8`; 콜리전/상호작용 반경/사거리 불변 → 밸런스·상호작용 무영향).
- **HUD/진행 후속2(2026-05-18)**: 상단 중앙 이펙트 바(상태이상+버프 아이콘) — 클릭 시 활성 효과 목록+남은시간 팝업, 잔여 <3s면 아이콘/행 깜빡임. 우측상단 미니맵(`MinimapView`, 맵 경계 안에 플레이어/적/포탈 점). coast/mountain 보스(kraken/glacier_titan/inferno_drake/crystal_lord) 처치 시 ChapterFlags 누적 마커 기록(Chapter enum/엔딩 불변 — 엔딩형 아님, 대사/계약/통계용).
- **권역 특화 드랍 v1(2026-05-18, 리뷰 후 독립 굴림으로 정정)**: 60개 비보스 적 `.tres`에 권역 테마 재료를 `EnemyStats.RegionDrop`/`RegionDropChance`(0.1)로 부여 — town=orc_leather, mine_1=iron_ore, outpost=bone_dust, mine_2=silver_ore, coast=sapphire_ore, snow=glacier_shard, volcano=drake_scale, mine_3=crystal_ore. `EnemyController`가 PossibleDrops 테이블과 **독립 굴림**(가중치 정규화 무관 → 기존 드랍 확률 불변). 씬 StatVariants 역추적 매핑, 보스 제외.
- **스킬 확장 v2(2026-05-18)**: 신규 능동 스킬 8종(독사격·독성마탄·재생의가호·분쇄강타·서리화살비·대지균열·신성작렬·연쇄뇌격) — 기존 검증된 SkillType 전략 재사용 + Element/Status/파라미터로 차별화(코드 무변경, 회귀 0). 스킬북 8 + 4개 거점 스킬상점에 레벨대별 분산 배치.
- 다음 우선순위: (1) Godot 실측: 이펙트 바/미니맵/축소/장식/신규 스킬 시연 (2) 계약 v2(권역별 다양화·티어) (3) 신규 SkillType 전략(코드) 추가 — 진짜 새 메커닉.

---

## 1. 기술 스택
- Engine: Godot 4.6 (C# / .NET 8)
- 장르: 탑다운 액션 RPG 던전 크롤러
- 플랫폼: 모바일 (Google Play / App Store) 출시 목표
- 목적: 개인 성취 (수익 무관)

---

## 2. 씬 구조
```
town (허브)
  └─ field_1 (오크류 필드)
       ├─ dungeon_1 (오크킹 보스)
       └─ field_2 (스켈레톤 필드)
            └─ dungeon_2 (스켈레톤 킹 보스)
```

### 맵 상태
| 맵 | 상태 | 비고 |
|----|------|------|
| `town.tscn` | `ColorRect` 플레이스홀더 | 기능 오브젝트만 배치 |
| `field_1.tscn` | **타일 기반 (MapGenerator)** | 최근 런타임 타일 생성 추가 |
| `field_2.tscn` | `ColorRect` | 스켈레톤 3종 + 던전2 포탈 |
| `dungeon_1.tscn` | 기본 구조 | 오크킹 보스 |
| `dungeon_2.tscn` | 기본 구조 | 스켈레톤 킹 보스 |

---

## 3. 구현 완료 시스템

| 시스템 | 상태 |
|--------|------|
| 인벤토리/장비 | ✅ 20슬롯, 장착/해제/파괴 |
| 강화 시스템 | ✅ +0~+10, 재료+골드, 실패 하락/파괴 |
| 레벨/EXP/스탯 | ✅ STR/CON/INT 포인트, 최대 Lv.50 |
| 스킬 시스템 | ✅ 4슬롯 Q/W/E/R, 쿨타임, MP |
| 세이브/로드 | ✅ CurrentScene 기반 |
| 씬 전환 | ✅ Portal → SceneManager |
| 드랍 시스템 | ✅ 가중치, 재료/포션, 보스 드랍 |
| 모바일 UI | ✅ 조이스틱, 공격/스킬/메뉴 버튼 |
| 플로팅 데미지 | ✅ 크리 구분, 플레이어/적 색상 분리 |
| 카메라 쉐이크 | ✅ |
| NPC | ✅ 상점/스킬샵/대장간/세이브 |
| 보스 시스템 | ✅ N킬마다 스폰, 영구 처치 |

---

## 4. 아트/에셋 방향 (현재 결정)

### 메인 에셋 운용
- **GPT 이미지 파이프라인 유지**: 아이콘, UI 버튼, 단일 오브젝트, 홍보/메뉴 이미지는 직접 생성해 보강한다.
- **Franuka 구매 보류**: 타일셋, 플레이어/몬스터 4방향 애니메이션, 던전/마을 월드 비주얼을 본격 교체할 때 재검토한다.
- **현재 자산 상태**: GPT 생성 아이콘은 `Resources/Generated/GPT/Icons/` 아래 카테고리별로 분리되어 있고, 아이템/스킬/UI 일부에 연결 완료.

### 판단 기준
- 아이콘/UI/단일 오브젝트 중심 작업 → GPT 파이프라인 우선
- 반복 타일셋/캐릭터 애니메이션/몬스터 애니메이션 작업 → Franuka 같은 구조화된 에셋 팩 재검토
- 모바일 출시 관점에서는 아트 교체보다 터치 UX, 화면 fit, 세이브/씬 전환 안정성이 우선

---

## 5. 작업 우선순위

현재 실행 순서:
1. **모바일 베이스라인** — `mobile-checklist.md` 기준으로 실기기/모바일 해상도 1회 검증
2. **상호작용 프롬프트 시스템** — `BaseInteractable` 기반 프롬프트 + Interaction 아이콘 8개 활성화
3. **UI 폴리싱** — CharacterWindow → ShopUI → SkillShopUI → EnhanceUI
4. **UI 공통화** — 슬롯/행 생성 중복 제거, 필요한 경우 ItemRowFactory 계열 추출
5. **코드 위생** — GetNode 패턴 통일, 들여쓰기 기준 유지, 핵심 단위 테스트 추가
6. **보류 항목** — Status 시스템, Franuka, 맵 비주얼 대개편

---

## 6. 현재 단계에서 하지 말 것
- 전체 맵을 동시에 꾸미기
- Unity 엔진 전환 검토
- 모바일 검증 없이 PC 에디터 기준으로 UI만 계속 확장
- 기존 파일 전체 자동 포맷팅으로 대량 diff 만들기
- Status 아이콘이 있다는 이유만으로 상태이상 시스템을 먼저 구현

---

## 7. 다음 실행 포인트

1. `.gitignore` / `.editorconfig` 기준 정리
2. `mobile-checklist.md`로 모바일 검증 항목 고정
3. 사용자가 Android 실기기 또는 모바일 해상도 실행 준비
4. 체크리스트 1회 수행 후 실제 문제를 우선순위에 반영
5. `BaseInteractable` 상호작용 프롬프트 시스템 구현

---

## 8. 참고 문서

### 활성 문서
- `2-source-code-reference.md` — 코드 파일 맵
- `mobile-checklist.md` — 모바일 실기기/해상도 QA + 핵심 루프 회귀 체크리스트
- `3-migration-plan.md` — Franuka 풀 교체 단계별 플랜
- `4-player-checklist.md` — 플레이어 1차 교체 체크리스트
- `5-town-plan.md` — town 레이아웃 + 타일 배치 순서
- `6-asset-inventory.md` — 번들 내 팩별 용도 매핑

### archive
- 과거 의사결정 이력 및 대안 경로 (참고용)

---

## 9. 한 줄 결론
현재 프로젝트는 핵심 기능 구현이 충분하므로, 다음 단계는 **모바일 기준 검증 루프를 먼저 만들고 상호작용 프롬프트로 월드 체감을 올린 뒤 UI/코드 정리를 이어가는 것**이다.
