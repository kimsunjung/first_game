using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Data;
using FirstGame.Core.Interfaces;

namespace FirstGame.Objects
{
	public partial class FieldItem : Area2D
	{
		[Export] public ItemData Item { get; set; }
		[Export] public int Quantity { get; set; } = 1;
		// 장신구 드롭의 인스턴스별 affix. EnemyController.SpawnFieldDrop이 주입.
		public List<ItemAffix> Affixes { get; set; } = new();

		private Sprite2D _sprite;
		// 희귀도 글로우 — 새 PNG 없이 코드로 생성한 가벼운 Sprite2D 1개(파티클/Light2D 미사용,
		// 모바일 성능 고려). Common은 글로우 없음. 알파만 사인 펄스.
		private Sprite2D _glow;
		private float _glowTime;
		private float _glowBaseScale = 1f;
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
		// 가방 만석으로 튕겨낸 상태 — 이 동안은 Drop()의 0.5초 재무장 타이머가
		// _canCollect를 켜지 못하게 막고, 2초 타이머만 해제하도록 한다.
		private bool _bagFullBounce = false;

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

			SetupGlow();

			BodyEntered += OnBodyEntered;

			// 착지 목표 Y값 설정 (현재 위치 언저리)
			_groundY = GlobalPosition.Y + (float)GD.RandRange(-10, 10);
			_startY = GlobalPosition.Y;
		}

		private static Color RarityGlowColor(ItemRarity r) => r switch
		{
			ItemRarity.Uncommon => new Color(0.40f, 1.00f, 0.45f),
			ItemRarity.Rare => new Color(0.35f, 0.65f, 1.00f),
			ItemRarity.Epic => new Color(0.75f, 0.40f, 1.00f),
			ItemRarity.Legendary => new Color(1.00f, 0.82f, 0.20f),
			_ => Colors.White,
		};

		// Common 초과 희귀도에서만 아이콘을 확대·틴트한 글로우 Sprite2D 1개 생성.
		private void SetupGlow()
		{
			if (Item == null || Item.Icon == null) return;
			if (Item.Rarity == ItemRarity.Common) return;

			float maxDim = Mathf.Max(Item.Icon.GetWidth(), Item.Icon.GetHeight());
			if (maxDim <= 0) return;
			float spriteScale = 16.0f / maxDim; // _sprite와 동일 표시 크기 기준
			// 희귀도 높을수록 글로우 약간 더 큼.
			float mult = Item.Rarity >= ItemRarity.Epic ? 2.0f : 1.7f;
			_glowBaseScale = spriteScale * mult;

			_glow = new Sprite2D
			{
				Name = "RarityGlow",
				Texture = Item.Icon,
				TextureFilter = CanvasItem.TextureFilterEnum.Linear,
				ZIndex = -1, // 아이콘 뒤
				Scale = new Vector2(_glowBaseScale, _glowBaseScale),
				Modulate = RarityGlowColor(Item.Rarity) with { A = 0.35f },
			};
			AddChild(_glow);
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
				if (!IsInstanceValid(this)) return;
				// 만석 튕김 중이면 2초 쿨다운이 전담 — 0.5초 타이머는 관여하지 않음.
				if (_bagFullBounce) return;
				_canCollect = true;
				// 0.5s 픽업 잠금 동안 플레이어가 이미 위에 서 있어 BodyEntered가 재발화하지 않는 케이스 보강.
				foreach (var body in GetOverlappingBodies())
				{
					if (body is Node2D n2d && n2d.IsInGroup("Player") && n2d is IItemCollector)
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
			if (_glow != null)
			{
				_glowTime += (float)delta * 3.0f;
				float a = 0.30f + 0.18f * Mathf.Sin(_glowTime);
				_glow.Modulate = _glow.Modulate with { A = a };
				float s = _glowBaseScale * (1.0f + 0.06f * Mathf.Sin(_glowTime));
				_glow.Scale = new Vector2(s, s);
			}

			if (_isMagnetized && !IsInstanceValid(_magnetTarget))
			{
				// 자석 대상(플레이어)이 씬 전환 등으로 해제됨 — 자석 상태 리셋.
				_isMagnetized = false;
				_magnetTarget = null;
				_magnetSpeed = 400.0f;
			}
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

			bool added = collector.CollectItem(Item, Quantity, Affixes);
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
				_bagFullBounce = true; // Drop()의 0.5초 타이머가 _canCollect를 못 켜게
				Drop(GlobalPosition, new Vector2((float)GD.RandRange(-1, 1), -1), 80f);
				GetTree().CreateTimer(2.0).Timeout += () =>
				{
					if (!IsInstanceValid(this)) return;
					_bagFullBounce = false;
					_canCollect = true;
				};
			}
		}
	}
}
