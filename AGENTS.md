# Codex Project Instructions

## Environment
- Current workspace: `/Users/ksj/personal/project/first_game`
- Shell: `zsh`
- Engine: Godot 4.6
- Language/runtime: C# / .NET 8
- Target platform: mobile first. PC/editor is for debugging only.
- Prefer `rg` / `rg --files` for search.
- Do not assume Windows/PowerShell instructions apply to Codex.

## Collaboration Rules
- The user often uses Claude Code for implementation and Codex for review, planning, validation, and prompts.
- Do not edit files unless the user explicitly asks Codex to implement or update files.
- If reviewing Claude changes, use a code-review stance: findings first, ordered by severity, with file/line references.
- Never revert user or Claude changes unless the user explicitly asks.
- Before committing/pushing, inspect `git status --short` and keep unrelated changes out of the commit.
- When project direction, major systems, map/world structure, asset pipeline status, or implemented feature status changes, update `AGENTS.md` so future Codex sessions do not rely on stale assumptions. Also update the relevant `PLAN_DOC/*.md` file when the change affects design, content, or asset inventory.

## Codex Memory Role
- `AGENTS.md` is the Codex-facing project memory and operating guide. Keep it aligned with the latest project flow, current implementation status, known risks, and validation expectations.
- Do not mirror `CLAUDE.md` blindly. `CLAUDE.md` is Claude Code's file; Codex should only edit it when the user explicitly asks. Otherwise, prepare a Claude-facing prompt when Claude memory needs to be updated.
- This file should stay concise and practical. It is not a changelog; update it when stale assumptions would affect reviews, implementation prompts, or validation.
- After large Claude batches, use this file plus `PLAN_DOC/*.md` as the starting context for review, then verify against actual code and scene files.

## Current Game Direction
- Genre: top-down mobile action RPG with classic PC-style hunting-zone progression.
- Goal: store release as a personal project, not monetization-first.
- The game is not an ending-first RPG. It should play more like an old classic hunting RPG: prepare in a hub, hunt/mining/boss in a zone, return with drops/gold/materials, upgrade gear/skills/consumables, then move to stronger hunting zones and repeat.
- Current phase: core systems exist; focus is mobile playability, content loop, asset integration, and regression safety.
- Main loop: hub preparation -> hunting/mining/boss attempts -> item/gold/material pickup -> inventory/equipment/affix farming -> weapon enhancement/skill growth -> harder zones/bosses -> save/return/teleport -> repeat.
- Story/chapters can guide or unlock areas, but should not imply the game ends after one final boss. Bosses are hunting-zone milestones and farming targets unless a specific design says otherwise.
- Avoid modern mobile RPG baggage: no auto-combat, gacha, collection book chores, daily/weekly homework, season grind, or Lineage-like monetization loops. Classic hunting-zone progression is intended; Lineage-like BM/automation is not.
- Keep feature additions pragmatic. Avoid large rewrites unless a localized design has become unmaintainable.

## Current World Structure
- Hub: `Scenes/Maps/town.tscn`
- Core existing hubs/fields: `town`, `field_outpost`, `field_1`, `field_2`, `field_3`
- Core existing dungeons/mines: `dungeon_1`, `dungeon_2`, `dungeon_3`, `mine_1`, `mine_2`
- Expansion/region work is moving toward larger hunting regions with additional hubs such as `harbor_village` and `mountain_refuge`, plus surrounding hunting scenes. Treat those as open-ended hunting-zone content, not one-time story maps.
- Enemy placement and intended zone identity are tracked in `PLAN_DOC/enemy-zone-plan.md`; broader region flow should be tracked in `PLAN_DOC/world-flow-plan.md` and `PLAN_DOC/regional-world-map-plan.md` when present.

