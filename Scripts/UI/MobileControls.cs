using Godot;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
    /// <summary>
    /// 모바일 터치 버튼 (공격/스킬/상호작용/메뉴) 처리.
    /// VirtualJoystick은 별도 Control 노드에서 처리.
    /// </summary>
    public partial class MobileControls : Control
    {
        private Button _attackButton;
        private Button[] _skillButtons = new Button[4];
        private Button _interactButton;
        private Button _inventoryButton;
        private Button _characterButton;
        private Button _skillWindowButton;

        // 스킬 버튼 위에 표시할 쿨타임 레이블 (프로그래밍 방식으로 생성)
        private Label[] _cooldownLabels = new Label[4];

        private static readonly Color ColorReady    = Colors.White;
        private static readonly Color ColorCooldown = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        private static readonly Color ColorNoMp     = new Color(0.4f, 0.6f, 1.0f, 0.8f);

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;

            _attackButton = GetNodeOrNull<Button>("AttackButton");
            for (int i = 0; i < 4; i++)
                _skillButtons[i] = GetNodeOrNull<Button>($"SkillButton{i + 1}");
            _interactButton    = GetNodeOrNull<Button>("InteractButton");
            _inventoryButton   = GetNodeOrNull<Button>("InventoryButton");
            _characterButton   = GetNodeOrNull<Button>("CharacterButton");
            _skillWindowButton = GetNodeOrNull<Button>("SkillWindowButton");

            _attackButton?.Connect("pressed", Callable.From(OnAttackPressed));
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                _skillButtons[i]?.Connect("pressed", Callable.From(() => GetPlayer()?.TriggerSkill(slot)));

                // 쿨타임 오버레이 레이블 생성
                if (_skillButtons[i] != null)
                {
                    var label = new Label();
                    label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.VerticalAlignment = VerticalAlignment.Center;
                    label.AddThemeFontSizeOverride("font_size", 14);
                    label.Text = "";
                    _skillButtons[i].AddChild(label);
                    _cooldownLabels[i] = label;
                }
            }
            _interactButton?.Connect("pressed", Callable.From(OnInteractPressed));
            _inventoryButton?.Connect("pressed", Callable.From(OnInventoryPressed));
            _characterButton?.Connect("pressed", Callable.From(OnCharacterPressed));
            _skillWindowButton?.Connect("pressed", Callable.From(OnSkillWindowPressed));
        }

        public override void _Process(double delta)
        {
            var player = GetPlayer();
            if (player == null) return;

            for (int i = 0; i < 4; i++)
            {
                var btn = _skillButtons[i];
                var lbl = _cooldownLabels[i];
                if (btn == null || lbl == null) continue;

                if (!player.HasSkillInSlot(i))
                {
                    btn.Modulate = ColorReady;
                    lbl.Text = "";
                    continue;
                }

                float remaining = player.GetSkillCooldownRemaining(i);
                int mpCost = player.GetSkillMpCost(i);
                bool hasMp = player.Stats.CurrentMp >= mpCost;

                if (remaining > 0f)
                {
                    btn.Modulate = ColorCooldown;
                    lbl.Text = remaining > 9.9f ? $"{remaining:F0}" : $"{remaining:F1}";
                }
                else if (!hasMp)
                {
                    btn.Modulate = ColorNoMp;
                    lbl.Text = $"MP\n{mpCost}";
                }
                else
                {
                    btn.Modulate = ColorReady;
                    lbl.Text = "";
                }
            }
        }

        private PlayerController GetPlayer()
            => GetTree().GetFirstNodeInGroup("Player") as PlayerController;

        private T FindWindow<T>(string nodeName) where T : CanvasLayer
            => GetTree().CurrentScene?.GetNodeOrNull<T>(nodeName);

        private void OnAttackPressed()
            => GetPlayer()?.Attack();

        private void OnInteractPressed()
        {
            var ev = new InputEventAction { Action = "interact", Pressed = true };
            Input.ParseInputEvent(ev);
        }

        private void OnInventoryPressed()
            => FindWindow<InventoryUI>("InventoryUI")?.Toggle();

        private void OnCharacterPressed()
            => FindWindow<CharacterWindow>("CharacterWindow")?.Toggle();

        private void OnSkillWindowPressed()
            => FindWindow<SkillWindow>("SkillWindow")?.Toggle();
    }
}
