using Godot;

namespace FirstGame.Entities.Enemies
{
	/// <summary>
	/// 보스 패턴 경보 도형을 코드로 직접 그린다 (PNG 불필요).
	/// TelegraphDuration 초 동안 표시 후 자동 제거.
	/// </summary>
	public partial class Telegraph : Node2D
	{
		public enum Shape { Circle, Rectangle, Line, Cone }

		private Shape _shape;
		private float _radius;
		private Vector2 _size;   // Rectangle 전용
		private float _angle;    // Line/Cone 전용 (radians)
		private float _duration;
		private float _elapsed;
		private bool _done;

		private static readonly Color _baseColor = new Color(1f, 0.15f, 0.05f, 0.55f);

		public static Telegraph CreateCircle(Node parent, Vector2 worldPos, float radius, float duration)
		{
			var t = new Telegraph { _shape = Shape.Circle, _radius = radius, _duration = duration };
			t.GlobalPosition = worldPos;
			parent.AddChild(t);
			return t;
		}

		public static Telegraph CreateLine(Node parent, Vector2 worldPos, float length, float angleRad, float duration)
		{
			var t = new Telegraph { _shape = Shape.Line, _radius = length, _angle = angleRad, _duration = duration };
			t.GlobalPosition = worldPos;
			parent.AddChild(t);
			return t;
		}

		public static Telegraph CreateCone(Node parent, Vector2 worldPos, float length, float dirAngle, float duration)
		{
			var t = new Telegraph { _shape = Shape.Cone, _radius = length, _angle = dirAngle, _duration = duration };
			t.GlobalPosition = worldPos;
			parent.AddChild(t);
			return t;
		}

		public static Telegraph CreateRect(Node parent, Vector2 worldPos, Vector2 size, float duration)
		{
			var t = new Telegraph { _shape = Shape.Rectangle, _size = size, _duration = duration };
			t.GlobalPosition = worldPos;
			parent.AddChild(t);
			return t;
		}

		public override void _Process(double delta)
		{
			if (_done) return;
			_elapsed += (float)delta;
			QueueRedraw();
			if (_elapsed >= _duration)
			{
				_done = true;
				QueueFree();
			}
		}

		public override void _Draw()
		{
			if (_done) return;
			float alpha = Mathf.Lerp(0.6f, 0.0f, _elapsed / Mathf.Max(0.001f, _duration));
			var color = new Color(_baseColor.R, _baseColor.G, _baseColor.B, alpha);

			switch (_shape)
			{
				case Shape.Circle:
					DrawCircle(Vector2.Zero, _radius, color);
					DrawArc(Vector2.Zero, _radius, 0, Mathf.Tau, 48, new Color(1, 0.2f, 0, 1), 2f);
					break;

				case Shape.Rectangle:
					var rect = new Rect2(-_size * 0.5f, _size);
					DrawRect(rect, color);
					DrawRect(rect, new Color(1, 0.2f, 0, 1), false, 2f);
					break;

				case Shape.Line:
					var dir = Vector2.Right.Rotated(_angle);
					DrawLine(Vector2.Zero, dir * _radius, new Color(1, 0.2f, 0, alpha * 1.6f), 12f);
					break;

				case Shape.Cone:
					float halfAngle = Mathf.Pi / 6f; // 60도 콘
					var points = new Vector2[3];
					points[0] = Vector2.Zero;
					points[1] = Vector2.Right.Rotated(_angle - halfAngle) * _radius;
					points[2] = Vector2.Right.Rotated(_angle + halfAngle) * _radius;
					DrawPolygon(points, new Color[] { color, color, color });
					break;
			}
		}
	}
}
