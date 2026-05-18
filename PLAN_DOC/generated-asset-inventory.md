# Generated GPT Asset Inventory

작성: 2026-05-13
목적: GPT로 생성한 아이콘·캐릭터 PNG와 `Resources/Items/*.tres` ItemData 현황을 카테고리별로 정리해 **이미지 재생성 중복**과 **ItemData 덮어쓰기**를 방지한다. 적 인벤토리는 [enemy-zone-plan.md](enemy-zone-plan.md), NPC 매핑은 [generated-npc-inventory.md](generated-npc-inventory.md)에 별도 정리.

**⚠️ 중복 생성 금지 (이미 존재 — 새 PNG/`.tres` 생성 전 반드시 확인)**: `antidote`, `battle_helm`, `chainmail_armor`, `copper_ore`, `dungeon_key`, `emerald_ring`, `enhance_stone`, `gold_ore`, `guard_potion`, `health_potion`, `hi_potion`, `iron_ore`, `knight_boots`, `knight_plate_armor`, `mana_potion`, `mega_potion`, `mithril_ore`, `moonstone_necklace`, `mystic_bracelet`, `mystic_robe`, `platinum_ore`, `ranger_hood`, `ranger_vest`, `return_scroll`, `ruby_ore`, `ruby_ring`, `sapphire_ore`, `shadow_boots`, `silver_ore`, `steel_helm`, `sun_amulet`, `swift_boots`, `swift_potion`, `warrior_bracelet`

**범례**
- `상태` 컬럼: `사용 중` = 게임 로직(상점/드랍/장착 등)에서 실제 참조 / `리소스만 등록` = `.tres` 존재하지만 어디서도 참조 안 됨(드랍 후보 등) / `미사용` = `.tres` 없고 PNG만 / `중복보류` = 의도적 미등록
- ⚠️**중복주의** = 사용자 요구사항으로 마킹된 중복 위험 항목

---

## Consumables (16개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `antidote` | `Resources/Generated/GPT/Icons/Items/antidote.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `antidote_plus` | `Resources/Generated/GPT/Icons/Items/antidote_plus.png` | ✅ | Uncommon | 사용 중 |
| `attack_potion` | `Resources/Generated/GPT/Icons/Items/attack_potion.png` | ✅ | Uncommon | 사용 중 |
| `crit_potion` | `Resources/Generated/GPT/Icons/Items/crit_potion.png` | ✅ | Uncommon | 사용 중 |
| `defense_potion` | `Resources/Generated/GPT/Icons/Items/defense_potion.png` | ✅ | Uncommon | 사용 중 |
| `guard_potion` | `Resources/Generated/GPT/Icons/Items/guard_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `health_potion` | `Resources/Generated/GPT/Icons/Items/health_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `hi_potion` | `Resources/Generated/GPT/Icons/Items/hi_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `holy_water` | `Resources/Generated/GPT/Icons/Items/holy_water.png` | ✅ | Uncommon | 사용 중 |
| `mana_potion` | `Resources/Generated/GPT/Icons/Items/mana_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `mega_health_potion` | `Resources/Generated/GPT/Icons/Items/mega_health_potion.png` | ✅ | Rare | 사용 중 |
| `mega_mana_potion` | `Resources/Generated/GPT/Icons/Items/mega_mana_potion.png` | ✅ | Rare | 사용 중 |
| `mega_potion` | `Resources/Generated/GPT/Icons/Items/mega_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `return_scroll` | `Resources/Generated/GPT/Icons/Items/return_scroll.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `revive_scroll` | `Resources/Generated/GPT/Icons/Items/revive_scroll.png` | ✅ | Rare | 사용 중 |
| `swift_potion` | `Resources/Generated/GPT/Icons/Items/swift_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |

## Materials (광물 제외) (9개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `ancient_bone` | `Resources/Generated/GPT/Icons/Items/ancient_bone.png` | ✅ | Rare | 사용 중 |
| `ancient_hide` | `Resources/Generated/GPT/Icons/Items/ancient_hide.png` | ✅ | Rare | 사용 중 |
| `bone_dust` | `Resources/Generated/GPT/Icons/Items/bone_dust.png` | ✅ | Common | 사용 중 |
| `boss_core` | `Resources/Generated/GPT/Icons/Items/boss_core.png` | ✅ | Common | 사용 중 |
| `demon_trophy` | `Resources/Generated/GPT/Icons/Items/boss_trophy.png` | ✅ | Common | 사용 중 |
| `dungeon_key` | `Resources/Generated/GPT/Icons/Items/dungeon_key.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `enhance_stone` | `Resources/Generated/GPT/Icons/Items/boss_core.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `material_wood` | `Resources/Generated/GPT/Icons/Items/material_wood.png` | ✅ | Common | 사용 중 |
| `orc_leather` | `Resources/Generated/GPT/Icons/Items/orc_leather.png` | ✅ | Common | 사용 중 |

## Ores (8개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `copper_ore` | `Resources/Generated/GPT/Icons/Ores/copper_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `gold_ore` | `Resources/Generated/GPT/Icons/Ores/gold_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `iron_ore` | `Resources/Generated/GPT/Icons/Ores/iron_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `mithril_ore` | `Resources/Generated/GPT/Icons/Ores/mithril_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `platinum_ore` | `Resources/Generated/GPT/Icons/Ores/platinum_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `ruby_ore` | `Resources/Generated/GPT/Icons/Ores/ruby_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `sapphire_ore` | `Resources/Generated/GPT/Icons/Ores/sapphire_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `silver_ore` | `Resources/Generated/GPT/Icons/Ores/silver_ore.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |

## Weapons (46개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `arcane_blade` | `Resources/Generated/GPT/Icons/Items/arcane_blade.png` | ✅ | Rare | 사용 중 |
| `arcane_hammer` | `Resources/Generated/GPT/Icons/Items/arcane_hammer.png` | ✅ | Rare | 사용 중 |
| `battle_axe` | `Resources/Generated/GPT/Icons/Items/battle_axe.png` | ✅ | Common | 사용 중 |
| `crossed_blades` | `Resources/Generated/GPT/Icons/Items/crossed_blades.png` | ✅ | Rare | 사용 중 |
| `crystal_staff` | `Resources/Generated/GPT/Icons/Items/crystal_staff_v2.png` | ✅ | Rare | 사용 중 |
| `dagger` | `Resources/Generated/GPT/Icons/Items/dagger.png` | ✅ | Common | 사용 중 |
| `dread_blade` | `Resources/Generated/GPT/Icons/Items/dread_blade_v2.png` | ✅ | Rare | 사용 중 |
| `executioner_axe` | `Resources/Generated/GPT/Icons/Items/executioner_axe.png` | ✅ | Rare | 사용 중 |
| `flame_sword` | `Resources/Generated/GPT/Icons/Items/flame_sword.png` | ✅ | Rare | 사용 중 |
| `frost_staff` | `Resources/Generated/GPT/Icons/Items/frost_staff.png` | ✅ | Uncommon | 사용 중 |
| `golden_mace` | `Resources/Generated/GPT/Icons/Items/golden_mace.png` | ✅ | Rare | 사용 중 |
| `golden_sword` | `Resources/Generated/GPT/Icons/Items/golden_sword.png` | ✅ | Rare | 사용 중 |
| `great_axe` | `Resources/Generated/GPT/Icons/Items/great_axe.png` | ✅ | Rare | 사용 중 |
| `great_sword` | `Resources/Generated/GPT/Icons/Items/great_sword.png` | ✅ | Common | 사용 중 |
| `halberd` | `Resources/Generated/GPT/Icons/Items/halberd.png` | ✅ | Rare | 사용 중 |
| `hand_axe` | `Resources/Generated/GPT/Icons/Items/hand_axe.png` | ✅ | Common | 사용 중 |
| `heavy_cleaver` | `Resources/Generated/GPT/Icons/Items/heavy_cleaver.png` | ✅ | Rare | 사용 중 |
| `heavy_mace` | `Resources/Generated/GPT/Icons/Items/heavy_mace.png` | ✅ | Rare | 사용 중 |
| `hero_sword` | `Resources/Generated/GPT/Icons/Items/hero_sword.png` | ✅ | Legendary | 사용 중 |
| `inferno_staff` | `Resources/Generated/GPT/Icons/Items/inferno_staff.png` | ✅ | Epic | 사용 중 |
| `iron_spear` | `Resources/Generated/GPT/Icons/Items/iron_spear.png` | ✅ | Common | 사용 중 |
| `iron_sword` | `Resources/Generated/GPT/Icons/Items/iron_sword.png` | ✅ | Common | 사용 중 |
| `kraken_trident` | `Resources/Generated/GPT/Icons/Items/kraken_trident.png` | ✅ | Legendary | 사용 중 |
| `magma_hammer` | `Resources/Generated/GPT/Icons/Items/magma_hammer.png` | ✅ | Epic | 사용 중 |
| `nature_staff` | `Resources/Generated/GPT/Icons/Items/nature_staff.png` | ✅ | Uncommon | 사용 중 |
| `ornate_sword` | `Resources/Generated/GPT/Icons/Items/ornate_sword.png` | ✅ | Common | 사용 중 |
| `phoenix_bow` | `Resources/Generated/GPT/Icons/Items/phoenix_bow.png` | ✅ | Rare | 사용 중 |
| `royal_halberd` | `Resources/Generated/GPT/Icons/Items/royal_halberd.png` | ✅ | Epic | 사용 중 |
| `scimitar` | `Resources/Generated/GPT/Icons/Items/scimitar.png` | ✅ | Rare | 사용 중 |
| `shadow_staff` | `Resources/Generated/GPT/Icons/Items/shadow_staff.png` | ✅ | Rare | 사용 중 |
| `spiked_mace` | `Resources/Generated/GPT/Icons/Items/spiked_mace.png` | ✅ | Rare | 사용 중 |
| `starlight_staff` | `Resources/Generated/GPT/Icons/Items/starlight_staff.png` | ✅ | Epic | 사용 중 |
| `steel_sword` | `Resources/Generated/GPT/Icons/Items/steel_sword.png` | ✅ | Common | 사용 중 |
| `twin_blades` | `Resources/Generated/GPT/Icons/Items/twin_blades.png` | ✅ | Rare | 사용 중 |
| `void_scepter` | `Resources/Generated/GPT/Icons/Items/void_scepter_v2.png` | ✅ | Epic | 사용 중 |
| `war_hammer` | `Resources/Generated/GPT/Icons/Items/war_hammer.png` | ✅ | Rare | 사용 중 |
| `wooden_club` | `Resources/Generated/GPT/Icons/Items/wooden_club.png` | ✅ | Common | 사용 중 |
| `wooden_staff` | `Resources/Generated/GPT/Icons/Items/wooden_staff.png` | ✅ | Common | 사용 중 |

#### 2026-05-15 신규 등록 — 해안·설원 확장 무기 8종

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Items/source_weapon_icons_coast_snow_2026_05_15.png` (1774×887, 4열×2행)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `cutlass` | `Icons/Items/cutlass.png` | `Resources/Items/cutlass.tres` | Uncommon | **사용 중** | 기존 `scimitar.png` → 전용 아이콘으로 교체 |
| `harpoon` | `Icons/Items/harpoon.png` | `Resources/Items/harpoon.tres` | Rare | **사용 중** | 기존 `iron_spear.png` → 전용 아이콘으로 교체 |
| `tide_staff` | `Icons/Items/tide_staff.png` | `Resources/Items/tide_staff.tres` | Rare | **사용 중** | 기존 `crystal_staff.png` → 전용 아이콘으로 교체 |
| `coral_bow` | `Icons/Items/coral_bow.png` | `Resources/Items/coral_bow.tres` | Rare | **사용 중** | 기존 `dragonbone_bow.png` → 전용 아이콘으로 교체 |
| `frost_blade` | `Icons/Items/frost_blade.png` | `Resources/Items/frost_blade.tres` | Rare | **사용 중** | 기존 `steel_sword.png` → 전용 아이콘으로 교체 |
| `winter_bow` | `Icons/Items/winter_bow.png` | `Resources/Items/winter_bow.tres` | Rare | **사용 중** | 기존 `composite_bow.png` → 전용 아이콘으로 교체 |
| `glacier_axe` | `Icons/Items/glacier_axe.png` | `Resources/Items/glacier_axe.tres` | Epic | **사용 중** | 기존 `great_axe.png` → 전용 아이콘으로 교체 |
| `frostfang_staff` | `Icons/Items/frostfang_staff.png` | `Resources/Items/frostfang_staff.tres` | Rare | **사용 중** | 기존 `frost_staff.png` → 전용 아이콘으로 교체 |

