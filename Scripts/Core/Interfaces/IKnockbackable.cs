using Godot;

namespace FirstGame.Core.Interfaces
{
	public interface IKnockbackable
	{
		void ApplyKnockback(Vector2 direction, float force);
	}
}
