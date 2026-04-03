using Godot;
using System;
using System.IO;
using System.Text.Json;
using FirstGame.Data;
using FirstGame.Core.Interfaces;

namespace FirstGame.Core
{
    public static class SaveManager
    {
        private const string SaveDir = "user://saves/";
        private const string AutoSaveSlot = "autosave";
        private const string ManualSaveSlot = "manual";

        public static SaveData PendingLoadData { get; set; } = null;

        public static event Action OnGameSaved;

        public static SaveData SaveGame(string slot = AutoSaveSlot)
        {
            // GameManager.Player를 통해 ISaveable 획득 (GetNodesInGroup 제거)
            var player = GameManager.Instance?.Player;
            if (player == null || player is not ISaveable saveable) return null;

            var data = new SaveData
            {
                PlayerGold = GameManager.Instance.PlayerGold,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 각 시스템이 자신의 데이터를 기록
            saveable.WriteSaveData(data);

            try
            {
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
            catch (Exception e)
            {
                GD.PrintErr($"SaveManager: 저장 실패 - {e.Message}");
            }
            return data;
        }

        /// <summary>씬 전환용: 저장 후 PendingLoadData에 메모리에서 직접 할당</summary>
        public static void SaveAndSetPending(string slot = AutoSaveSlot)
        {
            var data = SaveGame(slot);
            if (data != null) PendingLoadData = data;
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

            try
            {
                string json = File.ReadAllText(path);
                PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);
                if (PendingLoadData != null)
                    MigrateSaveData(PendingLoadData);
            }
            catch (Exception e)
            {
                GD.PrintErr($"SaveManager: 로드 실패 - {e.Message}");
                PendingLoadData = null;
            }

            var tree = (SceneTree)Engine.GetMainLoop();
            tree.Paused = false;

            // 마을(허브) 씬으로 이동
            tree.ChangeSceneToFile("res://Scenes/Maps/town.tscn");
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
            try
            {
                string json = File.ReadAllText(path);
                PendingLoadData = JsonSerializer.Deserialize<SaveData>(json);
                if (PendingLoadData != null)
                    MigrateSaveData(PendingLoadData);
            }
            catch (Exception e)
            {
                GD.PrintErr($"SaveManager: LoadIntoPending 실패 - {e.Message}");
                PendingLoadData = null;
            }
        }

        private static void MigrateSaveData(SaveData data)
        {
            if (data.Version >= SaveData.LatestVersion) return;

            if (data.Version < 2)
            {
                // v1→v2: 월드 상태 필드 초기화
                if (string.IsNullOrEmpty(data.CurrentScene))
                    data.CurrentScene = "res://Scenes/Maps/grassland.tscn";
                if (data.DayTime == 0f)
                    data.DayTime = 0.3f;
                data.DefeatedBosses ??= new();
            }

            if (data.Version < 3)
            {
                // v2→v3: 강화 시스템 + 보물상자 필드 초기화
                data.EquippedWeaponEnhancement = 0;
                data.EquippedArmorEnhancement = 0;
                data.EquippedAccessoryEnhancement = 0;
                data.OpenedChests ??= new();
            }

            data.Version = SaveData.LatestVersion;
            GD.Print($"SaveManager: 세이브 데이터 v{data.Version}으로 마이그레이션 완료");
        }

        public static bool HasSave(string slot = AutoSaveSlot)
        {
            string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
            return File.Exists(path);
        }
    }
}
