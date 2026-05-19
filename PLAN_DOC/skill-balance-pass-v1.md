# 스킬 밸런스 1차 점검 (v1, 2026-05-19)

목적: 신규/기존 능동 스킬의 핵심 수치를 한 표로 모으고, **화면 밖 적을
자동으로 때릴 위험**(모바일에서 보이지 않는 적에게 발동/어그로)을 점검해
보수적으로만 조정한다. 대규모 리밸런스가 아니라 회귀 방지 + 일관성.

## 자동타깃/광역 스킬 사거리 표

수치 출처: `Scripts/Data/Skills/SkillStrategies.cs` (전략 코드) + `Resources/Skills/*.tres`.
"획득 방식" = 적을 어떻게 고르는가. 모바일 카메라는 플레이어 중심, 가시
반경은 대략 가로 ±640 / 세로 ±360.

| 스킬 | Type | 획득 방식 | 사거리(px) | Cd | Mp | 배율 | 상태확률 | 속성 |
|---|---|---|---|---|---|---|---|---|
| FlameWave | 19 | 전방 콘(dot≤0.25) | 200 | 7.0 | 28 | 2x | 0.5 화상 | 불 |
| FrostNova | 20 | 자기중심 원 | 90 | 8.0 | 22 | 2x | 0.5 동결 | 얼음 |
| RainOfArrows | 25 | 자기중심 원 | 180 | 12.0 | 28 | 2x | - | - |
| Whirlwind | 5 | 자기중심 원 | (radius) | 6.0 | 20 | 2x | - | - |
| ChainLightning | 35 | 최근접 시작→연쇄 | **240**(첫)→170×5점프 | 9.0 | 34 | 2x | 0.4 감전 | 번개 |
| LifeDrain | 36 | 최근접 단일 | 240 | 8.0 | 28 | 3x | - | 암흑 |

## 투사체 스킬 (사거리 = 투사체 수명, 화면밖 위험 낮음)

투사체는 발사 방향으로 날아가며 PlayerController 투사체 수명이 사거리를
제한한다(자동 타깃팅 아님 → 보이지 않는 적을 콕 집어 때리지 않음).

| 스킬 | Type | 투사체 속도 | 비고 |
|---|---|---|---|
| FireBolt | 3 | 540 | 단발 |
| IceShard | 6 | 620 | 단발 |
| ArcaneMissile | 21 | 580 ×3 | 동일 방향 3연발(합산 3x) |
| PiercingShot | 23 | 700, 관통 2 | 최대 3적 관통 |

## 화면 밖 공격 위험 분석 & 조치

- **ChainLightning (조치함)**: 첫 타깃 auto-acquire가 300px로 형제
  자동타깃 스킬 중 최대였음 → 모바일 화면 밖 적을 시작점으로 잡을 수
  있어 **240px로 보수화**(LifeDrain과 동일). 연쇄 정체성(jumpRange
  170·maxJumps 5·점프당 0.8 감쇠)은 보존. `poolRange`는 `range` 기반
  파생값이라 자동 축소(240+5×170=1090, 후보 수집 전용 — 실제 타깃은
  reach 240/170로 제한되므로 임의 원거리 적을 때리지 않음).
- **LifeDrain**: 최근접 단일 240px. 화면 내. 흡혈은 `TakeDamageReporting`
  실제 적용 피해/3 (방어·저항·오버킬 클램프 후) — 과다회복 없음. 변경 없음.
- **RainOfArrows / FrostNova / Whirlwind / FlameWave**: 자기중심 또는
  전방 콘, 반경 ≤200px로 전부 화면 내. 변경 없음.
- **투사체류(FireBolt/IceShard/ArcaneMissile/PiercingShot)**: 자동
  타깃팅이 아니라 방향 발사 + 투사체 수명 제한 → 화면 밖 자동 타격
  위험 낮음. 변경 없음.

## 결론

## 스킬북 거점 분포 재점검 (작업 6)

씬 SkillBooks 배열 실측(중복 포함 배치 수):

| 거점 | 스킬북 수 | 성격 |
|---|---|---|
| town | **29** | 기본+다수 중상위 (사실상 슈퍼셋) |
| field_outpost | 9 | 전사 위주 |
| harbor_village | 8 | 궁수/신성 위주 |
| mountain_refuge | 6 | 마법사 고위 (ChainLightning 등) |

- **초반 OP 없음(검증됨)**: town의 중상위 스킬북(arcane_missile Lv12,
  rain_of_arrows Lv14, holy/earthquake 등)은 전부 `SkillData.RequiredLevel`
  게이트. `SkillShopUI`는 레벨 미달 시 "Lv.N 필요" 비활성 버튼만 노출(구매
  불가) → *구매 가능*하더라도 *조기 사용 불가*. 밸런스 사고 아님.
- **town 과집중(디자인 권고)**: town이 36종 중 29종을 보유해 "권역을
  탐험해 스킬북을 찾는" 루프가 약화됨(CLAUDE.md 설계 의도: town 10 /
  outpost 7 / harbor 5 / mountain 4). 이는 정합 버그가 아니라 분포
  디자인 이슈 → **권장 조치: town에서 권역 테마 스킬북(권역 거점에서도
  파는 중상위)을 제거해 거점별 정체성 복원.** 4개 .tscn SkillBooks 배열
  + ext_resource 정리가 필요한 다중 씬 변경이고 "어느 책을 어디로"가
  디자인 판단이라, 이번 패스에선 **재배치를 실행하지 않고 권고로 기록**
  (일방 결정 회피). 별도 승인 시 진행.
- **R15 신설(정합 자동검사)**: 스킬북 .tres의 RequiredClass /
  AvailableToAllClasses ↔ LearnedSkill(권위) 정합을 validate.py가 강제.
  기존 11개 스킬북이 메타 누락(기본 0/false)으로 스킬과 불일치였음 —
  `SkillShopUI`는 스킬 쪽(`item.LearnedSkill`)을 읽어 **런타임 영향은
  없었으나** 드리프트 제거 위해 11개 스킬북을 스킬 값에 맞춰 정렬.

## 결론

이번 패스에서 동작 영향 코드 변경은 **ChainLightning 첫 타깃 300→240 단 1건**.
나머지 6개 지목 스킬은 이미 화면 내 사거리거나 투사체 방향 발사라
보수 조정 불필요(과도한 리밸런스 회피). Cd/Mp/배율은 권역 진행 난이도와
정합 — 별도 결함 없음. 추후 실측(에디터 플레이)으로 체감 확인 권장.
