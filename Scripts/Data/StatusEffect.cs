using System;

namespace FirstGame.Data
{
    [Flags]
    public enum StatusEffect
    {
        None   = 0,
        Poison = 1,  // DOT 3/sec, 녹색
        Freeze = 2,  // 이동속도 ×0.5, 청색
        Curse  = 4,  // 방어 -10 (받는 피해 증가), 보라색
        Burn   = 8,  // DOT 5/sec, 주황색
        Shock  = 16, // 공격 쿨다운 ×1.5 (공격속도 33% 감소), 노란색
    }
}
