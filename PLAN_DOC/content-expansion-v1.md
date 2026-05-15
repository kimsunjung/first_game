# Content Expansion v1 — 신규 아이템·스킬·지역 연결표

> 작성: 2026-05-15 | World & Content Expansion Pass v1
>
> **v2 노트 (2026-05-15, Regional Hunting World Full Build):**
> 이 문서는 "엔딩형 RPG" 전제 하에 작성됐다. v2에서 게임이 **오픈엔드 사냥 RPG**로 재정의되면서 다음 표현은 의미가 달라졌다:
> - "dungeon_3 클리어 보상" / "최종 보스 드랍" → "outpost_region 정점 보스 드랍" (게임 종료가 아니라 다음 권역 진입의 신호)
> - "field_X 클리어 후 해금" → 현재 코드에는 동적 해금 시스템이 없고 ShopNPC.ShopItems는 정적이다. 해금 시기는 **거점별 정적 분산**으로 대체됐다 — `regional-world-map-plan.md` §6 참조.
> - v2에서 스킬북 26종이 town에서 4개 거점(town/field_outpost/harbor_village/mountain_refuge)으로 분산됐다.

---

## 신규 스킬 목록 (12개 추가)

### 전사 (Warrior, RequiredClass=0)

| 스킬명 | SkillType | Lv | MP | 쿨타임 | 효과 | 스킬북 가격 |
|--------|-----------|----|----|--------|------|------------|
| 클리브 | Cleave=15 | 8 | 18 | 5s | 전방 콘 2배 광역 | 700G (field_2 클리어 후) |
| 그라운드 슬램 | GroundSlam=16 | 12 | 30 | 10s | 주변 150f 3배 광역 | 1200G (field_3) |
| 배틀 크라이 | BattleCry=17 | 10 | 20 | 45s | 30초 공격+15 방어+8 | 900G (dungeon_2) |
| 처형 | Execute=18 | 15 | 25 | 8s | 단일 3배 강타 | 1800G (dungeon_3) |

### 마법사 (Mage, RequiredClass=1)

| 스킬명 | SkillType | Lv | MP | 쿨타임 | 효과 | 스킬북 가격 |
|--------|-----------|----|----|--------|------|------------|
| 플레임 웨이브 | FlameWave=19 | 10 | 28 | 7s | 전방 콘 화염 2배 광역 | 900G (field_2) |
| 프로스트 노바 | FrostNova=20 | 8 | 22 | 8s | 주변 90f 얼음 2배 광역 | 750G (field_2) |
| 아케인 미사일 | ArcaneMissile=21 | 12 | 30 | 6s | 전방 3발 연속 (합산 3배) | 1200G (field_3) |
| 마나 실드 | ManaShield=22 | 10 | 20 | 20s | 10초 방어+20 | 1000G (dungeon_2) |

### 궁수 (Archer, RequiredClass=2)

| 스킬명 | SkillType | Lv | MP | 쿨타임 | 효과 | 스킬북 가격 |
|--------|-----------|----|----|--------|------|------------|
| 관통 사격 | PiercingShot=23 | 8 | 20 | 6s | 고속 3배 단일 원거리 | 750G (field_2) |
| 백스텝 샷 | BackstepShot=24 | 6 | 15 | 5s | 사격 + 대시 동시 발동 | 600G (field_outpost) |
| 화살 비 | RainOfArrows=25 | 14 | 28 | 12s | 180f 2배 광역 | 1600G (field_3) |
| 헌터 포커스 | HunterFocus=26 | 10 | 18 | 30s | 15초 공격+8 크리+20% | 950G (dungeon_2) |

### 스킬 해금 위치 권장

```
field_outpost 스킬샵: skillbook_backstep_shot (궁수)
dungeon_2 클리어 보상 or 마을 업그레이드: battle_cry, mana_shield, hunter_focus
field_3 이후 마을 고급 스킬샵: arcane_missile, rain_of_arrows, ground_slam
dungeon_3 클리어 보상: skillbook_execute
```

---

## 기존 아이템 수정 목록

| 파일 | 수정 내용 |
|------|-----------|
| `swift_potion.tres` | UseEffect=None → Buff(이속+50, 공속+10%, 30s), 가격 180G |
| `antidote.tres` | UseEffect=None → CureStatus(HealAmount=1=독), 가격 80G |
| `guard_potion.tres` | 이름 "강화 방어 물약", UseEffect=None → Buff(방어+15, 90s), 가격 380G |

---

