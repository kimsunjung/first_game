using Godot;
using System;

namespace FirstGame.Data
{
	public enum EnemyBehavior
	{
		Melee,      // 기본: 다가가서 근접 공격
		Ranged,     // 원거리: 거리 유지하며 투사체 발사
		Passive     // 소극적: 피격 전까지 공격 안 함
	}

	[GlobalClass]
	public partial class EnemyStats : CharacterStats
	{
		[Export] public override float MoveSpeed { get; set; } = 100.0f;
		[Export] public float DetectionRange { get; set; } = 200.0f;
		[Export] public float AttackRange { get; set; } = 80.0f;
		[Export] public int BaseDamage { get; set; } = 2;
		[Export] public float AttackCooldown { get; set; } = 2.0f;
		[Export] public int ExperienceReward { get; set; } = 15;
		[Export] public int GoldReward { get; set; } = 20;

		// ─── 행동 패턴 ──────────────────────────────────────────────
		[Export] public EnemyBehavior Behavior { get; set; } = EnemyBehavior.Melee;
		[Export] public float PreferredRange { get; set; } = 150.0f; // Ranged 전용: 유지하려는 거리

		// ─── 식별 및 보스 설정 ──────────────────────────────────────
		[Export] public string EnemyTypeName { get; set; } = "Orc";
		[Export] public bool IsBoss { get; set; } = false;

		// ─── 스프라이트 설정 (Kenney 타일맵 좌표) ────────────────────
		[ExportGroup("Sprite")]
		[Export] public Vector2I SpriteAtlasCoords { get; set; } = new(10, 8);

		// ─── 드롭 테이블 ─────────────────────────────────────────────
		[ExportGroup("Drop Table")]
		[Export] public ItemData[] PossibleDrops { get; set; }
		[Export] public float[] DropWeights { get; set; }  // PossibleDrops와 같은 길이. 미설정 시 균등 확률
		[Export] public float DropChance { get; set; } = 0.5f;

		/// <summary>가중치 기반 랜덤 드랍 인덱스 반환. DropWeights 미설정 시 균등 선택.</summary>
		public int PickDropIndex()
		{
			if (PossibleDrops == null || PossibleDrops.Length == 0) return -1;
			if (DropWeights == null || DropWeights.Length != PossibleDrops.Length)
				return (int)(GD.Randi() % (uint)PossibleDrops.Length);

			float total = 0f;
			foreach (var w in DropWeights) total += w;
			if (total <= 0f) return (int)(GD.Randi() % (uint)PossibleDrops.Length);

			float roll = (float)GD.RandRange(0.0, total);
			float cumulative = 0f;
			for (int i = 0; i < DropWeights.Length; i++)
			{
				cumulative += DropWeights[i];
				if (roll < cumulative) return i;
			}
			return PossibleDrops.Length - 1;
		}
	}
}