## Armor (몸통) (19개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `chainmail_armor` | `Resources/Generated/GPT/Icons/Equipment/chainmail_armor.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `dark_armor` | `Resources/Generated/GPT/Icons/Equipment/dark_armor.png` | ✅ | Epic | 사용 중 |
| `dark_robe` | `Resources/Generated/GPT/Icons/Equipment/dark_robe.png` | ✅ | Epic | 사용 중 |
| `dark_vest` | `Resources/Generated/GPT/Icons/Equipment/dark_vest.png` | ✅ | Epic | 사용 중 |
| `flame_armor` | `Resources/Generated/GPT/Icons/Equipment/flame_armor.png` | ✅ | Rare | 사용 중 |
| `frost_armor` | `Resources/Generated/GPT/Icons/Equipment/frost_armor.png` | ✅ | Rare | 사용 중 |
| `frost_robe` | `Resources/Generated/GPT/Icons/Equipment/frost_robe.png` | ✅ | Rare | 사용 중 |
| `frost_vest` | `Resources/Generated/GPT/Icons/Equipment/frost_vest.png` | ✅ | Rare | 사용 중 |
| `iron_armor` | `Resources/Generated/GPT/Icons/Items/iron_armor.png` | ✅ | Common | 사용 중 |
| `knight_plate_armor` | `Resources/Generated/GPT/Icons/Equipment/knight_plate_armor.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `leather_armor` | `Resources/Generated/GPT/Icons/Items/leather_armor.png` | ✅ | Common | 사용 중 |
| `mystic_robe` | `Resources/Generated/GPT/Icons/Equipment/mystic_robe.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `phoenix_vest` | `Resources/Generated/GPT/Icons/Equipment/phoenix_vest.png` | ✅ | Rare | 사용 중 |
| `ranger_vest` | `Resources/Generated/GPT/Icons/Equipment/ranger_vest.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `steel_armor` | `Resources/Generated/GPT/Icons/Items/steel_armor.png` | ✅ | Common | 사용 중 |
| `storm_armor` | `Resources/Generated/GPT/Icons/Equipment/storm_armor.png` | ✅ | Rare | 사용 중 |
| `storm_robe` | `Resources/Generated/GPT/Icons/Equipment/storm_robe.png` | ✅ | Rare | 사용 중 |
| `storm_vest` | `Resources/Generated/GPT/Icons/Equipment/storm_vest.png` | ✅ | Rare | 사용 중 |
| `wooden_shield` | `Resources/Generated/GPT/Icons/Items/wooden_shield.png` | ✅ | Common | 사용 중 |

## Helmets (11개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `battle_helm` | `Resources/Generated/GPT/Icons/Equipment/battle_helm.png` | ✅ | Epic | 사용 중 ⚠️**중복주의** |
| `dark_helm` | `Resources/Generated/GPT/Icons/Equipment/dark_helm.png` | ✅ | Epic | 사용 중 |
| `flame_helm` | `Resources/Generated/GPT/Icons/Equipment/flame_helm.png` | ✅ | Uncommon | 사용 중 |
| `frost_helm` | `Resources/Generated/GPT/Icons/Equipment/frost_helm.png` | ✅ | Uncommon | 사용 중 |
| `iron_helm` | `` | ✅ | Uncommon | 사용 중 |
| `knight_helm` | `Resources/Generated/GPT/Icons/Items/helmet.png` | ✅ | Common | 사용 중 |
| `leather_cap` | `Resources/Generated/GPT/Icons/Items/leather_cap.png` | ✅ | Common | 사용 중 |
| `ranger_hood` | `Resources/Generated/GPT/Icons/Equipment/ranger_hood.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `steel_helm` | `Resources/Generated/GPT/Icons/Equipment/steel_helm.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `storm_helm` | `Resources/Generated/GPT/Icons/Equipment/storm_helm.png` | ✅ | Uncommon | 사용 중 |
| `wizard_hat` | `Resources/Generated/GPT/Icons/Items/wizard_hat.png` | ✅ | Rare | 사용 중 |

## Boots (10개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `dark_boots` | `Resources/Generated/GPT/Icons/Equipment/dark_boots.png` | ✅ | Epic | 사용 중 |
| `flame_boots` | `Resources/Generated/GPT/Icons/Equipment/flame_boots.png` | ✅ | Rare | 사용 중 |
| `glacier_boots` | `Resources/Generated/GPT/Icons/Equipment/glacier_boots.png` | ✅ | Rare | 사용 중 |
| `iron_boots` | `Resources/Generated/GPT/Icons/Items/iron_boots.png` | ✅ | Uncommon | 사용 중 |
| `knight_boots` | `Resources/Generated/GPT/Icons/Equipment/knight_boots.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `leather_boots` | `Resources/Generated/GPT/Icons/Items/boots.png` | ✅ | Common | 사용 중 |
| `shadow_boots` | `Resources/Generated/GPT/Icons/Equipment/shadow_boots.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `storm_boots` | `Resources/Generated/GPT/Icons/Equipment/storm_boots.png` | ✅ | Rare | 사용 중 |
| `swift_boots` | `Resources/Generated/GPT/Icons/Equipment/swift_boots.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `traveler_boots` | `Resources/Generated/GPT/Icons/Items/traveler_boots.png` | ✅ | Rare | 사용 중 |

## Gloves / Belts / Cloaks (13개)

화염/서리/폭풍/암흑 세트로 신규 등록된 항목만 기재. 나머지 동일 카테고리 `.tres`는 본 문서 미수록(추후 일괄 등록 시 갱신).

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `flame_gloves` | `Resources/Generated/GPT/Icons/Equipment/flame_gloves.png` | ✅ | Rare | 사용 중 |
| `frost_gloves` | `Resources/Generated/GPT/Icons/Equipment/frost_gloves.png` | ✅ | Rare | 사용 중 |
| `storm_gloves` | `Resources/Generated/GPT/Icons/Equipment/storm_gloves.png` | ✅ | Rare | 사용 중 |
| `dark_gloves` | `Resources/Generated/GPT/Icons/Equipment/dark_gloves.png` | ✅ | Epic | 사용 중 |
| `flame_belt` | `Resources/Generated/GPT/Icons/Equipment/flame_belt.png` | ✅ | Rare | 사용 중 |
| `frost_belt` | `Resources/Generated/GPT/Icons/Equipment/frost_belt.png` | ✅ | Rare | 사용 중 |
| `storm_belt` | `Resources/Generated/GPT/Icons/Equipment/storm_belt.png` | ✅ | Rare | 사용 중 |
| `dark_belt` | `Resources/Generated/GPT/Icons/Equipment/dark_belt.png` | ✅ | Epic | 사용 중 |
| `flame_cloak` | `Resources/Generated/GPT/Icons/Equipment/flame_cloak.png` | ✅ | Rare | 사용 중 |
| `ember_cloak` | `Resources/Generated/GPT/Icons/Equipment/ember_cloak.png` | ✅ | Epic | 사용 중 |
| `frost_cloak` | `Resources/Generated/GPT/Icons/Equipment/frost_cloak.png` | ✅ | Epic | 사용 중 |
| `storm_cloak` | `Resources/Generated/GPT/Icons/Equipment/storm_cloak.png` | ✅ | Rare | 사용 중 |
| `dark_cloak` | `Resources/Generated/GPT/Icons/Equipment/dark_cloak.png` | ✅ | Epic | 사용 중 |

## Rings (6개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `amethyst_ring` | `Resources/Generated/GPT/Icons/Equipment/amethyst_ring.png` | ✅ | Rare | 사용 중 (2026-05-18 `magic_ring.png` 돌려쓰기 → 전용 교체) |
| `emerald_ring` | `Resources/Generated/GPT/Icons/Equipment/emerald_ring.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `iron_ring` | `Resources/Generated/GPT/Icons/Items/iron_ring.png` | ✅ | Common | 사용 중 |
| `ruby_ring` | `Resources/Generated/GPT/Icons/Equipment/ruby_ring.png` | ✅ | Epic | 사용 중 ⚠️**중복주의** |
| `sapphire_ring` | `Resources/Generated/GPT/Icons/Items/sapphire_ring.png` | ✅ | Uncommon | 사용 중 |
| `shadow_ring` | `Resources/Generated/GPT/Icons/Items/shadow_ring.png` | ✅ | Epic | 사용 중 |

## Necklaces (5개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `guardian_amulet` | `Resources/Generated/GPT/Icons/Items/guardian_amulet.png` | ✅ | Epic | 사용 중 |
| `moonstone_necklace` | `Resources/Generated/GPT/Icons/Equipment/moonstone_necklace.png` | ✅ | Epic | 사용 중 ⚠️**중복주의** |
| `sun_amulet` | `Resources/Generated/GPT/Icons/Equipment/sun_amulet.png` | ✅ | Epic | 사용 중 ⚠️**중복주의** |
| `tooth_necklace` | `Resources/Generated/GPT/Icons/Items/warrior_necklace.png` | ✅ | Common | 사용 중 |
| `wolf_necklace` | `Resources/Generated/GPT/Icons/Items/wolf_necklace.png` | ✅ | Uncommon | 사용 중 |

## Bracelets (6개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `bronze_bracelet` | `Resources/Generated/GPT/Icons/Items/bracelet.png` | ✅ | Common | 사용 중 |
| `leather_bracelet` | `Resources/Generated/GPT/Icons/Items/leather_bracelet.png` | ✅ | Common | 사용 중 |
| `mystic_bracelet` | `Resources/Generated/GPT/Icons/Equipment/mystic_bracelet.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `runic_bracelet` | `Resources/Generated/GPT/Icons/Equipment/runic_bracelet.png` | ✅ | Epic | 사용 중 (2026-05-18 Icon 미지정 → 전용 신규 연결) |
| `silver_bracelet` | `Resources/Generated/GPT/Icons/Equipment/silver_bracelet.png` | ✅ | Rare | 사용 중 (2026-05-18 Icon 미지정 → 전용 신규 연결) |
| `warrior_bracelet` | `Resources/Generated/GPT/Icons/Equipment/warrior_bracelet.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |

## Skill Icons (16개)

`Resources/Generated/GPT/Icons/Skills/` — SkillData.Icon에 연결되는 스킬 아이콘.

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Icons/source_skill_icons_combat_set_2026_05_15.png` (1774×887, 4열×2행)

