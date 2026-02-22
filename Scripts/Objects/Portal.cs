using Godot;
using System;
using FirstGame.Core;

namespace FirstGame.Objects
{
	public partial class Portal : Area2D
	{
		[Export(PropertyHint.File, "*.tscn")] 
		public string TargetScenePath { get; set; } = "res://Scenes/Maps/act1_field_1.tscn";
		
		[Export] 
		public Vector2 TargetSpawnPosition { get; set; } = new Vector2(100, 100);

		private bool _playerInRange = false;
		private Label _promptLabel;

		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
			
			// 에디터에서 자유롭게 자식 노드로 라벨을 붙일 수 있도록
			_promptLabel = GetNodeOrNull<Label>("PromptLabel");
			if (_promptLabel != null) _promptLabel.Visible = false;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!_playerInRange || string.IsNullOrEmpty(TargetScenePath)) return;

			if (@event.IsActionPressed("interact") && !@event.IsEcho())
			{
				if (SceneManager.Instance != null)
				{
					GD.Print("포탈 작동!");
					SceneManager.Instance.ChangeScene(TargetScenePath, TargetSpawnPosition);
				}
				else
				{
					GD.PrintErr("SceneManager가 없습니다! (Autoload 확이 필요)");
				}
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body.IsInGroup("Player"))
			{
				_playerInRange = true;
				if (_promptLabel != null) _promptLabel.Visible = true;
			}
		}

		private void OnBodyExited(Node2D body)
		{
			if (body.IsInGroup("Player"))
			{
				_playerInRange = false;
				if (_promptLabel != null) _promptLabel.Visible = false;
			}
		}
	}
}
