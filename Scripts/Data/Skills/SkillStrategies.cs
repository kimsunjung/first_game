using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Data.Skills
{
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
			target.FireProjectile(dmg, element, new Color(1.0f, 0.4f, 0.1f), 540f);
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
			target.FireProjectile(dmg, skill.Element, new Color(0.95f, 0.9f, 0.55f), 620f);
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
			target.FireProjectile(dmg, element, new Color(0.6f, 0.85f, 1.0f), 500f);
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
				hits++;
			}
			if (hits > 0)
			{
				target.TriggerCameraShake(4f, 0.2f);
				FirstGame.Core.UIEffectManager.HitStop(0.07f, 0.05f);
			}
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