| asset id | sliced PNG | SkillData `.tres` | 상태 |
|---|---|---|---|
| `lightning_storm` | `Icons/Skills/lightning_storm.png` | `Resources/Skills/lightning_storm.tres` | **사용 중** |
| `precise_aim` | `Icons/Skills/precise_aim.png` | `Resources/Skills/precise_aim.tres` | **사용 중** |
| `iron_stance` | `Icons/Skills/iron_stance.png` | `Resources/Skills/iron_stance.tres` | **사용 중** |
| `arrow_shot` | `Icons/Skills/arrow_shot.png` | `Resources/Skills/arrow_shot.tres` | **사용 중** |
| `multi_shot` | `Icons/Skills/multi_shot.png` | `Resources/Skills/multi_shot.tres` | **사용 중** |
| `ice_shard` | `Icons/Skills/ice_shard.png` | `Resources/Skills/ice_shard.tres` | **사용 중** |
| `whirlwind` | `Icons/Skills/whirlwind.png` | `Resources/Skills/whirlwind.tres` | **사용 중** |
| `lifesteal_passive` | `Icons/Skills/lifesteal_passive.png` | `Resources/Skills/lifesteal_passive.tres` | **사용 중** |

#### 2026-05-15 코어 리프레시 (8개)

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Icons/source_skill_icons_core_refresh_2026_05_15.png` (1774×887, 4열×2행)

| asset id | sliced PNG | SkillData `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|
| `fire_bolt_v2` | `Icons/Skills/fire_bolt_v2.png` | `Resources/Skills/fire_bolt.tres` | **사용 중** | 기존 `fire_bolt.png` → v2로 교체 |
| `power_strike_v2` | `Icons/Skills/power_strike_v2.png` | `Resources/Skills/power_strike.tres` | **사용 중** | 기존 `power_strike.png` → v2로 교체 |
| `heal_self_v2` | `Icons/Skills/heal_self_v2.png` | `Resources/Skills/heal_self.tres` | **사용 중** | 기존 `heal_self.png` → v2로 교체 |
| `dash_v2` | `Icons/Skills/dash_v2.png` | `Resources/Skills/dash.tres` | **사용 중** | 기존 `dash.png` → v2로 교체 |
| `hp_regen_passive` | `Icons/Skills/hp_regen_passive.png` | `Resources/Skills/hp_regen_passive.tres` | **사용 중** | 기존 `heal_self.png` 재사용 → 전용 아이콘으로 교체 |
| `crit_boost_passive` | `Icons/Skills/crit_boost_passive.png` | `Resources/Skills/crit_boost_passive.tres` | **사용 중** | 기존 `heal_self.png` 재사용 → 전용 아이콘으로 교체 |
| `speed_boost_passive` | `Icons/Skills/speed_boost_passive.png` | `Resources/Skills/speed_boost_passive.tres` | **사용 중** | 기존 `dash.png` 재사용 → 전용 아이콘으로 교체 |
| `generic_passive_placeholder` | `Icons/Skills/generic_passive_placeholder.png` | (연결 없음) | 리소스만 등록 | 추후 신규 패시브 스킬 임시 아이콘용 |

## Skill Books (19개)

`Resources/Generated/GPT/Icons/Items/skillbook_*.png` — ItemData.Icon에 연결.

#### 기존 4개 코어 리프레시 (2026-05-15)

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Items/source_skillbook_icons_core_refresh_2026_05_15.png` (1774×887, 4열×2행)

| asset id | sliced PNG | ItemData `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|
| `skillbook_fire_bolt_v2` | `Icons/Items/skillbook_fire_bolt_v2.png` | `Resources/Items/skillbook_fire_bolt.tres` | **사용 중** | 기존 `skillbook_fire_bolt.png` → v2로 교체 |
| `skillbook_power_strike_v2` | `Icons/Items/skillbook_power_strike_v2.png` | `Resources/Items/skillbook_power_strike.tres` | **사용 중** | 기존 `skillbook_power_strike.png` → v2로 교체 |
| `skillbook_heal_v2` | `Icons/Items/skillbook_heal_v2.png` | `Resources/Items/skillbook_heal.tres` | **사용 중** | 기존 `skillbook_heal.png` → v2로 교체 |
| `skillbook_dash_v2` | `Icons/Items/skillbook_dash_v2.png` | `Resources/Items/skillbook_dash.tres` | **사용 중** | 기존 `skillbook_dash.png` → v2로 교체 |
| `skillbook_hp_regen` | `Icons/Items/skillbook_hp_regen.png` | `Resources/Items/skillbook_hp_regen.tres` | **사용 중** | 기존 `skillbook_heal.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_crit_boost` | `Icons/Items/skillbook_crit_boost.png` | `Resources/Items/skillbook_crit_boost.tres` | **사용 중** | 기존 `skillbook_power_strike.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_speed_boost` | `Icons/Items/skillbook_speed_boost.png` | `Resources/Items/skillbook_speed_boost.tres` | **사용 중** | 기존 `skillbook_dash.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_generic_passive` | `Icons/Items/skillbook_generic_passive.png` | (연결 없음) | 리소스만 등록 | 추후 신규 패시브 스킬북 임시 아이콘용 |

#### 2026-05-15 신규 등록 (8개 — 스킬북 확장 세트)

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Items/source_skillbook_icons_expanded_set_2026_05_15.png` (1774×887, 4열×2행)

| asset id | sliced PNG | ItemData `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|
| `skillbook_lightning_storm` | `Icons/Items/skillbook_lightning_storm.png` | `Resources/Items/skillbook_lightning_storm.tres` | **사용 중** | 기존 `skillbook_fire_bolt.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_precise_aim` | `Icons/Items/skillbook_precise_aim.png` | `Resources/Items/skillbook_precise_aim.tres` | **사용 중** | 기존 `skillbook_fire_bolt.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_iron_stance` | `Icons/Items/skillbook_iron_stance.png` | `Resources/Items/skillbook_iron_stance.tres` | **사용 중** | 기존 `skillbook_fire_bolt.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_arrow_shot` | `Icons/Items/skillbook_arrow_shot.png` | (연결 없음 — `.tres` 미존재) | 리소스만 등록 | 궁수 기본 스킬 arrow_shot은 시작 지급 방식 — 별도 스킬북 불필요 |
| `skillbook_multi_shot` | `Icons/Items/skillbook_multi_shot.png` | `Resources/Items/skillbook_multi_shot.tres` | **사용 중** | 기존 `skillbook_fire_bolt.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_ice_shard` | `Icons/Items/skillbook_ice_shard.png` | `Resources/Items/skillbook_ice_shard.tres` | **사용 중** | 기존 `skillbook_fire_bolt.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_whirlwind` | `Icons/Items/skillbook_whirlwind.png` | `Resources/Items/skillbook_whirlwind.tres` | **사용 중** | 기존 `skillbook_power_strike.png` 재사용 → 전용 아이콘으로 교체 |
| `skillbook_lifesteal` | `Icons/Items/skillbook_lifesteal.png` | `Resources/Items/skillbook_lifesteal.tres` | **사용 중** | 기존 `skillbook_heal.png` 재사용 → 전용 아이콘으로 교체 |

---

## UI 아이콘 (16개)

`Resources/Generated/GPT/Icons/UI/` 위치. ItemData 없음 — HUD/인벤토리/필터/메뉴 UI에서 직접 참조.

| asset id | 상태 |
|---|---|
| `all_items_filter.png` | 사용 중 |
| `attack.png` | 사용 중 |
| `blacksmith_enhance.png` | 사용 중 |
| `character_status.png` | 사용 중 |
| `close.png` | 사용 중 |
| `consumable_filter.png` | 사용 중 |
| `equipment_filter.png` | 사용 중 |
| `interact.png` | 사용 중 |
| `inventory.png` | 사용 중 |
| `locked_slot.png` | 사용 중 |
| `material_filter.png` | 사용 중 |
| `quick_slot.png` | 사용 중 |
| `settings.png` | 사용 중 |
| `shop.png` | 사용 중 |
| `skill_shop.png` | 사용 중 |
| `skill_window.png` | 사용 중 |

## Interaction 아이콘 (8개)

`Resources/Generated/GPT/Icons/Interaction/` 위치. 포털·NPC 대화·퀘스트·보스 경고 등 상호작용 프롬프트용.

| asset id | 상태 |
|---|---|
| `boss_warning.png` | 사용 중 |
| `dungeon_entrance.png` | 사용 중 |
| `npc_dialogue.png` | 사용 중 |
| `portal.png` | 사용 중 |
| `quest.png` | 사용 중 |
| `save_point.png` | 사용 중 |
| `town_return.png` | 사용 중 |
| `treasure_chest.png` | 사용 중 |

## Enemies (Generated PNG)

`Resources/Generated/GPT/Enemies/` 하위. 각 `EnemyStats.tres`는 `Resources/Enemies/`에 있고 [enemy-zone-plan.md](enemy-zone-plan.md)에서 맵별 배치 상태 추적. 여기서는 PNG asset id만 나열.

### Field1 (8개)

`forest_spider`, `forest_spirit`, `goblin_scout`, `hobgoblin_guard`, `orc_scout`, `slime`, `wild_boar`, `wild_wolf`

### Field2 (8개)

`bone_soldier`, `cursed_wolf`, `ghoul`, `grave_slime`, `grave_wraith`, `skeleton_archer`, `skeleton_wanderer`, `zombie_walker`

### Field3 (8개)

`bone_hound`, `cursed_banner_wraith`, `cursed_soldier`, `dark_wolf`, `fallen_knight`, `plague_ghoul`, `ruin_golem`, `shadow_bat`

### Mine (12개)

기존 8종: `cave_bat`, `mine_wraith`, `rock_golem`, `skeleton_miner`, `zombie_armored`, `zombie_basic`, `zombie_brute`, `zombie_fast`

mine_3 신규 4종 (2026-05-15, `Resources/Generated/GPT/SourceSheets/Enemies/source_enemies_mine3_crystal_final_2026_05_15.png`, 2×2): `crystal_archer`, `crystal_warlock`, `crystal_brute`, `corrupted_miner`

| asset id | PNG | `.tres` | 상태 |
|---|---|---|---|
| `crystal_archer` | `Enemies/Mine/crystal_archer.png` | `Resources/Enemies/mine_crystal_archer.tres` | 사용 중 |
| `crystal_warlock` | `Enemies/Mine/crystal_warlock.png` | `Resources/Enemies/mine_crystal_warlock.tres` | 사용 중 |
| `crystal_brute` | `Enemies/Mine/crystal_brute.png` | `Resources/Enemies/mine_crystal_brute.tres` | 사용 중 |
| `corrupted_miner` | `Enemies/Mine/corrupted_miner.png` | `Resources/Enemies/mine_corrupted_miner.tres` | 사용 중 |

### Dungeon1 (8개)

`goblin_trapper`, `orc_axe_warrior`, `orc_brute`, `orc_captain`, `orc_club`, `orc_rogue`, `orc_shaman`, `orc_warlord_boss`

### Dungeon2 (8개)

`bone_archer`, `bone_knight`, `crypt_wraith`, `ghoul_brute`, `skeleton_champion`, `skeleton_mage`, `skeleton_rogue`, `skeleton_warrior`

### Dungeon3 (8개)

`abyss_hound`, `abyss_wraith`, `ancient_lich`, `bone_golem`, `cursed_warlock`, `death_knight`, `dungeon_guardian`, `shadow_assassin`

