using Godot;
using System;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;
using FirstGame.Entities.Player;

namespace FirstGame.Entities.Enemies
{
	public partial class EnemyController : CharacterBody2D, IDamageable
	{
		[Export] public EnemyStats Stats { get; set; }
		
		// 추적할 타겟 (보통 플레이어) (Target to chase)
		private Node2D _target;
		private ProgressBar _healthBar;

		public override void _Ready()
		{
			// Stats 공유 문제 해결: 고유 인스턴스로 복제 (Fix shared resource issue: Duplicate)
			if (Stats != null)
				Stats = (EnemyStats)Stats.Duplicate();
			else
				Stats = new EnemyStats();

			// 체력 초기화 (Initialize health)
			Stats.CurrentHealth = Stats.MaxHealth;

			// 체력바 노드 가져오기 및 스타일 설정 (Get HealthBar node and set style)
			_healthBar = GetNode<ProgressBar>("HealthBar");
			_healthBar.MaxValue = Stats.MaxHealth;
			_healthBar.Value = Stats.CurrentHealth;

			// 체력바 스타일 커스터마이징 (Customize HealthBar Style)
			// 배경 스타일 (Background Style: Dark Gray)
			var bgStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
				CornerRadiusTopLeft = 2,
				CornerRadiusTopRight = 2,
				CornerRadiusBottomRight = 2,
				CornerRadiusBottomLeft = 2
			};
			_healthBar.AddThemeStyleboxOverride("background", bgStyle);

			// 채우기 스타일 (Fill Style: Red)
			var fillStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.8f, 0.1f, 0.1f, 1.0f),
				CornerRadiusTopLeft = 2,
				CornerRadiusTopRight = 2,
				CornerRadiusBottomRight = 2,
				CornerRadiusBottomLeft = 2
			};
			_healthBar.AddThemeStyleboxOverride("fill", fillStyle);
			
			// 체력바 크기 및 위치 조정 (Adjust size and position if needed)
			// 현재 씬 설정(enemy.tscn)을 따르되, 필요시 코드에서 강제할 수 있음.
		}

		private float _attackTimer = 0f;

		public override void _PhysicsProcess(double delta)
		{
			if (!IsInstanceValid(_target))
			{
				FindTarget();
				return;
			}

			// 쿨타임 감소 (Decrease Cooldown)
			_attackTimer -= (float)delta;
			
			// 타겟 방향 및 거리 계산 (Calculate Direction and Distance)
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
				float stopBuffer = 30.0f; // 정지 완충 거리 (Buffer distance for smooth stop)

				if (distance <= Stats.AttackRange + stopBuffer)
				{
					// 감속 구간: 공격 사거리 근처에서 서서히 정지 (Slow down near attack range)
					float ratio = Mathf.Max(0.15f, (distance - Stats.AttackRange) / stopBuffer);
					Velocity = direction * Stats.MoveSpeed * ratio;
				}
				else
				{
					// 일반 추적 (Normal Chase)
					Velocity = direction * Stats.MoveSpeed;
				}
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
			// 공격 조건 체크: 쿨타임 종료 + 타겟 유효 + 데미지 처리 가능 인터페이스 (Check Attack Conditions)
			if (_attackTimer <= 0f && IsInstanceValid(_target) && _target is IDamageable target)
			{
				target.TakeDamage(Stats.BaseDamage);
				_attackTimer = Stats.AttackCooldown; // 쿨타임 리셋 (Reset Cooldown)
				GD.Print($"[Enemy] 공격 수행! 데미지: {Stats.BaseDamage} (Attacked! Damage: {Stats.BaseDamage})");
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
			_healthBar.Value = Stats.CurrentHealth;
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

			// 아이템 드롭 (Item Drop)
			if (Stats.PossibleDrops != null && Stats.PossibleDrops.Length > 0)
			{
				if (GD.Randf() <= Stats.DropChance)
				{
					int index = (int)(GD.Randi() % Stats.PossibleDrops.Length);
					var droppedItem = Stats.PossibleDrops[index];
					var players = GetTree().GetNodesInGroup("Player");
					if (players.Count > 0 && players[0] is PlayerController player)
					{
						player.Inventory.AddItem(droppedItem);
						GD.Print($"아이템 드롭: {droppedItem.ItemName} (Item Dropped: {droppedItem.ItemName})");
					}
				}
			}

			// 자동 저장 (Auto Save)
			SaveManager.SaveGame(); 
			QueueFree(); // 오브젝트 삭제 (Destroy Object)
		}
	}
}
