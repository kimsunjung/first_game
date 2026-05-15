using System;

namespace FirstGame.Data
{
    [Flags]
    public enum StatusEffect
    {
        None   = 0,
        Poison = 1,
        Freeze = 2,
        Curse  = 4,
    }
}
