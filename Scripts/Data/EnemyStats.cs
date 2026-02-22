using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class EnemyStats : CharacterStats
	{
		[Export] public new float MoveSpeed { get; set; } = 100.0f;
		[Export] public float DetectionRange { get; set; } = 200.0f;
		[Export] public float AttackRange { get; set; } = 80.0f;
		[Export] public int BaseDamage { get; set; } = 5;
		[Export] public float AttackCooldown { get; set; } = 1.0f;
		[Export] public int ExperienceReward { get; set; } = 15;

		// ─── 애니메이션 설정 ─────────────────────────────────────────
		[ExportGroup("Animation")]
		[Export] public string AnimBasePath { get; set; } =
			"res://Resources/Tilesets/Pixel Crawler - Free Pack/Entities/Mobs/Orc Crew/Orc/";

		[Export] public string AnimIdleFile { get; set; } = "Idle/Idle-Sheet_padded.png";
		[Export] public int AnimIdleFrames { get; set; } = 4;

		[Export] public string AnimRunFile { get; set; } = "Run/Run-Sheet.png";
		[Export] public int AnimRunFrames { get; set; } = 6;

		[Export] public string AnimDeathFile { get; set; } = "Death/Death-Sheet.png";
		[Export] public int AnimDeathFrames { get; set; } = 6;

		// ─── 드롭 테이블 ─────────────────────────────────────────────
		[ExportGroup("Drop Table")]
		[Export] public ItemData[] PossibleDrops { get; set; }
		[Export] public float DropChance { get; set; } = 0.5f;
	}
}
