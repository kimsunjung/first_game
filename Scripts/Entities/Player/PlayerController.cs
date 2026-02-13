using Godot;
using System;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController : CharacterBody2D, IDamageable
	{
		[Export] public PlayerStats Stats { get; set; }

		public void TakeDamage(int damage)
		{
			Stats.CurrentHealth -= damage;
			GD.Print($"Player took {damage} damage. HP: {Stats.CurrentHealth}/{Stats.MaxHealth}");
			// Handle player death logic here later
		}

		public override void _Ready()
		{
			// Initialize if Stats is null (though it should be assigned in Inspector)
			if (Stats == null)
			{
				Stats = new PlayerStats();
			}
			
			GD.Print("Player Initialized");
		}

		public override void _PhysicsProcess(double delta)
		{
			GetInput();
			MoveAndSlide();
		}

		private void GetInput()
		{
			// Movement
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			Velocity = inputDir * Stats.MoveSpeed;

			// Attack
			if (Input.IsActionJustPressed("attack"))
			{
				Attack();
			}
		}

		private void Attack()
		{
			GD.Print("Player Attacked!");
			
			// Simple hitbox: Find enemies within range
			// For better precision, use Area2D in the scene later. 
			// For now, let's use a simple distance check for immediate feedback.
			var enemies = GetTree().GetNodesInGroup("Enemy");
			foreach (Node2D enemyNode in enemies)
			{
				if (enemyNode is IDamageable damageableEnemy && enemyNode is Node2D node)
				{
					float distance = GlobalPosition.DistanceTo(node.GlobalPosition);
					if (distance <= Stats.AttackRange) // Ensure AttackRange is in PlayerStats/CharacterStats
					{
						damageableEnemy.TakeDamage(Stats.BaseDamage);
					}
				}
			}
		}
	}
}
