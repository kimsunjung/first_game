using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class PlayerStats : CharacterStats
	{
		[Export] public int BaseDamage { get; set; } = 10;
	}
}
