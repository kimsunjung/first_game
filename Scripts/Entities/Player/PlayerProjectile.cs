using Godot;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Player
{
	/// <summary>
	/// 플레이어가 발사하는 투사체. 적과 충돌 시 데미지 + 소멸.
	/// Mage/Archer 평타·원거리 스킬에서 즉시 데미지 대신 사용.
	/// </summary>
	public partial class PlayerProjectile : Area2D
	{
		public int Damage { get; set; } = 5;
		public float Speed { get; set; } = 420f;
		public Vector2 Direction { get; set; } = Vector2.Right;
		public ElementType Element { get; set; } = ElementType.None;
		public Color ProjectileColor { get; set; } = new Color(1.0f, 0.6f, 0.2f);
		public Texture2D Texture { get; set; }
		public float TextureScale { get; set; } = 0.5f;
		// 단일 타겟 보장 — 한 번 적중하면 즉시 소멸.
		public bool SingleHit { get; set; } = true;
		// 관통 횟수 — 0이면 첫 적중 시 소멸. N이면 최대 N+1 적 관통.
		public int PierceCount { get; set; } = 0;
		// 적중 시 InflictedStatusChance 확률로 InflictedStatus를 InflictedStatusDuration 동안 부여.
		// 기본 None → 데이터 미설정 스킬/평타는 영향 없음 (EnemyProjectile 와 대칭).
		public StatusEffect InflictedStatus { get; set; } = StatusEffect.None;
		public float InflictedStatusDuration { get; set; } = 3.0f;
		public float InflictedStatusChance { get; set; } = 0f;
		// 최대 비거리(px) — 화면 밖 적까지 평타/스킬이 길게 닿지 않도록 제한.
		// 자동 조준 사거리보다 약간 길게 두어 움직이는 적을 따라잡을 여지만 남긴다.
		public float MaxTravel { get; set; } = 320f;

		private float _traveled = 0f;
		private float _lifetime = 1.0f;
		private bool _consumed = false;
		private readonly System.Collections.Generic.HashSet<ulong> _hitIds = new();

		public override void _Ready()
		{
			// 적과 충돌 검출 — 적 collision_layer=2.
			CollisionLayer = 0;
			CollisionMask = 2;
			Monitoring = true;

			var shape = new CollisionShape2D();
			var circle = new CircleShape2D { Radius = 6f };
			shape.Shape = circle;
			AddChild(shape);

			if (Texture != null)
			{
				var sprite = new Sprite2D
				{
					Texture = Texture,
					Scale = new Vector2(TextureScale, TextureScale),
					TextureFilter = TextureFilterEnum.Nearest
				};
				AddChild(sprite);
				Rotation = Direction.Angle();
			}

			AreaEntered += OnAreaEntered;
			BodyEntered += OnBodyEntered;
			QueueRedraw();
		}

		public override void _Draw()
		{
			if (Texture != null) return;
			DrawCircle(Vector2.Zero, 10f, new Color(ProjectileColor.R, ProjectileColor.G, ProjectileColor.B, 0.15f));
			DrawCircle(Vector2.Zero, 6f,  new Color(ProjectileColor.R, ProjectileColor.G, ProjectileColor.B, 0.45f));
			DrawCircle(Vector2.Zero, 3.5f, ProjectileColor);
			DrawCircle(Vector2.Zero, 1.6f, new Color(1f, 1f, 1f, 0.85f));
		}

		public override void _PhysicsProcess(double delta)
		{
			float step = Speed * FirstGame.Core.BalanceData.Movement.ProjectileSpeedMultiplier * (float)delta;
			Position += Direction * step;
			_traveled += step;
			if (_traveled >= MaxTravel) { QueueFree(); return; }
			_lifetime -= (float)delta;
			if (_lifetime <= 0f) QueueFree();
		}

		private void OnAreaEntered(Area2D area)
		{
			TryHit(area);
		}

		private void OnBodyEntered(Node2D body)
		{
			TryHit(body);
		}

		private void TryHit(Node2D node)
		{
			if (_consumed) return;
			if (node is IDamageable target)
			{
				ulong id = node.GetInstanceId();
				if (_hitIds.Contains(id)) return;
				_hitIds.Add(id);
				target.TakeDamage(Damage, Element);
				if (InflictedStatus != StatusEffect.None
				    && InflictedStatusChance > 0f
				    && GD.Randf() <= InflictedStatusChance)
					target.ApplyStatusEffect(InflictedStatus, InflictedStatusDuration);
				SpawnHitEffect();
				if (PierceCount <= 0)
				{
					_consumed = true;
					QueueFree();
				}
				else
				{
					PierceCount--;
				}
			}
		}

		private void SpawnHitEffect()
		{
			var flash = new FirstGame.Entities.Enemies.HitFlash { GlobalPosition = GlobalPosition };
			GetTree()?.CurrentScene?.AddChild(flash);
			flash.Play(ProjectileColor);
		}
	}
}
