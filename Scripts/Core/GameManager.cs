using Godot;
using System;

namespace FirstGame.Core
{
	public partial class GameManager : Node
	{
		// 싱글톤 패턴 (Singleton Pattern)
		public static GameManager Instance { get; private set; }

		// 예시: 전역 상태 (골드) (Example Global State)
		private int _playerGold = 0;
		public int PlayerGold 
		{ 
			get => _playerGold;
			set
			{
				_playerGold = value;
				OnGoldChanged?.Invoke(_playerGold);
			}
		}

		// UI 업데이트용 이벤트 (Event for UI updates)
		public event Action<int> OnGoldChanged;

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				// 옵션: 씬 로드 시 유지 (Optional: Make this persist across scene loads)
				// ProcessMode = ProcessModeEnum.Always; 
				GD.Print("게임 매니저 초기화됨 (GameManager Initialized)");
			}
			else
			{
				QueueFree();
			}
		}

		public override void _ExitTree()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
	}
}
