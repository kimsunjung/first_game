using Godot;

namespace FirstGame.UI
{
    /// <summary>
    /// 화면 좌측 하단의 가상 조이스틱. 터치 입력을 받아 VirtualInput.JoystickDirection을 갱신한다.
    /// </summary>
    public partial class VirtualJoystick : Control
    {
        private const float OuterRadius = 80f;
        private const float ThumbRadius = 30f;
        private const float ActivationRadius = 120f;

        private int _touchIndex = -1;
        private Vector2 _thumbOffset = Vector2.Zero;

        private Vector2 Center => Size / 2;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch)
            {
                if (touch.Pressed && _touchIndex < 0)
                {
                    Vector2 localPos = touch.Position - GlobalPosition;
                    if (localPos.DistanceTo(Center) <= ActivationRadius)
                    {
                        _touchIndex = touch.Index;
                        SetThumb(localPos);
                        GetViewport().SetInputAsHandled();
                    }
                }
                else if (!touch.Pressed && touch.Index == _touchIndex)
                {
                    ResetJoystick();
                    GetViewport().SetInputAsHandled();
                }
            }
            else if (@event is InputEventScreenDrag drag && drag.Index == _touchIndex)
            {
                if (!GetViewportRect().HasPoint(drag.Position))
                {
                    ResetJoystick();
                    return;
                }
                SetThumb(drag.Position - GlobalPosition);
                GetViewport().SetInputAsHandled();
            }
        }

        public override void _Notification(int what)
        {
            if (what == NotificationApplicationFocusOut || what == NotificationPredelete)
                ResetJoystick();
        }

        private void ResetJoystick()
        {
            _touchIndex = -1;
            _thumbOffset = Vector2.Zero;
            VirtualInput.JoystickDirection = Vector2.Zero;
            QueueRedraw();
        }

        private void SetThumb(Vector2 localPos)
        {
            Vector2 delta = localPos - Center;
            if (delta.Length() > OuterRadius)
                delta = delta.Normalized() * OuterRadius;
            _thumbOffset = delta;
            VirtualInput.JoystickDirection = delta.Length() > 10f ? delta.Normalized() : Vector2.Zero;
            QueueRedraw();
        }

        public override void _Draw()
        {
            DrawArc(Center, OuterRadius, 0, Mathf.Tau, 40, new Color(1, 1, 1, 0.25f), 3f);
            DrawCircle(Center + _thumbOffset, ThumbRadius, new Color(1, 1, 1, 0.35f));
        }
    }
}
