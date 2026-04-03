using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FirstGame.Core.Interfaces;

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
			var enemies = target.GetNearbyEnemies(350f);
			float bestDist = 350f;
			IDamageable hitTarget = null;

			foreach (var (node, damageable) in enemies)
			{
				float d = target.GlobalPosition.DistanceTo(node.GlobalPosition);
				if (d < bestDist)
				{
					bestDist = d;
					hitTarget = damageable;
				}
			}

			if (hitTarget != null)
			{
				bool fbCrit = GD.Randf() < target.CritRate;
				int multiplier = skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2;
				long rawDmg = (long)target.BaseDamage * multiplier;
				if (fbCrit) rawDmg = (long)(rawDmg * target.CritMultiplier);
				int dmg = (int)Math.Min(rawDmg, int.MaxValue);
				hitTarget.TakeDamage(dmg);
				target.TriggerCameraShake(7f, 0.3f);
				GD.Print($"파이어볼트 명중! ({dmg} 데미지{(fbCrit ? " CRIT!" : "")})");
			}
			else
			{
				GD.Print("파이어볼트: 대상 없음");
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