### Field4 + Dungeon4 (항구·해안) (8개) — 2026-05-15 신규

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Enemies/source_enemies_field4_dungeon4_coast_2026_05_15.png` (1774×887, 4열×2행)

| asset id | category | sliced PNG | `.tres` 연결 | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `pirate_grunt` | Field4 | `Enemies/Field4/pirate_grunt.png` | `Resources/Enemies/field4_pirate_grunt.tres` | **사용 중** | 기존 `Dungeon1/orc_club.png` → 전용 이미지로 교체 |
| `pirate_brute` | Field4 | `Enemies/Field4/pirate_brute.png` | `Resources/Enemies/field4_pirate_brute.tres` | **사용 중** | 기존 `Dungeon1/orc_brute.png` → 전용 이미지로 교체 |
| `pirate_sniper` | Field4 | `Enemies/Field4/pirate_sniper.png` | `Resources/Enemies/field4_pirate_sniper.tres` | **사용 중** | 기존 `Dungeon1/orc_rogue.png` → 전용 이미지로 교체 |
| `giant_crab` | Field4 | `Enemies/Field4/giant_crab.png` | `Resources/Enemies/field4_giant_crab.tres` | **사용 중** | 기존 `Field3/ruin_golem.png` → 전용 이미지로 교체 |
| `seagull_swarm` | Field4 | `Enemies/Field4/seagull_swarm.png` | `Resources/Enemies/field4_seagull_swarm.tres` | **사용 중** | 기존 `Field1/forest_spider.png` → 전용 이미지로 교체 |
| `drowned_sailor` | Dungeon4 | `Enemies/Dungeon4/drowned_sailor.png` | `Resources/Enemies/dungeon4_drowned_sailor.tres` | **사용 중** | 기존 `Field2/zombie_walker.png` → 전용 이미지로 교체 |
| `siren` | Dungeon4 | `Enemies/Dungeon4/siren.png` | `Resources/Enemies/dungeon4_siren.tres` | **사용 중** | 기존 `Dungeon2/skeleton_mage.png` → 전용 이미지로 교체 |
| `deep_lurker` | Dungeon4 | `Enemies/Dungeon4/deep_lurker.png` | `Resources/Enemies/dungeon4_deep_lurker.tres` | **사용 중** | 기존 `Field3/shadow_bat.png` → 전용 이미지로 교체 |

### Dungeon4 추가 (1개) + Field5 설원 (7개) — 2026-05-15 신규

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Enemies/source_enemies_dungeon4_field5_snow_2026_05_15.png` (1774×887, 4열×2행)

| asset id | category | sliced PNG | `.tres` 연결 | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `coral_golem` | Dungeon4 | `Enemies/Dungeon4/coral_golem.png` | `Resources/Enemies/dungeon4_coral_golem.tres` | **사용 중** | 기존 `Dungeon3/bone_golem.png` → 전용 이미지로 교체 |
| `frost_wolf` | Field5 | `Enemies/Field5/frost_wolf.png` | `Resources/Enemies/field5_frost_wolf.tres` | **사용 중** | 기존 `Field1/wild_wolf.png` → 전용 이미지로 교체 |
| `yeti` | Field5 | `Enemies/Field5/yeti.png` | `Resources/Enemies/field5_yeti.tres` | **사용 중** | 기존 `Field3/ruin_golem.png` → 전용 이미지로 교체 |
| `ice_imp` | Field5 | `Enemies/Field5/ice_imp.png` | `Resources/Enemies/field5_ice_imp.tres` | **사용 중** | 기존 `Field1/goblin_scout.png` → 전용 이미지로 교체 |
| `snow_witch` | Field5 | `Enemies/Field5/snow_witch.png` | `Resources/Enemies/field5_snow_witch.tres` | **사용 중** | 기존 `Dungeon2/skeleton_mage.png` → 전용 이미지로 교체 |
| `polar_bear` | Field5 | `Enemies/Field5/polar_bear.png` | `Resources/Enemies/field5_polar_bear.tres` | **사용 중** | 기존 `Field1/wild_boar.png` → 전용 이미지로 교체 |
| `frost_archer` | Field5 | `Enemies/Field5/frost_archer.png` | `Resources/Enemies/field5_frost_archer.tres` | **사용 중** | 기존 `Field2/skeleton_archer.png` → 전용 이미지로 교체 |
| `icicle_elemental` | Field5 | `Enemies/Field5/icicle_elemental.png` | `Resources/Enemies/field5_icicle_elemental.tres` | **사용 중** | 기존 `Dungeon3/abyss_wraith.png` → 전용 이미지로 교체 |

### Field6 화산 (7개) + Mine3 수정 광산 추가 (1개) — 2026-05-15 신규

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Enemies/source_enemies_field6_mine3_volcanic_crystal_2026_05_15.png` (1774×887, 4열×2행)

| asset id | category | sliced PNG | `.tres` 연결 | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `lava_slime` | Field6 | `Enemies/Field6/lava_slime.png` | `Resources/Enemies/field6_lava_slime.tres` | **사용 중** | 기존 `Field2/grave_slime.png` → 전용 이미지로 교체 |
| `fire_imp` | Field6 | `Enemies/Field6/fire_imp.png` | `Resources/Enemies/field6_fire_imp.tres` | **사용 중** | 기존 `Field1/goblin_scout.png` → 전용 이미지로 교체 |
| `salamander` | Field6 | `Enemies/Field6/salamander.png` | `Resources/Enemies/field6_salamander.tres` | **사용 중** | 기존 `Field1/forest_spider.png` → 전용 이미지로 교체 |
| `magma_golem` | Field6 | `Enemies/Field6/magma_golem.png` | `Resources/Enemies/field6_magma_golem.tres` | **사용 중** | 기존 `Field3/ruin_golem.png` → 전용 이미지로 교체 |
| `phoenix_chick` | Field6 | `Enemies/Field6/phoenix_chick.png` | `Resources/Enemies/field6_phoenix_chick.tres` | **사용 중** | 기존 `Field1/forest_spirit.png` → 전용 이미지로 교체 |
| `ember_archer` | Field6 | `Enemies/Field6/ember_archer.png` | `Resources/Enemies/field6_ember_archer.tres` | **사용 중** | 기존 `Field2/skeleton_archer.png` → 전용 이미지로 교체 |
| `lava_serpent` | Field6 | `Enemies/Field6/lava_serpent.png` | `Resources/Enemies/field6_lava_serpent.tres` | **사용 중** | 기존 `Field2/cursed_wolf.png` → 전용 이미지로 교체 |
| `crystal_grunt` | Mine3 | `Enemies/Mine/crystal_grunt.png` | `Resources/Enemies/mine_crystal_grunt.tres` | **사용 중** | 기존 `Mine/zombie_basic.png` → 전용 이미지로 교체 |

### NamedBosses (12개)

`ancient_lich`, `crystal_guardian`, `forest_alpha_wolf`, `graveyard_wight`, `mine_golem`, `orc_warlord`, `plague_brute`, `skeleton_king`

#### 2026-05-15 신규 등록 (4개 — 보스 교체 시트)

**소스 시트**: `Resources/Generated/GPT/SourceSheets/Enemies/source_enemies_boss_replacements_2026_05_15.png`

| asset id | category | sliced PNG | `.tres` 연결 | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `kraken` | NamedBoss | `Enemies/NamedBosses/kraken.png` | `Resources/Enemies/boss_dungeon4_kraken.tres` | **사용 중** | 기존 `ancient_lich.png` 재사용 스프라이트 → 전용 이미지로 교체 |
| `glacier_titan` | NamedBoss | `Enemies/NamedBosses/glacier_titan.png` | `Resources/Enemies/field5_named_glacier_titan.tres` | **사용 중** | 기존 `plague_brute.png` 재사용 스프라이트 → 전용 이미지로 교체 |
| `inferno_drake` | NamedBoss | `Enemies/NamedBosses/inferno_drake.png` | `Resources/Enemies/field6_named_inferno_drake.tres` | **사용 중** | 기존 `graveyard_wight.png` 재사용 스프라이트 → 전용 이미지로 교체 |
| `crystal_lord` | NamedBoss | `Enemies/NamedBosses/crystal_lord.png` | `Resources/Enemies/boss_mine3_crystal_lord.tres` | **사용 중** | 기존 `crystal_guardian.png` 재사용 스프라이트 → 전용 이미지로 교체 |

## NPCs (Generated PNG) (8개)

`Resources/Generated/GPT/NPCs/Town/`. 씬 매핑은 [generated-npc-inventory.md](generated-npc-inventory.md) 참조.

| asset id | 씬 매핑 | 상태 |
|---|---|---|
| `blacksmith.png` | `blacksmith_npc.tscn` | 사용 중 |
| `general_merchant.png` | `shop_npc.tscn` | 사용 중 |
| `healer.png` | `(미구현)` | 리소스만 등록 |
| `mining_foreman.png` | `(미구현)` | 리소스만 등록 |
| `quest_elder.png` | `(미구현)` | 리소스만 등록 |
| `skill_master.png` | `skill_shop_npc.tscn` | 사용 중 |
| `storage_keeper.png` | `Scenes/Objects/storage_npc.tscn` (town 창고 NPC) | 사용 중 (hub-loop v1) |
| `teleport_guide.png` | `teleport_npc.tscn` | 사용 중 |

## Unused / 보관 전용

### Shields (방패 시스템 미도입)

`Resources/Generated/GPT/Icons/Unused/Shields/` — ItemData 미생성 (방패 슬롯/타입 없음).

`iron_shield.png`, `ranger_buckler.png`, `royal_guard_shield.png`, `tower_shield.png`

### 갑옷 중복 보관

`Resources/Generated/GPT/Icons/Unused/Duplicates/` — `09_54_01` 갑옷 시트와 중복으로 분리한 백업본. ItemData 미생성.

`dup_chainmail_armor.png`, `dup_knight_plate_armor.png`, `dup_mystic_robe.png`, `dup_ranger_vest.png`

## World Objects (Map Interactables)

`Resources/Generated/GPT/Objects/World/` — 맵에 배치되는 상호작용 오브젝트. ItemData가 아니라 `.tscn` 씬 안 Sprite로 사용.

**시트**: `Resources/Generated/GPT/SourceSheets/Objects/source_world_interactables_2026_05_13.png`

| asset id | category | sliced PNG | 씬 연결 | 상태 | 보류 사유 |
|---|---|---|---|---|---|
| `save_crystal_shrine` | Save | `Objects/World/save_crystal_shrine.png` | `Scenes/Objects/save_point.tscn` (Sprite2D texture) | **사용 중** | — |
| `magic_portal` | Portal | `Objects/World/magic_portal.png` | `Scenes/Objects/portal.tscn` (Visual ColorRect → Sprite2D) | **사용 중** | — |
| `dungeon_entrance_gate` | Map Transition | `Objects/World/dungeon_entrance_gate.png` | (씬 없음) | 리소스만 등록 | 던전 입구 전용 씬 미구현 — 현재는 일반 Portal 사용 |
| `quest_notice_board` | Quest | `Objects/World/quest_notice_board.png` | `Scenes/Objects/contract_board_npc.tscn` (Sprite2D, 허브 4곳) | **사용 중** (사냥 계약 v1, 2026-05-18) | 사냥 계약 보드 NPC 스프라이트 |
| `treasure_chest_closed` | Loot | `Objects/World/treasure_chest_closed.png` | (씬 없음) | 미구현 보류 | 보물상자 시스템 미도입 |
| `treasure_chest_open` | Loot | `Objects/World/treasure_chest_open.png` | (씬 없음) | 미구현 보류 | 상자 open state 미구현 |
| `mining_ore_vein` | Mining | `Objects/World/mining_ore_vein.png` | (씬 미연결) | 중복보류 | `MiningNode.cs`가 `OreItem.Icon`을 런타임에 Sprite로 사용 중 — 광맥 일괄 그래픽으로 교체하면 ore별 구분 정보 손실 |
| `breakable_wooden_crate` | Breakable | `Objects/World/breakable_wooden_crate.png` | (씬 없음) | 미구현 보류 | 파괴 가능 오브젝트 시스템 미도입 |

이번 작업으로 새 시스템(보물상자/게시판/파괴 가능 상자)은 추가하지 않음 — PNG 슬라이싱 + SavePoint/Portal 스프라이트 교체만.

## Combat Effects

`Resources/Generated/GPT/Effects/Combat/` — 전투/스킬 시각 이펙트. 현재 게임 코드에 sprite-based 이펙트 spawn 시스템이 없어 모두 리소스만 등록. 추후 EffectSpawner 같은 헬퍼 추가 시 연결 예정.

**시트**: `Resources/Generated/GPT/SourceSheets/Effects/source_combat_effects_2026_05_13.png`

| asset id | sliced PNG | 상태 | 후보 연결 위치 / 보류 사유 |
|---|---|---|---|
| `sword_slash_arc` | `Effects/Combat/sword_slash_arc.png` | 리소스만 등록 | basic attack / PowerStrike 휘두를 때 — 현재 `PlayerController.Combat.cs`는 attack 트리거만 있고 비주얼 이펙트 spawn 없음 |
| `heavy_impact_spark` | `Effects/Combat/heavy_impact_spark.png` | 리소스만 등록 | enemy hit feedback — 현재 데미지 적용 시 Sprite2D spawn 안 함, 보류 |
| `fire_bolt_impact` | `Effects/Combat/fire_bolt_impact.png` | 미구현 보류 | FireBolt 스킬 구현 자체가 코드 검색에서 발견 안 됨 |
| `healing_glow` | `Effects/Combat/healing_glow.png` | 미구현 보류 | HealSelf 스킬 구현 미발견 |
| `dash_trail` | `Effects/Combat/dash_trail.png` | 리소스만 등록 | `PlayerController.Movement.cs`의 Dash는 속도 배율만 적용, 시각 trail 미구현 |
| `poison_puff` | `Effects/Combat/poison_puff.png` | 미구현 보류 | 상태이상 시스템 미도입 |
| `level_up_sparkle` | `Effects/Combat/level_up_sparkle.png` | 미구현 보류 | 레벨업 표시 이펙트 시스템 미도입 |
| `item_pickup_glow` | `Effects/Combat/item_pickup_glow.png` | 미구현 보류 | 아이템 픽업 시 이펙트 spawn 시스템 미도입 |

이번 작업으로 새 EffectSpawner/SkillEffectPool 등은 만들지 않았다. PNG 슬라이싱만 완료.

## Projectiles

`Resources/Generated/GPT/Projectiles/` — 투사체 스프라이트 8종. 모든 이미지는 오른쪽 방향 기준 — 런타임에서 `Rotation = Direction.Angle()`로 방향 처리.

**연결 구조 (최소 변경 적용 완료):**
- `EnemyStats`: `ProjectileTexture` / `ProjectileScale` Export 추가 (`Scripts/Data/EnemyStats.cs:44~48`)
- `EnemyProjectile`: `Texture` 지정 시 Sprite2D 자식 추가 + `Rotation = Direction.Angle()`. `Texture == null`이면 기존 `_Draw()` DrawCircle 폴백 유지 → **하위호환**
- `EnemyController.SpawnProjectile`: `Stats.ProjectileTexture`가 있으면 투사체에 전달
- `FireBoltStrategy`는 여전히 히트스캔 — 플레이어 투사체 시스템은 별개라 미연결

**시트**: `Resources/Generated/GPT/SourceSheets/Projectiles/source_projectiles_2026_05_13.png`
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Projectiles/slice_projectiles.py` (4×2, 96×96 canvas, padding 50)

