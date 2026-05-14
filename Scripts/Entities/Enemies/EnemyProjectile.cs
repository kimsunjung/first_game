using Godot;
using FirstGame.Core.Interfaces;

namespace FirstGame.Entities.Enemies
{
	public partial class EnemyProjectile : Area2D
	{
		public int Damage { get; set; } = 3;
		public float Speed { get; set; } = 120f;
		public Vector2 Direction { get; set; } = Vector2.Right;
		public Color ProjectileColor { get; set; } = new Color(0.6f, 0.2f, 1f);
		// 지정 시 Sprite2D 텍스처 렌더링으로 전환. null이면 기존 DrawCircle 폴백.
		public Texture2D Texture { get; set; }
		public float TextureScale { get; set; } = 0.5f;

		private float _lifetime = 3f;

		public override void _Ready()
		{
			CollisionLayer = 0;
			CollisionMask = 1 | 4; // 플레이어 + 벽/장애물 레이어
			Monitoring = true;

			var shape = new CollisionShape2D();
			var circle = new CircleShape2D();
			circle.Radius = 5f;
			shape.Shape = circle;
			AddChild(shape);

			if (Texture != null)
			{
				var sprite = new Sprite2D
				{
					Texture = Texture,
					Scale = new Vector2(TextureScale, TextureScale),
					TextureFilter = TextureFilterEnum.Nearest,
				};
				AddChild(sprite);
				Rotation = Direction.Angle();
			}

			BodyEntered += OnBodyEntered;
			QueueRedraw();
		}

		public override void _Draw()
		{
			if (Texture != null) return; // 스프라이트 렌더링 사용 시 폴백 그리기 생략

			// 글로우 (외곽)
			DrawCircle(Vector2.Zero, 9f, new Color(ProjectileColor.R, ProjectileColor.G, ProjectileColor.B, 0.15f));
			// 중간 광채
			DrawCircle(Vector2.Zero, 6f, new Color(ProjectileColor.R, ProjectileColor.G, ProjectileColor.B, 0.4f));
			// 코어
			DrawCircle(Vector2.Zero, 3.5f, ProjectileColor);
			// 밝은 중심
			DrawCircle(Vector2.Zero, 1.5f, new Color(1f, 1f, 1f, 0.8f));
		}

		public override void _PhysicsProcess(double delta)
		{
			Position += Direction * Speed * FirstGame.Core.BalanceData.Movement.ProjectileSpeedMultiplier * (float)delta;
			_lifetime -= (float)delta;
			if (_lifetime <= 0f) QueueFree();
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body is IDamageable target)
			{
				target.TakeDamage(Damage);
			}

			// 플레이어든 벽이든 무언가에 맞으면 투사체는 소멸
			SpawnHitEffect();
			QueueFree();
		}

		private void SpawnHitEffect()
		{
			var flash = new HitFlash { GlobalPosition = GlobalPosition };
			GetTree().CurrentScene.AddChild(flash);
			flash.Play(ProjectileColor);
		}
	}

	/// <summary>투사체 히트 시 원형 확산 플래시. 자체 _Draw로 그려 빈 Sprite2D 노드 누적 방지.</summary>
	public partial class HitFlash : Node2D
	{
		private const float StartRadius = 4f;
		private const float EndRadius = 18f;
		private const float Duration = 0.18f;

		private float _radius = StartRadius;
		private float _alpha = 0.9f;
		private Color _color = Colors.White;

		public void Play(Color color)
		{
			_color = color;
			var tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenMethod(Callable.From<float>(r => { _radius = r; QueueRedraw(); }), StartRadius, EndRadius, Duration);
			tween.TweenMethod(Callable.From<float>(a => { _alpha = a; QueueRedraw(); }), 0.9f, 0f, Duration);
			tween.Chain().TweenCallback(Callable.From(QueueFree));
		}

		public override void _Draw()
		{
			DrawCircle(Vector2.Zero, _radius, new Color(_color.R, _color.G, _color.B, _alpha));
		}
	}
}
