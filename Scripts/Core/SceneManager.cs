using Godot;

namespace FirstGame.Core
{
	public partial class SceneManager : Node
	{
		public static SceneManager Instance { get; private set; }

		// 포탈 이동 시 다음 씬에서 적용할 스폰 위치
		public Vector2? NextSpawnPosition { get; set; } = null;

		// 재진입 가드 — 포탈 중복 트리거(같은 프레임 두 번/겹친 포탈)로 ChangeScene이
		// 이중 호출돼 디스크 이중 기록·잘못된 목적지가 되는 것을 차단. 큐잉 후 짧게 잠금.
		private bool _changing = false;

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
			// 0. 재진입 차단 — 직전 전환이 아직 처리 중이면 무시.
			if (_changing)
			{
				GD.Print($"SceneManager: 전환 진행 중 — 중복 요청 무시 ({scenePath})");
				return false;
			}

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
			// 전환 전 static 이벤트 초기화 — 구 씬 노드 구독 dead 참조 백스톱
			// (노드별 _ExitTree -= 가 1차 방어, 여기가 2차). QuestManager 구독은 자동 복원.
			EventManager.ResetAll();
			var err = GetTree().ChangeSceneToPacked(packed);
			if (err != Error.Ok)
			{
				GD.PrintErr($"SceneManager: ChangeSceneToPacked 실패 ({scenePath}) err={err}");
				return false;
			}
			// 큐잉 성공 — 재진입 잠금. 다음 씬 로드까지 약간의 시간 후 해제.
			_changing = true;
			GetTree().CreateTimer(0.5, processAlways: true).Timeout += () => _changing = false;

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