| asset id | sliced PNG | 상태 | 연결 적 / 보류 사유 |
|---|---|---|---|
| `fire_bolt_projectile` | `Projectiles/fire_bolt_projectile.png` | 리소스만 등록 | 불 속성 적 부재. FireBolt 스킬은 히트스캔이라 미연결 |
| `ice_shard_projectile` | `Projectiles/ice_shard_projectile.png` | 리소스만 등록 | 얼음 속성 적 부재 |
| `arcane_bolt_projectile` | `Projectiles/arcane_bolt_projectile.png` | **사용 중** | `skeleton_mage`, `dungeon2_skeleton_mage` (scale 0.2) |
| `poison_glob_projectile` | `Projectiles/poison_glob_projectile.png` | **사용 중** | `orc_shaman`, `dungeon1_orc_shaman` (scale 0.2) |
| `bone_arrow_projectile` | `Projectiles/bone_arrow_projectile.png` | **사용 중** | `field2_skeleton_archer`, `dungeon2_bone_archer` (scale 0.2) |
| `dark_orb_projectile` | `Projectiles/dark_orb_projectile.png` | **사용 중** | `dungeon3_cursed_warlock`, `dungeon3_ancient_lich` (scale 0.2) / `dungeon3_ancient_lich_boss`, `boss_dungeon3_ancient_lich` (scale 0.3, 보스 강조) |
| `stone_shard_projectile` | `Projectiles/stone_shard_projectile.png` | 리소스만 등록 | ranged 골렘 부재 (`ruin_golem` 등은 근접) |
| `holy_spark_projectile` | `Projectiles/holy_spark_projectile.png` | 리소스만 등록 | 신성/회복 발사체 사용 적 부재 |

**ranged 적 ↔ 텍스처 매핑 (10마리, 모두 `Behavior = 1` Ranged)**:
- `field2_skeleton_archer`, `dungeon2_bone_archer` → bone_arrow (궁수)
- `skeleton_mage`, `dungeon2_skeleton_mage` → arcane_bolt (마법사)
- `orc_shaman`, `dungeon1_orc_shaman` → poison_glob (샤먼)
- `dungeon3_cursed_warlock`, `dungeon3_ancient_lich`, `dungeon3_ancient_lich_boss`, `boss_dungeon3_ancient_lich` → dark_orb (어둠/리치)

미연결 4종(fire/ice/stone/holy)은 후속 적 추가 시 .tres에서 텍스처만 지정하면 즉시 활성화.

## Loot Objects

`Resources/Generated/GPT/Objects/Loot/` — 드랍/전리품/보상 월드 오브젝트 8종. 게임화폐 바닥 드롭(small/large_gold_pile)이 함께 도입됨.

**시트**: `Resources/Generated/GPT/SourceSheets/Objects/source_loot_reward_objects_2026_05_13.png`
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Objects/slice_loot_reward.py` (4×2, 96×96 canvas, padding 50)

**골드 드롭 시스템 (신규)**
- `Scripts/Objects/GoldPickup.cs` — Area2D 기반. `Amount` Export. `Amount >= LargeThreshold(50)`이면 `large_gold_pile`, 아니면 `small_gold_pile`로 자동 분기. FieldItem 픽업 패턴(통통튀기 + 자석) 복제, ItemData 미사용
- `Scenes/Objects/gold_pickup.tscn` — small/large 텍스처 모두 ext_resource 보유
- `EnemyController.Die`: 일반 적은 `SpawnGoldDrop(goldAmount)` → 바닥 드롭. 보스는 트랜잭션 일관성을 위해 직접 `PlayerGold +=` + 즉시 `UIEffectManager.SpawnGoldLabel` 표시
- `FloatingLabel.InitGold(int amount)` + `UIEffectManager.SpawnGoldLabel(worldPos, amount)` 추가 — 금색 "+amount G" 표시 (font_color = 1, 0.85, 0.2)

| asset id | sliced PNG | 상태 | 연결 위치 / 보류 사유 |
|---|---|---|---|
| `small_gold_pile` | `Objects/Loot/small_gold_pile.png` | **사용 중** | `gold_pickup.tscn` SmallTexture — Amount < 50 |
| `large_gold_pile` | `Objects/Loot/large_gold_pile.png` | **사용 중** | `gold_pickup.tscn` LargeTexture — Amount ≥ 50 |
| `loot_pouch` | `Objects/Loot/loot_pouch.png` | 리소스만 등록 | 일반 드랍 외형 후보. FieldItem이 ItemData.Icon만 표시 — 추후 외형 통일 시 사용 가능 |
| `rare_loot_glow` | `Objects/Loot/rare_loot_glow.png` | 리소스만 등록 | 희귀도별 드랍 마커 시스템 부재 — FieldItem에 rarity 글로우 오버레이 시스템 추가 시 후속 연결 |
| `epic_loot_glow` | `Objects/Loot/epic_loot_glow.png` | 리소스만 등록 | 동일 — 에픽 등급 마커 |
| `boss_reward_chest` | `Objects/Loot/boss_reward_chest.png` | 리소스만 등록 | 보스 보상 상자 시스템 부재. 현재 `EnemyController.Die`의 보스 처리는 인벤토리 직접 지급 + PendingReward fallback — 상자 매개 단계 없음. 기존 `treasure_chest_closed/open`과 별도 관리 |
| `broken_monster_trophy` | `Objects/Loot/broken_monster_trophy.png` | 리소스만 등록 | 보스/네임드 보상 연출 후보. 사망 위치에 잠시 남는 트로피 등 — 연출 시스템 미구현 |
| `sparkling_item_drop` | `Objects/Loot/sparkling_item_drop.png` | 리소스만 등록 | 반짝이는 일반 드랍 후보. FieldItem.tscn에 sparkle 오버레이 시스템 추가 시 연결 |

## Town Service Props

`Resources/Generated/GPT/Objects/Town/` — 마을 서비스(상점/대장간/스킬샵/창고/치유) 장식 소품 8종. **새 기능을 만들지 않고** `town.tscn`에 순수 Sprite2D 장식으로만 배치. CollisionShape2D/Area2D 미생성, NPC 씬은 미수정.

**시트**: `Resources/Generated/GPT/SourceSheets/Objects/source_town_service_props_2026_05_13.png`
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Objects/slice_town_service_props.py` (4×2, 128×128 canvas, padding 50)

**배치 노드**: `Scenes/Maps/town.tscn` → `Decorations` (Node2D, z_index = -1) 자식. 모두 scale 0.3, texture_filter = 1(Nearest).

| asset id | sliced PNG | 상태 | 연결 위치 / 보류 사유 |
|---|---|---|---|
| `shop_sign` | `Objects/Town/shop_sign.png` | **사용 중** | ShopNPC(320,80) 위 (320,35) 장식 |
| `potion_shelf` | `Objects/Town/potion_shelf.png` | **사용 중** | ShopNPC 우측 (370,78) 장식 |
| `blacksmith_forge` | `Objects/Town/blacksmith_forge.png` | **사용 중** | BlacksmithNPC(120,240) 우측 (180,215) 장식 |
| `anvil_workstation` | `Objects/Town/anvil_workstation.png` | **사용 중** | BlacksmithNPC 아래 (120,295) 장식 |
| `weapon_rack` | `Objects/Town/weapon_rack.png` | **사용 중** | BlacksmithNPC 좌측 (60,215) 장식 |
| `magic_book_stand` | `Objects/Town/magic_book_stand.png` | **사용 중** | SkillShopNPC(520,80) 좌측 (475,78) 장식 |
| `storage_crates_stack` | `Objects/Town/storage_crates_stack.png` | 리소스만 등록(미사용 장식 후보) | 창고 기능 자체는 hub-loop v1에서 `storage_keeper` NPC(`storage_npc.tscn`)로 구현됨. 이 PNG는 아직 미배치 장식 에셋 — town 창고 주변 데코 후보 |
| `healer_shrine` | `Objects/Town/healer_shrine.png` | 리소스만 등록 | Healer(치유사) NPC/기능 미구현 — 회복은 상점 포션만으로 처리 중. 도입 시 신전 형 NPC와 함께 배치 후보 |

