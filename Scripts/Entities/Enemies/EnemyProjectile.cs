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

		private float _lifetime = 3f;
		private float _trailTimer = 0f;

		public override void _Ready()
		{
			CollisionLayer = 0;
			CollisionMask = 1; // 플레이어 레이어
			Monitoring = true;

			var shape = new CollisionShape2D();
			var circle = new CircleShape2D();
			circle.Radius = 5f;
			shape.Shape = circle;
			AddChild(shape);

			BodyEntered += OnBodyEntered;
			QueueRedraw();
		}

		public override void _Draw()
		{
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
			Position += Direction * Speed * (float)delta;
			_lifetime -= (float)delta;

			// 잔상 효과
			_trailTimer -= (float)delta;
			if (_trailTimer <= 0f)
			{
				_trailTimer = 0.05f;
				SpawnTrail();
			}

			if (_lifetime <= 0f) QueueFree();
		}

		private void SpawnTrail()
		{
			var trail = new Node2D();
			trail.GlobalPosition = GlobalPosition;
			GetTree().CurrentScene.AddChild(trail);

			// 잔상은 서서히 사라짐
			trail.Modulate = new Color(ProjectileColor.R, ProjectileColor.G, ProjectileColor.B, 0.5f);
			var tween = trail.CreateTween();
			tween.TweenProperty(trail, "modulate:a", 0f, 0.3f);
			tween.TweenCallback(Callable.From(() =>
			{
				if (IsInstanceValid(trail)) trail.QueueFree();
			}));
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body is IDamageable target)
			{
				target.TakeDamage(Damage);

				// 히트 이펙트
				SpawnHitEffect();
				QueueFree();
			}
		}

		private void SpawnHitEffect()
		{
			var hit = new Node2D();
			hit.GlobalPosition = GlobalPosition;
			GetTree().CurrentScene.AddChild(hit);

			// 히트 시 원형 확장 이펙트
			var sprite = new Sprite2D();
			sprite.Modulate = ProjectileColor;
			sprite.Scale = new Vector2(0.3f, 0.3f);
			hit.AddChild(sprite);

			var tween = hit.CreateTween();
			tween.TweenProperty(sprite, "scale", new Vector2(2.5f, 2.5f), 0.2f);
			tween.Parallel().TweenProperty(sprite, "modulate:a", 0f, 0.2f);
			tween.TweenCallback(Callable.From(() =>
			{
				if (IsInstanceValid(hit)) hit.QueueFree();
			}));
		}
	}
}
