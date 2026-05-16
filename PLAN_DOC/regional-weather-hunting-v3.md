# Regional Weather & Hunting Identity v3 (2026-05-16)

## 왜 신규 맵이 아니라 기존 사냥터 정체성 강화인가
맵 수는 v2에서 이미 충분(4권역 28 zone). 반복 파밍 체감이 밋밋한 원인은 (1) 균등 스폰이라 지역색이 없고 (2) 지역 고유 분위기/위험이 없고 (3) 상태저항 대비책이 약해서였다. v3는 새 PNG 없이 **가중치 스폰 + 코드 날씨 + 상태저항 배선 + 날씨 테마 적/소모품**으로 기존 사냥터의 개성과 준비-파밍 루프를 강화한다.

## WeatherKind 의미
| Kind | 의미 | 대표 위험 |
|---|---|---|
| Clear | 무효과 | 없음 |
| MeadowBreeze | 초원 산들바람 | 없음 |
| ForestMist | 숲 안개 | (선택) Poison 미약 |
| GraveFog | 묘지 독무 | Curse |
| SeaRain | 바닷비 | 없음 |
| SeaStorm | 바다 폭풍 | Shock |
| Snowfall | 눈발 | (약) Freeze |
| Blizzard | 눈보라 | Freeze |
| VolcanicAsh | 화산재 | Burn |
| Heatwave | 열파 | Burn |
| CrystalDust | 수정 먼지 | (약) Shock |
| DungeonGloom | 던전 어둠 | (선택) Curse |

`Scripts/Maps/BiomeWeatherController.cs` — 씬 루트에 붙는 Node2D. `_Ready`에서 화면 좌표(CanvasLayer layer=0, HUD 아래)로 ColorRect 오버레이 + CpuParticles2D(상한 120, 모바일 보호) 생성. `HazardEnabled`면 `HazardInterval`마다 `StatusChance`로 플레이어 `Stats.ApplyStatus`. 날씨 상태는 저장하지 않음(씬 고정). DayNightCycle 미변경.

## 맵별 날씨/위험
| 맵 | Kind | Hazard |
|---|---|---|
| town / field_outpost | Clear / MeadowBreeze | off |
| harbor_village / mountain_refuge | SeaRain / Snowfall (약, VI 0.10) | off (허브 위험 없음) |
| town_outskirts / green_meadow | MeadowBreeze | off |
| goblin_woods | ForestMist | Poison 3% |
| old_orc_road | ForestMist | off |
| field_2 / ruined_crossroad | GraveFog | Curse 5% / 6% |
| field_3 | DungeonGloom | Curse 7% |
| dungeon_1 / dungeon_2 / dungeon_3 | DungeonGloom | off / Curse 5% / Curse 7% |
| harbor_outskirts / crab_beach | SeaRain | off |
| pirate_camp / field_4_harbor / dungeon_4_sunken_shrine | SeaStorm | Shock 5% / 6% / 7% |
| snowfield_edge | Snowfall | Freeze 4% |
| frozen_valley / field_5_snowfield | Blizzard | Freeze 7% / 8% |
| volcano_approach | VolcanicAsh | Burn 5% |
| lava_field / field_6_volcano | Heatwave | Burn 7% / 8% |
| mine_1 / mine_2 / mine_3 | CrystalDust | off / Shock 3% / Shock 6% |

위험은 4초 간격·낮은 확률이라 무대비 입장도 즉사하지 않으며, 상태저항 소모품으로 체감상 확실히 줄어든다. 허브는 위험 없음.

