using Godot;
using System.Collections.Generic;

namespace FirstGame.Core.Interfaces
{
	/// <summary>
	/// 스킬 전략이 대상에게 접근하기 위한 인터페이스.
	/// Data 레이어(SkillStrategies)가 Entities(PlayerController)를 직접 참조하지 않도록 함.
	/// </summary>
	public interface ISkillTarget
	{
		Vector2 GlobalPosition { get; }
		int BaseDamage { get; }
		float CritRate { get; }
		float CritMultiplier { get; }
		void SetPowerStrikeActive(bool active);
		void ActivateDash(float duration);
		void TriggerCameraShake(float intensity, float duration);
		void HealSelf(int amount);
		IEnumerable<(Node2D Node, IDamageable Target)> GetNearbyEnemies(float range);
	}
}
