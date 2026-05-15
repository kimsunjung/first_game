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

## Current Game Direction
- Genre: top-down mobile action RPG / dungeon crawler.
- Goal: store release as a personal project, not monetization-first.
- Current phase: core systems exist; focus is mobile playability, content loop, asset integration, and regression safety.
- Main loop: quest and exploration -> combat/mining -> item/gold pickup -> inventory/equipment/affix farming -> weapon enhancement -> harder zones/bosses -> save/return/teleport.
- Keep feature additions pragmatic. Avoid large rewrites unless a localized design has become unmaintainable.

## Current World Structure
- Hub: `Scenes/Maps/town.tscn`
- Fields: `field_1`, `field_2`, `field_3`, `field_outpost`
- Dungeons: `dungeon_1`, `dungeon_2`, `dungeon_3`
- Mines: `mine_1`, `mine_2`
- Enemy placement and intended zone identity are tracked in `PLAN_DOC/enemy-zone-plan.md`.

## Important Systems
- Autoloads: `GameManager`, `AudioManager`, `SceneManager`
- Save/load: `Scripts/Core/SaveManager.cs`, `Scripts/Data/SaveData.cs`, `Scripts/Core/GameTransaction.cs`
- Player: `Scripts/Entities/Player/PlayerController*.cs`
- Enemies: `Scripts/Entities/Enemies/EnemyController.cs`, `EnemySpawner.cs`, `EnemyProjectile.cs`
- Inventory/equipment/affix: `Scripts/Data/Inventory.cs`, `AffixGenerator.cs`, `ItemData.cs`
- Quests: `Scripts/Core/QuestManager.cs`, `Scripts/Data/QuestData.cs`
- Mobile controls: `Scripts/UI/MobileControls.cs`, `VirtualJoystick.cs`, `VirtualInput.cs`
- Balance: `Resources/Balance/game_balance.json`

## Stability Rules
- Save/scene/inventory code is sensitive. Use `GameTransaction` for multi-step mutations that combine inventory, rewards, gold, pending rewards, or scene changes.
- `PlayerGold` changes do not automatically imply a durable save unless the code explicitly requests one. Check save timing around shops, drops, rewards, and teleport/return flows.
- Enemy `Stats` resources must be duplicated before runtime mutation.
- Boss defeat IDs must stay unique per boss/zone. Do not reuse a shared boss key across dungeons.
- Mobile back button, app pause, and focus loss should preserve progress.

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
  - `dotnet build`
  - `git diff --check`
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