## Village Decorations

`Resources/Generated/GPT/Objects/Village/` — 마을/필드 분위기용 일반 장식 8종. **기능 추가 없음**, `town.tscn`의 기존 `Decorations` 노드에 순수 Sprite2D로만 일부 배치. Collision/Area2D 미생성. Town Service Props(상점 기능 소품)와는 별 카테고리 — 겹치는 항목 없음.

**시트**: `Resources/Generated/GPT/SourceSheets/Objects/source_village_decorations_2026_05_14.png`
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Objects/slice_village_decorations.py` (4×2, 128×128 canvas, padding 50)

**배치 노드**: `Scenes/Maps/town.tscn` → `Decorations` (Node2D, z_index = -1) 자식. 모두 scale 0.3, texture_filter = 1(Nearest).

| asset id | sliced PNG | `.tres` | 상태 | 연결 위치 / 보류 사유 |
|---|---|---|---|---|
| `water_well` | `Objects/Village/water_well.png` | — | **사용 중** | `town.tscn` (240, 175) — 마을 중앙 광장 |
| `street_lantern` | `Objects/Village/street_lantern.png` | — | **사용 중** | `town.tscn` (570, 130) — SkillShopNPC 우측, 포털 인근 |
| `flower_planter` | `Objects/Village/flower_planter.png` | — | **사용 중** | `town.tscn` (75, 135) — SavePoint 아래 |
| `barrel_group` | `Objects/Village/barrel_group.png` | — | **사용 중** | `town.tscn` (470, 250) — MaterialShopNPC 좌측 |
| `wooden_fence_segment` | `Objects/Village/wooden_fence_segment.png` | — | 리소스만 등록 | town 640×360 이미 NPC+장식 밀집 — 울타리 라인 배치 시 동선 차단 위험. 필드(`field_N`) 영역 구획 시 후속 |
| `hay_bale_stack` | `Objects/Village/hay_bale_stack.png` | — | 리소스만 등록 | 농촌/필드 분위기에 맞춰 필드 맵에 배치 후보 — 현 town 컨셉(석조 광장)과 톤 차이 |
| `campfire_ring` | `Objects/Village/campfire_ring.png` | — | 리소스만 등록 | 야영지 분위기 — 필드 휴식 포인트/던전 입구 외부 등에 후속 배치 후보 |
| `market_cloth_stall` | `Objects/Village/market_cloth_stall.png` | — | 리소스만 등록 | 노점 부스 — town 기존 ShopNPC와 컨셉 중복 위험. 별도 시장 맵 도입 시 사용 |

## Expansion Environment Objects (16개)

`Resources/Generated/GPT/Objects/{Coast,Snowfield,Volcano,Dungeon,Mine,Field}/` — 확장 지역(항구/설원/화산/던전/광산/필드)용 환경 오브젝트. **이번 작업에서는 PNG 리소스 등록만**. `.tscn` 배치, CollisionShape2D/Area2D, 채집·포털·퀘스트·상호작용 로직 미추가.

**시트 #1 (확장 지역, 2026-05-15)**: `Resources/Generated/GPT/SourceSheets/Objects/source_environment_objects_expansion_2026_05_15.png` (4×2, 1774×887, Codex 생성)
**슬라이서 #1**: `Resources/Generated/GPT/SourceSheets/Objects/slice_environment_objects_expansion_2026_05_15.py` (4×2, 128×128 canvas)
**시트 #2 (광산/던전/필드, 2026-05-15)**: `Resources/Generated/GPT/SourceSheets/Objects/source_environment_objects_mine_dungeon_field_2026_05_15.png` (4×2, 1774×887, Codex 생성)
**슬라이서 #2**: `Resources/Generated/GPT/SourceSheets/Objects/slice_environment_objects_mine_dungeon_field_2026_05_15.py` (4×2, 128×128 canvas)

| asset id | category | sliced PNG | 상태 | 실제 연결 위치 | 중복/주의 |
|---|---|---|---|---|---|
| `broken_ship` | Coast | `Objects/Coast/broken_ship.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `lighthouse` | Coast | `Objects/Coast/lighthouse.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `coral_outcrop` | Coast | `Objects/Coast/coral_outcrop.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `snow_drift` | Snowfield | `Objects/Snowfield/snow_drift.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `ice_pillar` | Snowfield | `Objects/Snowfield/ice_pillar.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `lava_pool` | Volcano | `Objects/Volcano/lava_pool.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `ember_rock` | Volcano | `Objects/Volcano/ember_rock.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | — |
| `dungeon_entrance` | Dungeon | `Objects/Dungeon/dungeon_entrance.png` | 리소스만 등록 | 없음 | ⚠️ 동명 자산 3종 별도 관리: 본 환경 오브젝트(`Objects/Dungeon/dungeon_entrance.png`) / 상호작용 아이콘(`Icons/Interaction/dungeon_entrance.png`) / 월드 게이트(`Objects/World/dungeon_entrance_gate.png`). 셋 모두 별도 용도, 교체·삭제 금지. |
| `crystal_vein` | Mine | `Objects/Mine/crystal_vein.png` | 리소스만 등록 | 없음 | 광맥 채집 시스템 추가 시 `mine_1`/`mine_2.tscn`에 Sprite 배치 후보. 기존 `Objects/World/mining_ore_vein.png`(런타임에 OreItem.Icon 사용)와 별도 — 본 신규 자산은 일반 장식/시각 마커용. |
| `mine_cart` | Mine | `Objects/Mine/mine_cart.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | `mine_1`/`mine_2` 맵 장식 후보 — 광산 분위기 연출용. |
| `ore_crates` | Mine | `Objects/Mine/ore_crates.png` | 리소스만 등록 | 없음 | 광산 입구·저장소 장식 후보. |
| `dungeon_brazier` | Dungeon | `Objects/Dungeon/dungeon_brazier.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | `dungeon_1~3.tscn` 통로·보스 챔버 장식 후보 — 화염 이펙트 연결 시 후속 시스템 필요. |
| `broken_pillar` | Dungeon | `Objects/Dungeon/broken_pillar.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | 던전 폐허 장식 후보. |
| `rune_stone` | Dungeon | `Objects/Dungeon/rune_stone.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | 보스 봉인/챕터 플래그 표식 후보 — 상호작용 시스템 미추가. |
| `field_camp_tent` | Field | `Objects/Field/field_camp_tent.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | `field_outpost`/`field_1~3` 휴식 포인트 장식 후보. |
| `signpost_waypoint` | Field | `Objects/Field/signpost_waypoint.png` | **사용 중** (장식 v1 2026-05-18) | 해당 권역 사냥터 .tscn Decorations Sprite2D | 필드 분기점 표지 장식 후보 — 텍스트/툴팁 시스템 별도. |

## 참고

- 전체 ItemData `.tres`: **101개**
- Equipment 폴더 신규 PNG: 16개 — `Resources/Generated/GPT/Icons/Equipment/` (최근 추가)
- Equipment 폴더와 Items 폴더에 같은 이름의 PNG가 모두 있을 경우, `.tres`의 Icon 경로가 어느 쪽을 가리키는지 확인 필요. 이번 인벤토리는 `.tres`의 실제 참조 경로를 기준으로 표기.
- 새 PNG 생성 전: 본 문서 + `Resources/Items/` 디렉터리 + 위 ⚠️중복주의 리스트 3중 확인.

---

## Expansion v1 전용 아이콘 등록 (2026-05-15, 40개)

Codex가 v1 신규 시트 6장을 생성. 슬라이스 후 임시 재사용 아이콘 → 전용 아이콘으로 교체. 모든 항목 64×64 PNG 캔버스 (largest_blob isolation + bbox trim + LANCZOS).

### Skill Icons (12개)

**시트**: `Resources/Generated/GPT/SourceSheets/Icons/source_skill_icons_expansion_v1_combat_magic_2026_05_15.png` (1774×887, 4×2)
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Icons/slice_skill_icons_expansion_v1_combat_magic_2026_05_15.py`

| asset id | category | sliced PNG | 연결 `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `cleave` | Skill Icons | `Icons/Skills/cleave.png` | `Resources/Skills/cleave.tres` | 사용 중 | whirlwind.png → cleave.png |
| `ground_slam` | Skill Icons | `Icons/Skills/ground_slam.png` | `Resources/Skills/ground_slam.tres` | 사용 중 | power_strike_v2.png → ground_slam.png |
| `battle_cry` | Skill Icons | `Icons/Skills/battle_cry.png` | `Resources/Skills/battle_cry.tres` | 사용 중 | iron_stance.png → battle_cry.png |
| `execute` | Skill Icons | `Icons/Skills/execute.png` | `Resources/Skills/execute.tres` | 사용 중 | power_strike.png → execute.png |
| `flame_wave` | Skill Icons | `Icons/Skills/flame_wave.png` | `Resources/Skills/flame_wave.tres` | 사용 중 | fire_bolt_v2.png → flame_wave.png |
| `frost_nova` | Skill Icons | `Icons/Skills/frost_nova.png` | `Resources/Skills/frost_nova.tres` | 사용 중 | ice_shard.png → frost_nova.png |
| `arcane_missile` | Skill Icons | `Icons/Skills/arcane_missile.png` | `Resources/Skills/arcane_missile.tres` | 사용 중 | lightning_storm.png → arcane_missile.png |
| `mana_shield` | Skill Icons | `Icons/Skills/mana_shield.png` | `Resources/Skills/mana_shield.tres` | 사용 중 | generic_passive_placeholder.png → mana_shield.png |

**시트**: `Resources/Generated/GPT/SourceSheets/Icons/source_skill_icons_expansion_v1_archer_2026_05_15.png` (1254×1254, 2×2)
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Icons/slice_skill_icons_expansion_v1_archer_2026_05_15.py`

| asset id | category | sliced PNG | 연결 `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `piercing_shot` | Skill Icons | `Icons/Skills/piercing_shot.png` | `Resources/Skills/piercing_shot.tres` | 사용 중 | arrow_shot.png → piercing_shot.png |
| `backstep_shot` | Skill Icons | `Icons/Skills/backstep_shot.png` | `Resources/Skills/backstep_shot.tres` | 사용 중 | dash.png → backstep_shot.png |
| `rain_of_arrows` | Skill Icons | `Icons/Skills/rain_of_arrows.png` | `Resources/Skills/rain_of_arrows.tres` | 사용 중 | multi_shot.png → rain_of_arrows.png |
| `hunter_focus` | Skill Icons | `Icons/Skills/hunter_focus.png` | `Resources/Skills/hunter_focus.tres` | 사용 중 | precise_aim.png → hunter_focus.png |

### Skill Books (12개)

**시트**: `Resources/Generated/GPT/SourceSheets/Items/source_skillbook_icons_expansion_v1_combat_magic_2026_05_15.png` (1774×887, 4×2)
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Items/slice_skillbook_icons_expansion_v1_combat_magic_2026_05_15.py`

