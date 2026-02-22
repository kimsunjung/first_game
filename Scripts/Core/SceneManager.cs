using Godot;
using System;
using FirstGame.Core;
using FirstGame.Entities.Player;

namespace FirstGame.Core
{
	public partial class SceneManager : Node
	{
		public static SceneManager Instance { get; private set; }

		// 포탈 이용 시 나타날 시작 위치를 전달하기 위한 임시 변수
		public Vector2? NextSpawnPosition { get; set; } = null;

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				ProcessMode = ProcessModeEnum.Always; // 씬 넘길 때 Pause 되어도 동작하도록
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

		public void ChangeScene(string scenePath, Vector2 spawnPosition)
		{
			GD.Print($"씬 이동 준비: {scenePath} (목표 좌표: {spawnPosition})");

			// 1. 현재 상태 강제 세이브
			SaveManager.SaveGame();

			// 2. 세이브 데이터를 PendingLoadData에 로드 (새 씬에서 스탯/인벤토리 복원용)
			SaveManager.LoadIntoPending();

			// 3. 다음 씬에서 사용할 스폰 위치 저장
			NextSpawnPosition = spawnPosition;

			// 4. 씬 전환
			GetTree().ChangeSceneToFile(scenePath);
		}
	}
}
