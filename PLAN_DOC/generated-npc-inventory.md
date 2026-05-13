# Generated GPT NPC Inventory

작성: 2026-05-13
배경: GPT로 생성한 마을 NPC PNG와 씬 매핑 추적용. 적 인벤토리는 [enemy-zone-plan.md](enemy-zone-plan.md) 참조.

## 시트 1: town_services_2026_05_13 (8 NPC)

원본: `Resources/Generated/GPT/SourceSheets/NPCs/source_npcs_town_services_2026_05_13.png`
슬라이서: `Resources/Generated/GPT/SourceSheets/NPCs/slice_npcs_town.py`
출력: `Resources/Generated/GPT/NPCs/Town/<name>.png` (128×128 정사각, 양옆 padding 50px + connected-component blob)

| # | PNG | 기존 NPC 씬 매핑 | 상태 |
|---|---|---|---|
| 1 | `general_merchant.png` | `Scenes/Objects/shop_npc.tscn` (ShopNPC) | **적용 완료** — Sprite2D 텍스처 교체, scale 0.4, modulate 흰색 |
| 2 | `blacksmith.png` | `Scenes/Objects/blacksmith_npc.tscn` (BlacksmithNPC) | **적용 완료** |
| 3 | `skill_master.png` | `Scenes/Objects/skill_shop_npc.tscn` (SkillShopNPC) | **적용 완료** |
| 4 | `teleport_guide.png` | `Scenes/Objects/teleport_npc.tscn` (TeleportNPC) | **적용 완료** |
| 5 | `quest_elder.png` | (대응 씬 없음) | **PNG만 등록** — 퀘스트 시스템 도착 시 사용 |
| 6 | `healer.png` | (대응 씬 없음) | **PNG만 등록** — 회복 NPC 도입 시 사용 |
| 7 | `storage_keeper.png` | (대응 씬 없음) | **PNG만 등록** — 창고 시스템 도착 시 사용 |
| 8 | `mining_foreman.png` | (대응 씬 없음) | **PNG만 등록** — 광산 퀘스트/관리 NPC 후보 |

기존 NPC 기능(상점/스킬상점/대장간/텔레포트/material_shop) 로직·UI는 무변경. Sprite2D `texture` / `scale` / `modulate`만 교체. CollisionShape2D 반경(80) 유지.

## 마을 사이즈 축소 (2026-05-13)

`Scenes/Maps/town.tscn` 800×480 → **640×360**. NPC 배치 간격 96 → 80px로 압축. 플레이어/포털 좌표 비례 축소. NPC 4개 + SavePoint + MaterialShopNPC 6개 모두 y=96 단일 줄에 유지.

## 알려진 제약

- NPC 표시 크기는 SpriteScale 0.4 (128 → ~51px). 플레이테스트 후 미세 조정 필요.
- `material_shop_npc.tscn`은 이전 PR에서 만든 별도 NPC — 이번 4개 매핑에 포함 안 됨. 추후 별도 PNG로 분리 가능.
- 미적용 4종(quest_elder/healer/storage_keeper/mining_foreman)은 PNG만 있고 .tres·씬 없음. 기능 도입 시 새 .tscn 작성 + Sprite2D에 텍스처 연결.
