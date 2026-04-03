using Godot;

namespace FirstGame.UI
{
    /// <summary>
    /// 모바일 가상 입력 상태 저장소. 조이스틱 방향을 PlayerController에서 읽는다.
    /// </summary>
    public static class VirtualInput
    {
        public static Vector2 JoystickDirection { get; set; } = Vector2.Zero;
    }
}
