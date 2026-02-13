using Godot;
using System;

namespace FirstGame.Core
{
	public partial class GameManager : Node
	{
		// Singleton Pattern
		public static GameManager Instance { get; private set; }

		// Example Global State (Gold)
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

		// Event for UI updates (WinForms style)
		public event Action<int> OnGoldChanged;

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				// Optional: Make this persist across scene loads
				// ProcessMode = ProcessModeEnum.Always; 
				GD.Print("GameManager Initialized");
			}
			else
			{
				QueueFree();
			}
		}
	}
}
