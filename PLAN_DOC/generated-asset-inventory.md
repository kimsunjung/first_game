# Generated GPT Asset Inventory

작성: 2026-05-13
목적: GPT로 생성한 아이콘·캐릭터 PNG와 `Resources/Items/*.tres` ItemData 현황을 카테고리별로 정리해 **이미지 재생성 중복**과 **ItemData 덮어쓰기**를 방지한다. 적 인벤토리는 [enemy-zone-plan.md](enemy-zone-plan.md), NPC 매핑은 [generated-npc-inventory.md](generated-npc-inventory.md)에 별도 정리.

**⚠️ 중복 생성 금지 (이미 존재 — 새 PNG/`.tres` 생성 전 반드시 확인)**: `antidote`, `battle_helm`, `chainmail_armor`, `copper_ore`, `dungeon_key`, `emerald_ring`, `enhance_stone`, `gold_ore`, `guard_potion`, `health_potion`, `hi_potion`, `iron_ore`, `knight_boots`, `knight_plate_armor`, `mana_potion`, `mega_potion`, `mithril_ore`, `moonstone_necklace`, `mystic_bracelet`, `mystic_robe`, `platinum_ore`, `ranger_hood`, `ranger_vest`, `return_scroll`, `ruby_ore`, `ruby_ring`, `sapphire_ore`, `shadow_boots`, `silver_ore`, `steel_helm`, `sun_amulet`, `swift_boots`, `swift_potion`, `warrior_bracelet`

**범례**
- `상태` 컬럼: `사용 중` = 게임 로직(상점/드랍/장착 등)에서 실제 참조 / `리소스만 등록` = `.tres` 존재하지만 어디서도 참조 안 됨(드랍 후보 등) / `미사용` = `.tres` 없고 PNG만 / `중복보류` = 의도적 미등록
- ⚠️**중복주의** = 사용자 요구사항으로 마킹된 중복 위험 항목

---

