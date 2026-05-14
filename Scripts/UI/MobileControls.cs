using Godot;
using FirstGame.Entities;
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
        private const int SkillButtonCount = 6;
        private Button[] _skillButtons = new Button[SkillButtonCount];
        private Button _interactButton;
        private Texture2D _interactDefaultIcon;
        private Button _inventoryButton;
        private Button _characterButton;
        private Button _skillWindowButton;

        // 스킬 버튼 위에 표시할 쿨타임 레이블 (프로그래밍 방식으로 생성)
        private Label[] _cooldownLabels = new Label[SkillButtonCount];

        // 같은 프레임 중복 트리거 방지. emulate_mouse_from_touch=true 환경에서
        // 첫 터치는 Pressed 시그널(MouseButton)로, 두 번째 터치는 ScreenTouch 직접 핸들러로
        // 들어오는데 단일 터치가 양쪽 모두로 보이는 케이스를 차단.
        private ulong _lastAttackFrame = 0;
        private readonly ulong[] _lastSkillFrame = new ulong[SkillButtonCount];

        private static readonly string[] SlotKeys = { "Q", "W", "E", "R", "T", "Y" };

        private static readonly Color ColorReady    = Colors.White;
        private static readonly Color ColorCooldown = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        private static readonly Color ColorNoMp     = new Color(0.4f, 0.6f, 1.0f, 0.8f);

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;

            _attackButton = GetNodeOrNull<Button>("AttackButton");
            for (int i = 0; i < SkillButtonCount; i++)
                _skillButtons[i] = GetNodeOrNull<Button>($"SkillButton{i + 1}");
            _interactButton    = GetNodeOrNull<Button>("InteractButton");
            // 씬 편집기에서 지정한 기본 아이콘 보관 — 타깃 PromptIcon이 null이면 이걸로 폴백.
            if (_interactButton != null) _interactDefaultIcon = _interactButton.Icon;
            _inventoryButton   = GetNodeOrNull<Button>("InventoryButton");
            _characterButton   = GetNodeOrNull<Button>("CharacterButton");
            _skillWindowButton = GetNodeOrNull<Button>("SkillWindowButton");

            _attackButton?.Connect("pressed", Callable.From(OnAttackPressed));
            // emulate_mouse_from_touch=true에서 두 번째 터치(이동 중 공격)는 마우스 이벤트로
            // 변환되지 않아 Pressed 시그널이 안 뜬다. ScreenTouch를 직접 받아 보강.
            if (_attackButton != null) _attackButton.GuiInput += OnAttackGuiInput;
            for (int i = 0; i < SkillButtonCount; i++)
            {
                int slot = i;
                _skillButtons[i]?.Connect("pressed", Callable.From(() => OnSkillPressed(slot)));
                // 멀티터치 보강 — 조이스틱 누른 채 스킬 버튼 터치(두 번째 터치)도 받도록 ScreenTouch 직접 처리
                if (_skillButtons[i] != null)
                    _skillButtons[i].GuiInput += (ev) => OnSkillGuiInput(ev, slot);

                // 쿨타임 오버레이 레이블 생성
                if (_skillButtons[i] != null)
                {
                    _skillButtons[i].ExpandIcon = true;

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

            // 상호작용 버튼은 가까운 인터랙터블이 있을 때만 표시
            BaseInteractable.CurrentChanged += OnInteractableChanged;
            UpdateInteractButton(BaseInteractable.Current);
        }

        public override void _ExitTree()
        {
            BaseInteractable.CurrentChanged -= OnInteractableChanged;
        }

        private void OnInteractableChanged(BaseInteractable target) => UpdateInteractButton(target);

        private void UpdateInteractButton(BaseInteractable target)
        {
            if (_interactButton == null) return;
            if (target == null)
            {
                _interactButton.Visible = false;
                return;
            }
            _interactButton.Visible = true;
            // PromptIcon이 없는 타깃이면 기본 아이콘으로 복귀 — 이전 타깃 아이콘 잔존 방지.
            _interactButton.Icon = target.PromptIcon ?? _interactDefaultIcon;
        }

        public override void _Process(double delta)
        {
            var player = GetPlayer();
            if (player == null) return;

            for (int i = 0; i < SkillButtonCount; i++)
            {
                var btn = _skillButtons[i];
                var lbl = _cooldownLabels[i];
                if (btn == null || lbl == null) continue;

                if (!player.HasSkillInSlot(i))
                {
                    btn.Icon = null;
                    btn.Text = SlotKeys[i];
                    btn.Modulate = ColorReady;
                    lbl.Text = "";
                    continue;
                }

                var skill = player.Stats.LearnedSkills[i];
                btn.Icon = skill.Icon;
                btn.Text = skill.Icon != null ? "" : SlotKeys[i];

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
        {
            ulong frame = Engine.GetProcessFrames();
            if (_lastAttackFrame == frame) return; // 같은 프레임 중복 차단
            _lastAttackFrame = frame;
            GetPlayer()?.Attack();
        }

        private void OnAttackGuiInput(InputEvent @event)
        {
            if (@event is InputEventScreenTouch t && t.Pressed)
            {
                OnAttackPressed();
                _attackButton.AcceptEvent();
            }
        }

        private void OnSkillPressed(int slot)
        {
            if (slot < 0 || slot >= _lastSkillFrame.Length) return;
            ulong frame = Engine.GetProcessFrames();
            if (_lastSkillFrame[slot] == frame) return;
            _lastSkillFrame[slot] = frame;
            GetPlayer()?.TriggerSkill(slot);
        }

        private void OnSkillGuiInput(InputEvent @event, int slot)
        {
            if (@event is InputEventScreenTouch t && t.Pressed)
            {
                OnSkillPressed(slot);
                _skillButtons[slot]?.AcceptEvent();
            }
        }

        private void OnInteractPressed()
        {
            // pressed=true / pressed=false 한 쌍으로 보내 IsActionPressed("interact")가 stuck되지 않도록.
            // 현재는 _UnhandledInput 단발 처리지만, 향후 IsActionPressed 기반 로직이 추가되어도 안전.
            Input.ParseInputEvent(new InputEventAction { Action = "interact", Pressed = true });
            Input.ParseInputEvent(new InputEventAction { Action = "interact", Pressed = false });
        }

        private void OnInventoryPressed()
            => FindWindow<InventoryUI>("InventoryUI")?.Toggle();

        private void OnCharacterPressed()
            => FindWindow<CharacterWindow>("CharacterWindow")?.Toggle();

        private void OnSkillWindowPressed()
            => FindWindow<SkillWindow>("SkillWindow")?.Toggle();
    }
}
