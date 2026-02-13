using Godot;
using System;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Enemies
{
	public partial class EnemyController : CharacterBody2D, IDamageable
	{
		[Export] public EnemyStats Stats { get; set; }
		
		// Target to chase (usually the player)
		private Node2D _target;

		public override void _Ready()
		{
			if (Stats == null) Stats = new EnemyStats();
			
			// Initialize health
			Stats.CurrentHealth = Stats.MaxHealth;
			
			// Find player by group or singleton (Adjust based on your preference)
			// For now, let's assume we find a node in the "Player" group, or we can set it later.
			// A simple way is to look for the player when physics process runs or via a detection area.
		}

		private float _attackTimer = 0f;

		public override void _PhysicsProcess(double delta)
		{
			if (_target == null)
			{
				FindTarget();
				return;
			}

            // Cooldown Timer
            _attackTimer -= (float)delta;

			// Simple Chase AI
			Vector2 direction = GlobalPosition.DirectionTo(_target.GlobalPosition);
			float distance = GlobalPosition.DistanceTo(_target.GlobalPosition);

			if (distance <= Stats.AttackRange)
			{
                // In Attack Range: Stop and Attack
				Velocity = Vector2.Zero;
                TryAttack();
			}
			else if (distance <= Stats.DetectionRange)
			{
                // In Chase Range: Move towards target
				Velocity = direction * Stats.MoveSpeed;
			}
            else
            {
                // Out of range: Stop
                Velocity = Vector2.Zero;
            }
            
            MoveAndSlide();
		}
        
        private void TryAttack()
        {
            if (_attackTimer <= 0f && _target is IDamageable target)
            {
                target.TakeDamage(Stats.BaseDamage);
                _attackTimer = Stats.AttackCooldown;
                GD.Print($"Enemy attacked for {Stats.BaseDamage} damage!");
            }
        }
		
		private void FindTarget()
		{
			// Simple way to find player: Get first node in "Player" group
			// User needs to add Player to "Player" group in editor
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
			{
				_target = players[0] as Node2D;
			}
		}

		// IDamageable Implementation
		public void TakeDamage(int damage)
		{
			Stats.CurrentHealth -= damage;
			GD.Print($"Enemy took {damage} damage. Current Health: {Stats.CurrentHealth}");

			// Visual Feedback: Flash white/red
			// Fix: Modulate the Sprite2D, not the root, because root modulation affects all children but might be overridden or subtle.
			// Also, since the enemy is already red, let's flash it to White (bright) or Transparent.
			var sprite = GetNode<Sprite2D>("Sprite2D");
			if (sprite != null)
			{
				var originalColor = sprite.Modulate;
				sprite.Modulate = Colors.White; // Flash white to indicate hit
				GetTree().CreateTimer(0.1).Timeout += () => 
				{
					if (IsInstanceValid(sprite)) sprite.Modulate = originalColor;
				};
			}

			if (Stats.CurrentHealth <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			GD.Print("Enemy Died!");
			// Reward Gold
			GameManager.Instance.PlayerGold += 10;
			QueueFree();
		}
	}
}
