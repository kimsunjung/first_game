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
		// 단일 PNG 스프라이트 (정면 1프레임 등). 지정 시 타일맵 좌표 대신 사용. EnemyController.SetupAnimations가 분기.
		[Export] public Texture2D Sprite { get; set; }
		// AnimatedSprite2D만의 시각 스케일. 1.0이 기본. 128×128처럼 큰 원본 PNG를 게임용 크기로 축소할 때 사용.
		// 콜리전·체력바·elite root scale은 영향 받지 않음 — root Scale은 EnemySpawner의 elite 처리가 점유.
		[Export] public float SpriteScale { get; set; } = 1.0f;
		// CollisionShape2D만의 스케일. 1.0이 기본. SpriteScale로 시각이 커진 적의 충돌 박스 정합성 회복용.
		// 14×14 기본 콜리전 기준으로 배수 적용 — root Scale 미사용이라 elite scale과 독립적으로 동작.
		[Export] public float CollisionScale { get; set; } = 1.0f;

		// ─── 속성 (Elemental) ───────────────────────────────────────
		[ExportGroup("Element")]
		// 적 자체 속성 — 동일 속성 공격에 25% 저항.
		[Export] public ElementType Element { get; set; } = ElementType.None;
		// 약점 속성 — 일치하는 공격에 50% 추가 데미지.
		[Export] public ElementType Weakness { get; set; } = ElementType.None;

		// ─── 투사체 (Ranged 전용) ───────────────────────────────────
		[ExportGroup("Projectile")]
		// Ranged 적의 투사체 텍스처. null이면 EnemyProjectile이 기존 DrawCircle 폴백 렌더링 사용.
		[Export] public Texture2D ProjectileTexture { get; set; }
		[Export] public float ProjectileScale { get; set; } = 0.5f;

		// ─── 보스 패턴 (BossController 전용) ───────────────────────────
		[ExportGroup("Boss Patterns")]
		// IsBoss=true 또는 명명 미니보스에 패턴 배열 지정. null이면 패턴 없음.
		[Export] public FirstGame.Entities.Enemies.BossPatternData[] Patterns { get; set; }

		// ─── 상태이상 부여 (공격 시 확률적으로 플레이어에게 적용) ───────
		[ExportGroup("Status Effect")]
		[Export] public StatusEffect InflictedStatus { get; set; } = StatusEffect.None;
		[Export] public float InflictedStatusDuration { get; set; } = 3.0f;
		[Export] public float InflictedStatusChance { get; set; } = 0.25f;

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
