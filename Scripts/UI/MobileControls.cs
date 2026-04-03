using Godot;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
    /// <summary>
    /// 모바일 터치 버튼 (공격/스킬/상호작용) 처리.
    /// VirtualJoystick은 별도 Control 노드에서 처리.
    /// </summary>
    public partial class MobileControls : Control
    {
        private Button _attackButton;
        private Button[] _skillButtons = new Button[4];
        private Button _interactButton;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;

            _attackButton = GetNodeOrNull<Button>("AttackButton");
            for (int i = 0; i < 4; i++)
                _skillButtons[i] = GetNodeOrNull<Button>($"SkillButton{i + 1}");
            _interactButton = GetNodeOrNull<Button>("InteractButton");

            _attackButton?.Connect("pressed", Callable.From(OnAttackPressed));
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                _skillButtons[i]?.Connect("pressed", Callable.From(() => GetPlayer()?.TriggerSkill(slot)));
            }
            _interactButton?.Connect("pressed", Callable.From(OnInteractPressed));
        }

        private PlayerController GetPlayer()
            => GetTree().GetFirstNodeInGroup("Player") as PlayerController;

        private void OnAttackPressed()
            => GetPlayer()?.Attack();

        private void OnInteractPressed()
        {
            var ev = new InputEventAction { Action = "interact", Pressed = true };
            Input.ParseInputEvent(ev);
        }
    }
}
