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

		public void ChangeScene(string scenePath, Vector2 spawnPosition)
		{
			SaveManager.SaveGame();
			SaveManager.LoadIntoPending();
			NextSpawnPosition = spawnPosition;
			GetTree().ChangeSceneToFile(scenePath);
		}
	}
}
