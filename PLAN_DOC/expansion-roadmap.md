# 기능 확장 로드맵

> 사용자 요청: 맵 제작 방안 + 아이템/스킬/퀘스트/다른 마을/이동수단/사냥터/적종류/속성 등 기능 확장.
> 이 문서는 방향성·우선순위·구현 옵션 정리. 사용자가 선택하면 별도 작업 분할.

## 1. 맵 제작 방안

현재: 11개 .tscn 직접 편집 (수작업, 1280×720 고정, 벽 4개 + 포털 + EnemySpawner).

### 옵션 A: TileMap 기반 데코레이션
- 각 맵 .tscn에 TileMapLayer 추가
- 타일셋 별도(`Resources/Tilesets/`)
- 장점: 시각 풍부, 길/풀/돌 표현
- 단점: 작업량 큼, 모바일 성능 부담

### 옵션 B: 기존 ColorRect Background + 환경 데코 Sprite2D 추가 (권장)
- 마을: 우물/벤치/건물 sprite 배치
- 필드: 나무/바위 sprite + 풀밭 패턴
- 던전: 횃불/석상/제단 sprite
- 환경 sprite는 `Objects/<zone>/<asset>.png` 폴더로 정리, 충돌 박스 옵션
- 장점: 빠른 작업, 가벼움, 모바일 적합
- 단점: 시각 단조로움 가능

### 옵션 C: 단순 절차 데코 (런타임 랜덤 배치)
- MapGenerator.cs에 데코 sprite 배치 함수
- 같은 seed로 동일 배치 → SaveData.FieldSeeds 활용
- 장점: 변화 있음, 작업량 적음
- 단점: 일관성 떨어질 수 있음

**제안**: B를 기본, C로 일부 필드에 적용.

## 2. 아이템 확장 방향

현재: ~220개. 부족 카테고리:

- **방패 (Shield)**: ItemType.Shield 추가, BonusDefense + 블록 확률
- **무기 옵션**: 화염/냉기 등 elemental damage rider, 처치 시 골드 보너스
- **장신구 세트**: 3개 장착 시 set bonus (예: 늑대 세트 — 공속+이속)
- **소비 가능 음식**: HP 점진 회복, 30초 buff 등 — 포션과 차별
- **세트 효과 시스템**: 화염·냉기·폭풍·암흑 세트 4부위 이상 장착 시 보너스

## 3. 스킬 확장

현재: 12종. 클래스별 5개 + 공통 3개 권장.

- **전사 추가**: 방패 차징, 회오리 베기 강화, 광폭화(공속/공력+ 방어-)
- **마법사**: 라이트닝 체인, 메테오(딜레이+광역), 텔레포트 단축
- **궁수**: 더블샷, 폭발 화살, 잠복(이동속도+ 다음 공격 크리)
- **공통**: 회복샘(주변 회복), 워크리(파티 buff), 함정 설치

스킬 .tres 추가 + SkillStrategies.cs에 strategy class 추가.

## 4. 퀘스트 확장

현재: 메인 10 + 사이드 30.

- **챕터별 사이드 분기**: 각 챕터 진행 중에만 받을 수 있는 시한 퀘스트
- **연쇄 퀘스트**: A완료 시 B해금, B완료 시 C해금
- **랭크 시스템**: 일일 5개 완료 시 보너스 골드/아이템
- **PVP/이벤트 퀘스트**: 보스 재처치 / 시간 단축

QuestData에 `PrerequisiteQuestId` + `IsDailyReset` 필드 추가.

## 5. 다른 마을 추가

현재: town 하나. 확장 후보:

- **항구 마을 (Harbor Village)**: field_4 안쪽에 작은 NPC 거점. 어부 NPC, 항해 정보 NPC
- **눈의 마을 (Snow Hamlet)**: field_5에 임시 캠프. 사냥꾼 NPC, 가죽 상점
- **화산 베이스캠프**: field_6에 광부 NPC + 광물 거래 상점

각 마을: 800×450 작은 .tscn + 2~3 NPC + 1 상점.

## 6. 이동수단

- **텔레포트 주문서**: 구현됨 (Phase K)
- **수상 ship/vessel**: town → 항구 → 다른 마을 (씬 전환 + 짧은 컷씬)
- **펠리컨 라이드** (옵션): field_3 NPC 부여, field_5/6 즉시 이동
- **순간이동 NPC**: 이미 town에 teleport_npc.tscn 존재 — 방문한 모든 씬 목적지 추가

## 7. 사냥터 추가

현재: outpost, field_1~6, dungeon_1~4, mine_1~3. 12개.

추가 후보:
- **고대 숲 (Ancient Forest)**: field_2와 field_3 사이 중간 난이도
- **버려진 채석장**: mine_2 끝 비밀 통로
- **하늘 정원** (엔드게임): field_6 너머 부유 섬, 보스 후 해금

## 8. 적 종류 확장

현재: 90종+. 부족한 행동:
- **분열형**: 죽으면 작은 적 2마리로 분리 (예: 슬라임)
- **호위형**: 다른 적 옆에서 데미지 감소 buff
- **자폭형**: 가까이 가면 폭발 → 광역 데미지
- **부활형**: 죽고 N초 후 부활 (1회)

`EnemyBehavior` enum 확장 + EnemyController에 처리 추가.

## 9. 속성 시스템 확장

현재 ElementType: None/Fire/Ice/Lightning/Dark.

- **상태이상 추가**: Burn(DOT)/Freeze(이동 0)/Shock(스킬 봉인)/Bleed(이동 시 추가 피해)
- **저항 시스템**: 적 .tres에 ResistFire/Ice/Lightning/Dark 비율
- **연계 효과**: Wet(Lightning에 약점) / Frozen(Crush에 강타)

EnemyStats에 `ElementResistance` Dictionary 추가, TakeDamage가 속성별 배율 적용.

## 우선순위 제안

1순위 (코어 즐길거리): 8(적 종류 확장), 3(스킬 확장), 9(속성 시스템)
2순위 (콘텐츠 폭): 2(아이템 세트), 7(사냥터), 4(퀘스트)
3순위 (감성): 1(맵 데코), 5(다른 마을), 6(이동수단)

각 항목은 2~6시간 단위로 분할 가능. 사용자가 우선 항목을 지정하면 별도 메가 작업으로 진행.
