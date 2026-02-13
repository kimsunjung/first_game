using Godot;
using System;
using System.IO;
using System.Text.Json;
using FirstGame.Data;
using FirstGame.Entities.Player;
using System.Collections.Generic;

namespace FirstGame.Core
{
    public static class SaveManager
    {
        private const string SaveDir = "user://saves/";
        private const string AutoSaveSlot = "autosave";
        private const string ManualSaveSlot = "manual";

        // 씬 리로드 후 적용할 데이터 (static이라 씬 전환에도 유지됨)
        public static SaveData PendingLoadData { get; set; } = null;

        // HUD에서 구독하여 "저장됨!" 알림 표시
        public static event Action OnGameSaved;

        public static void SaveGame(string slot = AutoSaveSlot)
        {
            var tree = (SceneTree)Engine.GetMainLoop();
            var players = tree.GetNodesInGroup("Player");
            if (players.Count == 0) return;

            var player = players[0] as Node2D;
            var playerCtrl = players[0] as PlayerController;
            if (playerCtrl == null) return;

            var data = new SaveData
            {
                PlayerPosX = player.GlobalPosition.X,
                PlayerPosY = player.GlobalPosition.Y,
                PlayerHealth = playerCtrl.Stats.CurrentHealth,
                PlayerMaxHealth = playerCtrl.Stats.MaxHealth,
                PlayerGold = GameManager.Instance.PlayerGold,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 인벤토리 저장 (Save Inventory)
            data.InventoryItems = new List<SavedItemSlot>();
            foreach (var invSlot in playerCtrl.Inventory.Slots)
            {
                data.InventoryItems.Add(new SavedItemSlot
                {
                    ItemPath = invSlot.Item.ResourcePath,
                    Quantity = invSlot.Quantity
                });
            }

            if (playerCtrl.Inventory.EquippedWeapon != null)
                data.EquippedWeaponPath = playerCtrl.Inventory.EquippedWeapon.ResourcePath;
            if (playerCtrl.Inventory.EquippedArmor != null)
                data.EquippedArmorPath = playerCtrl.Inventory.EquippedArmor.ResourcePath;

            // 디렉토리 생성
            DirAccess.MakeDirRecursiveAbsolute(
                ProjectSettings.GlobalizePath(SaveDir)
            );

            // JSON 파일 저장
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);

            OnGameSaved?.Invoke();
            GD.Print($"게임이 저장되었습니다: {slot}");
        }

        public static void LoadGame(string slot = null)
        {
            // slot이 null이면 최신 저장 파일을 찾음 (수동 > 자동 우선)
            if (slot == null)
            {
                if (HasSave(ManualSaveSlot)) slot = ManualSaveSlot;
                else if (HasSave(AutoSaveSlot)) slot = AutoSaveSlot;
                else
                {
                    GD.Print("저장된 파일이 없습니다. 새로 시작합니다.");
                    var t = (SceneTree)Engine.GetMainLoop();
                    t.Paused = false;
                    t.ReloadCurrentScene();
                    return;
                }
            }

            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            string json = File.ReadAllText(path);
            PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);

            var tree = (SceneTree)Engine.GetMainLoop();
            tree.Paused = false;
            tree.ReloadCurrentScene();
        }

        public static bool HasSave(string slot = AutoSaveSlot)
        {
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            return File.Exists(path);
        }
    }
}