## 신규 소모품 (8개)

| 파일 | 아이템명 | 효과 | 해금 위치 | 가격 |
|------|---------|------|----------|------|
| `tidal_tonic.tres` | 조류 강장제 | Heal HP+60 | field_4_harbor 상점 | 150G |
| `warming_draught.tres` | 온기 묘약 | Buff 방어+12 이속+20, 60s | field_5_snowfield 상점 | 220G |
| `defrost_potion.tres` | 해빙제 | CureStatus 빙결 해제 | field_5_snowfield 상점 | 120G |
| `fire_resist_elixir.tres` | 화염 저항 엘릭서 | Buff 방어+20, 90s | field_6_volcano 상점 | 350G |
| `magma_brew.tres` | 마그마 브루 | Buff 공격+18, 45s | field_6_volcano 상점 | 400G |
| `battle_elixir.tres` | 전투 엘릭서 | Buff 공격+20 크리+15%, 45s | field_3/6 상점 or 보스전 전 NPC | 600G |
| `curse_water.tres` | 저주 해제수 | CureStatus 전체(HealAmount=7) | dungeon_3 or field_3 상점 | 280G |
| `mana_herb_extract.tres` | 마나 초근 추출액 | RestoreMana+120 | field_4 or 마을 이후 상점 | 200G |

---

## 신규 재료 (8개)

| 파일 | 아이템명 | 희귀도 | 입수 방법 | 용도 |
|------|---------|--------|----------|------|
| `sea_kelp.tres` | 해초 | Common | field_4 적 드랍 / 채취 | 제작/강화 재료 |
| `glacier_shard.tres` | 빙하 파편 | Uncommon | field_5 적 드랍 | 냉기 무기 강화 |
| `lava_stone.tres` | 용암석 | Uncommon | field_6 적 드랍 | 화염 무기 강화 |
| `titan_scale.tres` | 빙하 타이탄 비늘 | Rare | GlacierTitan 드랍(25%) | 최고급 방어구 강화 |
| `titan_core.tres` | 빙하 타이탄 핵 | Epic | GlacierTitan 드랍(10%) | 전설급 제작 소재 |
| `drake_scale.tres` | 드레이크 비늘 | Rare | InfernoDrake 드랍(30%) | 화염 무기 최고급 강화 |
| `drake_eye.tres` | 드레이크의 눈 | Epic | InfernoDrake 드랍(15%) | 전설급 제작 소재 |
| `kraken_ink.tres` | 크라켄 먹물 | Rare | Kraken 드랍(30%) | 어둠 무기 강화 |

---

## 보스 드랍 테이블 업데이트

### GlacierTitan (field5_named_glacier_titan.tres)

| 아이템 | 가중치 | 비고 |
|--------|--------|------|
| health_potion | 30% | 위안 드랍 |
| glacier_axe | 20% | 무기 |
| frost_cloak | 15% | 방어구 |
| **titan_scale (신규)** | **25%** | 강화 재료 |
| **titan_core (신규)** | **10%** | 희귀 재료 |

DropChance: 0.7 → **1.0** (보스는 무조건 드랍)

### InfernoDrake (field6_named_inferno_drake.tres)

| 아이템 | 가중치 | 비고 |
|--------|--------|------|
| health_potion | 25% | 위안 드랍 |
| inferno_staff | 18% | 무기 |
| ember_cloak | 12% | 방어구 |
| **drake_scale (신규)** | **30%** | 강화 재료 |
| **drake_eye (신규)** | **15%** | 희귀 재료 |

DropChance: 0.7 → **1.0** (보스는 무조건 드랍)

### Kraken (boss_dungeon4_kraken.tres)

| 아이템 | 가중치 | 비고 |
|--------|--------|------|
| health_potion | 30% | 위안 드랍 |
| kraken_trident | 25% | 무기 |
| dark_armor | 15% | 방어구 |
| **kraken_ink (신규)** | **30%** | 강화 재료 |

---

## game_balance.json 신규 구역

```json
"field_outpost":          { "hpMul": 1.2, "atkMul": 1.1, "expMul": 1.2 },
"field_4_harbor":         { "hpMul": 3.5, "atkMul": 2.5, "expMul": 3.2 },
"dungeon_4_sunken_shrine":{ "hpMul": 5.5, "atkMul": 3.5, "expMul": 5.0 },
"field_5_snowfield":      { "hpMul": 6.0, "atkMul": 3.8, "expMul": 5.5 },
"field_6_volcano":        { "hpMul": 8.0, "atkMul": 5.0, "expMul": 7.0 },
"mine_3":                 { "hpMul": 3.0, "atkMul": 2.0, "expMul": 2.5 }
```