## 신규 적 12종 (기존 스프라이트 임시 재사용 — 전용 PNG 없음)
| 적 .tres | 재사용 스프라이트 | 행동/상태 | 주요 드랍 |
|---|---|---|---|
| field1_mist_spider | Field1/forest_spider.png | 근접 빠름, Poison 25% | health_potion/antidote/material_wood |
| field1_grove_spirit | Field1/forest_spirit.png | 원거리, Curse 15% | mana_potion/material_wood |
| field2_fog_wraith | Field2/grave_wraith.png | 근접, Curse 25% | bone_dust/curse_water |
| field3_ruin_sentinel | Field3/ruin_golem.png | 고방어·저속 | ancient_bone/ancient_hide |
| field4_storm_siren | Dungeon4/siren.png | 원거리, Shock 30% | sea_kelp/storm_ward/mana_herb_extract |
| field4_reef_raider | Field4/pirate_grunt.png | 빠른 근접 | cutlass/sea_kelp/health_potion |
| dungeon4_tide_lurker | Dungeon4/deep_lurker.png | 소극·고데미지 | kraken_ink/tidal_tonic |
| field5_blizzard_witch | Field5/snow_witch.png | 원거리, Freeze 35% | glacier_shard/defrost_potion |
| field5_frostbound_bear | Field5/polar_bear.png | 튼튼 근접 | titan_scale(15%)/warming_draught |
| field6_ash_imp | Field6/fire_imp.png | 원거리, Burn 25% | lava_stone/cooling_salve |
| field6_cinder_salamander | Field6/salamander.png | 빠름, Burn 20% | lava_stone/ash_filter |
| mine3_crystal_shocker | Mine/crystal_warlock.png | 원거리, Shock 25% | crystal_ore/prismatic_crystal(15%) |

> 임시 재사용: 위 스프라이트는 전용 아트가 아니라 같은 권역 기존 적 PNG를 재사용한다. 전용 PNG는 후속 TODO. `generated-asset-inventory.md`는 신규 PNG가 없으므로 수정하지 않음.

## 신규 소모품 6개 (기존 아이콘 임시 재사용)
| 아이템 | 효과 | 아이콘 재사용 | 배치 |
|---|---|---|---|
| mist_charm | StatusResist +0.15 / 90s | antidote.png | town 소모품상점(저가) |
| purifying_incense | Curse+Poison 해제 (mask 5) | curse_water.png | town 소모품상점(저가) |
| storm_ward | StatusResist +0.20 / 90s | tidal_tonic.png | harbor_village·field_4_harbor |
| grounding_salve | Shock 해제 (mask 16) | mana_herb_extract.png | harbor_village·field_4_harbor |
| ash_filter | StatusResist +0.18, 방어 +6 / 90s | fire_resist_elixir.png | mountain_refuge·field_5·field_6 |
| cooling_salve | Burn 해제 (mask 8) | defrost_potion.png | mountain_refuge·field_5·field_6 |

## 기존 지역 소모품 변경
| 아이템 | 변경 |
|---|---|
| warming_draught | 기존 방어/이속 buff 유지 + BuffStatusResist 0.20 |
| fire_resist_elixir | BuffStatusResist 0.25 추가 |
| magma_brew | 공격 buff 유지 + BuffStatusResist 0.10 |
| tidal_tonic | HP 회복 유지 + BuffStatusResist 0.10 (※ UseEffect=Heal이라 현재는 비활성 필드 — 후속에서 Buff형 분리 시 활성) |
| curse_water | CureStatus mask 7 → **31**(Poison1+Freeze2+Curse4+Burn8+Shock16 전체) |
| defrost_potion | 변경 없음(Freeze 해제 유지) |

## 스폰 가중치 시스템 사용법
`EnemySpawner.StatWeights : float[]` (신규 export). 규칙:
- `null` 이거나 `StatVariants` 와 길이가 다르거나 합이 0 이하 → 기존 균등 랜덤 fallback.
- 유효하면 룰렛 가중치 선택. zone scaling / elite / Duplicate 흐름은 그대로.
- 씬 .tscn 예: `StatWeights = PackedFloat32Array(1.0, 1.0, 1.0, 0.4)` — 앞 3종 일반, 마지막(신규 테마 적) 희귀(≈9%).
- `tools/validate/validate.py` R3: StatWeights 존재 시 항목 수==StatVariants, 합>0 검사.

v3에서 18개 사냥터에 신규 테마 적 1종을 희귀 가중치(0.25~0.4)로 추가. field_1(초보), 보스 던전의 보스 구조는 유지.

## 남은 후속 TODO
- 전용 날씨 오버레이/파티클 아트, 신규 적 12종 전용 PNG, 신규 소모품 전용 아이콘.
- Godot 에디터 모바일 실측: 오버레이 농도, 상태이상 빈도 짜증도, 희귀몹 출현율, 파티클 성능.
- tidal_tonic 등 "회복+저항" 동시형을 위한 소모품 효과 다중화(현재 UseEffect 단일 분기).
- 기존 일반몹의 common/regional 세부 가중치 정밀 튜닝(현재는 기존=균등, 신규=희귀).
