using Godot;
using System;
using FirstGame.Data;
using FirstGame.Core.Interfaces;

namespace FirstGame.Objects
{
	public partial class FieldItem : Area2D
	{
		[Export] public ItemData Item { get; set; }
		[Export] public int Quantity { get; set; } = 1;

		private Sprite2D _sprite;
		private float _startY;
		private float _timePassed;
		private bool _isMagnetized = false;
		private Node2D _magnetTarget;
		private float _magnetSpeed = 400.0f;

		// 물리 드랍 (바닥에 통통 튀는) 연출용 변수
		private Vector2 _velocity;
		private float _gravity = 800.0f;
		private float _groundY;
		private bool _isLanded = false;
		private int _bounceCount = 0;
		private bool _canCollect = true; // 드롭 직후 수집 방지용

		public override void _Ready()
		{
			_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (_sprite != null && Item?.Icon != null)
			{
				_sprite.Texture = Item.Icon;
				// 아이콘 크기에 관계없이 필드에서 16x16 픽셀로 표시
				float targetSize = 16.0f;
				float maxDim = Mathf.Max(Item.Icon.GetWidth(), Item.Icon.GetHeight());
				if (maxDim > 0)
				{
					float s = targetSize / maxDim;
					_sprite.Scale = new Vector2(s, s);
				}
			}

			BodyEntered += OnBodyEntered;

			// 착지 목표 Y값 설정 (현재 위치 언저리)
			_groundY = GlobalPosition.Y + (float)GD.RandRange(-10, 10);
			_startY = GlobalPosition.Y;
		}

		public void Drop(Vector2 startPos, Vector2 direction, float force)
		{
			GlobalPosition = startPos;
			_velocity = direction.Normalized() * force;
			_velocity.Y = -Mathf.Abs(force); // 위로 솟구치게
			_isLanded = false;
			_bounceCount = 0;
			_groundY = startPos.Y + (float)GD.RandRange(0, 20); // 초기 Y 위치 기준 약간 아래

			// 드롭 후 0.5초간 수집 불가 (통통 튀는 연출이 보이도록)
			_canCollect = false;
			GetTree().CreateTimer(0.5).Timeout += () =>
			{
				if (IsInstanceValid(this)) _canCollect = true;
			};
		}

		public override void _Process(double delta)
		{
			if (_isMagnetized && _magnetTarget != null)
			{
				// 플레이어에게 빨려들어감
				Vector2 dir = (_magnetTarget.GlobalPosition - GlobalPosition).Normalized();
				GlobalPosition += dir * _magnetSpeed * (float)delta;
				_magnetSpeed += 1000.0f * (float)delta; // 갈수록 빨라짐

				if (GlobalPosition.DistanceTo(_magnetTarget.GlobalPosition) < 20.0f)
				{
					CollectItem(_magnetTarget as IItemCollector);
				}
				return;
			}

			if (!_isLanded)
			{
				// 통통 튀는 물리 연출
				_velocity.Y += _gravity * (float)delta;
				GlobalPosition += _velocity * (float)delta;

				if (GlobalPosition.Y >= _groundY && _velocity.Y > 0)
				{
					GlobalPosition = new Vector2(GlobalPosition.X, _groundY);
					_bounceCount++;
					_velocity.Y *= -0.5f; // 반발력
					_velocity.X *= 0.6f;  // 마찰

					if (_bounceCount > 2 || Mathf.Abs(_velocity.Y) < 30f)
					{
						_isLanded = true;
						_startY = GlobalPosition.Y; // 둥둥 떠다니기 기준점 리셋
					}
				}
			}
			else
			{
				// 착지 후 살짝 위아래로 둥둥 떠다니는 애니메이션
				_timePassed += (float)delta * 3.0f;
				Vector2 pos = GlobalPosition;
				pos.Y = _startY + Mathf.Sin(_timePassed) * 5.0f;
				GlobalPosition = pos;
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (_isMagnetized || !_canCollect) return;

			if (body.IsInGroup("Player") && body is IItemCollector)
			{
				_isMagnetized = true;
				_magnetTarget = body;
			}
		}

		private void CollectItem(IItemCollector collector)
		{
			if (Item == null)
			{
				QueueFree();
				return;
			}

			if (collector == null)
			{
				QueueFree();
				return;
			}

			bool added = collector.CollectItem(Item, Quantity);
			if (added)
			{
				FirstGame.Core.AudioManager.Instance?.PlaySFX("pickup.wav");
				QueueFree();
			}
			else
			{
				// 가방이 꽉 찼으면 살짝 튕겨내고 2초 쿨다운
				// 재드랍으로 플레이어 Area2D를 벗어나게 하여 이후 BodyEntered 재발동 보장
				_isMagnetized = false;
				_magnetTarget = null;
				_magnetSpeed = 400.0f;
				_canCollect = false;
				Drop(GlobalPosition, new Vector2((float)GD.RandRange(-1, 1), -1), 80f);
				GetTree().CreateTimer(2.0).Timeout += () =>
				{
					if (IsInstanceValid(this)) _canCollect = true;
				};
			}
		}
	}
}
