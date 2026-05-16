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
- Status resist: `CharacterStats.StatusResist` (0~0.85 clamp via `PlayerStats.ModifyStatusResist`); `ItemData.BonusStatusResist` (equip) / `BuffStatusResist` (consumable Buff, `ApplyBuffEx` optional arg).
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
  - `python3 tools/validate/validate.py` and `python3 tools/validate/balance.py` (CI gates; validate.py R3 checks `StatWeights` length==`StatVariants` and sum>0)
  - `dotnet build first_game.csproj -c Debug --nologo`
  - `dotnet test tools/Tests/FirstGame.Tests.csproj --nologo`
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