---

## 이전 TODO 중 완료된 항목 (v2 후속 패스에서 처리)

- ✅ 스킬샵 NPC ShopItems 배열에 신규 스킬북 26종 연결 — town 10권 + field_outpost 7권(NEW SkillShopNPC) + harbor_village 5권 + mountain_refuge 4권으로 거점별 분산
- ✅ field_4_harbor / field_5_snowfield / field_6_volcano / harbor_village / mountain_refuge 상점 NPC에 지역 특산 소모품 배치
- ✅ `titan_scale` / `titan_core` / `drake_scale` / `drake_eye` 를 `Resources/Balance/game_balance.json` enhanceMaterials에 추가
- ✅ ManaShield 실제 MP 흡수: `PlayerStats.IsManaShieldActive` + `PlayerController.Combat.TakeDamage`에서 MP로 데미지 소모. 방어 버프 아닌 진짜 흡수
- ✅ PiercingShot 실제 관통: `PlayerProjectile.PierceCount` + `_hitIds` HashSet으로 N명 관통, 같은 적 중복 히트 방지
- ✅ BackstepShot 강제 후방 대시: `ISkillTarget.ActivateDashInDirection` + `_dashForcedDir`로 입력과 무관하게 -FacingDirection 방향 대시
- ✅ 상태이상 시스템 양방향 (Poison/Freeze/Curse): `Scripts/Data/StatusEffect.cs` 신규, `CharacterStats.ApplyStatus/TickStatuses/GetStatusModulate`, 적→플레이어 `EnemyController.TryInflictStatus`, 플레이어→적 동일 경로
- ✅ CureStatus 아이템 실효성: `Inventory.cs` CureStatus 분기에서 `target.CureStatuses(slot.Item.HealAmount)` 호출 (HealAmount를 비트마스크로 사용)
- ✅ 원거리 적 상태이상 적용: `EnemyProjectile.InflictedStatus/Duration/Chance` + `EnemyController.SpawnProjectile`에서 Stats 값 위임 + `OnBodyEntered`에서 PlayerController 적중 시 ApplyStatus
- ✅ Repeatable boss 분리: `EnemySpawner.RepeatableBoss` 플래그로 first-kill 기록(GameManager.DefeatedBosses)과 재출현 조건을 분리. 야외/광산 보스(field_5/field_6/mine_3/dungeon_4)는 RepeatableBoss=true로 파밍 가능, 메인 던전 보스(dungeon_1/2/3)는 1회용 유지

## 남은 TODO

- **Burn (화상) / Shock (감전) 상태이상 신규 추가**: `StatusEffect` 플래그에 Burn=8, Shock=16 등 추가하고 CharacterStats.TickStatuses에서 dot/이속감소 등 효과 정의
- **플레이어 스킬의 상태이상 부여**: 현재 상태이상은 적→플레이어 방향만 활성. 플레이어 스킬(FrostNova → Freeze, FlameWave → Burn, PoisonShot 등)에서 적에게 상태이상을 거는 부분은 SkillStrategies에 아직 미구현. ISkillTarget 또는 직접 적 컨트롤러 호출 경로 필요
- **상태이상 가시화 강화**: 현재는 player.Modulate / enemy.AnimatedSprite2D.Modulate의 색조 변화만 표시. 상태 아이콘 HUD, 남은 시간 게이지 등 미구현
- **단계별 상점 해금**: 챕터 플래그 기반으로 ShopNPC.ShopItems를 동적 필터링 (현재는 정적 배열)
- **Status Resistance**: 장비/스킬로 특정 상태이상 저항/면역 부여 시스템 (현재 모든 적중이 확률만으로 결정됨)

---

## 파일 통계 (Expansion v1 완료 기준)

| 카테고리 | 신규 | 수정 | 총계 |
|----------|------|------|------|
| C# 코드 | 0 | 2 | — |
| SkillData .tres | 12 | 0 | 27 |
| ItemData .tres (스킬북) | 12 | 0 | 222+ |
| ItemData .tres (소모품) | 8 | 3 | — |
| ItemData .tres (재료) | 8 | 0 | — |
| EnemyStats .tres | 0 | 3 | 97 |
| game_balance.json | 6개 구역 | — | 14개 구역 |
| 문서 .md | 2 | 0 | — |
