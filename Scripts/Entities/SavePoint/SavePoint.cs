using Godot;
using FirstGame.Core;

namespace FirstGame.Entities
{
    public partial class SavePoint : Area2D
    {
        private bool _playerInRange = false;
        private Label _promptLabel;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            _promptLabel = GetNode<Label>("PromptLabel");
            _promptLabel.Visible = false;
        }

        public override void _Process(double delta)
        {
            // 플레이어가 범위 안에 있고 상호작용 키(E)를 누르면 저장
            if (_playerInRange)
            {
                 if (Input.IsActionJustPressed("interact"))
                 {
                     GD.Print("SavePoint: E key pressed, saving game...");
                     SaveManager.SaveGame("manual");
                 }
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            GD.Print($"SavePoint: Body detected - {body.Name} (Group: {body.IsInGroup("Player")})");
            if (body.IsInGroup("Player"))
            {
                _playerInRange = true;
                _promptLabel.Visible = true;
                GD.Print("SavePoint: Player Entered Range");
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                _playerInRange = false;
                _promptLabel.Visible = false;
                GD.Print("SavePoint: Player Exited Range");
            }
        }
    }
}
