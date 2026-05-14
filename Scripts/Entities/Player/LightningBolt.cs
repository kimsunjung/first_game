using Godot;

namespace FirstGame.Entities.Player
{
	/// <summary>
	/// 두 위치 사이 번개 라인. 짧은 fade out 후 자동 제거.
	/// </summary>
	public partial class LightningBolt : Node2D
	{
		private Vector2 _from;
		private Vector2 _to;
		private float _life = 0.3f;
		private float _elapsed = 0f;

		public LightningBolt(Vector2 from, Vector2 to)
		{
			_from = from;
			_to = to;
		}

		public override void _Process(double delta)
		{
			_elapsed += (float)delta;
			QueueRedraw();
			if (_elapsed >= _life) QueueFree();
		}

		public override void _Draw()
		{
			float alpha = Mathf.Clamp(1f - _elapsed / _life, 0f, 1f);
			var local0 = _from - GlobalPosition;
			var local1 = _to - GlobalPosition;
			// 외곽 글로우
			DrawLine(local0, local1, new Color(0.6f, 0.8f, 1f, alpha * 0.45f), 8f);
			// 중간
			DrawLine(local0, local1, new Color(0.8f, 0.95f, 1f, alpha * 0.7f), 4f);
			// 코어
			DrawLine(local0, local1, new Color(1f, 1f, 1f, alpha), 2f);
		}
	}
}