## Current Regional Direction
- `town_region`: `town`, `field_1`, `mine_1`, `dungeon_1`, plus surrounding hunting maps such as `town_outskirts`, `green_meadow`, `goblin_woods`, `old_orc_road` when present.
- `outpost_region`: `field_outpost`, `field_2`, `field_3`, `mine_2`, `dungeon_2`, `dungeon_3`, and `ruined_crossroad` when present.
- `coast_region`: `harbor_village`, `field_4_harbor`, `dungeon_4_sunken_shrine`, and coast hunting maps such as `harbor_outskirts`, `crab_beach`, `pirate_camp` when present.
- `mountain_region`: `mountain_refuge`, `field_5_snowfield`, `field_6_volcano`, `mine_3`, and mountain hunting maps such as `snowfield_edge`, `frozen_valley`, `volcano_approach`, `lava_field` when present.
- Map structure should feel like "hub + surrounding hunting grounds + mines/dungeons/boss areas", not "one field always owns one dungeon".

## Important Systems
- Autoloads: `GameManager`, `AudioManager`, `SceneManager`
- Save/load: `Scripts/Core/SaveManager.cs`, `Scripts/Data/SaveData.cs`, `Scripts/Core/GameTransaction.cs`
- Player: `Scripts/Entities/Player/PlayerController*.cs`
- Enemies: `Scripts/Entities/Enemies/EnemyController.cs`, `EnemySpawner.cs`, `EnemyProjectile.cs`
- Regional weather (v3): `Scripts/Maps/BiomeWeatherController.cs` — per-scene Node2D, code overlay/particles + low-rate hazard via `Stats.ApplyStatus`. No PNG, no save state.
- Status resist: `CharacterStats.StatusResist` is a **raw accumulator** (equip + buff summed, no clamp on the stored value); `PlayerStats.ModifyStatusResist` just does `+= delta`. The 0~0.85 immunity cap is applied **only at use** in `CharacterStats.ApplyStatus` (so buff expiry can't erode equip resist). `ItemData.BonusStatusResist` (equip) / `BuffStatusResist` (consumable Buff, `ApplyBuffEx` optional arg).
- Hub loop v1 (2026-05-18, see `PLAN_DOC/hub-preparation-loop-v1.md`):
  - **Storage**: `SaveData.StorageItems` (v12, backfill in `MigrateSaveData` v11→12), `GameManager._storage`/`RestoreStorage`/`AddStorageSlot`/`RemoveStorageAt`, `StorageUI.cs`/`storage_ui.tscn`, `StorageNPC.cs`/`storage_npc.tscn` (town). Deposit/Withdraw via `GameTransaction` + explicit SaveGame; equipped slots never storable (3-guarded).
  - **Crafting**: `Resources/Recipes/recipes.json` + `CraftingData.cs` loader, `CraftingUI.cs`/`crafting_ui.tscn`, `CraftingNPC.cs`/`crafting_npc.tscn`. Deterministic (no RNG/destroy); transactional consume+gold+result.
  - **Reforge**: `ReforgeUI.cs`/`reforge_ui.tscn`, `ReforgeNPC.cs`/`reforge_npc.tscn`. Unequipped jewelry only; re-rolls `slot.Affixes` via `AffixGenerator.GenerateForJewelry`; no SaveData schema change.
  - **Hardening (2026-05-18, Codex review)**: `ResetForNewGame()` now clears `_storage` (no carryover into new game). `CraftingUI.Craft()` aggregates materials by ResourcePath and grants result first inside the transaction (no partial deduction on failure; GameTransaction is not a memory rollback). `validate.py` R9 validates recipes.json (unique id, paths resolve, qty>0/gold>=0, no dup material path per recipe, materials must be Consumable/Material type — `ConsumeItems` ignores IsEquipped so equipment is forbidden as material). `StorageNPC` moved (560,300)->(470,305) clear of the harbor portal.
- Hunting Contracts v1 (2026-05-18, see `PLAN_DOC/hub-preparation-loop-v1.md` D/E section):
  - **Manager**: `Scripts/Core/HuntingContractManager.cs` — `GameManager.ContractManager` permanent instance, fully independent of main `QuestManager` (do NOT extend QuestManager for contracts). `EventManager.ResetAll()` calls `ContractManager.Resubscribe()`.
  - **Data**: `Scripts/Data/ContractData.cs` (`ContractType{Kill=0,Gather=1,BossKill=2,Mining=3}` + `ContractData` + `ContractProgress` get/set), `Resources/Contracts/contracts.json` (16, 4/region) + `ContractsData.cs` loader.
  - **Save**: `SaveData.ActiveContracts` (**v13**, backfill in `MigrateSaveData` v12→13), serialized in `SaveManager.BuildSaveData`, restored in `PlayerController.LoadFromSaveData`, cleared in `ResetForNewGame`.
  - **Progress events**: Kill=`OnEnemyKilledTyped` (StripElitePrefix), BossKill=new `EventManager.OnBossKilled(bossId)` (EnemyController post-RecordBossDefeat), Mining=new `OnOreMined(orePath)` (MiningNode after txn, before SaveGame), Gather=`Inventory.OnItemPickedUp` accumulate (no consume on turn-in).
  - **Rules**: max 3 active, no dup, re-acceptable after completion, abandon = no reward + immediate SaveGame. Completion: gold/EXP always + item via `AddPendingReward` fallback (no loss); `GameTransaction` + explicit SaveGame (QuestManager.CompleteQuest isolation pattern).
  - **UI/NPC**: `ContractBoardUI.cs`/`contract_board_ui.tscn` (child of NPC scene), `ContractBoardNPC.cs`/`contract_board_npc.tscn` (CircleShape r36, `quest_notice_board.png`). Placed in all 4 hubs with `Region` Export override (town/outpost/coast/mountain).
  - **Polish**: mine_3 +5 MiningNodes (crystal/deep/prismatic/enhance_stone; mine_1/2 unchanged — NodeName is save key). FieldItem rarity loot glow = code Sprite2D (no PNG/Light2D/Particle). Validate R10/R11/R12; xUnit `ContractTests`.
  - **Decoration (2026-05-18 follow-up, DONE)**: 18 hunting-ground scenes got a `Decorations` Node2D with 2–3 biome Sprite2D each. **No Collision + z_index=-2** → physically cannot block movement/portals/spawns (visual only). Positions chosen by a one-shot script that rejects any 1280×720 corner candidate within 165px of an existing node position. mine_3 skipped (mining nodes occupy corners).
  - **Hub expansion (2026-05-18 follow-up, DONE)**: Storage/Crafting/Reforge NPCs added to field_outpost/harbor_village/mountain_refuge (were town-only). Reuses existing npc/ui scenes + same NpcIds (storage_keeper/crafting_station/reforge_station — no new ChapterDialogue ids). All 4 hubs now complete the prep loop.
  - **UX/scale (2026-05-18 follow-up, DONE)**: HUD status chips now show dedicated icons (`Resources/Generated/GPT/Icons/Status/{poison,freeze,burn,shock}.png`; `HUD.LoadStatusIcon`, Curse → color fallback, no regression). HUD bottom-left current-map name via `Scripts/Data/MapNames.cs` (Godot-free, xUnit `MapNamesTests`). Character visual size −20% via `Scripts/Core/GameScale.cs` `CharacterVisual=0.8`: applied to enemy `_animSprite.Scale` (EnemyController, always) and player.tscn AnimatedSprite2D + all 16 NPC scene Sprite2D scales. **Collision/interaction radius/attack range NOT scaled → combat balance & interactivity unchanged (visual-only).**
  - **HUD/progress (2026-05-18 follow-up2, DONE)**: Top-center effects bar (status+buff icons) — click any icon → `_effectsPopup` listing active effects + remaining seconds; icon & row blink when remain < `BlinkThreshold` (3s). Buffs exposed via new `PlayerStats.GetActiveBuffs()`. Minimap `Scripts/UI/MinimapView.cs` (`_Draw`-based, top-right, plots player/enemies/portals; bounds from scene `Background` ColorRect, fallback 1280×720). New `ChapterFlags` constants Kraken/GlacierTitan/InfernoDrake/CrystalLord recorded in `GameManager.RecordBossDefeat` + old-save backfill switch — **additive markers only, `Chapter` enum & `CurrentChapter` unchanged (not an ending game; no SaveData version bump)**.
  - **Region drops + skill expansion (2026-05-18 follow-up3, DONE)**: Region-themed drops on 60 non-boss enemy `.tres` via `EnemyStats.RegionDrop`/`RegionDropChance` (default 0.1), rolled **independently** of the PossibleDrops table in `EnemyController` (does not enter weight normalization → existing drop probabilities unchanged; truly additive). 8 new active skills with **unique `SkillType` 27–34** + delegating strategy subclasses (`VenomShotStrategy : ArrowShotStrategy`, attribute `Inherited=false` so each registers separately) + Element/Status/param differentiation + 8 skillbooks across 4 hub shops.
  - **Claude review hardening v2 (2026-05-18, DONE — see hub-preparation-loop-v1.md)**: [F1] `outpost_kill_skeleton` target `Skeleton`→`SkeletonWanderer` (only scene-spawned EnemyTypeName); validate R10 now derives valid Kill targets from scene-referenced enemy resources only. [F2] BossKill on non-repeatable already-defeated boss = unobtainable → `CanAccept`/`AvailableForRegion` reject + `RestoreFromSave` drops it (TurnInReady kept). [F3] `Advance()` calls `RequestAutoSave()` so gather/kill/mine progress isn't lost to inventory-autosave ordering. [F4] `StartLightningStorm` now carries element+status; storm tick applies `ApplyStatusEffect` (Chain Bolt Shock works; `lightning_storm` None = unchanged). [F5] reverted in-table weight appends → independent RegionDrop roll above.
  - **Claude adversarial hardening v3 (2026-05-18, DONE)**: F3 escalated — Gather progress now does immediate `SaveGame()` (others stay `RequestAutoSave`, intentionally autosave-grade). F2 decoupled — boss-contract pruning moved out of `RestoreFromSave` into `HuntingContractManager.PruneUnobtainable()`, called once at end of `PlayerController.LoadFromSaveData` (no restore-order dependency). F5 economy — `RegionDropChance` 0.1→0.05 across 60 .tres to match old in-table effective rate (~3–5%/kill); rationale in `EnemyStats`. validate R10 adds `_collect_mining_ore_paths()` so Mining `targetOreItemPath` must be an ore some scene `MiningNode` actually mines (symmetry with strengthened Kill check). HUD throttle 0.15→0.08. LightningStorm single-instance constraint documented in code. **2 genuinely-new mechanic skills (dedicated strategies, not delegation)**: `ChainLightning` (SkillType 35, enemy→adjacent bounce ×5, 20% falloff/jump) and `LifeDrain` (36, single hit + heal 1/3 of damage) using only existing ISkillTarget surface; `.tres`+skillbooks in mountain/harbor shops. Residual (honest): non-Gather progress keeps game-wide autosave durability; v2 8 skills remain reskins (only v3's 2 are new mechanics).
  - **7-agent adversarial hardening v4 (2026-05-19, DONE)**: Closed two economy ship-blockers. **Gather = Option A**: `NotifyItemAcquired` now sets progress from *current inventory count* (not accumulate); `Complete()` for Gather re-validates `HasItems` and `ConsumeItems(Goal)` inside the transaction (insufficient → fail + recompute). Kills the buy/rebuy + mining-double-feed + amount-undercount infinite-money exploit. **enhance_stone** (Price=0 gated currency) removed from all repeatable contracts (gold compensated), kept only on one-time non-repeatable boss bounty qty1; `balance.py` **B5** makes any repeatable contract granting enhance_stone a hard error. `SaveManager.SaveGame()` now also defers when `_autoSaveSuspendCount>0` (no mid-transaction disk write). `BackfillRegionFlagsV13` runs unconditionally (idempotent; fixes pre-fix v13 local saves — Codex P3). `Accept` → immediate `SaveGame()` (Codex P3, consistency). New skill resources git-added (Codex P1). `RegionDropChance` tiered: common 0.05 / sapphire 0.03 / upgrade-tier drake_scale·glacier_shard·crystal_ore 0.02 (EnemyStats comment corrected — it is NOT old-rate-neutral, it's a DropChance-independent faucet). `ChainLightning` pool widened to `range+maxJumps*jumpRange`. Contract boards moved off hub center axis (field_outpost 520,150 / harbor·mountain 420,140). HUD: popup outside-tap dismiss + rebuild only on active-set signature change; minimap auto-hidden on single-screen hubs (≤760×440). validate **R13** (skill .tres Type global-unique + chain_lightning=35/life_drain=36) and **R14** (RepeatableBossIds set must equal scene RepeatableBoss=true BossIds) + R6/R10 ItemData script_class checks for RegionDrop/contract reward·target.
  - **Regional shop redistribution v2 + recipes v2 (2026-05-19, DONE)**: town defaults 14 commons/uncommons, field_outpost mid-tier, harbor_village coast (composite_bow/tide/storm), mountain_refuge frost/flame high-tier. `IsShopBlocked=true` (hunter_bow/composite_bow) removed from regional shops; winter_bow/phoenix_bow added to mountain. `ShopUI` class filter widened to all equipment (not just Weapon). `recipes.json` +2 (town_leather_armor / town_field_remedy). `shop_npc.tscn` ShopName "기본 장비 상점". Data-only — no SaveData/code changes.
  - **World portal alignment v1 (2026-05-20, DONE)**: 75-portal audit across 29 scenes. Cosmology decision: **outpost-west-of-town** (mirror-consistent both sides). `outpost.PortalToTown` LEFT(40,180)→RIGHT-TOP(600,80). `town.PortalToOutpost` spawn (100,180)→(520,120) inside outpost RIGHT. 10 spawn-coord fixes for ping-pong/spawn-on-portal (field_5/6 → field_3 spawn off `PortalToDungeon3`; 8 adjacent-portal proximities ≥150px). Mountain BossKill contracts +3 (glacier_titan_f5 / inferno_drake_f6 / crystal_lord_m3 — RepeatableBoss, gold/exp only). validate **R16** (ShopItems must not contain `IsShopBlocked=true` or `Price≤0`; Price-line missing treated as 0). Portal trigger is **[F]-press only** via `BaseInteractable._UnhandledInput` (not auto-overlap) — proximity = visual disambiguation concern, not ping-pong.
  - **World integration pass v2 + validators R17/R18/R19/B6 (2026-05-20, DONE)**: Recipe gold fixes 3 (mana_brew 25→45G, enhance_stone_craft 60→200G, antidote_craft 15→65G). Validators **R17** (portal spawn within 50px of non-pair portal in target scene = error; 50~150px auto warning, exit-code-neutral; round-trip pair excluded; 0 errors / 9 warnings current state), **R18** (recipe arbitrage: `result.SellPrice × qty ≤ gold + Σ material.SellPrice × qty`, recipes.json `_rule` annotated, SellPrice missing → 0), **R19** (scene RepeatableBoss=true BossId must appear in contracts.json BossKill targetBossId — drift safety net beyond R14 / hardcoded xUnit). **B6** (repeatable contract total reward (gold + rewardItem SellPrice × **rewardItemQuantity**) / level > 100 error, > 70 warn; gold-only contracts also checked; key matches runtime `HuntingContractManager` — earlier `rewardItemQty` typo fixed). xUnit +3 (44 total): `EveryRegion_HasAtLeastOneKillContract` / `EveryRepeatableBoss_HasContract` / `RepeatableContracts_GoldPerLevel_WithinCap`. New PLAN_DOC: `world-map-layout-v2.md`, `boss-farming-v2.md`, `mining-loop-v2.md`, `hunting-journal-v1.md` (design only; Phase 1–3 deferred — static data + UI + NPC), `economy-balance-rules-v1.md` (currency taxonomy, price tiers, gate currency = enhance_stone, validator rule index). Identified orphan ores (copper/platinum/ruby/sapphire/deep_ore — no recipe/contract use; documented in mining-loop-v2). No code/scene/SaveData changes outside the recipe gold tweaks.
- Inventory/equipment/affix: `Scripts/Data/Inventory.cs`, `AffixGenerator.cs`, `ItemData.cs`
- Quests: `Scripts/Core/QuestManager.cs`, `Scripts/Data/QuestData.cs`
- Mobile controls: `Scripts/UI/MobileControls.cs`, `VirtualJoystick.cs`, `VirtualInput.cs`
- Balance: `Resources/Balance/game_balance.json`

## Stability Rules
- Save/scene/inventory code is sensitive. Use `GameTransaction` for multi-step mutations that combine inventory, rewards, gold, pending rewards, or scene changes.
- `PlayerGold` changes do not automatically imply a durable save unless the code explicitly requests one. Check save timing around shops, drops, rewards, and teleport/return flows.
- Enemy `Stats` resources must be duplicated before runtime mutation.
- Boss defeat IDs must stay unique per boss/zone. Do not reuse a shared boss key across dungeons.
- Be careful with boss farming semantics. `DefeatedBosses` has historically represented first-time defeat/unlock state; do not claim repeatable boss farming works unless code explicitly supports it.
- Godot `.tscn` / `.tres` connections can fail at runtime even when `dotnet build` succeeds. Always validate scene paths, ext_resource paths, exported property names, and node names for scene-heavy changes.
- Mobile back button, app pause, and focus loss should preserve progress.

## Current Review Hotspots
- Large Claude scene-generation batches need path/export checks: `TargetScenePath`, `StatVariants`, `MaxEnemies`, `SpawnRadius`, `BossStatVariant`, `BossId`, `ShopItems`, `SkillBooks`, save points, and spawn positions.
- `EnemySpawner` uses `StatVariants`, `StatWeights` (v3, optional weighted spawn), `SpawnInterval`, `MaxEnemies`, `SpawnRadius`, `SpawnAroundPlayer`, `MinSpawnDistance`, `BossStatVariant`, and `BossId`. Old scene properties such as `EnemyStatsList`, `SpawnCount`, `SpawnAreaSize`, and `SpawnAreaOffset` should not be used.
- v3 weather/spawn: see `PLAN_DOC/regional-weather-hunting-v3.md`. New themed enemies/consumables reuse existing PNG/icons temporarily (recorded there, not in generated-asset-inventory). `StatWeights` must match `StatVariants` length with sum > 0 or it falls back to uniform. Hub scenes must not get hazard weather.
- If status effects are touched, check both melee and ranged paths. Ranged enemies need status data passed through `EnemyProjectile`; player skills need explicit status application if the design says they inflict Freeze/Burn/Curse/etc.
- If skill shops are redistributed, verify actual `SkillShopUI.SkillBooks` arrays in each scene, not only documentation.
- If documentation says a feature is complete, confirm the implementation matches. Keep TODO sections current after fixes.
- Portals trigger on `[F]`/interact press only (`BaseInteractable._UnhandledInput`, action "interact"), not on body overlap. Proximity between portals is a visual-disambiguation concern (prompt label legibility, accidental press), not an auto-teleport ping-pong concern. R17 enforces ≥50px to non-pair portals; round-trip pairs are intentionally close.

## Asset Pipeline Rules
- GPT-generated assets are managed under `Resources/Generated/GPT/`.
- Source sheets should be preserved under `Resources/Generated/GPT/SourceSheets/`.
- Sliced/runtime assets should be placed in category folders such as `Icons`, `Enemies`, `Objects`, `Projectiles`, and `Effects`.
- Update `PLAN_DOC/generated-asset-inventory.md` whenever generated assets are registered, skipped, connected, or marked resource-only.
- Avoid duplicate generation. Check `PLAN_DOC/generated-asset-inventory.md` before proposing new image prompts.
- Do not overwrite existing PNG, `.tres`, `.tscn`, or source sheets unless the user explicitly requests replacement.
- Root-level `ChatGPT Image*.png` files should not be tracked or exported.
- For enemies in the current prototype phase, single idle sprites plus runtime `FlipH` are acceptable unless the user asks for directional animation.

## Current Art Direction
- GPT image pipeline is preferred for icons, UI icons, projectiles, effects, single world objects, NPCs, and prototype enemy sprites.
- Keep Franuka or other paid asset bundles on hold unless replacing tilesets or full 4-direction character/monster animation sets.
- Player has GPT-generated walk/attack animation experiments; enemies are mostly single-sprite resources.
- Avoid expanding trap systems or complex map gimmicks unless the user explicitly asks.

## Validation
- For code changes, run:
  - `python3 tools/validate/validate.py` — R1~R19 (Storm 3, 2026-05-20): R3 `StatWeights` length==`StatVariants` sum>0; R9 recipes integrity; R10 contracts (Kill targets vs scene-spawned enemies, BossKill bosses, Mining ores); R13 skill type uniqueness; R14 `RepeatableBossIds` code ↔ scene; R16 `ShopItems` no `IsShopBlocked`/`Price≤0` (Price missing → 0); **R17 portal spawn 50px to non-pair portal = error, 50~150px = auto warning**; **R18 recipe arbitrage (`result.SellPrice × qty ≤ gold + Σ mat.SellPrice × qty`)**; **R19 scene RepeatableBoss BossId ↔ contracts.json BossKill targetBossId coverage**.
  - `python3 tools/validate/balance.py` — B1 spawned enemy drops nonempty; B2 difficulty (hp×atk) ↔ expMul anti-inversion; B3 zone mul positive; B4 scene-referenced boss has BossId; B5 repeatable contract cannot grant `enhance_stone` (gate currency); **B6 repeatable contract total reward (`gold + rewardItem SellPrice × rewardItemQuantity`) / level > 100 error, > 70 warn; gold-only contracts also covered**.
  - `dotnet build first_game.csproj -c Debug --nologo`
  - `dotnet test tools/Tests/FirstGame.Tests.csproj --nologo` (44 tests; ContractsJsonTests covers region Kill coverage, repeatable-boss exposure, gold-per-level cap; FeatureGateTests, MapNamesTests, MapCoverageTests, ChapterDialogueTests, EnumStabilityTests, ContractTests)
  - `git diff --check`
- For scene/world changes, also verify:
  - all new `.tscn` files exist and are not empty placeholder-only scenes
  - all `TargetScenePath` values point to existing scenes
  - all `ext_resource path="res://..."` references exist
  - all enemy spawners use current exported property names and have non-empty `StatVariants`
  - `game_balance.json` contains zone keys matching scene filenames when zone scaling is expected
  - shop/skill shop arrays are populated and not accidentally concentrated in one hub
- For asset registration, also verify:
  - paths exist
  - `.import` files are generated when Godot has imported them
  - `generated-asset-inventory.md` status matches actual usage
  - Android export excludes source sheets and raw ChatGPT originals
- For mobile-facing changes, recommend or perform checklist testing against `PLAN_DOC/mobile-checklist.md`.

## Documentation Notes
- `CLAUDE.md` is Claude Code's instruction file. Do not update it unless the user specifically asks.
- `AGENTS.md` is the Codex-facing instruction file.
- Some `PLAN_DOC` files are historical and may be stale. Prefer actual code and these current references:
  - `PLAN_DOC/README.md`
  - `PLAN_DOC/generated-asset-inventory.md`
  - `PLAN_DOC/enemy-zone-plan.md`
  - `PLAN_DOC/mobile-checklist.md`
  - `PLAN_DOC/hub-preparation-loop-v1.md` (hub loop / contracts v1~v4 history)
  - `PLAN_DOC/shop-system.md` (regional shop v2)
  - `PLAN_DOC/world-map-layout-v1.md` + `world-map-layout-v2.md` (75-portal audit, cosmology, R17 status)
  - `PLAN_DOC/boss-farming-v2.md` (repeatable boss 4: kraken_d4 / glacier_titan_f5 / inferno_drake_f6 / crystal_lord_m3)
  - `PLAN_DOC/mining-loop-v2.md` (ore↔recipe/contract matrix, orphan ores)
  - `PLAN_DOC/hunting-journal-v1.md` (design-only; not implemented yet)
  - `PLAN_DOC/economy-balance-rules-v1.md` (currency taxonomy, validator rule index R16~R18 + B5~B6)
