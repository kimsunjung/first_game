# Hunting Journal v1 (2026-05-20) — 설계 문서

## 위치 선언 (Important)
**컬렉션북·도감 NOT.** 보상형 수집 / 일일 숙제 / VIP 잠금 / 시즌 패스 모두 **금지**.
이 시스템의 목적은 단 하나:

> "지금 내 캐릭터 레벨로 어디서 무엇을 사냥하면 좋은가" 를 플레이어가
> 빠르게 확인할 수 있는 **파밍 안내서**.

플레이어가 새 권역에 진입할 때 "여기 적은 뭘 떨구지? 저항은 뭘 챙기지?" 하는
당연한 질문에 답하는 정적 데이터 뷰. 보상·진행도·완료 보너스 없음.

## 데이터 구조 (Godot-free, SaveData 무관)

### Scripts/Data/HuntingJournal.cs (신규 — 미구현)
```csharp
public static class HuntingJournal
{
    public record ZoneEntry(
        string SceneName,         // "field_5_snowfield"
        string DisplayName,        // "설원 (필드 5)"
        string Region,             // "mountain_region"
        int RecommendedLevel,      // 15
        string[] EnemyTypes,       // ["FrostWolf", "IceImp", ...]
        string[] PrimaryDrops,    // ["glacier_shard", "frost_essence"]
        string[] OreNodes,         // [] or ["mine_3 광맥"]
        string BossId,             // "" or "glacier_titan_f5"
        bool BossRepeatable,
        string[] RecommendedResists, // ["Freeze", "Shock"]
        string[] RecommendedItems,    // ["warming_draught", "antidote"]
        string[] PortalsFrom        // ["field_3 (RIGHT)", "snowfield_edge (RIGHT)"]
    );

    // 28개 사냥터 + 5개 던전/광산 정적 데이터
    public static readonly ZoneEntry[] Entries = { ... };

    public static ZoneEntry Get(string sceneName) => ...;
    public static ZoneEntry[] ByRegion(string region) => ...;
}
```

**SaveData 변경 없음**. 모든 데이터는 정적 컴파일 — 빌드 시점에 결정.

## UI 설계 (Scripts/UI/HuntingJournalUI.cs — 신규)

### 진입점
- 메인 메뉴 / HUD 부메뉴 / 4 허브의 ContractBoard NPC 옆 NPC (예: "전령" `journal_npc`)
- 모바일: 단일 버튼 → 풀스크린 패널

### 화면 구조 (모바일 480×720 기준)
```
┌─────────────────────────────────┐
│ [< 권역 탭]  [town/outpost/coast/mtn] │
├─────────────────────────────────┤
│ 권역 사냥터 목록 (스크롤)         │
│  • town_outskirts (Lv 1+)        │
│  • green_meadow (Lv 2+)          │
│  • goblin_woods (Lv 3+)          │
│  • old_orc_road (Lv 4+)          │
│  • field_1 (Lv 4+)               │
│  • mine_1 (Lv 4+) [광산]         │
│  • dungeon_1 (Lv 6+) [던전 보스] │
├─────────────────────────────────┤
│ 선택한 사냥터 상세                │
│  적: 고블린, 거미, 늑대           │
│  드랍: leather, gold, ...        │
│  광맥: -                          │
│  보스: -                          │
│  저항 권장: 없음                  │
│  소모품 권장: health_potion       │
│  진입 경로: town > field_1        │
└─────────────────────────────────┘
```

## 발견(Discovery) 모델

### 옵션 A (권장): 모두 공개
- 플레이어 입장에서 "다음에 어디로?" 안내가 핵심 가치 → 발견 게이트는 정보
  접근성을 떨어뜨림.
- SaveData 무관, 구현 단순.

### 옵션 B (보류): 권역 단위 발견
- 권역 첫 진입 시 그 권역만 해금. 이미 SaveData v13 에 region 플래그 있음
  (`BackfillRegionFlagsV13`).
- 약간의 발견감을 주지만 권역 4개뿐이라 정보가치 손실 작음. 보류 가능.

### 옵션 C (거부): 사냥터 개별 발견
- 사냥터 28개 개별 추적 → SaveData v14 마이그레이션 필요. 위험 / 가치 낮음.

**v1 결정: 옵션 A (모두 공개)** — SaveData 무영향, 모바일 단순성, 안내 목적
최우선.

## 자동 검증 (구현 시)
- xUnit `HuntingJournalTests`:
  - 모든 ZoneEntry.SceneName 이 실제 .tscn 파일에 존재
  - RecommendedLevel == MapLevels.Get(SceneName) (단일 진실 소스)
  - BossId 가 있으면 RepeatableBoss 표기와 씬 데이터 일치
  - Region ∈ {town_region, outpost_region, coast_region, mountain_region}
- 광맥/적 종류는 씬 추출보다는 수동 큐레이션(권역 정체성 차원에서 중요한 것만)

## 구현 위험도 평가

| 항목 | 위험도 | 사유 |
|---|---|---|
| Scripts/Data/HuntingJournal.cs | 낮음 | 정적 데이터, SaveData 무관 |
| Scripts/UI/HuntingJournalUI.cs | 중간 | 신규 풀스크린 UI, 모바일 가독성 튜닝 필요 |
| 진입 NPC 추가 (4 허브) | 중간 | 신규 NPC 노드 4 씬 추가. 기존 ContractBoardNPC 같은 패턴이므로 회귀 적음 |
| SaveData 변경 | **없음** | v1 결정에서 옵션 A 채택, 발견 시스템 없음 |
| xUnit 추가 | 낮음 | 정적 검증, 위험 없음 |
| 신규 PNG | **없음** | 텍스트 패널만, 아이콘은 기존 사용 |

## v1 구현 범위 (제안)

**Phase 1 — 데이터** (이 패스 가능)
- [ ] `Scripts/Data/HuntingJournal.cs` 정적 데이터 작성
- [ ] xUnit `HuntingJournalTests` (SceneName 정합, Level 정합, BossId 정합)

**Phase 2 — UI** (별도 패스)
- [ ] `Scripts/UI/HuntingJournalUI.cs` 풀스크린 패널
- [ ] 권역 탭 / 사냥터 목록 / 상세
- [ ] HUD 부메뉴 진입점

**Phase 3 — NPC** (별도 패스)
- [ ] `journal_npc.tscn` 4 허브 배치
- [ ] ChapterDialogue 등록 (선택, no-op 가능)

**v1 패스 (2026-05-20) 결정**: **Phase 1 도 보류**. 이번 패스는 설계 문서
까지만. Phase 1 ~ 3 은 다음 프롬프트에서 사용자가 명시할 때 진행.

이유:
1. 정적 데이터라도 28개 사냥터 × 6 필드 × 정확도 검증 = 큰 한 번 작업.
2. 잘못된 정보가 들어가면 안내 가치를 깎음 → 한 번에 정확하게 작성해야 함.
3. UI 없이 데이터만 있으면 사실상 검증/문서 외 가치 없음 → UI 와 한 번에.

## 모바일 단순성 가드
- 화면당 정보 6~8 줄 이내.
- 폰트 크기 14pt 이상.
- 텍스트만 — 아이콘 cosmetic 추가는 PNG 재사용으로만.
- 보상·진행도·% 게이지 **금지** (컬렉션북화 거부).

## 참고
- `Scripts/Data/MapLevels.cs` 가 RecommendedLevel 단일 진실 소스.
- `Scripts/Data/MapNames.cs` 가 DisplayName 단일 진실 소스.
- 두 정적 클래스에 의존해 HuntingJournal 데이터 입력을 줄임.
