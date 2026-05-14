using Godot;
using System.Collections.Generic;
using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
	/// <summary>
	/// 스킬 전략이 대상에게 접근하기 위한 인터페이스.
	/// Data 레이어(SkillStrategies)가 Entities(PlayerController)를 직접 참조하지 않도록 함.
	/// </summary>
	public interface ISkillTarget
	{
		Vector2 GlobalPosition { get; }
		Vector2 FacingDirection { get; }
		int BaseDamage { get; }
		float CritRate { get; }
		float CritMultiplier { get; }
		void SetPowerStrikeActive(bool active);
		void ActivateDash(float duration);
		void TriggerCameraShake(float intensity, float duration);
		void HealSelf(int amount);
		IEnumerable<(Node2D Node, IDamageable Target)> GetNearbyEnemies(float range);
		// 원거리 스킬 — 정면 방향으로 투사체 발사. 닿을 때 데미지.
		void FireProjectile(int damage, ElementType element, Color color, float speed = 460f);
	}
}