## Consumables (8개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `antidote` | `Resources/Generated/GPT/Icons/Items/antidote.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `guard_potion` | `Resources/Generated/GPT/Icons/Items/guard_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `health_potion` | `Resources/Generated/GPT/Icons/Items/health_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `hi_potion` | `Resources/Generated/GPT/Icons/Items/hi_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `mana_potion` | `Resources/Generated/GPT/Icons/Items/mana_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `mega_potion` | `Resources/Generated/GPT/Icons/Items/mega_potion.png` | ✅ | Common | 사용 중 ⚠️**중복주의** |
| `return_scroll` | `Resources/Generated/GPT/Icons/Items/return_scroll.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
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

## Weapons (33개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `arcane_blade` | `Resources/Generated/GPT/Icons/Items/arcane_blade.png` | ✅ | Rare | 사용 중 |
| `arcane_hammer` | `Resources/Generated/GPT/Icons/Items/arcane_hammer.png` | ✅ | Rare | 사용 중 |
| `battle_axe` | `Resources/Generated/GPT/Icons/Items/battle_axe.png` | ✅ | Common | 사용 중 |
| `crossed_blades` | `Resources/Generated/GPT/Icons/Items/crossed_blades.png` | ✅ | Rare | 사용 중 |
| `crystal_staff` | `Resources/Generated/GPT/Icons/Items/crystal_staff.png` | ✅ | Rare | 사용 중 |
| `dagger` | `Resources/Generated/GPT/Icons/Items/dagger.png` | ✅ | Common | 사용 중 |
| `dread_blade` | `Resources/Generated/GPT/Icons/Items/dread_blade.png` | ✅ | Rare | 사용 중 |
| `executioner_axe` | `Resources/Generated/GPT/Icons/Items/executioner_axe.png` | ✅ | Rare | 사용 중 |
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
| `iron_spear` | `Resources/Generated/GPT/Icons/Items/iron_spear.png` | ✅ | Common | 사용 중 |
| `iron_sword` | `Resources/Generated/GPT/Icons/Items/iron_sword.png` | ✅ | Common | 사용 중 |
| `nature_staff` | `Resources/Generated/GPT/Icons/Items/nature_staff.png` | ✅ | Uncommon | 사용 중 |
| `ornate_sword` | `Resources/Generated/GPT/Icons/Items/ornate_sword.png` | ✅ | Common | 사용 중 |
| `royal_halberd` | `Resources/Generated/GPT/Icons/Items/royal_halberd.png` | ✅ | Epic | 사용 중 |
| `scimitar` | `Resources/Generated/GPT/Icons/Items/scimitar.png` | ✅ | Rare | 사용 중 |
| `shadow_staff` | `Resources/Generated/GPT/Icons/Items/shadow_staff.png` | ✅ | Rare | 사용 중 |
| `spiked_mace` | `Resources/Generated/GPT/Icons/Items/spiked_mace.png` | ✅ | Rare | 사용 중 |
| `starlight_staff` | `Resources/Generated/GPT/Icons/Items/starlight_staff.png` | ✅ | Epic | 사용 중 |
| `steel_sword` | `Resources/Generated/GPT/Icons/Items/steel_sword.png` | ✅ | Common | 사용 중 |
| `twin_blades` | `Resources/Generated/GPT/Icons/Items/twin_blades.png` | ✅ | Rare | 사용 중 |
| `void_scepter` | `Resources/Generated/GPT/Icons/Items/void_scepter.png` | ✅ | Epic | 사용 중 |
| `war_hammer` | `Resources/Generated/GPT/Icons/Items/war_hammer.png` | ✅ | Rare | 사용 중 |
| `wooden_club` | `Resources/Generated/GPT/Icons/Items/wooden_club.png` | ✅ | Common | 사용 중 |
| `wooden_staff` | `Resources/Generated/GPT/Icons/Items/wooden_staff.png` | ✅ | Common | 사용 중 |

## Armor (몸통) (8개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `chainmail_armor` | `Resources/Generated/GPT/Icons/Equipment/chainmail_armor.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `iron_armor` | `Resources/Generated/GPT/Icons/Items/iron_armor.png` | ✅ | Common | 사용 중 |
| `knight_plate_armor` | `Resources/Generated/GPT/Icons/Equipment/knight_plate_armor.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `leather_armor` | `Resources/Generated/GPT/Icons/Items/leather_armor.png` | ✅ | Common | 사용 중 |
| `mystic_robe` | `Resources/Generated/GPT/Icons/Equipment/mystic_robe.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `ranger_vest` | `Resources/Generated/GPT/Icons/Equipment/ranger_vest.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `steel_armor` | `Resources/Generated/GPT/Icons/Items/steel_armor.png` | ✅ | Common | 사용 중 |
| `wooden_shield` | `Resources/Generated/GPT/Icons/Items/wooden_shield.png` | ✅ | Common | 사용 중 |

## Helmets (8개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `battle_helm` | `Resources/Generated/GPT/Icons/Equipment/battle_helm.png` | ✅ | Epic | 사용 중 ⚠️**중복주의** |
| `dark_helm` | `Resources/Generated/GPT/Icons/Items/dark_helm.png` | ✅ | Common | 사용 중 |
| `iron_helm` | `` | ✅ | Uncommon | 사용 중 |
| `knight_helm` | `Resources/Generated/GPT/Icons/Items/helmet.png` | ✅ | Common | 사용 중 |
| `leather_cap` | `Resources/Generated/GPT/Icons/Items/leather_cap.png` | ✅ | Common | 사용 중 |
| `ranger_hood` | `Resources/Generated/GPT/Icons/Equipment/ranger_hood.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `steel_helm` | `Resources/Generated/GPT/Icons/Equipment/steel_helm.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `wizard_hat` | `Resources/Generated/GPT/Icons/Items/wizard_hat.png` | ✅ | Rare | 사용 중 |

## Boots (6개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `iron_boots` | `Resources/Generated/GPT/Icons/Items/iron_boots.png` | ✅ | Uncommon | 사용 중 |
| `knight_boots` | `Resources/Generated/GPT/Icons/Equipment/knight_boots.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `leather_boots` | `Resources/Generated/GPT/Icons/Items/boots.png` | ✅ | Common | 사용 중 |
| `shadow_boots` | `Resources/Generated/GPT/Icons/Equipment/shadow_boots.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |
| `swift_boots` | `Resources/Generated/GPT/Icons/Equipment/swift_boots.png` | ✅ | Uncommon | 사용 중 ⚠️**중복주의** |
| `traveler_boots` | `Resources/Generated/GPT/Icons/Items/traveler_boots.png` | ✅ | Rare | 사용 중 |

## Rings (6개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `amethyst_ring` | `Resources/Generated/GPT/Icons/Items/magic_ring.png` | ✅ | Rare | 사용 중 |
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
| `runic_bracelet` | `` | ✅ | Epic | 사용 중 |
| `silver_bracelet` | `` | ✅ | Rare | 사용 중 |
| `warrior_bracelet` | `Resources/Generated/GPT/Icons/Equipment/warrior_bracelet.png` | ✅ | Rare | 사용 중 ⚠️**중복주의** |

## Skill Books (4개)

| asset id | PNG 경로 | `.tres` | Rarity | 상태 |
|---|---|---|---|---|
| `skillbook_dash` | `Resources/Generated/GPT/Icons/Items/skillbook_dash.png` | ✅ | Common | 사용 중 |
| `skillbook_fire_bolt` | `Resources/Generated/GPT/Icons/Items/skillbook_fire_bolt.png` | ✅ | Common | 사용 중 |
| `skillbook_heal` | `Resources/Generated/GPT/Icons/Items/skillbook_heal.png` | ✅ | Common | 사용 중 |
| `skillbook_power_strike` | `Resources/Generated/GPT/Icons/Items/skillbook_power_strike.png` | ✅ | Common | 사용 중 |

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

### Mine (8개)

`cave_bat`, `mine_wraith`, `rock_golem`, `skeleton_miner`, `zombie_armored`, `zombie_basic`, `zombie_brute`, `zombie_fast`

### Dungeon1 (8개)

`goblin_trapper`, `orc_axe_warrior`, `orc_brute`, `orc_captain`, `orc_club`, `orc_rogue`, `orc_shaman`, `orc_warlord_boss`

### Dungeon2 (8개)

`bone_archer`, `bone_knight`, `crypt_wraith`, `ghoul_brute`, `skeleton_champion`, `skeleton_mage`, `skeleton_rogue`, `skeleton_warrior`

### Dungeon3 (8개)

`abyss_hound`, `abyss_wraith`, `ancient_lich`, `bone_golem`, `cursed_warlock`, `death_knight`, `dungeon_guardian`, `shadow_assassin`

### NamedBosses (8개)

`ancient_lich`, `crystal_guardian`, `forest_alpha_wolf`, `graveyard_wight`, `mine_golem`, `orc_warlord`, `plague_brute`, `skeleton_king`

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
| `storage_keeper.png` | `(미구현)` | 리소스만 등록 |
| `teleport_guide.png` | `teleport_npc.tscn` | 사용 중 |

## Unused / 보관 전용

### Shields (방패 시스템 미도입)

`Resources/Generated/GPT/Icons/Unused/Shields/` — ItemData 미생성 (방패 슬롯/타입 없음).

`iron_shield.png`, `ranger_buckler.png`, `royal_guard_shield.png`, `tower_shield.png`

### 갑옷 중복 보관

`Resources/Generated/GPT/Icons/Unused/Duplicates/` — `09_54_01` 갑옷 시트와 중복으로 분리한 백업본. ItemData 미생성.

`dup_chainmail_armor.png`, `dup_knight_plate_armor.png`, `dup_mystic_robe.png`, `dup_ranger_vest.png`

## 참고

- 전체 ItemData `.tres`: **101개**
- Equipment 폴더 신규 PNG: 16개 — `Resources/Generated/GPT/Icons/Equipment/` (최근 추가)
- Equipment 폴더와 Items 폴더에 같은 이름의 PNG가 모두 있을 경우, `.tres`의 Icon 경로가 어느 쪽을 가리키는지 확인 필요. 이번 인벤토리는 `.tres`의 실제 참조 경로를 기준으로 표기.
- 새 PNG 생성 전: 본 문서 + `Resources/Items/` 디렉터리 + 위 ⚠️중복주의 리스트 3중 확인.
