using Godot;
using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
	/// <summary>
	/// Core 레이어에서 플레이어에 접근하기 위한 인터페이스.
	/// GameManager(Core)가 PlayerController(Entities)를 직접 참조하지 않도록 함.
	/// Node2D 메서드가 필요한 경우 (as Node2D) 캐스팅으로 처리.
	/// </summary>
	public interface IPlayer
	{
		PlayerStats Stats { get; }
		Inventory Inventory { get; }
		bool IsDead { get; }
		Vector2 GlobalPosition { get; }
	}
}
