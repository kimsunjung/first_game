using Godot;
using FirstGame.Core;

namespace FirstGame.Objects
{
	public partial class Portal : Area2D
	{
		[Export(PropertyHint.File, "*.tscn")]
		public string TargetScenePath { get; set; } = "";

		[Export]
		public Vector2 TargetSpawnPosition { get; set; } = new Vector2(100, 100);

		private bool _playerInRange = false;
		private Label _promptLabel;

		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;

			_promptLabel = GetNodeOrNull<Label>("PromptLabel");
			if (_promptLabel != null) _promptLabel.Visible = false;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!_playerInRange || string.IsNullOrEmpty(TargetScenePath)) return;

			if (@event.IsActionPressed("interact") && !@event.IsEcho())
			{
				SceneManager.Instance?.ChangeScene(TargetScenePath, TargetSpawnPosition);
				GetViewport().SetInputAsHandled();
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
