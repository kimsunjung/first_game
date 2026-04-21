using Godot;

namespace FirstGame.Entities
{
	/// <summary>
	/// 플레이어 접근 시 프롬프트를 표시하고 상호작용을 처리하는 Area2D 베이스 클래스.
	/// 상속 후 OnInteract()만 구현하면 됩니다.
	/// </summary>
	public abstract partial class BaseInteractable : Area2D
	{
		protected bool PlayerInRange { get; private set; } = false;
		private Label _promptLabel;

		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
			_promptLabel = GetNodeOrNull<Label>("PromptLabel");
			if (_promptLabel != null)
			{
				if (string.IsNullOrEmpty(_promptLabel.Text))
					_promptLabel.Text = "[F] 상호작용";
				_promptLabel.Visible = false;
			}
			OnReady();
		}

		/// <summary>서브클래스 초기화용. base._Ready() 대신 이 메서드를 오버라이드하세요.</summary>
		protected virtual void OnReady() { }

		/// <summary>플레이어가 상호작용 키를 누르면 호출됩니다.</summary>
		protected abstract void OnInteract();

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!PlayerInRange) return;
			if (@event.IsActionPressed("interact") && !@event.IsEcho())
			{
				OnInteract();
				GetViewport().SetInputAsHandled();
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (!body.IsInGroup("Player")) return;
			PlayerInRange = true;
			if (_promptLabel != null) _promptLabel.Visible = true;
			OnPlayerEntered(body);
		}

		private void OnBodyExited(Node2D body)
		{
			if (!body.IsInGroup("Player")) return;
			PlayerInRange = false;
			if (_promptLabel != null) _promptLabel.Visible = false;
			OnPlayerExited(body);
		}

		protected virtual void OnPlayerEntered(Node2D player) { }
		protected virtual void OnPlayerExited(Node2D player) { }
	}
}
