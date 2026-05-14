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

## World Objects (Map Interactables)

`Resources/Generated/GPT/Objects/World/` — 맵에 배치되는 상호작용 오브젝트. ItemData가 아니라 `.tscn` 씬 안 Sprite로 사용.

**시트**: `Resources/Generated/GPT/SourceSheets/Objects/source_world_interactables_2026_05_13.png`

| asset id | category | sliced PNG | 씬 연결 | 상태 | 보류 사유 |
|---|---|---|---|---|---|
| `save_crystal_shrine` | Save | `Objects/World/save_crystal_shrine.png` | `Scenes/Objects/save_point.tscn` (Sprite2D texture) | **사용 중** | — |
| `magic_portal` | Portal | `Objects/World/magic_portal.png` | `Scenes/Objects/portal.tscn` (Visual ColorRect → Sprite2D) | **사용 중** | — |
| `dungeon_entrance_gate` | Map Transition | `Objects/World/dungeon_entrance_gate.png` | (씬 없음) | 리소스만 등록 | 던전 입구 전용 씬 미구현 — 현재는 일반 Portal 사용 |
| `quest_notice_board` | Quest | `Objects/World/quest_notice_board.png` | (씬 없음) | 미구현 보류 | 퀘스트 게시판 시스템 미도입 |
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
| `storage_crates_stack` | `Objects/Town/storage_crates_stack.png` | 리소스만 등록 | Storage(창고) 기능 미구현 — 인벤토리 외 별도 저장 시스템 부재. 도입 시 town에 NPC와 함께 배치 후보 |
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

## 참고

- 전체 ItemData `.tres`: **101개**
- Equipment 폴더 신규 PNG: 16개 — `Resources/Generated/GPT/Icons/Equipment/` (최근 추가)
- Equipment 폴더와 Items 폴더에 같은 이름의 PNG가 모두 있을 경우, `.tres`의 Icon 경로가 어느 쪽을 가리키는지 확인 필요. 이번 인벤토리는 `.tres`의 실제 참조 경로를 기준으로 표기.
- 새 PNG 생성 전: 본 문서 + `Resources/Items/` 디렉터리 + 위 ⚠️중복주의 리스트 3중 확인.
