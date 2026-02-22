using Godot;
using System;
using FirstGame.Data;
using FirstGame.Entities.Player;

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

		public override void _Ready()
		{
			_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (_sprite != null && Item != null)
			{
				_sprite.Texture = Item.Icon;
			}

			// 파티클 (옵션)
			var cpuParticles = GetNodeOrNull<CpuParticles2D>("RarityParticles");
			if (cpuParticles != null && Item != null)
			{
				SetRarityColor(cpuParticles);
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
		}

		private void SetRarityColor(CpuParticles2D particles)
		{
			if (Item == null) return;
			
			switch (Item.Rarity)
			{
				case ItemRarity.Common: particles.Color = Colors.White; break;
				case ItemRarity.Uncommon: particles.Color = Colors.LightGreen; break;
				case ItemRarity.Rare: particles.Color = Colors.DodgerBlue; break;
				case ItemRarity.Epic: particles.Color = Colors.Purple; break;
				case ItemRarity.Legendary: particles.Color = Colors.Gold; break;
				default: particles.Color = Colors.White; break;
			}
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
					CollectItem((PlayerController)_magnetTarget);
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
			if (_isMagnetized) return;
			
			if (body.IsInGroup("Player") && body is PlayerController player)
			{
				// 자석 모드 온
				_isMagnetized = true;
				_magnetTarget = player;
			}
		}

		private void CollectItem(PlayerController player)
		{
			if (Item == null) 
			{
				QueueFree();
				return;
			}

			bool added = player.Inventory.AddItem(Item, Quantity);
			if (added)
			{
				// 사운드 재생
				FirstGame.Core.AudioManager.Instance?.PlaySFX("pickup.wav");
				QueueFree();
			}
			else
			{
				// 가방이 꽉 찼으면 다시 바닥에 떨어짐
				_isMagnetized = false;
				_magnetTarget = null;
				_magnetSpeed = 400.0f;
				
				// 살짝 튕겨내는 연출
				Drop(GlobalPosition, new Vector2((float)GD.RandRange(-1, 1), -1), 150f);
			}
		}
	}
}
