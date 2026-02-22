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

        public static SaveData PendingLoadData { get; set; } = null;

        public static event Action OnGameSaved;

        public static void SaveGame(string slot = AutoSaveSlot)
        {
            var tree = (SceneTree)Engine.GetMainLoop();
            var players = tree.GetNodesInGroup("Player");
            if (players.Count == 0) return;

            var playerCtrl = players[0] as PlayerController;
            if (playerCtrl == null) return;

            var player = players[0] as Node2D;

            var data = new SaveData
            {
                PlayerPosX = player.GlobalPosition.X,
                PlayerPosY = player.GlobalPosition.Y,
                PlayerHealth = playerCtrl.Stats.CurrentHealth,
                PlayerMaxHealth = playerCtrl.Stats.MaxHealth,
                PlayerMp = playerCtrl.Stats.CurrentMp,
                PlayerLevel = playerCtrl.Stats.Level,
                PlayerExp = playerCtrl.Stats.Exp,
                StatPoints = playerCtrl.Stats.StatPoints,
                StrPoints = playerCtrl.Stats.StrPoints,
                ConPoints = playerCtrl.Stats.ConPoints,
                IntPoints = playerCtrl.Stats.IntPoints,
                PlayerGold = GameManager.Instance.PlayerGold,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 인벤토리 저장
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

            // 퀵슬롯 저장
            data.QuickSlotPaths = new List<string>();
            foreach (var item in playerCtrl.Inventory.QuickSlots)
            {
                data.QuickSlotPaths.Add(item != null ? item.ResourcePath : "");
            }

            // 습득한 스킬 저장
            data.LearnedSkillPaths = new List<string>();
            foreach (var skill in playerCtrl.Stats.LearnedSkills)
            {
                if (!string.IsNullOrEmpty(skill.ResourcePath))
                    data.LearnedSkillPaths.Add(skill.ResourcePath);
            }

            DirAccess.MakeDirRecursiveAbsolute(
                ProjectSettings.GlobalizePath(SaveDir)
            );

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

            if (!File.Exists(path))
            {
                GD.PrintErr($"SaveManager: Save file not found at {path}");
                return;
            }

            string json = File.ReadAllText(path);
            PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);

            var tree = (SceneTree)Engine.GetMainLoop();
            tree.Paused = false;
            tree.ReloadCurrentScene();
        }

        /// <summary>씬 전환 시 사용. 파일에서 읽어 PendingLoadData에 넣되, 씬 리로드는 하지 않음.</summary>
        public static void LoadIntoPending(string slot = null)
        {
            if (slot == null)
            {
                if (HasSave(ManualSaveSlot)) slot = ManualSaveSlot;
                else if (HasSave(AutoSaveSlot)) slot = AutoSaveSlot;
                else return;
            }
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);
        }

        public static bool HasSave(string slot = AutoSaveSlot)
        {
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            return File.Exists(path);
        }
    }
}
