using Godot;
using FirstGame.Core;

namespace FirstGame.Objects
{
	public partial class GoldPickup : Area2D
	{
		[Export] public int Amount { get; set; } = 10;
		[Export] public Texture2D SmallTexture { get; set; }
		[Export] public Texture2D LargeTexture { get; set; }
		[Export] public int LargeThreshold { get; set; } = 50;

		private Sprite2D _sprite;
		private float _startY;
		private float _timePassed;
		private bool _isMagnetized = false;
		private Node2D _magnetTarget;
		private float _magnetSpeed = 400.0f;

		private Vector2 _velocity;
		private float _gravity = 800.0f;
		private float _groundY;
		private bool _isLanded = false;
		private int _bounceCount = 0;
		private bool _canCollect = true;

		public override void _Ready()
		{
			_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (_sprite != null)
			{
				var tex = (Amount >= LargeThreshold) ? LargeTexture : SmallTexture;
				if (tex == null) tex = SmallTexture ?? LargeTexture;
				if (tex != null)
				{
					_sprite.Texture = tex;
					float targetSize = 16.0f;
					float maxDim = Mathf.Max(tex.GetWidth(), tex.GetHeight());
					if (maxDim > 0)
					{
						float s = targetSize / maxDim;
						_sprite.Scale = new Vector2(s, s);
					}
				}
			}

			BodyEntered += OnBodyEntered;
			_groundY = GlobalPosition.Y + (float)GD.RandRange(-10, 10);
			_startY = GlobalPosition.Y;
		}

		public void Drop(Vector2 startPos, Vector2 direction, float force)
		{
			GlobalPosition = startPos;
			_velocity = direction.Normalized() * force;
			_velocity.Y = -Mathf.Abs(force);
			_isLanded = false;
			_bounceCount = 0;
			_groundY = startPos.Y + (float)GD.RandRange(0, 20);

			_canCollect = false;
			GetTree().CreateTimer(0.5).Timeout += () =>
			{
				if (!IsInstanceValid(this)) return;
				_canCollect = true;
				// 0.5s 픽업 잠금 동안 플레이어가 이미 위에 서 있어 BodyEntered가 재발화하지 않는 케이스 보강.
				foreach (var body in GetOverlappingBodies())
				{
					if (body is Node2D n2d && n2d.IsInGroup("Player"))
					{
						_isMagnetized = true;
						_magnetTarget = n2d;
						break;
					}
				}
			};
		}

		public override void _Process(double delta)
		{
			if (_isMagnetized && IsInstanceValid(_magnetTarget))
			{
				Vector2 dir = (_magnetTarget.GlobalPosition - GlobalPosition).Normalized();
				GlobalPosition += dir * _magnetSpeed * (float)delta;
				_magnetSpeed += 1000.0f * (float)delta;

				if (GlobalPosition.DistanceTo(_magnetTarget.GlobalPosition) < 20.0f)
				{
					Collect();
				}
				return;
			}
			if (_isMagnetized) _isMagnetized = false; // 타깃이 해제된 경우 자석 상태 해제

			if (!_isLanded)
			{
				_velocity.Y += _gravity * (float)delta;
				GlobalPosition += _velocity * (float)delta;

				if (GlobalPosition.Y >= _groundY && _velocity.Y > 0)
				{
					GlobalPosition = new Vector2(GlobalPosition.X, _groundY);
					_bounceCount++;
					_velocity.Y *= -0.5f;
					_velocity.X *= 0.6f;

					if (_bounceCount > 2 || Mathf.Abs(_velocity.Y) < 30f)
					{
						_isLanded = true;
						_startY = GlobalPosition.Y;
					}
				}
			}
			else
			{
				_timePassed += (float)delta * 3.0f;
				Vector2 pos = GlobalPosition;
				pos.Y = _startY + Mathf.Sin(_timePassed) * 5.0f;
				GlobalPosition = pos;
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (_isMagnetized || !_canCollect) return;
			if (body.IsInGroup("Player"))
			{
				_isMagnetized = true;
				_magnetTarget = body;
			}
		}

		private void Collect()
		{
			if (GameManager.Instance != null)
			{
				GameManager.Instance.PlayerGold += Amount;
				// 적 처치 시점의 자동저장은 PlayerGold 변동 전 스냅샷이므로,
				// 픽업 시점에 다시 저장 요청해 골드 손실 방지. throttle 덕에 빈번 호출도 안전.
				SaveManager.RequestAutoSave();
			}

			UIEffectManager.SpawnGoldLabel(GlobalPosition, Amount);
			AudioManager.Instance?.PlaySFX("pickup.wav");
			QueueFree();
		}
	}
}
