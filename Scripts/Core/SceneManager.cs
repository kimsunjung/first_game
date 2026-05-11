using Godot;

namespace FirstGame.Core
{
	public partial class SceneManager : Node
	{
		public static SceneManager Instance { get; private set; }

		// 포탈 이동 시 다음 씬에서 적용할 스폰 위치
		public Vector2? NextSpawnPosition { get; set; } = null;

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				ProcessMode = ProcessModeEnum.Always;
				GD.Print("SceneManager 초기화됨");
			}
			else
			{
				QueueFree();
			}
		}

		public override void _ExitTree()
		{
			if (Instance == this) Instance = null;
		}

		/// <summary>씬 전환 — capture-first 패턴.
		/// 1) 현재 씬에서 SaveData 캡처 (PlayerController.WriteSaveData가 GetTree().CurrentScene을
		///    읽으므로 ChangeSceneToPacked *전*에 해야 함)
		/// 2) 캡처본에 목적지 override (CurrentScene/스폰/방문 이력)
		/// 3) ChangeSceneToPacked 큐잉
        /// 4) 성공 시에만 PendingLoadData 설정 + 디스크 영속화 + 메모리 visit 등록
        /// 실패 시 false 반환, 사이드 이펙트 전무 — 호출자는 반환값으로 골드/아이템 롤백 판단.</summary>
		public bool ChangeScene(string scenePath, Vector2 spawnPosition)
		{
			// 1. 경로 + 로드 가능성 사전 검증.
			if (string.IsNullOrEmpty(scenePath) || !ResourceLoader.Exists(scenePath))
			{
				GD.PrintErr($"SceneManager: 씬 경로 없음 ({scenePath})");
				return false;
			}
			var packed = ResourceLoader.Load<PackedScene>(scenePath);
			if (packed == null)
			{
				GD.PrintErr($"SceneManager: PackedScene 로드 실패 ({scenePath})");
				return false;
			}

			// 2. SaveData 캡처 — *현재 씬*이 살아 있는 상태에서. ChangeSceneToPacked가 deferred라도
			//    이후 같은 동기 블록에서 CurrentScene 접근이 안전한지 보장할 수 없으므로 미리 캡처.
			var captured = SaveManager.BuildSaveData();
			if (captured == null)
			{
				GD.PrintErr("SceneManager: SaveData 캡처 실패 (Player/ISaveable 누락)");
				return false;
			}

			// 3. 캡처본에 목적지 오버라이드. 새 visit도 캡처본에 포함시켜 디스크/PendingLoadData에 반영.
			captured.CurrentScene = scenePath;
			captured.PlayerPosX = spawnPosition.X;
			captured.PlayerPosY = spawnPosition.Y;
			if (captured.VisitedScenes != null && !captured.VisitedScenes.Contains(scenePath))
				captured.VisitedScenes.Add(scenePath);

			Engine.TimeScale = 1.0;

			// 4. 씬 전환 큐잉. 실패 시 mutation 없이 false 반환.
			var err = GetTree().ChangeSceneToPacked(packed);
			if (err != Error.Ok)
			{
				GD.PrintErr($"SceneManager: ChangeSceneToPacked 실패 ({scenePath}) err={err}");
				return false;
			}

			// 5. 큐잉 성공 — 메모리 visit 기록 + PendingLoadData 설정 + 디스크 영속화.
			//    씬 전환은 이미 큐잉됐으므로 디스크 쓰기 실패해도 게임은 계속 진행. 다만 다음
			//    세션에 stale 상태로 로드될 수 있어 명확히 로그를 남긴다 (.bak fallback이 있어
			//    원자 쓰기 자체는 안전).
			GameManager.Instance?.RecordSceneVisit(scenePath);
			SaveManager.PendingLoadData = captured;
			if (!SaveManager.WriteSaveDataToDisk(captured))
				GD.PrintErr($"SceneManager: 씬 전환은 성공했으나 save 디스크 쓰기 실패 ({scenePath})");
			NextSpawnPosition = spawnPosition;
			return true;
		}
	}
}