| asset id | category | sliced PNG | 연결 `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `skillbook_cleave` | Skill Books | `Icons/Items/skillbook_cleave.png` | `Resources/Items/skillbook_cleave.tres` | 사용 중 | skillbook_whirlwind.png → skillbook_cleave.png |
| `skillbook_ground_slam` | Skill Books | `Icons/Items/skillbook_ground_slam.png` | `Resources/Items/skillbook_ground_slam.tres` | 사용 중 | skillbook_power_strike.png → skillbook_ground_slam.png |
| `skillbook_battle_cry` | Skill Books | `Icons/Items/skillbook_battle_cry.png` | `Resources/Items/skillbook_battle_cry.tres` | 사용 중 | skillbook_iron_stance.png → skillbook_battle_cry.png |
| `skillbook_execute` | Skill Books | `Icons/Items/skillbook_execute.png` | `Resources/Items/skillbook_execute.tres` | 사용 중 | skillbook_power_strike_v2.png → skillbook_execute.png |
| `skillbook_flame_wave` | Skill Books | `Icons/Items/skillbook_flame_wave.png` | `Resources/Items/skillbook_flame_wave.tres` | 사용 중 | skillbook_fire_bolt.png → skillbook_flame_wave.png |
| `skillbook_frost_nova` | Skill Books | `Icons/Items/skillbook_frost_nova.png` | `Resources/Items/skillbook_frost_nova.tres` | 사용 중 | skillbook_ice_shard.png → skillbook_frost_nova.png |
| `skillbook_arcane_missile` | Skill Books | `Icons/Items/skillbook_arcane_missile.png` | `Resources/Items/skillbook_arcane_missile.tres` | 사용 중 | skillbook_lightning_storm.png → skillbook_arcane_missile.png |
| `skillbook_mana_shield` | Skill Books | `Icons/Items/skillbook_mana_shield.png` | `Resources/Items/skillbook_mana_shield.tres` | 사용 중 | skillbook_generic_passive.png → skillbook_mana_shield.png |

**시트**: `Resources/Generated/GPT/SourceSheets/Items/source_skillbook_icons_expansion_v1_archer_2026_05_15.png` (1254×1254, 2×2)
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Items/slice_skillbook_icons_expansion_v1_archer_2026_05_15.py`

| asset id | category | sliced PNG | 연결 `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `skillbook_piercing_shot` | Skill Books | `Icons/Items/skillbook_piercing_shot.png` | `Resources/Items/skillbook_piercing_shot.tres` | 사용 중 | skillbook_arrow_shot.png → skillbook_piercing_shot.png |
| `skillbook_backstep_shot` | Skill Books | `Icons/Items/skillbook_backstep_shot.png` | `Resources/Items/skillbook_backstep_shot.tres` | 사용 중 | skillbook_dash.png → skillbook_backstep_shot.png |
| `skillbook_rain_of_arrows` | Skill Books | `Icons/Items/skillbook_rain_of_arrows.png` | `Resources/Items/skillbook_rain_of_arrows.tres` | 사용 중 | skillbook_multi_shot.png → skillbook_rain_of_arrows.png |
| `skillbook_hunter_focus` | Skill Books | `Icons/Items/skillbook_hunter_focus.png` | `Resources/Items/skillbook_hunter_focus.tres` | 사용 중 | skillbook_precise_aim.png → skillbook_hunter_focus.png |

### Consumables (8개)

**시트**: `Resources/Generated/GPT/SourceSheets/Items/source_consumable_icons_regional_expansion_v1_2026_05_15.png` (1774×887, 4×2)
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Items/slice_consumable_icons_regional_expansion_v1_2026_05_15.py`

