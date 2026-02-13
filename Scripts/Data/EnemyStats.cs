using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class EnemyStats : CharacterStats
	{
		[Export] public float DetectionRange { get; set; } = 200.0f;
		[Export] public float AttackRange { get; set; } = 40.0f;
		[Export] public int BaseDamage { get; set; } = 5;
		[Export] public float AttackCooldown { get; set; } = 1.0f; // Added for attack logic
	}
}
