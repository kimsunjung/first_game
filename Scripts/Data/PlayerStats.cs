using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class PlayerStats : CharacterStats
	{
		[Export] public int BaseDamage { get; set; } = 10;
        [Export] public float AttackRange { get; set; } = 80.0f; // 공격 사거리 (Attack Range)
        // 필요 시 플레이어 전용 스탯 추가 (Add player-specific stats here)
	}
}
