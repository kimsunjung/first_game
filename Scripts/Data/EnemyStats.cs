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
		[Export] public float AttackCooldown { get; set; } = 1.0f; // 공격 쿨타임 (Added for attack logic)
	}
}
