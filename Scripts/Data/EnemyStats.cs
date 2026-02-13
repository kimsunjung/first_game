using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class EnemyStats : CharacterStats
	{
		[Export] public float DetectionRange { get; set; } = 200.0f;
		[Export] public float AttackRange { get; set; } = 70.0f;
		[Export] public int BaseDamage { get; set; } = 5;
		[Export] public float AttackCooldown { get; set; } = 1.0f; // 공격 쿨타임 (Attack Cooldown)

        // 드롭 테이블 (Drop Table)
        [ExportGroup("Drop Table")]
        [Export] public ItemData[] PossibleDrops { get; set; }
        [Export] public float DropChance { get; set; } = 0.5f; // 50% 확률 (50% Chance)
	}
}
