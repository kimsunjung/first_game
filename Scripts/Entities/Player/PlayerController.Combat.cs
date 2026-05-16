using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// 적 공격/투사체가 플레이어에게 상태이상을 거는 통합 경로 (IDamageable).
		public void ApplyStatusEffect(StatusEffect status, float duration)
		{
			if (IsDead || Stats == null) return;
			Stats.ApplyStatus(status, duration);
		}

		// ─── 피격 / 사망 ────────────────────────────────────────────
		public void TakeDamage(int damage)
		{
			if (IsDead) return;
			// 저주: 방어력 10 감소
			int curseDefPenalty = Stats.HasStatus(Data.StatusEffect.Curse) ? 10 : 0;
			int effectiveDefense = System.Math.Max(0, Stats.Defense - curseDefPenalty);
			int finalDamage = System.Math.Max(1, damage - effectiveDefense);

			// ManaShield: 데미지를 HP 대신 MP로 흡수
			if (Stats.IsManaShieldActive && Stats.CurrentMp > 0)
			{
				int mpAbsorb = System.Math.Min(finalDamage, Stats.CurrentMp);
				Stats.CurrentMp -= mpAbsorb;
				finalDamage -= mpAbsorb;
				if (finalDamage <= 0)
				{
					SpawnFloatingLabel(GlobalPosition, mpAbsorb, false, true);
					AudioManager.Instance?.PlaySFX("player_hit.wav");
					return; // 완전 흡수
				}
			}
			Stats.CurrentHealth -= finalDamage;
			_lastDamageTime = Time.GetTicksMsec() / 1000.0;
			AudioManager.Instance?.PlaySFX("player_hit.wav");
			SpawnFloatingLabel(GlobalPosition, finalDamage, false, true);
			if (finalDamage >= Stats.MaxHealth * 0.2f)
			{
				TriggerCameraShake(5f, 0.3f);
				// 큰 피해는 임팩트를 강조하는 히트 스톱 — 위협 강조.
				UIEffectManager.HitStop(0.08f, 0.05f);
			}

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

			if (Stats.CurrentHealth <= 0)
			{
				// ReviveOnDeath 자동 소비 — 인벤에 있으면 1개 사용해 HP 50% 부활.
				if (TryAutoRevive()) return;
				Die();
			}
			else
				PlayHitAnimation();
		}

		/// <summary>인벤에 ReviveOnDeath 효과 아이템이 있으면 1개 소비 → HP 50% 회복. 반환 true=부활 성공.</summary>
		private bool TryAutoRevive()
		{
			if (Inventory == null) return false;
			for (int i = 0; i < Inventory.Slots.Count; i++)
			{
				var slot = Inventory.Slots[i];
				if (slot.Item == null) continue;
				if (slot.Item.Type != ItemType.Consumable) continue;
				if (slot.Item.UseEffect != ItemUseEffect.ReviveOnDeath) continue;

				Inventory.RemoveItem(i, 1);
				Stats.CurrentHealth = Mathf.Max(1, Stats.MaxHealth / 2);
				AudioManager.Instance?.PlaySFX("potion_use.wav");
				GD.Print($"[부활] {slot.Item.ItemName} 자동 소비! HP 50%로 부활");
				return true;
			}
			return false;
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
			// 공격 cooldown — AutoAttackInterval / AttackSpeed. AttackSpeed가 1.10이면 cooldown 약 0.45초.
			if (_attackCooldown > 0f) return;
			float baseInterval = BalanceData.Combat.AutoAttackInterval;
			float spd = System.Math.Max(0.1f, Stats.AttackSpeed);
			_attackCooldown = baseInterval / spd;
			// Shock: 공격 쿨다운 ×1.5 (적과 동일 규칙 — 플레이어 측 적용).
			if (Stats.HasStatus(Data.StatusEffect.Shock)) _attackCooldown *= 1.5f;
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
				// 황금 펄스 tween까지 같이 정리 — 직접 대입 시 tween이 살아남아 시각 효과가 남음
				SetPowerStrikeActive(false);
				TriggerCameraShake(5f, 0.2f);
			}
			if (isCrit) damage = (int)(damage * Stats.CritMultiplier);

			PlayAttackAnimation();

			bool hitAny = false;

			// 클래스별 평타 분기:
			// Warrior=근접 즉시타격 / Mage·Archer=투사체 발사 (적중 시 데미지)
			if (Stats.PlayerClass == PlayerClass.Mage)
			{
				FireRangedAttack(damage, ElementType.Fire, new Color(1.0f, 0.55f, 0.15f), 460f);
				return;
			}
			if (Stats.PlayerClass == PlayerClass.Archer)
			{
				FireRangedAttack(damage, ElementType.None, new Color(0.85f, 0.85f, 0.6f), 520f);
				return;
			}

			// Warrior 근접 — 단일 타격: 정면 콘(dot>0.5) + 사거리 내 후보 중 가장 가까운 적 1마리만 타격.
			float effectiveRange = Stats.AttackRange;
			Node2D bestTarget = null;
			float bestDistance = float.MaxValue;
			Vector2 bestKnockDir = _facingDirection;
			var enemies = GameManager.Instance?.ActiveEnemies;
			if (enemies != null)
			{
				foreach (Node2D enemyNode in enemies)
				{
					if (enemyNode is not IDamageable) continue;
					float distance = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
					if (distance > effectiveRange) continue;
					Vector2 dirToEnemy = (enemyNode.GlobalPosition - GlobalPosition).Normalized();
					// 매우 가까우면 방향 체크 생략(겹쳤을 때 공격 가능).
					if (distance >= 20f && _facingDirection.Dot(dirToEnemy) <= 0.5f) continue;
					if (distance < bestDistance)
					{
						bestDistance = distance;
						bestTarget = enemyNode;
						bestKnockDir = distance < 5f ? _facingDirection : dirToEnemy;
					}
				}
			}

			if (bestTarget != null && bestTarget is IDamageable damageableEnemy)
			{
				damageableEnemy.TakeDamage(damage, ElementType.None);
				hitAny = true;
				if (bestTarget is IKnockbackable knockable)
					knockable.ApplyKnockback(bestKnockDir, BalanceData.Combat.EnemyKnockback);
				TriggerCameraShake(isCrit ? 3f : 2f, isCrit ? 0.15f : 0.1f);
				// Lifesteal 패시브 — 적중 데미지의 N%를 자신 HP로 회복. 최소 1HP 보장.
				if (Stats.LifestealPercent > 0f)
				{
					int heal = System.Math.Max(1, (int)(damage * Stats.LifestealPercent));
					Stats.CurrentHealth = System.Math.Min(Stats.MaxHealth, Stats.CurrentHealth + heal);
				}
			}

			if (hitAny)
				UIEffectManager.HitStop(isCrit ? 0.10f : 0.06f, 0.05f);
		}

		/// <summary>Mage/Archer 평타용 — _facingDirection 쪽으로 PlayerProjectile 발사.</summary>
		private void FireRangedAttack(int damage, ElementType element, Color color, float speed)
		{
			var proj = new PlayerProjectile
			{
				Damage = damage,
				Speed = speed,
				Direction = GetAimDirection(),
				Element = element,
				ProjectileColor = color,
				SingleHit = true
			};
			GetParent().AddChild(proj);
			proj.GlobalPosition = GlobalPosition;
			TriggerCameraShake(1.5f, 0.06f);
		}
	}
}
