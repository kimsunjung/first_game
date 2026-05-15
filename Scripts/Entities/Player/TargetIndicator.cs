using Godot;

namespace FirstGame.Entities.Player
{
	/// <summary>
	/// 자동 타겟된 적 위에 표시되는 노란 화살표.
	/// _Draw로 단순 삼각형 + 펄스 애니메이션.
	/// </summary>
	public partial class TargetIndicator : Node2D
	{
		private float _pulse = 0f;
		private static readonly Color YellowGlow = new Color(1f, 0.85f, 0.2f, 0.9f);

		public override void _Process(double delta)
		{
			_pulse += (float)delta * 4f;
			QueueRedraw();
		}

		public override void _Draw()
		{
			float bob = Mathf.Sin(_pulse) * 3f;
			float size = 8f;
			// 아래 화살표(▼) — 적 위에서 가리킴
			var pts = new Vector2[]
			{
				new Vector2(-size, -size + bob),
				new Vector2( size, -size + bob),
				new Vector2(0,     size * 0.3f + bob)
			};
			DrawColoredPolygon(pts, YellowGlow);
			DrawPolyline(new Vector2[] { pts[0], pts[1], pts[2], pts[0] },
				new Color(1f, 0.7f, 0f, 1f), 1.5f);
		}
	}
}
