using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Data.Skills
{
	/// <summary>광역/투사체 스킬 적중 시 적에게 상태이상을 거는 헬퍼.
	/// SkillData.InflictedStatus가 None이면 no-op이므로 기존 .tres 안전.</summary>
	internal static class SkillStatusHelpers
	{
		public static void TryInflictStatus(IDamageable target, SkillData skill)
		{
			if (skill == null || target == null) return;
			if (skill.InflictedStatus == StatusEffect.None) return;
			if (skill.InflictedStatusChance <= 0f) return;
			if (GD.Randf() > skill.InflictedStatusChance) return;
			target.ApplyStatusEffect(skill.InflictedStatus, skill.InflictedStatusDuration);
		}
	}

	[SkillStrategy(SkillType.PowerStrike)]
	public class PowerStrikeStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			target.SetPowerStrikeActive(true);
			GD.Print($"파워 스트라이크! 다음 공격 {skill.BonusDamageMultiplier}배 데미지");
		}
	}

	[SkillStrategy(SkillType.HealSelf)]
	public class HealSelfStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			target.HealSelf(skill.HealAmount);
			GD.Print($"힐! HP +{skill.HealAmount}");
		}
	}

	[SkillStrategy(SkillType.Dash)]
	public class DashStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float duration = skill.DurationSeconds > 0 ? skill.DurationSeconds : 2.0f;
			target.ActivateDash(duration);
			GD.Print("대시!");
		}
	}

	[SkillStrategy(SkillType.FireBolt)]
	public class FireBoltStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			bool fbCrit = GD.Randf() < target.CritRate;
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			long rawDmg = (long)target.BaseDamage * multiplier;
			if (fbCrit) rawDmg = (long)(rawDmg * target.CritMultiplier);
			int dmg = (int)Math.Min(rawDmg, int.MaxValue);
			ElementType element = skill.Element != ElementType.None ? skill.Element : ElementType.Fire;
			target.FireProjectile(dmg, element, new Color(1.0f, 0.4f, 0.1f), 540f,
				skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance);
			target.TriggerCameraShake(3f, 0.12f);
		}
	}

	// ─── 신규 능동 스킬 ───────────────────────────────────────────

	/// <summary>ArrowShot (궁수 시작) — FireBolt와 동일 패턴이지만 무속성 단일 원거리.
	/// 사거리 약간 짧고 MP 소모 낮음.</summary>
	[SkillStrategy(SkillType.ArrowShot)]
	public class ArrowShotStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			bool crit = GD.Randf() < target.CritRate;
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			long raw = (long)target.BaseDamage * multiplier;
			if (crit) raw = (long)(raw * target.CritMultiplier);
			int dmg = (int)Math.Min(raw, int.MaxValue);
			target.FireProjectile(dmg, skill.Element, new Color(0.95f, 0.9f, 0.55f), 620f,
				skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance);
			target.TriggerCameraShake(2f, 0.10f);
		}
	}

	/// <summary>Whirlwind (전사) — 자신 주변 일정 반경 모든 적에 1.5x 데미지 광역.</summary>
	[SkillStrategy(SkillType.Whirlwind)]
	public class WhirlwindStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float radius = 100f;
			float mul = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			bool anyHit = false;
			foreach (var (node, damageable) in target.GetNearbyEnemies(radius))
			{
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)(target.BaseDamage * mul);
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, skill.Element);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				anyHit = true;
			}
			if (anyHit)
			{
				target.TriggerCameraShake(6f, 0.25f);
				FirstGame.Core.UIEffectManager.HitStop(0.08f, 0.05f);
			}
		}
	}

	/// <summary>IceShard (마법사) — FireBolt와 유사한 단일 원거리. 속성 Ice 기본.
	/// 슬로우 효과는 후속 작업 — 현재 데미지만.</summary>
	/// <summary>LightningStorm (마법사) — 10초간 매 2초마다 가장 가까운 적에 번개.
	/// 능동 발동 시 PlayerStats에 stormDuration 누적, PlayerController가 매 프레임 tick.</summary>
	[SkillStrategy(SkillType.LightningStorm)]
	public class LightningStormStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float duration = skill.DurationSeconds > 0f ? skill.DurationSeconds : 10f;
			target.StartLightningStorm(duration, 2f);
			target.TriggerCameraShake(4f, 0.2f);
		}
	}

	/// <summary>PreciseAim (궁수) — 10초간 크리율 +30%. ApplyBuffEx 사용.</summary>
	[SkillStrategy(SkillType.PreciseAim)]
	public class PreciseAimStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float dur = skill.DurationSeconds > 0f ? skill.DurationSeconds : 10f;
			target.ApplyTempBuff(0, 0, 0.30f, dur);
		}
	}

	/// <summary>IronStance (전사) — 10초간 방어력 +N. ApplyBuffEx 사용.</summary>
	[SkillStrategy(SkillType.IronStance)]
	public class IronStanceStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float dur = skill.DurationSeconds > 0f ? skill.DurationSeconds : 10f;
			int defBonus = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 10;
			target.ApplyTempBuff(0, defBonus, 0f, dur);
		}
	}

	[SkillStrategy(SkillType.IceShard)]
	public class IceShardStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			bool crit = GD.Randf() < target.CritRate;
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			long raw = (long)target.BaseDamage * multiplier;
			if (crit) raw = (long)(raw * target.CritMultiplier);
			int dmg = (int)Math.Min(raw, int.MaxValue);
			ElementType element = skill.Element != ElementType.None ? skill.Element : ElementType.Ice;
			target.FireProjectile(dmg, element, new Color(0.6f, 0.85f, 1.0f), 500f,
				skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance);
			target.TriggerCameraShake(3f, 0.12f);
		}
	}

	/// <summary>MultiShot (궁수) — 전방 콘 안의 모든 적에 1배 데미지(다중 타격).
	/// Whirlwind와 차별점: 자신 주변이 아니라 전방 방향성 + 사거리 큼.</summary>
	[SkillStrategy(SkillType.MultiShot)]
	public class MultiShotStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float range = 220f;
			int hits = 0;
			foreach (var (node, damageable) in target.GetNearbyEnemies(range))
			{
				// 전방 콘 dot > 0.4 (약 ±66도) — Whirlwind보다 좁고 ArrowShot보다 넓음.
				Vector2 dir = (node.GlobalPosition - target.GlobalPosition).Normalized();
				if (target.FacingDirection.Dot(dir) <= 0.4f) continue;

				bool crit = GD.Randf() < target.CritRate;
				int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 1;
				long raw = (long)target.BaseDamage * multiplier;
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, skill.Element);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				hits++;
			}
			if (hits > 0)
			{
				target.TriggerCameraShake(4f, 0.2f);
				FirstGame.Core.UIEffectManager.HitStop(0.07f, 0.05f);
			}
		}
	}

	// ═══════════════════════════════════════════════════════════════
	// Content Expansion v1 — 전사 신규 스킬
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Cleave (전사) — 전방 콘(±70도) 내 모든 적에 1.8배 데미지.
	/// MultiShot과 유사하지만 근접 범위(140f)로 전사 근접 특화.</summary>
	[SkillStrategy(SkillType.Cleave)]
	public class CleaveStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float range = 140f;
			float mul = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			int hits = 0;
			foreach (var (node, damageable) in target.GetNearbyEnemies(range))
			{
				Vector2 dir = (node.GlobalPosition - target.GlobalPosition).Normalized();
				if (target.FacingDirection.Dot(dir) <= 0.25f) continue; // ±75도 콘
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)(target.BaseDamage * mul);
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, skill.Element);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				hits++;
			}
			if (hits > 0)
			{
				target.TriggerCameraShake(5f, 0.20f);
				FirstGame.Core.UIEffectManager.HitStop(0.08f, 0.05f);
			}
		}
	}

	/// <summary>GroundSlam (전사) — 자신 주변 150f 반경 모든 적에 2.5배 광역 데미지.
	/// Whirlwind보다 범위 크고 배율 높지만 쿨타임도 긺.</summary>
	[SkillStrategy(SkillType.GroundSlam)]
	public class GroundSlamStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float radius = 150f;
			float mul = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 3;
			bool anyHit = false;
			foreach (var (_, damageable) in target.GetNearbyEnemies(radius))
			{
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)(target.BaseDamage * mul);
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, skill.Element);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				anyHit = true;
			}
			if (anyHit)
			{
				target.TriggerCameraShake(9f, 0.35f);
				FirstGame.Core.UIEffectManager.HitStop(0.12f, 0.06f);
			}
			else
			{
				target.TriggerCameraShake(4f, 0.15f);
			}
		}
	}

	/// <summary>BattleCry (전사) — 30초간 공격력+방어력 동시 버프. 전투 개시 전 사용.</summary>
	[SkillStrategy(SkillType.BattleCry)]
	public class BattleCryStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float dur = skill.DurationSeconds > 0f ? skill.DurationSeconds : 30f;
			int dmgBonus = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 15;
			target.ApplyTempBuff(dmgBonus, 8, 0f, dur);
			GD.Print($"[BattleCry] 공격+{dmgBonus} 방어+8 {dur}초 발동");
		}
	}

	/// <summary>Execute (전사) — 단일 대상에게 3배 강타. 처형 일격 컨셉.</summary>
	[SkillStrategy(SkillType.Execute)]
	public class ExecuteStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 3;
			bool crit = GD.Randf() < target.CritRate;
			long raw = (long)target.BaseDamage * multiplier;
			if (crit) raw = (long)(raw * target.CritMultiplier);
			int dmg = (int)Math.Min(raw, int.MaxValue);
			ElementType element = skill.Element != ElementType.None ? skill.Element : ElementType.None;
			target.FireProjectile(dmg, element, new Color(0.8f, 0.1f, 0.1f), 480f,
				skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance);
			target.TriggerCameraShake(8f, 0.30f);
			FirstGame.Core.UIEffectManager.HitStop(0.15f, 0.08f);
		}
	}

	// ═══════════════════════════════════════════════════════════════
	// Content Expansion v1 — 마법사 신규 스킬
	// ═══════════════════════════════════════════════════════════════

	/// <summary>FlameWave (마법사) — 전방 콘(±75도) 200f 내 적에 1.8배 화염 광역.
	/// FireBolt의 광역 버전 컨셉.</summary>
	[SkillStrategy(SkillType.FlameWave)]
	public class FlameWaveStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float range = 200f;
			float mul = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			ElementType elem = skill.Element != ElementType.None ? skill.Element : ElementType.Fire;
			int hits = 0;
			foreach (var (node, damageable) in target.GetNearbyEnemies(range))
			{
				Vector2 dir = (node.GlobalPosition - target.GlobalPosition).Normalized();
				if (target.FacingDirection.Dot(dir) <= 0.25f) continue;
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)(target.BaseDamage * mul);
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, elem);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				hits++;
			}
			if (hits > 0)
			{
				target.TriggerCameraShake(5f, 0.20f);
				FirstGame.Core.UIEffectManager.HitStop(0.08f, 0.05f);
			}
		}
	}

	/// <summary>FrostNova (마법사) — 자신 주변 90f 반경 얼음 폭발. IceShard의 광역 버전.</summary>
	[SkillStrategy(SkillType.FrostNova)]
	public class FrostNovaStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float radius = 90f;
			float mul = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			ElementType elem = skill.Element != ElementType.None ? skill.Element : ElementType.Ice;
			bool anyHit = false;
			foreach (var (_, damageable) in target.GetNearbyEnemies(radius))
			{
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)(target.BaseDamage * mul);
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, elem);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				anyHit = true;
			}
			if (anyHit)
			{
				target.TriggerCameraShake(5f, 0.22f);
				FirstGame.Core.UIEffectManager.HitStop(0.08f, 0.05f);
			}
		}
	}

	/// <summary>ArcaneMissile (마법사) — 전방으로 3발 연속 발사 (각 1배 데미지 → 합산 3배).
	/// 동일 방향 3발이므로 근접 단일 대상에 집중 화력.</summary>
	[SkillStrategy(SkillType.ArcaneMissile)]
	public class ArcaneMissileStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 1;
			ElementType elem = skill.Element != ElementType.None ? skill.Element : ElementType.None;
			Color color = new Color(0.7f, 0.4f, 1.0f);
			for (int i = 0; i < 3; i++)
			{
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)target.BaseDamage * multiplier;
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				target.FireProjectile(dmg, elem, color, 580f,
					skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance);
			}
			target.TriggerCameraShake(4f, 0.15f);
		}
	}

	/// <summary>ManaShield (마법사) — duration초 동안 받는 데미지를 HP 대신 MP로 흡수.</summary>
	[SkillStrategy(SkillType.ManaShield)]
	public class ManaShieldStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float dur = skill.DurationSeconds > 0f ? skill.DurationSeconds : 10f;
			target.ActivateManaShield(dur);
			GD.Print($"[ManaShield] {dur}초간 MP로 데미지 흡수");
		}
	}

	// ═══════════════════════════════════════════════════════════════
	// Content Expansion v1 — 궁수 신규 스킬
	// ═══════════════════════════════════════════════════════════════

	/// <summary>PiercingShot (궁수) — 고속 3배 강타 + 최대 3적 관통.</summary>
	[SkillStrategy(SkillType.PiercingShot)]
	public class PiercingShotStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 3;
			bool crit = GD.Randf() < target.CritRate;
			long raw = (long)target.BaseDamage * multiplier;
			if (crit) raw = (long)(raw * target.CritMultiplier);
			int dmg = (int)Math.Min(raw, int.MaxValue);
			ElementType elem = skill.Element != ElementType.None ? skill.Element : ElementType.None;
			target.FireProjectileEx(dmg, elem, new Color(1.0f, 0.85f, 0.3f), 700f, 2,
				skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance); // 최대 3적 관통
			target.TriggerCameraShake(4f, 0.15f);
		}
	}

	/// <summary>BackstepShot (궁수) — 전방 사격 후 후방으로 강제 대시. 거리 유지 포지셔닝 스킬.</summary>
	[SkillStrategy(SkillType.BackstepShot)]
	public class BackstepShotStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			bool crit = GD.Randf() < target.CritRate;
			long raw = (long)target.BaseDamage * multiplier;
			if (crit) raw = (long)(raw * target.CritMultiplier);
			int dmg = (int)Math.Min(raw, int.MaxValue);
			ElementType elem = skill.Element != ElementType.None ? skill.Element : ElementType.None;
			target.FireProjectile(dmg, elem, new Color(0.95f, 0.9f, 0.55f), 580f,
				skill.InflictedStatus, skill.InflictedStatusDuration, skill.InflictedStatusChance);
			// facing 반대 방향으로 강제 대시 (적 반대 방향으로 물러남)
			float dashDur = skill.DurationSeconds > 0f ? skill.DurationSeconds : 0.35f;
			target.ActivateDashInDirection(dashDur, -target.FacingDirection);
			target.TriggerCameraShake(3f, 0.12f);
		}
	}

	/// <summary>RainOfArrows (궁수) — 자신 주변 180f 반경 모든 적에 1.5배 광역 화살.
	/// 넓은 범위로 다수 처리에 강함.</summary>
	[SkillStrategy(SkillType.RainOfArrows)]
	public class RainOfArrowsStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			const float radius = 180f;
			float mul = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
			ElementType elem = skill.Element != ElementType.None ? skill.Element : ElementType.None;
			bool anyHit = false;
			foreach (var (_, damageable) in target.GetNearbyEnemies(radius))
			{
				bool crit = GD.Randf() < target.CritRate;
				long raw = (long)(target.BaseDamage * mul);
				if (crit) raw = (long)(raw * target.CritMultiplier);
				int dmg = (int)Math.Min(raw, int.MaxValue);
				damageable.TakeDamage(dmg, elem);
				SkillStatusHelpers.TryInflictStatus(damageable, skill);
				anyHit = true;
			}
			if (anyHit)
			{
				target.TriggerCameraShake(5f, 0.20f);
				FirstGame.Core.UIEffectManager.HitStop(0.07f, 0.05f);
			}
		}
	}

	/// <summary>HunterFocus (궁수) — 15초간 공격력+치명타 버프. 사냥 집중 상태 컨셉.</summary>
	[SkillStrategy(SkillType.HunterFocus)]
	public class HunterFocusStrategy : ISkillStrategy
	{
		public void Execute(ISkillTarget target, SkillData skill)
		{
			float dur = skill.DurationSeconds > 0f ? skill.DurationSeconds : 15f;
			int dmgBonus = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 8;
			target.ApplyTempBuff(dmgBonus, 0, 0.20f, dur);
			GD.Print($"[HunterFocus] 공격+{dmgBonus} 크리+20% {dur}초 발동");
		}
	}

	/// <summary>
	/// Reflection 기반 스킬 전략 팩토리.
	/// [SkillStrategy(SkillType.X)] 어트리뷰트가 붙은 클래스를 자동 등록.
	/// 새 스킬 추가 시 factory 수정 불필요 (OCP 준수).
	/// </summary>
	public static class SkillStrategyFactory
	{
		private static readonly Dictionary<SkillType, Type> _registry;

		static SkillStrategyFactory()
		{
			_registry = typeof(ISkillStrategy).Assembly.GetTypes()
				.Where(t => !t.IsAbstract && t.GetCustomAttribute<SkillStrategyAttribute>() != null)
				.ToDictionary(
					t => t.GetCustomAttribute<SkillStrategyAttribute>().Type,
					t => t);

			GD.Print($"[SkillStrategyFactory] {_registry.Count}개 스킬 전략 등록됨");
		}

		public static ISkillStrategy Create(SkillType type)
			=> _registry.TryGetValue(type, out var t)
				? (ISkillStrategy)Activator.CreateInstance(t)
				: null;
	}
}