| asset id | category | sliced PNG | 연결 `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `tidal_tonic` | Consumables | `Icons/Items/tidal_tonic.png` | `Resources/Items/tidal_tonic.tres` | 사용 중 | hi_potion.png → tidal_tonic.png |
| `warming_draught` | Consumables | `Icons/Items/warming_draught.png` | `Resources/Items/warming_draught.tres` | 사용 중 | health_potion.png → warming_draught.png |
| `defrost_potion` | Consumables | `Icons/Items/defrost_potion.png` | `Resources/Items/defrost_potion.tres` | 사용 중 | antidote_plus.png → defrost_potion.png |
| `fire_resist_elixir` | Consumables | `Icons/Items/fire_resist_elixir.png` | `Resources/Items/fire_resist_elixir.tres` | 사용 중 | defense_potion.png → fire_resist_elixir.png |
| `magma_brew` | Consumables | `Icons/Items/magma_brew.png` | `Resources/Items/magma_brew.tres` | 사용 중 | attack_potion.png → magma_brew.png |
| `battle_elixir` | Consumables | `Icons/Items/battle_elixir.png` | `Resources/Items/battle_elixir.tres` | 사용 중 | mega_potion.png → battle_elixir.png |
| `curse_water` | Consumables | `Icons/Items/curse_water.png` | `Resources/Items/curse_water.tres` | 사용 중 | holy_water.png → curse_water.png |
| `mana_herb_extract` | Consumables | `Icons/Items/mana_herb_extract.png` | `Resources/Items/mana_herb_extract.tres` | 사용 중 | herb_leaf.png → mana_herb_extract.png |

### Materials (8개)

**시트**: `Resources/Generated/GPT/SourceSheets/Items/source_material_icons_regional_expansion_v1_2026_05_15.png` (1774×887, 4×2)
**슬라이서**: `Resources/Generated/GPT/SourceSheets/Items/slice_material_icons_regional_expansion_v1_2026_05_15.py`

| asset id | category | sliced PNG | 연결 `.tres` | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `sea_kelp` | Materials | `Icons/Items/sea_kelp.png` | `Resources/Items/sea_kelp.tres` | 사용 중 | material_wood.png → sea_kelp.png |
| `glacier_shard` | Materials | `Icons/Items/glacier_shard.png` | `Resources/Items/glacier_shard.tres` | 사용 중 | stone_ore.png → glacier_shard.png |
| `lava_stone` | Materials | `Icons/Items/lava_stone.png` | `Resources/Items/lava_stone.tres` | 사용 중 | stone_ore.png → lava_stone.png |
| `titan_scale` | Materials | `Icons/Items/titan_scale.png` | `Resources/Items/titan_scale.tres` | 사용 중 | ancient_hide.png → titan_scale.png |
| `titan_core` | Materials | `Icons/Items/titan_core.png` | `Resources/Items/titan_core.tres` | 사용 중 | boss_core.png → titan_core.png |
| `drake_scale` | Materials | `Icons/Items/drake_scale.png` | `Resources/Items/drake_scale.tres` | 사용 중 | ancient_hide.png → drake_scale.png |
| `drake_eye` | Materials | `Icons/Items/drake_eye.png` | `Resources/Items/drake_eye.tres` | 사용 중 | boss_trophy.png → drake_eye.png |
| `kraken_ink` | Materials | `Icons/Items/kraken_ink.png` | `Resources/Items/kraken_ink.tres` | 사용 중 | boss_core.png → kraken_ink.png |

**참고**: 신규 40개 PNG는 `.import` 파일을 동반하지 않는다 (Godot 에디터가 처음 열릴 때 자동 생성). 기존 임시 아이콘 PNG/`.import` 및 원본 시트 6장은 보존됨.

### 아이콘 연결 보정 (UX 정리 패스, 2026-05-16)

전사 무기/단검 아이콘이 잘못 연결돼 있던 활 3종을 기존 활 아이콘으로 재연결. 신규 PNG 생성 없음 — 기존 추적 자산만 재연결.

| 대상 `.tres` | 변경 | 비고 |
|---|---|---|
| `Resources/Items/starter_bow.tres` | `dagger.png` → `coral_bow.png` | 궁수 시작 무기, 단검 아이콘 오연결 수정 |
| `Resources/Items/fishermans_bow.tres` | `twin_blades.png` → `winter_bow.png` | 활에 쌍검 아이콘 오연결 수정 |
| `Resources/Items/long_bow.tres` | (없음) → `phoenix_bow.png` | Icon 미지정 → 활 아이콘 신규 연결 |

### 허브 루프 v1 자산 연결 (2026-05-18)

새 PNG 생성 없음. 기존 자산을 신규 NPC/오브젝트 씬에 연결:

| 자산 | 연결처 | 상태 |
|---|---|---|
| `NPCs/Town/storage_keeper.png` | `Scenes/Objects/storage_npc.tscn` (town 창고) | 사용 중 |
| `Objects/Town/anvil_workstation.png` | `Scenes/Objects/crafting_npc.tscn` (town 제작대) | 사용 중 |
| `Objects/Town/blacksmith_forge.png` | `Scenes/Objects/reforge_npc.tscn` (town 재련대) | 사용 중 |

사냥터 환경 오브젝트 13종은 **장식 v1(2026-05-18)** 으로 18개 사냥터 .tscn(coast/snow/volcano/dungeon/mine/field)에 `Decorations` Node2D 하위 Sprite2D(Collision 없음, z_index=-2)로 배치됨. Loot glow는 `FieldItem.cs` 코드 글로우로 구현(새 PNG 없음). 미사용 잔여: `storage_crates_stack.png`, `ore_crates.png`, `crystal_vein.png`(코너 부족/광맥 NPC 우선) — 후속 배치 후보.

### GPT 장비 아이콘 시트 32종 슬라이스 + Icon 연결 (2026-05-18)

GPT 생성 4x2 시트 4장(원본 1774x887 RGB, 체커보드 배경)을 셀 분할 →
테두리 flood fill로 체커보드 제거 → 콘텐츠 crop +
여백 → 64x64 RGBA로 저장. **각 `.tres`의 Icon 만 연결, stats/Type/Rarity/
Price/드랍 등 다른 필드는 무수정.** 무기(활) 4종은 `Icons/Items/`, 나머지
28종은 `Icons/Equipment/`.

원본 시트 보관(SourceSheets):
- `S1` `SourceSheets/Items/source_equipment_icons_missing_bows_robes_2026_05_18.png`
- `S2` `SourceSheets/Items/source_equipment_icons_gloves_bracelets_2026_05_18.png`
- `S3` `SourceSheets/Items/source_equipment_icons_rings_belts_cloaks_2026_05_18.png`
- `S4` `SourceSheets/Items/source_equipment_icons_misc_replacements_2026_05_18.png`

| asset id | 분류 | 시트 | sliced PNG | 연결 대상 (`.tres` Icon) | 상태 | 교체 메모 |
|---|---|---|---|---|---|---|
| `composite_bow` | 활 | S1 | `Icons/Items/composite_bow.png` | `Resources/Items/composite_bow.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `hunter_bow` | 활 | S1 | `Icons/Items/hunter_bow.png` | `Resources/Items/hunter_bow.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `elven_bow` | 활 | S1 | `Icons/Items/elven_bow.png` | `Resources/Items/elven_bow.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `dragonbone_bow` | 활 | S1 | `Icons/Items/dragonbone_bow.png` | `Resources/Items/dragonbone_bow.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `apprentice_robe` | 로브 | S1 | `Icons/Equipment/apprentice_robe.png` | `Resources/Items/apprentice_robe.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `archmage_robe` | 로브 | S1 | `Icons/Equipment/archmage_robe.png` | `Resources/Items/archmage_robe.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `void_robe` | 로브 | S1 | `Icons/Equipment/void_robe.png` | `Resources/Items/void_robe.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `vampire_ring` | 반지 | S1 | `Icons/Equipment/vampire_ring.png` | `Resources/Items/vampire_ring.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `iron_helm` | 투구 | S2 | `Icons/Equipment/iron_helm.png` | `Resources/Items/iron_helm.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `runic_bracelet` | 팔찌 | S2 | `Icons/Equipment/runic_bracelet.png` | `Resources/Items/runic_bracelet.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `silver_bracelet` | 팔찌 | S2 | `Icons/Equipment/silver_bracelet.png` | `Resources/Items/silver_bracelet.tres` Icon | 사용 중 | Icon 미지정 → 전용 신규 |
| `archer_gloves` | 장갑 | S2 | `Icons/Equipment/archer_gloves.png` | `Resources/Items/archer_gloves.tres` Icon | 사용 중 | `Items/bracelet.png` 돌려쓰기 → 전용 교체 |
| `iron_gauntlets` | 장갑 | S2 | `Icons/Equipment/iron_gauntlets.png` | `Resources/Items/iron_gauntlets.tres` Icon | 사용 중 | `Items/bracelet.png` 돌려쓰기 → 전용 교체 |
| `leather_gloves` | 장갑 | S2 | `Icons/Equipment/leather_gloves.png` | `Resources/Items/leather_gloves.tres` Icon | 사용 중 | `Items/bracelet.png` 돌려쓰기 → 전용 교체 |
| `mage_gloves` | 장갑 | S2 | `Icons/Equipment/mage_gloves.png` | `Resources/Items/mage_gloves.tres` Icon | 사용 중 | `Items/bracelet.png` 돌려쓰기 → 전용 교체 |
| `guardian_bracelet` | 팔찌 | S2 | `Icons/Equipment/guardian_bracelet.png` | `Resources/Items/guardian_bracelet.tres` Icon | 사용 중 | `Equipment/warrior_bracelet.png` 돌려쓰기 → 전용 교체 |
| `fire_ring` | 반지 | S3 | `Icons/Equipment/fire_ring.png` | `Resources/Items/fire_ring.tres` Icon | 사용 중 | `Items/ruby_ring.png` 돌려쓰기 → 전용 교체 |
| `night_watch_ring` | 반지 | S3 | `Icons/Equipment/night_watch_ring.png` | `Resources/Items/night_watch_ring.tres` Icon | 사용 중 | `Items/shadow_ring.png` 돌려쓰기 → 전용 교체 |
| `vampire_necklace` | 목걸이 | S3 | `Icons/Equipment/vampire_necklace.png` | `Resources/Items/vampire_necklace.tres` Icon | 사용 중 | `Items/warrior_necklace.png` 돌려쓰기 → 전용 교체 |
| `swift_bracelet` | 팔찌 | S3 | `Icons/Equipment/swift_bracelet.png` | `Resources/Items/swift_bracelet.tres` Icon | 사용 중 | `Items/leather_bracelet.png` 돌려쓰기 → 전용 교체 |
| `leather_belt` | 벨트 | S3 | `Icons/Equipment/leather_belt.png` | `Resources/Items/leather_belt.tres` Icon | 사용 중 | `Items/leather_pouch.png` 돌려쓰기 → 전용 교체 |
| `iron_belt` | 벨트 | S3 | `Icons/Equipment/iron_belt.png` | `Resources/Items/iron_belt.tres` Icon | 사용 중 | `Items/leather_pouch.png` 돌려쓰기 → 전용 교체 |
| `traveler_cloak` | 망토 | S3 | `Icons/Equipment/traveler_cloak.png` | `Resources/Items/traveler_cloak.tres` Icon | 사용 중 | `Items/leather_armor.png` 돌려쓰기 → 전용 교체 |
| `warrior_cape` | 망토 | S3 | `Icons/Equipment/warrior_cape.png` | `Resources/Items/warrior_cape.tres` Icon | 사용 중 | `Items/iron_armor.png` 돌려쓰기 → 전용 교체 |
| `crimson_robe` | 로브 | S4 | `Icons/Equipment/crimson_robe.png` | `Resources/Items/crimson_robe.tres` Icon | 사용 중 | `Equipment/mystic_robe.png` 돌려쓰기 → 전용 교체 |
| `knight_helm` | 투구 | S4 | `Icons/Equipment/knight_helm.png` | `Resources/Items/knight_helm.tres` Icon | 사용 중 | `Items/helmet.png` 돌려쓰기 → 전용 교체 |
| `mage_cloak` | 망토 | S4 | `Icons/Equipment/mage_cloak.png` | `Resources/Items/mage_cloak.tres` Icon | 사용 중 | `Items/leather_armor.png` 돌려쓰기 → 전용 교체 |
| `scout_cloak` | 망토 | S4 | `Icons/Equipment/scout_cloak.png` | `Resources/Items/scout_cloak.tres` Icon | 사용 중 | `Items/orc_leather.png` 돌려쓰기 → 전용 교체 |
| `arcane_belt` | 벨트 | S4 | `Icons/Equipment/arcane_belt.png` | `Resources/Items/arcane_belt.tres` Icon | 사용 중 | `Items/leather_pouch.png` 돌려쓰기 → 전용 교체 |
| `swift_belt` | 벨트 | S4 | `Icons/Equipment/swift_belt.png` | `Resources/Items/swift_belt.tres` Icon | 사용 중 | `Items/leather_pouch.png` 돌려쓰기 → 전용 교체 |
| `amethyst_ring` | 반지 | S4 | `Icons/Equipment/amethyst_ring.png` | `Resources/Items/amethyst_ring.tres` Icon | 사용 중 | `Items/magic_ring.png` 돌려쓰기 → 전용 교체 |
| `storm_ring` | 반지 | S4 | `Icons/Equipment/storm_ring.png` | `Resources/Items/storm_ring.tres` Icon | 사용 중 | `Equipment/emerald_ring.png` 돌려쓰기 → 전용 교체 |

**`.import` 생성 완료**: Godot 4.6.2 mono headless import로 신규 32 PNG와 원본
시트 4장의 `.png.import`를 생성했다. `.tres`는 `path=`로 참조하며, 원본 생성
이미지/기존 PNG/`.tres`/씬 미삭제, 루트 `ChatGPT Image*.png` 미추적. 위 32개
inline 레거시 표 행(돌려쓰기 경로 표기)은 이 절이 최신 권위 — 개별 행은 점진적으로
갱신.

### 이미지 매칭 감사 보정 (2026-05-18)

전체 `Resources/Items/*.tres`와 `Resources/Enemies/*.tres`의 이미지 연결을 전수 점검해
누락/의미 불일치/변형 재사용을 보정했다. 신규 AI 시트 생성 없이 기존 시트/아이콘/스프라이트를
슬라이스하거나 색상 변형해 연결했다. Godot 4.6.2 mono headless import로 이 절의
신규 PNG 16개도 `.png.import` 생성 완료.

#### 아이템 아이콘 보정

| asset id | category | PNG | 연결 대상 | 상태 | 교체 메모 |
|---|---|---|---|---|---|
| `speed_potion` | Consumables | `Icons/Items/speed_potion.png` | `Resources/Items/speed_potion.tres` Icon | 사용 중 | Icon 미지정 → 기존 potion sheet의 녹색 물약 슬라이스 |
| `corrupted_stone` | Materials | `Icons/Items/corrupted_stone.png` | `Resources/Items/corrupted_stone.tres` Icon | 사용 중 | `silver_ore.png` 돌려쓰기 → 검은 광석 슬라이스 |
| `crystal_ore` | Ores/Materials | `Icons/Ores/crystal_ore.png` | `Resources/Items/crystal_ore.tres` Icon | 사용 중 | `silver_ore.png` 돌려쓰기 → 수정 광석 슬라이스 |
| `enhance_stone` | Materials | `Icons/Items/enhance_stone.png` | `Resources/Items/enhance_stone.tres` Icon | 사용 중 | `boss_core.png` 돌려쓰기 → 수정 아이콘 색상 변형 |
| `prismatic_crystal` | Materials | `Icons/Items/prismatic_crystal.png` | `Resources/Items/prismatic_crystal.tres` Icon | 사용 중 | `silver_ore.png` 돌려쓰기 → 프리즘 색상 변형 |
| `storm_ward` | Consumables | `Icons/Items/storm_ward.png` | `Resources/Items/storm_ward.tres` Icon | 사용 중 | `tidal_tonic.png` 돌려쓰기 → shock ward 합성 아이콘 |

#### 적 스프라이트 보정

구형/레거시 EnemyStats 중 Sprite 미지정 리소스는 기존 GPT 적 스프라이트를 연결했다. 현재 주력 배치가
신규 `dungeon*/field*/mine_*` 리소스로 이동했더라도, 레거시 리소스가 다시 참조될 때 atlas fallback으로
되돌아가지 않도록 하기 위한 안전 보정이다.

| EnemyStats | Sprite 연결 | 상태 | 메모 |
|---|---|---|---|
| `boss_orc_king.tres` | `Enemies/NamedBosses/orc_warlord.png` | 사용 중 | legacy fallback |
| `boss_skeleton_king.tres` | `Enemies/NamedBosses/skeleton_king.png` | 사용 중 | dungeon_2 BossStatVariant |
| `orc_basic.tres` | `Enemies/Dungeon1/orc_club.png` | 사용 중 | legacy fallback |
| `orc_warrior.tres` | `Enemies/Dungeon1/orc_axe_warrior.png` | 사용 중 | legacy fallback |
| `orc_rogue.tres` | `Enemies/Dungeon1/orc_rogue.png` | 사용 중 | legacy fallback |
| `orc_shaman.tres` | `Enemies/Dungeon1/orc_shaman.png` | 사용 중 | legacy fallback |
| `skeleton_base.tres` | `Enemies/Field2/skeleton_wanderer.png` | 사용 중 | legacy fallback |
| `skeleton_mage.tres` | `Enemies/Dungeon2/skeleton_mage.png` | 사용 중 | legacy fallback |
| `skeleton_rogue.tres` | `Enemies/Dungeon2/skeleton_rogue.png` | 사용 중 | legacy fallback |
| `skeleton_warrior.tres` | `Enemies/Dungeon2/skeleton_warrior.png` | 사용 중 | legacy fallback |
| `zombie_basic.tres` | `Enemies/Field2/zombie_walker.png` | 사용 중 | legacy fallback |

이미지 돌려쓰기 변형 10종은 기존 스프라이트 기반 색상 변형 PNG를 만들어 전용 경로로 연결했다.

| EnemyStats | 신규 PNG | 원본 | 상태 |
|---|---|---|---|
| `dungeon4_tide_lurker.tres` | `Enemies/Dungeon4/tide_lurker.png` | `deep_lurker.png` | 사용 중 |
| `field1_grove_spirit.tres` | `Enemies/Field1/grove_spirit.png` | `forest_spirit.png` | 사용 중 |
| `field1_mist_spider.tres` | `Enemies/Field1/mist_spider.png` | `forest_spider.png` | 사용 중 |
| `field2_fog_wraith.tres` | `Enemies/Field2/fog_wraith.png` | `grave_wraith.png` | 사용 중 |
| `field3_ruin_sentinel.tres` | `Enemies/Field3/ruin_sentinel.png` | `ruin_golem.png` | 사용 중 |
| `field4_reef_raider.tres` | `Enemies/Field4/reef_raider.png` | `pirate_grunt.png` | 사용 중 |
| `field5_blizzard_witch.tres` | `Enemies/Field5/blizzard_witch.png` | `snow_witch.png` | 사용 중 |
| `field5_frostbound_bear.tres` | `Enemies/Field5/frostbound_bear.png` | `polar_bear.png` | 사용 중 |
| `field6_ash_imp.tres` | `Enemies/Field6/ash_imp.png` | `fire_imp.png` | 사용 중 |
| `mine3_crystal_shocker.tres` | `Enemies/Mine/crystal_shocker.png` | `crystal_warlock.png` | 사용 중 |
