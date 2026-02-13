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

		private bool _isDead = false;

		public void TakeDamage(int damage)
		{
			if (_isDead) return;

			Stats.CurrentHealth -= damage;
			GD.Print($"Player took {damage} damage. HP: {Stats.CurrentHealth}/{Stats.MaxHealth}");
			
			if (Stats.CurrentHealth <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			_isDead = true;
			GD.Print("플레이어 사망! (Player Died!)");
			EventManager.TriggerPlayerDeath();
			SetPhysicsProcess(false); // 이동 비활성화 (Disable movement)
		}

		public override void _Ready()
		{
			// Stats가 null이면 초기화 (Initialize if Stats is null)
			// 공유 문제 방지를 위해 복제 (Duplicate to prevent shared resource issues)
			if (Stats != null)
				Stats = (PlayerStats)Stats.Duplicate();
			else
				Stats = new PlayerStats();

			if (SaveManager.PendingLoadData != null)
			{
				// 저장 데이터 적용 (Apply loaded data)
				var data = SaveManager.PendingLoadData;
				GlobalPosition = new Vector2(data.PlayerPosX, data.PlayerPosY);
				Stats.MaxHealth = data.PlayerMaxHealth;    // MaxHealth 먼저 설정 (Set MaxHealth first)
				Stats.CurrentHealth = data.PlayerHealth;   // 그 다음 CurrentHealth (Then CurrentHealth)
				GameManager.Instance.PlayerGold = data.PlayerGold; // 골드 복원 (Restore Gold)
				SaveManager.PendingLoadData = null;        // 적용 후 초기화 (Reset after applying)
			}
			else if (!SaveManager.HasSave())
			{
				// 최초 실행 시 초기 자동저장 생성 (Create initial auto-save on first run)
				SaveManager.SaveGame();
			}

			_isDead = false; // 부활 시 초기화 (Reset on respawn)
			
			GD.Print("플레이어 초기화됨 (Player Initialized)");
		}

		public override void _PhysicsProcess(double delta)
		{
			GetInput();
			MoveAndSlide();
		}

		private void GetInput()
		{
			// 이동 (Movement)
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			Velocity = inputDir * Stats.MoveSpeed;

			// 공격 (Attack)
			if (Input.IsActionJustPressed("attack"))
			{
				Attack();
			}
		}

		private void Attack()
		{
			GD.Print("플레이어 공격! (Player Attacked!)");
			
			// 간단한 히트박스: 범위 내 적 찾기 (Simple hitbox: Find enemies within range)
			// 나중에는 Area2D를 사용하여 더 정밀하게 구현 예정. (For better precision, use Area2D in the scene later.)
			// 지금은 즉각적인 피드백을 위해 간단한 거리 체크 사용. (For now, use a simple distance check for immediate feedback.)
			var enemies = GetTree().GetNodesInGroup("Enemy");
			foreach (Node2D enemyNode in enemies)
			{
				if (enemyNode is IDamageable damageableEnemy && enemyNode is Node2D node)
				{
					float distance = GlobalPosition.DistanceTo(node.GlobalPosition);
					if (distance <= Stats.AttackRange) // AttackRange가 PlayerStats/CharacterStats에 있는지 확인 (Ensure AttackRange is in PlayerStats/CharacterStats)
					{
						damageableEnemy.TakeDamage(Stats.BaseDamage);
					}
				}
			}
		}
	}
}
