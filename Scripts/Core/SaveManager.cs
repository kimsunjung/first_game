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

		// 일반 적 처치 같은 빈번한 자동 저장 호출의 throttle 간격(ms).
		// 보스 처치/씬 전환/수동 저장은 RequestAutoSave를 우회하므로 즉시 저장됨.
		private const ulong AutoSaveThrottleMs = 30_000;
		private static ulong _lastAutoSaveMs = 0;
		// throttle 내 RequestAutoSave는 dirty 표시만 — 만료 후 TickDirty에서 자동 flush.
		// 종료 다이얼로그 '예' 직전에도 FlushDirtySave를 호출해 진행 손실을 막는다.
		private static bool _dirty = false;

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
				Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
				PendingRewardItems = new System.Collections.Generic.List<SavedItemSlot>(GameManager.Instance.PendingRewards)
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
				_lastAutoSaveMs = Godot.Time.GetTicksMsec();
				_dirty = false;
				GD.Print($"게임이 저장되었습니다: {slot}");
			}
			catch (Exception e)
			{
				GD.PrintErr($"SaveManager: 저장 실패 - {e.Message}");
			}
			return data;
		}

		/// <summary>
		/// 빈번하게 호출되는 자동 저장(예: 일반 적 처치). throttle 간격 내면 dirty 표시만 — 만료 시
		/// TickDirty 또는 다음 RequestAutoSave 호출에서 자동 flush.
		/// 보스 처치/씬 전환/수동 저장은 SaveGame을 직접 호출해 즉시 저장한다.
		/// </summary>
		public static void RequestAutoSave(string slot = AutoSaveSlot)
		{
			ulong nowMs = Godot.Time.GetTicksMsec();
			if (_lastAutoSaveMs > 0 && nowMs - _lastAutoSaveMs < AutoSaveThrottleMs)
			{
				_dirty = true;
				return;
			}
			SaveGame(slot);
		}

		/// <summary>
		/// 주기적으로 호출(GameManager._Process). throttle 만료 + dirty면 자동 저장.
		/// 일반몹 처치 후 아무 행동 없이 앱이 종료되더라도 30초 뒤에는 보존됨.
		/// </summary>
		public static void TickDirty(string slot = AutoSaveSlot)
		{
			if (!_dirty) return;
			ulong nowMs = Godot.Time.GetTicksMsec();
			if (_lastAutoSaveMs == 0 || nowMs - _lastAutoSaveMs >= AutoSaveThrottleMs)
				SaveGame(slot);
		}

		/// <summary>
		/// 종료 직전 호출 — dirty 상태면 throttle 무시하고 즉시 저장.
		/// 모바일 뒤로가기 종료 다이얼로그 '예' 누르기 직전 진행 손실 차단.
		/// </summary>
		public static void FlushDirtySave(string slot = AutoSaveSlot)
		{
			if (!_dirty) return;
			SaveGame(slot);
		}

		/// <summary>
		/// 종료/백그라운드 진입 등 외부 트리거에서 호출. dirty 여부와 무관하게
		/// player가 있으면 무조건 저장 — 퀘스트 완료/상점/장비 등 dirty flag 미설정
		/// 변경도 보존되도록 한다.
		/// </summary>
		public static void FlushBeforeExit(string slot = AutoSaveSlot)
		{
			if (GameManager.Instance?.Player == null) return;
			SaveGame(slot);
		}

		/// <summary>새 게임 시작 시 throttle/dirty 초기화. 다음 RequestAutoSave가 즉시 통과한다.</summary>
		public static void ResetAutoSaveThrottle()
		{
			_lastAutoSaveMs = 0;
			_dirty = false;
		}

		/// <summary>씬 전환용: 저장 후 PendingLoadData에 메모리에서 직접 할당</summary>
		public static void SaveAndSetPending(string slot = AutoSaveSlot)
		{
			var data = SaveGame(slot);
			if (data != null) PendingLoadData = data;
		}

		/// <summary>씬 전환 후 CurrentScene과 스폰 위치를 목적지 기준으로 덮어씀 (메모리 + 파일)</summary>
		public static void OverrideCurrentScene(string scenePath, Godot.Vector2 spawnPosition, string slot = AutoSaveSlot)
		{
			if (PendingLoadData == null) return;
			PendingLoadData.CurrentScene = scenePath;
			PendingLoadData.PlayerPosX = spawnPosition.X;
			PendingLoadData.PlayerPosY = spawnPosition.Y;
			try
			{
				string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
				if (!File.Exists(path)) return;
				File.WriteAllText(path, JsonSerializer.Serialize(PendingLoadData, new JsonSerializerOptions { WriteIndented = true }));
			}
			catch (Exception e)
			{
				GD.PrintErr($"SaveManager: CurrentScene 업데이트 실패 - {e.Message}");
			}
		}

		public static void LoadGame(string slot = null)
		{
			if (slot == null)
			{
				slot = SelectNewerSaveSlot();
				if (slot == null)
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

			// 저장된 씬으로 이동 (없으면 마을로)
			string targetScene = PendingLoadData?.CurrentScene;
			if (string.IsNullOrEmpty(targetScene))
				targetScene = "res://Scenes/Maps/town.tscn";
			tree.ChangeSceneToFile(targetScene);
		}

		/// <summary>씬 전환 시 사용. 파일에서 읽어 PendingLoadData에 넣되, 씬 리로드는 하지 않음.</summary>
		public static void LoadIntoPending(string slot = null)
		{
			if (slot == null)
			{
				slot = SelectNewerSaveSlot();
				if (slot == null) return;
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
					data.CurrentScene = "res://Scenes/Maps/town.tscn";
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

			if (data.Version < 4)
			{
				// v3→v4: 부위별 슬롯(Helmet/Boots/Necklace/Ring1/Ring2/Bracelet) 추가.
				// 누락 path는 빈 문자열로 자동 초기화 (SaveData 기본값 = "").
				// 구 Accessory의 Necklace/Ring 재분류 + 강화 +N 보존은
				// PlayerController.LoadFromSaveData → Inventory.RestoreFromSaveData에서 처리.
				data.EquippedHelmetPath ??= "";
				data.EquippedBootsPath ??= "";
				data.EquippedNecklacePath ??= "";
				data.EquippedRing1Path ??= "";
				data.EquippedRing2Path ??= "";
				data.EquippedBraceletPath ??= "";
			}

			data.Version = SaveData.LatestVersion;
			GD.Print($"SaveManager: 세이브 데이터 v{data.Version}으로 마이그레이션 완료");
		}

		public static bool HasSave(string slot = AutoSaveSlot)
		{
			string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
			return File.Exists(path);
		}

		/// <summary>
		/// manual/autosave 둘 다 있으면 파일 mtime 최신인 쪽을 자동 선택. 둘 중 하나만 있으면
		/// 그 슬롯, 둘 다 없으면 null. "이어하기"가 항상 가장 최근 진행을 로드하도록.
		/// FlushBeforeExit가 autosave에만 저장한 후 stale manual save가 우선 로드되는 결함 방지.
		/// </summary>
		private static string SelectNewerSaveSlot()
		{
			bool hasManual = HasSave(ManualSaveSlot);
			bool hasAuto = HasSave(AutoSaveSlot);
			if (hasManual && hasAuto)
			{
				string pathManual = ProjectSettings.GlobalizePath(SaveDir + ManualSaveSlot + ".json");
				string pathAuto = ProjectSettings.GlobalizePath(SaveDir + AutoSaveSlot + ".json");
				DateTime tManual = File.GetLastWriteTime(pathManual);
				DateTime tAuto = File.GetLastWriteTime(pathAuto);
				return tManual >= tAuto ? ManualSaveSlot : AutoSaveSlot;
			}
			if (hasManual) return ManualSaveSlot;
			if (hasAuto) return AutoSaveSlot;
			return null;
		}
	}
}
