using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class PlayerStats : CharacterStats
	{
		[Export] public int BaseDamage { get; set; } = 10;
        // 필요 시 플레이어 전용 스탯 추가 (Add player-specific stats here)
	}
}
