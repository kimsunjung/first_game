using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// ─── 피격 / 사망 ────────────────────────────────────────────
		public void TakeDamage(int damage)
		{
			if (IsDead) return;
			int finalDamage = System.Math.Max(1, damage - Stats.Defense);
			Stats.CurrentHealth -= finalDamage;
			_lastDamageTime = Time.GetTicksMsec() / 1000.0;
			AudioManager.Instance?.PlaySFX("player_hit.wav");
			SpawnFloatingLabel(GlobalPosition, finalDamage, false, true);
			if (finalDamage >= Stats.MaxHealth * 0.2f)
				TriggerCameraShake(5f, 0.3f);

			// 플레이어 넉백: 가장 가까운 적 반대 방향으로 밀림
			var enemies = GameManager.Instance?.ActiveEnemies;
			if (enemies != null)
			{
				Node2D nearest = null;
				float minDist = float.MaxValue;
				foreach (var e in enemies)
				{
					float d = GlobalPosition.DistanceTo(e.GlobalPosition);
					if (d < minDist) { minDist = d; nearest = e; }
				}
				if (nearest != null)
				{
					Vector2 dir = (GlobalPosition - nearest.GlobalPosition).Normalized();
					_knockbackVelocity = dir * BalanceData.Combat.PlayerKnockback;
				}
			}

			PlayHitAnimation();
			if (Stats.CurrentHealth <= 0) Die();
		}

		public void GainExp(int amount)
		{
			Stats.AddExp(amount);
		}

		private void Die()
		{
			IsDead = true;
			SetPhysicsProcess(false);
			PlayDeathAnimation();
			EventManager.TriggerPlayerDeath();
		}

		// ─── 공격 ───────────────────────────────────────────────────
		public void Attack()
		{
			if (IsDead || _isAnimLocked) return;
			AudioManager.Instance?.PlaySFX("player_attack.wav");

			int damage = Stats.BaseDamage;
			bool isCrit = GD.Randf() < Stats.CritRate;
			int multiplier = 1;
			if (_powerStrikeActive)
			{
				SkillData ps = null;
				foreach (var s in Stats.LearnedSkills)
					if (s.Type == SkillType.PowerStrike) { ps = s; break; }
				multiplier = ps != null ? ps.BonusDamageMultiplier : 2;
				damage *= multiplier;
				_powerStrikeActive = false;
				TriggerCameraShake(5f, 0.2f);
			}
			if (isCrit) damage = (int)(damage * Stats.CritMultiplier);

			PlayAttackAnimation();

			bool hitAny = false;

			// 적 공격
			var enemies = GameManager.Instance?.ActiveEnemies;
			if (enemies != null)
			{
				foreach (Node2D enemyNode in enemies)
				{
					if (enemyNode is IDamageable damageableEnemy)
					{
						float distance = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
						if (distance <= Stats.AttackRange)
						{
							Vector2 dirToEnemy = (enemyNode.GlobalPosition - GlobalPosition).Normalized();
							// 거리가 매우 가까우면 방향 체크 생략 (겹쳤을 때 공격 가능)
							if (distance < 20f || _facingDirection.Dot(dirToEnemy) > 0.5f)
							{
								damageableEnemy.TakeDamage(damage);
								hitAny = true;

								// 적 넉백: 겹쳤을 때는 바라보는 방향으로
								Vector2 knockDir = distance < 5f ? _facingDirection : dirToEnemy;
								if (enemyNode is IKnockbackable knockable)
									knockable.ApplyKnockback(knockDir, BalanceData.Combat.EnemyKnockback);

								if (isCrit)
									TriggerCameraShake(3f, 0.15f);
								else
									TriggerCameraShake(2f, 0.1f);
							}
						}
					}
				}
			}

			// 히트스톱: 적중 시 잠깐 프리즈 (이미 히트스탑 중이면 중복 타이머 생성 방지)
			if (hitAny && Engine.TimeScale > 0.1)
			{
				float stopDuration = isCrit ? 0.08f : 0.05f;
				Engine.TimeScale = 0.05;
				GetTree().CreateTimer(stopDuration, true, false, true).Timeout += () =>
				{
					if (IsInstanceValid(this))
						Engine.TimeScale = 1.0;
				};
			}
		}
	}
}
