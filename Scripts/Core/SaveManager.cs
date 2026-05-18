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
		// 다단계 트랜잭션(예: 귀환 주문서 = RemoveItem → 씬 전환) 도중 OnInventoryChanged 같은
		// 이벤트가 RequestAutoSave를 트리거해 중간 상태를 디스크에 박는 결함 차단. 카운터 기반으로
		// 중첩 안전. SceneManager.ChangeScene이 캡처/저장을 직접 책임지므로, 트랜잭션 동안에는
		// auto-save가 들어와도 dirty만 표시되고 디스크 쓰기는 미루어진다.
		private static int _autoSaveSuspendCount = 0;
		public static void SuspendAutoSave() => _autoSaveSuspendCount++;
		public static void ResumeAutoSave()
		{
			if (_autoSaveSuspendCount > 0) _autoSaveSuspendCount--;
		}

		public static SaveData PendingLoadData { get; set; } = null;

		// 신규 게임 시작 시 MainMenu가 선택한 클래스를 town.tscn 로드 후 PlayerController가 읽음.
		// null이면 Warrior(기본) 사용.
		public static FirstGame.Data.PlayerClass? PendingNewGameClass { get; set; } = null;

		public static event Action OnGameSaved;

		/// <summary>현재 게임 상태에서 SaveData를 구축. 디스크 쓰기 없음.
		/// 씬 트리 의존 코드(예: PlayerController.WriteSaveData가 CurrentScene 읽기)는 여기서
		/// 실행되므로 ChangeScene 전에 호출해야 안전하다.</summary>
		public static SaveData BuildSaveData()
		{
			var player = GameManager.Instance?.Player;
			if (player == null || player is not ISaveable saveable) return null;

			var data = new SaveData
			{
				PlayerGold = GameManager.Instance.PlayerGold,
				Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
				PendingRewardItems = new System.Collections.Generic.List<SavedItemSlot>(GameManager.Instance.PendingRewards),
				StorageItems = new System.Collections.Generic.List<SavedItemSlot>(GameManager.Instance.Storage)
			};
			saveable.WriteSaveData(data);
			return data;
		}

		/// <summary>미리 구축된 SaveData를 디스크에 원자적으로 기록. 성공 시 true.
		/// SceneManager.ChangeScene이 큐잉 성공 후 호출 — 실패한 씬 전환이 save를 더럽히지 않게.</summary>
		public static bool WriteSaveDataToDisk(SaveData data, string slot = AutoSaveSlot)
		{
			if (data == null) return false;
			try
			{
				DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(SaveDir));
				string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
				string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
				WriteAtomic(path, json);
				OnGameSaved?.Invoke();
				_lastAutoSaveMs = Godot.Time.GetTicksMsec();
				_dirty = false;
				GD.Print($"게임이 저장되었습니다: {slot}");
				return true;
			}
			catch (Exception e)
			{
				GD.PrintErr($"SaveManager: 저장 실패 - {e.Message}");
				return false;
			}
		}

		public static SaveData SaveGame(string slot = AutoSaveSlot)
		{
			// 복원 critical section 중에는 절대 저장하지 않는다. LoadFromSaveData 도중
			// OnEquipmentChanged→RequestAutoSave 등이 부분 복원 상태를 디스크에 박는 것을 차단.
			// dirty만 남겨 EndRestoreState 후 TickDirty/다음 RequestAutoSave가 정상 flush.
			if (FirstGame.Core.GameManager.Instance?.IsRestoringState == true)
			{
				_dirty = true;
				return null;
			}
			var data = BuildSaveData();
			if (data == null) return null;
			WriteSaveDataToDisk(data, slot);
			return data;
		}

		/// <summary>
		/// tmp 파일에 쓰고 fsync 후 target으로 원자적 교체. 기존 target은 .bak으로 보존.
		/// 모바일 OS kill / 스토리지 오류로 쓰기 중간에 끊겨도 본 파일이나 .bak 중 하나는 항상 유효.
		/// </summary>
		private static void WriteAtomic(string path, string json)
		{
			string tmp = path + ".tmp";
			string bak = path + ".bak";

			// 1) tmp에 쓰고 디스크 동기화
			using (var fs = new FileStream(tmp, FileMode.Create, System.IO.FileAccess.Write, FileShare.None))
			using (var sw = new StreamWriter(fs))
			{
				sw.Write(json);
				sw.Flush();
				fs.Flush(true); // OS 버퍼 → 디스크
			}

			// 2) target이 이미 있으면 File.Replace로 원자 치환 + .bak 보존
			if (File.Exists(path))
			{
				File.Replace(tmp, path, bak, ignoreMetadataErrors: true);
			}
			else
			{
				// 첫 저장: stale .bak가 남아 있으면 새 게임 첫 저장 후 본 파일이
				// 깨졌을 때 TryReadSaveFile이 이전 세이브를 부활시킬 수 있으므로 삭제.
				if (File.Exists(bak)) File.Delete(bak);
				File.Move(tmp, path);
			}
		}

		/// <summary>
		/// 빈번하게 호출되는 자동 저장(예: 일반 적 처치). throttle 간격 내면 dirty 표시만 — 만료 시
		/// TickDirty 또는 다음 RequestAutoSave 호출에서 자동 flush.
		/// 보스 처치/씬 전환/수동 저장은 SaveGame을 직접 호출해 즉시 저장한다.
		/// </summary>
		public static void RequestAutoSave(string slot = AutoSaveSlot)
		{
			// 트랜잭션 격리 — 다단계 작업(예: 귀환 주문서) 도중에 OnInventoryChanged가
			// RequestAutoSave를 호출해도 중간 상태가 디스크에 박히지 않게 dirty만 표시.
			// Resume 후 다음 RequestAutoSave 또는 TickDirty에서 flush됨.
			if (_autoSaveSuspendCount > 0)
			{
				_dirty = true;
				return;
			}
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
			if (_autoSaveSuspendCount > 0) return; // 트랜잭션 진행 중 — flush 미루기
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

		// SaveAndSetPending/OverrideCurrentScene는 capture-first 패턴 도입 후 호출처가 없어 제거됨.
		// 씬 전환 후 save 동기화는 SceneManager.ChangeScene이 BuildSaveData → 캡처 override →
		// 큐잉 성공 시에만 WriteSaveDataToDisk + PendingLoadData 설정으로 일원화한다.

		/// <summary>슬롯 미지정 로드용 — 선호 슬롯의 본/.bak이 모두 손상돼도 다른 슬롯을
		/// 시도한다. 정상 세이브가 하나라도 있으면 빈 town으로 떨어지지 않게 하는 게 목적.
		/// 명시 슬롯 로드는 이 경로를 쓰지 않고 해당 슬롯만 처리한다.</summary>
		private static SaveData LoadAnySlotWithFallback(string preferred, out string usedSlot)
		{
			usedSlot = null;
			string other = preferred == ManualSaveSlot ? AutoSaveSlot : ManualSaveSlot;
			foreach (var s in new[] { preferred, other })
			{
				string p = ProjectSettings.GlobalizePath(SaveDir + s + ".json");
				// 본/.bak 중 하나라도 존재할 때만 시도 (TryReadSaveFile이 본→.bak 순 처리).
				if (!File.Exists(p) && !File.Exists(p + ".bak")) continue;
				var d = TryReadSaveFile(p);
				if (d != null)
				{
					usedSlot = s;
					if (s != preferred)
						GD.PrintErr($"SaveManager: '{preferred}' 슬롯(본/.bak) 손상 — '{s}' 슬롯으로 폴백 로드 성공");
					return d;
				}
				GD.PrintErr($"SaveManager: '{s}' 슬롯 본/.bak 모두 손상");
			}
			return null;
		}

		public static void LoadGame(string slot = null)
		{
			var tree = (SceneTree)Engine.GetMainLoop();
			tree.Paused = false;

			SaveData data;
			if (slot == null)
			{
				string preferred = SelectNewerSaveSlot();
				if (preferred == null)
				{
					GD.Print("저장된 파일이 없습니다. 새로 시작합니다.");
					tree.ReloadCurrentScene();
					return;
				}
				data = LoadAnySlotWithFallback(preferred, out _);
				if (data == null)
					GD.PrintErr("SaveManager: 모든 슬롯(본/.bak) 손상 — 데이터 없이 마을로 시작");
			}
			else
			{
				// 명시 슬롯은 기존 의미 유지 — 해당 슬롯 본/.bak만 시도(교차 폴백 없음).
				string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
				data = (File.Exists(path) || File.Exists(path + ".bak")) ? TryReadSaveFile(path) : null;
				if (data == null)
					GD.PrintErr($"SaveManager: 명시 슬롯 '{slot}' 본/.bak 모두 손상/없음 — 데이터 없이 마을로 시작");
			}

			PendingLoadData = data;

			// 저장된 씬으로 이동 (데이터 없거나 손상/경로 무효면 마을로)
			const string townScene = "res://Scenes/Maps/town.tscn";
			string targetScene = PendingLoadData?.CurrentScene;
			if (string.IsNullOrEmpty(targetScene) || !ResourceLoader.Exists(targetScene))
				targetScene = townScene;

			// ResetAll은 씬 전환 큐잉 성공 *후*에만 — 실패 시 현재 씬 HUD/Player/Spawner
			// static 이벤트 구독이 살아 있어야 기존 씬이 정상 동작한다. (SceneManager와 동일 정책)
			var err = tree.ChangeSceneToFile(targetScene);
			if (err != Error.Ok && targetScene != townScene)
			{
				GD.PrintErr($"SaveManager: '{targetScene}' 전환 실패(err={err}) — 마을로 재시도");
				err = tree.ChangeSceneToFile(townScene);
			}
			if (err == Error.Ok)
				EventManager.ResetAll();
			else
				GD.PrintErr($"SaveManager: 씬 전환 실패(err={err}) — 현재 씬/구독 유지");
		}

		/// <summary>씬 전환 시 사용. 파일에서 읽어 PendingLoadData에 넣되, 씬 리로드는 하지 않음.</summary>
		public static void LoadIntoPending(string slot = null)
		{
			if (slot == null)
			{
				string preferred = SelectNewerSaveSlot();
				if (preferred == null) return;
				PendingLoadData = LoadAnySlotWithFallback(preferred, out _);
				return;
			}
			string path = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
			if (!File.Exists(path) && !File.Exists(path + ".bak")) return;
			PendingLoadData = TryReadSaveFile(path);
		}

		/// <summary>본 파일 → 깨졌으면 .bak → 둘 다 실패면 null. 마이그레이션도 여기서 적용.</summary>
		private static SaveData TryReadSaveFile(string path)
		{
			SaveData data = ReadAndParse(path);
			if (data != null) return data;

			string bak = path + ".bak";
			if (File.Exists(bak))
			{
				GD.PrintErr($"SaveManager: 본 파일 손상 — .bak에서 복구 시도 ({bak})");
				data = ReadAndParse(bak);
				if (data != null)
				{
					GD.Print("SaveManager: .bak 복구 성공");
					return data;
				}
				GD.PrintErr("SaveManager: .bak도 손상 — 로드 포기");
			}
			return null;
		}

		private static SaveData ReadAndParse(string path)
		{
			try
			{
				string json = File.ReadAllText(path);
				if (string.IsNullOrWhiteSpace(json)) return null;
				var data = JsonSerializer.Deserialize<SaveData>(json);
				if (data != null) MigrateSaveData(data);
				return data;
			}
			catch (Exception e)
			{
				GD.PrintErr($"SaveManager: 파싱 실패 ({path}) - {e.Message}");
				return null;
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

			if (data.Version < 5)
			{
				// v4→v5: 절차적 필드맵 seed dictionary 추가. 누락 시 빈 dictionary —
				// 다음 진입 시 새 seed 발급 후 저장돼 이후 세션부터 일관 유지.
				data.FieldSeeds ??= new System.Collections.Generic.Dictionary<string, int>();
			}

			if (data.Version < 6)
			{
				// v5→v6: 방문한 씬 목록. 기존 유저는 v6 이전엔 방문 이력을 안 남겼으므로
				// CurrentScene 번호 + DefeatedBosses로 진행도를 역추정해 하위 지역을 자동
				// unlock한다. 완벽 복원은 불가하지만 "field_3까지 갔는데 텔레포트 NPC가
				// field_1만 인식" 같은 회귀를 방지.
				data.VisitedScenes ??= new System.Collections.Generic.List<string>();
				BackfillVisitedScenesV6(data);
			}

			if (data.Version < 8)
			{
				// v7→v8: 광산 채광 완료 노드 목록. 누락 시 빈 리스트 — 다음 채광부터 영속화 시작.
				// 광산이 신규 콘텐츠라 v7 이하 세이브엔 채광 기록 자체가 없어 backfill 불필요.
				data.MinedNodes ??= new System.Collections.Generic.List<string>();
			}

			if (data.Version < 9)
			{
				// v8→v9: 광물 리소스 명칭 통일. magnetite.tres("자철석") → silver_ore.tres("은광석").
				// 인벤/보류보상/퀵슬롯에 박혀 있던 구 경로를 일괄 치환 — 세이브 로드 시점에 깨지지 않도록.
				MigrateMagnetiteToSilver(data);
			}

			if (data.Version < 10)
			{
				// v9→v10: 메인 스토리 챕터 플래그 도입. 기존 진행도를 DefeatedBosses와 VisitedScenes
				// 기반으로 backfill — 기존 플레이어가 v10 로드 시 NPC 대사가 Prologue부터 재시작되는
				// 회귀 차단. (PlayerClassId=0=Warrior, DexPoints=0은 JSON 누락 시 자동 0 폴백이라
				// 별도 처리 불필요.)
				BackfillChapterFlagsV10(data);
			}

			if (data.Version < 12)
			{
				// v11→v12: 공유 창고 도입. 기존 세이브는 빈 창고로 안전 로드.
				data.StorageItems ??= new();
			}

			data.Version = SaveData.LatestVersion;
			GD.Print($"SaveManager: 세이브 데이터 v{data.Version}으로 마이그레이션 완료");
		}

		/// <summary>v5→v6 마이그: CurrentScene 번호 + DefeatedBosses 수를 근거로 하위 지역을
		/// 방문 이력에 자동 등록. 텔레포트 NPC가 기존 진행도를 인식하도록 보정.</summary>
		private static void BackfillVisitedScenesV6(SaveData data)
		{
			void Add(string path)
			{
				if (!data.VisitedScenes.Contains(path)) data.VisitedScenes.Add(path);
			}

			Add("res://Scenes/Maps/town.tscn"); // 마을은 항상 도달 가능
			if (!string.IsNullOrEmpty(data.CurrentScene)) Add(data.CurrentScene);

			// CurrentScene이 field_N이면 field_1..N 모두 unlock.
			// dungeon_N이면 field_1..N + dungeon_1..N 모두 unlock (던전 들어갔으니 그 위 필드도 거쳐옴).
			int fieldMax = 0, dungeonMax = 0;
			if (!string.IsNullOrEmpty(data.CurrentScene))
			{
				var m = System.Text.RegularExpressions.Regex.Match(data.CurrentScene, @"field_(\d+)\.tscn$");
				if (m.Success && int.TryParse(m.Groups[1].Value, out int fn)) fieldMax = fn;
				var dm = System.Text.RegularExpressions.Regex.Match(data.CurrentScene, @"dungeon_(\d+)\.tscn$");
				if (dm.Success && int.TryParse(dm.Groups[1].Value, out int dn))
				{
					dungeonMax = dn;
					fieldMax = System.Math.Max(fieldMax, dn);
				}
			}

			// 처치한 보스 수도 진행도 시그널 — N개 처치했으면 dungeon_1..N도 다녀왔다고 본다.
			if (data.DefeatedBosses != null)
				dungeonMax = System.Math.Max(dungeonMax, System.Math.Min(data.DefeatedBosses.Count, 3));
			// 던전까지 갔으면 그 번호의 필드도 거쳐 옴.
			fieldMax = System.Math.Max(fieldMax, dungeonMax);

			for (int i = 1; i <= fieldMax && i <= 3; i++)
				Add($"res://Scenes/Maps/field_{i}.tscn");
			for (int i = 1; i <= dungeonMax && i <= 3; i++)
				Add($"res://Scenes/Maps/dungeon_{i}.tscn");
		}

		private const string OldMagnetitePath = "res://Resources/Items/magnetite.tres";
		private const string NewSilverOrePath = "res://Resources/Items/silver_ore.tres";

		private static void MigrateMagnetiteToSilver(SaveData data)
		{
			if (data.InventoryItems != null)
			{
				foreach (var slot in data.InventoryItems)
					if (slot != null && slot.ItemPath == OldMagnetitePath) slot.ItemPath = NewSilverOrePath;
			}
			if (data.PendingRewardItems != null)
			{
				foreach (var slot in data.PendingRewardItems)
					if (slot != null && slot.ItemPath == OldMagnetitePath) slot.ItemPath = NewSilverOrePath;
			}
			if (data.QuickSlotPaths != null)
			{
				for (int i = 0; i < data.QuickSlotPaths.Count; i++)
					if (data.QuickSlotPaths[i] == OldMagnetitePath) data.QuickSlotPaths[i] = NewSilverOrePath;
			}
		}

		/// <summary>v9→v10 마이그: 챕터 플래그를 DefeatedBosses/VisitedScenes 기반으로 backfill.
		/// 기존 플레이어의 진행도 표지(NPC 대사 단계)가 Prologue로 초기화되지 않도록.</summary>
		private static void BackfillChapterFlagsV10(SaveData data)
		{
			if (data.ChapterFlags == null) data.ChapterFlags = new System.Collections.Generic.List<string>();
			void Add(string flag) { if (!data.ChapterFlags.Contains(flag)) data.ChapterFlags.Add(flag); }

			// VisitedScenes 기반 — field_outpost 또는 그 이후 진입 흔적이 있으면 Prologue 통과.
			if (data.VisitedScenes != null)
			{
				foreach (var s in data.VisitedScenes)
				{
					if (string.IsNullOrEmpty(s)) continue;
					if (s.EndsWith("/field_outpost.tscn")) Add(FirstGame.Data.ChapterFlags.OutpostEntered);
					if (s.EndsWith("/dungeon_3.tscn")) Add(FirstGame.Data.ChapterFlags.AbyssUnsealed);
				}
			}
			// 보스 처치 기반 — 챕터별 플래그 자동 채움.
			if (data.DefeatedBosses != null)
			{
				foreach (var b in data.DefeatedBosses)
				{
					switch (b)
					{
						case "orc_warlord_d1":   Add(FirstGame.Data.ChapterFlags.OrcWarlordKilled); break;
						case "skeleton_king_d2": Add(FirstGame.Data.ChapterFlags.SkeletonKingKilled); break;
						case "ancient_lich_d3":  Add(FirstGame.Data.ChapterFlags.LichKilled); break;
					}
				}
			}
			// 보스 처치된 적이 있는데 OutpostEntered가 누락이면 추가 — 던전을 클리어한 플레이어는
			// 당연히 Prologue를 지났음을 보장.
			if (data.ChapterFlags.Count > 0 && !data.ChapterFlags.Contains(FirstGame.Data.ChapterFlags.OutpostEntered))
				Add(FirstGame.Data.ChapterFlags.OutpostEntered);
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
		// 본 파일이 삭제되고 .bak만 생존한 슬롯도 후보로 인정 (LoadAnySlotWithFallback이
		// 실제 .bak 복구를 처리하므로 선택 단계에서 누락하면 안 됨).
		private static bool SlotExists(string slot)
		{
			string p = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
			return File.Exists(p) || File.Exists(p + ".bak");
		}

		// 본/.bak 중 더 최근 mtime — 최신 슬롯 판정용.
		private static DateTime SlotWriteTime(string slot)
		{
			string p = ProjectSettings.GlobalizePath(SaveDir + slot + ".json");
			DateTime t = DateTime.MinValue;
			if (File.Exists(p)) t = File.GetLastWriteTime(p);
			if (File.Exists(p + ".bak"))
			{
				DateTime tb = File.GetLastWriteTime(p + ".bak");
				if (tb > t) t = tb;
			}
			return t;
		}

		private static string SelectNewerSaveSlot()
		{
			bool hasManual = SlotExists(ManualSaveSlot);
			bool hasAuto = SlotExists(AutoSaveSlot);
			if (hasManual && hasAuto)
				return SlotWriteTime(ManualSaveSlot) >= SlotWriteTime(AutoSaveSlot)
					? ManualSaveSlot : AutoSaveSlot;
			if (hasManual) return ManualSaveSlot;
			if (hasAuto) return AutoSaveSlot;
			return null;
		}
	}
}
