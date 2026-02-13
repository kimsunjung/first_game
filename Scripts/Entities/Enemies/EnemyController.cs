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
		
		// 추적할 타겟 (보통 플레이어) (Target to chase)
		private Node2D _target;

		public override void _Ready()
		{
			// Stats 공유 문제 해결: 고유 인스턴스로 복제 (Fix shared resource issue: Duplicate)
			if (Stats != null)
				Stats = (EnemyStats)Stats.Duplicate();
			else
				Stats = new EnemyStats();
			
			// 체력 초기화 (Initialize health)
			Stats.CurrentHealth = Stats.MaxHealth;
			
			// 그룹으로 플레이어 찾기 (Find player by group)
			// 지금은 "Player" 그룹의 첫 번째 노드를 찾음. (Assumes "Player" group has target)
		}

		private float _attackTimer = 0f;

		public override void _PhysicsProcess(double delta)
		{
			if (!IsInstanceValid(_target))
			{
				FindTarget();
				return;
			}

			// 쿨타임 타이머 (Cooldown Timer)
			_attackTimer -= (float)delta;

			// 간단한 추적 AI (Simple Chase AI)
			Vector2 direction = GlobalPosition.DirectionTo(_target.GlobalPosition);
			float distance = GlobalPosition.DistanceTo(_target.GlobalPosition);

			// 디버깅: 거리 확인 (Debug distance)
			// GD.Print($"Enemy Distance to Player: {distance}");

			if (distance <= Stats.AttackRange)
			{
				// 공격 사거리 내: 정지 및 공격 (In Attack Range: Stop and Attack)
				Velocity = Vector2.Zero;
				TryAttack();
			}
			else if (distance <= Stats.DetectionRange)
			{
				// 추적 사거리 내: 타겟 향해 이동 (In Chase Range: Move towards target)
				Velocity = direction * Stats.MoveSpeed;
			}
			else
			{
				// 사거리 밖: 정지 (Out of range: Stop)
				Velocity = Vector2.Zero;
			}
			
			MoveAndSlide();
		}
		
		private void TryAttack()
		{
			// 타겟 유효성 검사 추가 (Add target validation)
			if (_attackTimer <= 0f && IsInstanceValid(_target) && _target is IDamageable target)
			{
				target.TakeDamage(Stats.BaseDamage);
				_attackTimer = Stats.AttackCooldown;
				GD.Print($"적이 {Stats.BaseDamage}의 데미지로 공격! (Enemy attacked!)");
			}
		}
		
		private void FindTarget()
		{
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
			{
				_target = players[0] as Node2D;
			}
			// 물리 프로세스가 실행될 때 플레이어를 찾거나 탐지 영역(Detection Area)을 사용하는 방법도 있음. (Or use Detection Area)
		}

		// IDamageable 구현 (IDamageable Implementation)
		public void TakeDamage(int damage)
		{
			Stats.CurrentHealth -= damage;
			GD.Print($"적이 {damage} 데미지를 입음. 현재 체력: {Stats.CurrentHealth} (Enemy took damage)");

			// 시각적 피드백: 흰색으로 깜빡임 (Visual Feedback: Flash white)
			var sprite = GetNode<Sprite2D>("Sprite2D");
			if (sprite != null)
			{
				var originalColor = sprite.Modulate;
				sprite.Modulate = Colors.White; // 피격 시 흰색으로 깜빡임 (Flash white)
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
			GD.Print("적 사망! (Enemy Died!)");
			// 골드 보상 (Reward Gold)
			GameManager.Instance.PlayerGold += 10;
			SaveManager.SaveGame(); // 적 처치 후 자동저장
			QueueFree();
		}
	}
}
