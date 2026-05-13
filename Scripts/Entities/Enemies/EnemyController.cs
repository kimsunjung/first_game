using Godot;
using System;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities.Enemies
{
	public enum EnemyState { Idle, Chase, Attack, Flee, Dead }

	public partial class EnemyController : CharacterBody2D, IDamageable, IKnockbackable
	{
		[Export] public EnemyStats Stats { get; set; }

		/// <summary>
		/// 보스 처치 식별자. 같은 boss .tres를 여러 던전에서 공유할 때 처치 키 충돌을
		/// 방지하기 위해 EnemySpawner가 스폰 시 주입. 비어있으면 EnemyTypeName fallback.
		/// </summary>
		public string BossId { get; set; } = "";

		private Node2D _target;
		private ProgressBar _healthBar;
		private AnimatedSprite2D _animSprite;
		private bool _isDying = false;
		private float _attackTimer = 0f;
		private EnemyState _state = EnemyState.Idle;
		private Vector2 _knockbackVelocity = Vector2.Zero;
		private bool _isAttacking = false;
		private bool _isProvoked = false; // Passive 행동: 피격 전까지 비공격

		// 진행 중인 공격 tween 참조 — 사망 시 Kill해 죽은 적이 데미지를 주거나
		// 투사체를 발사하는 결함 차단. CreateTween()이 반환한 SceneTreeTween을 보관.
		private Tween _attackTween;

		public override void _Ready()
		{
			if (Stats != null)
			{
				var drops = Stats.PossibleDrops;
				var dropChance = Stats.DropChance;
				Stats = (EnemyStats)Stats.Duplicate();
				Stats.PossibleDrops = drops;
				Stats.DropChance = dropChance;
			}
			else
				Stats = new EnemyStats();

			Stats.CurrentHealth = Stats.MaxHealth;

			_healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
			if (_healthBar != null)
			{
				_healthBar.MaxValue = Stats.MaxHealth;
				_healthBar.Value = Stats.CurrentHealth;

				var bgStyle = new StyleBoxFlat
				{
					BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
					CornerRadiusTopLeft = 2,
					CornerRadiusTopRight = 2,
					CornerRadiusBottomRight = 2,
					CornerRadiusBottomLeft = 2
				};
				_healthBar.AddThemeStyleboxOverride("background", bgStyle);

				var fillStyle = new StyleBoxFlat
				{
					BgColor = new Color(0.8f, 0.1f, 0.1f, 1.0f),
					CornerRadiusTopLeft = 2,
					CornerRadiusTopRight = 2,
					CornerRadiusBottomRight = 2,
					CornerRadiusBottomLeft = 2
				};
				_healthBar.AddThemeStyleboxOverride("fill", fillStyle);
			}

			CollisionMask |= 4;

			GameManager.Instance?.RegisterEnemy(this);
			SetupAnimations();
		}

		public override void _ExitTree()
		{
			GameManager.Instance?.UnregisterEnemy(this);
		}

		private void SetupAnimations()
		{
			_animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_animSprite == null)
			{
				GD.PrintErr("EnemyController: AnimatedSprite2D 노드를 찾을 수 없음");
				return;
			}

			var frames = new SpriteFrames();
			if (frames.HasAnimation("default"))
				frames.RemoveAnimation("default");

			var tilemap = AnimationHelper.KenneyTilemap;
			if (tilemap == null) { GD.PrintErr("EnemyController: Kenney 타일맵 로드 실패"); return; }

			int ts = KenneyTiles.TileSize;
			var coords = Stats.SpriteAtlasCoords;

			// Kenney 정적 타일: 1프레임 애니메이션
			AnimationHelper.AddSingleTileAnimation(frames, "idle", tilemap, coords, ts, 6, true);
			AnimationHelper.AddSingleTileAnimation(frames, "run", tilemap, coords, ts, 6, true);
			AnimationHelper.AddSingleTileAnimation(frames, "death", tilemap, coords, ts, 8, false);

			_animSprite.SpriteFrames = frames;
			_animSprite.Play("idle");
			_animSprite.AnimationFinished += OnAnimationFinished;
		}

		public void ApplyKnockback(Vector2 direction, float force)
		{
			_knockbackVelocity = direction * force;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_isDying) return;

			if (!IsInstanceValid(_target))
			{
				FindTarget();
				return;
			}

			_attackTimer -= (float)delta;

			Vector2 direction = GlobalPosition.DirectionTo(_target.GlobalPosition);
			float distance = GlobalPosition.DistanceTo(_target.GlobalPosition);

			float detectionMult = 1.0f;

			if (_animSprite != null)
				_animSprite.FlipH = direction.X < 0;

			// 행동 패턴별 상태 결정
			DetermineState(distance, detectionMult);

			// 상태별 행동
			switch (_state)
			{
				case EnemyState.Attack:
					if (Stats.Behavior == EnemyBehavior.Ranged)
						ExecuteRangedAttack();
					else
						ExecuteAttack();
					break;
				case EnemyState.Chase:
					ExecuteChase(direction, distance);
					break;
				case EnemyState.Flee:
					ExecuteFlee(direction);
					break;
				default:
					ExecuteIdle();
					break;
			}

			// 넉백 적용 및 감쇠
			if (_knockbackVelocity.LengthSquared() > 25f)
			{
				Velocity += _knockbackVelocity;
				_knockbackVelocity = _knockbackVelocity.MoveToward(Vector2.Zero, 800f * (float)delta);
			}
			else
			{
				_knockbackVelocity = Vector2.Zero;
			}

			MoveAndSlide();
		}

		private void DetermineState(float distance, float detectionMult = 1.0f)
		{
			float detection = Stats.DetectionRange * detectionMult;

			switch (Stats.Behavior)
			{
				case EnemyBehavior.Passive:
					if (!_isProvoked)
					{
						_state = EnemyState.Idle;
						return;
					}
					// 도발되면 근접처럼 행동
					_state = distance <= Stats.AttackRange ? EnemyState.Attack
						   : distance <= detection ? EnemyState.Chase
						   : EnemyState.Idle;
					break;

				case EnemyBehavior.Ranged:
					if (distance > detection)
						_state = EnemyState.Idle;
					else if (distance < Stats.PreferredRange * 0.45f)
						_state = EnemyState.Flee; // 너무 가까우면 도주
					else if (distance <= Stats.PreferredRange)
						_state = EnemyState.Attack; // 사거리 내 → 공격
					else
						_state = EnemyState.Chase; // 사거리 밖 → 접근
					break;

				case EnemyBehavior.Melee:
				default:
					_state = distance <= Stats.AttackRange ? EnemyState.Attack
						   : distance <= detection ? EnemyState.Chase
						   : EnemyState.Idle;
					break;
			}
		}

		private void ExecuteIdle()
		{
			Velocity = Vector2.Zero;
			PlayAnim("idle");
		}

		private void ExecuteChase(Vector2 direction, float distance)
		{
			float stopBuffer = 30.0f;
			float speed = Stats.MoveSpeed * FirstGame.Core.BalanceData.Movement.EnemySpeedMultiplier;
			if (distance <= Stats.AttackRange + stopBuffer)
			{
				float ratio = Mathf.Max(0.15f, (distance - Stats.AttackRange) / stopBuffer);
				Velocity = direction * speed * ratio;
			}
			else
			{
				Velocity = direction * speed;
			}
			PlayAnim("run");
		}

		private void ExecuteFlee(Vector2 directionToTarget)
		{
			// 플레이어 반대 방향으로 도주
			float speed = Stats.MoveSpeed * FirstGame.Core.BalanceData.Movement.EnemySpeedMultiplier;
			Velocity = -directionToTarget * speed * 0.9f;
			PlayAnim("run");
		}

		private void ExecuteRangedAttack()
		{
			Velocity = Vector2.Zero;
			TryRangedAttack();
			PlayAnim("idle");
		}

		private void ExecuteAttack()
		{
			Velocity = Vector2.Zero;
			TryAttack();
			PlayAnim("idle");
		}

		private void PlayAnim(string animName)
		{
			if (_animSprite != null && _animSprite.Animation != animName)
				_animSprite.Play(animName);
		}

		private void TryAttack()
		{
			if (_isAttacking) return;
			if (_attackTimer <= 0f && IsInstanceValid(_target) && _target is IDamageable target)
			{
				_isAttacking = true;
				_attackTimer = Stats.AttackCooldown;

				// 공격 방향 계산
				Vector2 attackDir = (_target.GlobalPosition - GlobalPosition).Normalized();
				if (attackDir == Vector2.Zero) attackDir = Vector2.Right;

				// 1단계: 뒤로 살짝 웅크리기 (준비 동작)
				if (_animSprite != null)
				{
					_animSprite.Modulate = new Color(1.3f, 0.8f, 0.8f, 1f); // 붉은 톤
					var tween = CreateTween();
					_attackTween = tween;
					tween.TweenProperty(_animSprite, "position",
						(Vector2)_animSprite.Position - attackDir * 4f, 0.1f);

					// 2단계: 앞으로 돌진
					tween.TweenProperty(_animSprite, "position",
						(Vector2)_animSprite.Position + attackDir * 12f, 0.08f);

					// 3단계: 원위치 복귀 — 사망 후 콜백 실행 방지: _isDying 검사 추가.
					tween.TweenCallback(Callable.From(() =>
					{
						if (_isDying) return;
						if (IsInstanceValid(this) && IsInstanceValid(_target))
						{
							((IDamageable)_target).TakeDamage(Stats.BaseDamage);
							AudioManager.Instance?.PlaySFX("enemy_attack.wav");
							SpawnAttackEffect(attackDir);
						}
					}));
					tween.TweenProperty(_animSprite, "position", Vector2.Zero, 0.12f);
					tween.TweenProperty(_animSprite, "modulate",
						new Color(1f, 1f, 1f, 1f), 0.1f);
					tween.TweenCallback(Callable.From(() => _isAttacking = false));
				}
				else
				{
					target.TakeDamage(Stats.BaseDamage);
					AudioManager.Instance?.PlaySFX("enemy_attack.wav");
					_isAttacking = false;
				}
			}
		}

		private void TryRangedAttack()
		{
			if (_isAttacking) return;
			if (_attackTimer <= 0f && IsInstanceValid(_target))
			{
				_isAttacking = true;
				_attackTimer = Stats.AttackCooldown;

				Vector2 attackDir = (_target.GlobalPosition - GlobalPosition).Normalized();
				if (attackDir == Vector2.Zero) attackDir = Vector2.Right;

				// 시전 연출: 파란/보라 톤 + 몸 부풀리기
				if (_animSprite != null)
				{
					_animSprite.Modulate = new Color(0.7f, 0.7f, 1.4f, 1f);
					var tween = CreateTween();
					_attackTween = tween;
					tween.TweenProperty(_animSprite, "scale", new Vector2(1.2f, 0.8f), 0.2f);
					// 사망 후 투사체 발사 차단: _isDying 검사 추가.
					tween.TweenCallback(Callable.From(() =>
					{
						if (_isDying) return;
						if (IsInstanceValid(this) && IsInstanceValid(_target))
						{
							SpawnProjectile(attackDir);
							AudioManager.Instance?.PlaySFX("enemy_attack.wav");
						}
					}));
					tween.TweenProperty(_animSprite, "scale", Vector2.One, 0.15f);
					tween.TweenProperty(_animSprite, "modulate", new Color(1f, 1f, 1f, 1f), 0.15f);
					tween.TweenCallback(Callable.From(() => _isAttacking = false));
				}
				else
				{
					SpawnProjectile(attackDir);
					_isAttacking = false;
				}
			}
		}

		private void SpawnProjectile(Vector2 direction)
		{
			var proj = new EnemyProjectile();
			proj.GlobalPosition = GlobalPosition + direction * 15f;
			proj.Direction = direction;
			proj.Damage = Stats.BaseDamage;
			proj.Speed = 130f;
			proj.ProjectileColor = new Color(0.6f, 0.2f, 1f); // 보라색 마법탄
			GetTree().CurrentScene.AddChild(proj);
		}

		private void SpawnAttackEffect(Vector2 direction)
		{
			// 슬래시 이펙트 (간단한 스프라이트 플래시)
			var slash = new Sprite2D();
			slash.Position = direction * 20f;
			slash.Modulate = new Color(1f, 0.3f, 0.2f, 0.9f);
			slash.Scale = new Vector2(0.5f, 0.5f);
			AddChild(slash);

			var tween = slash.CreateTween();
			tween.TweenProperty(slash, "scale", new Vector2(2f, 2f), 0.15f);
			tween.Parallel().TweenProperty(slash, "modulate:a", 0f, 0.15f);
			tween.TweenCallback(Callable.From(() =>
			{
				if (IsInstanceValid(slash)) slash.QueueFree();
			}));
		}

		private void FindTarget()
		{
			_target = GameManager.Instance?.Player as Node2D;
		}

		public void TakeDamage(int damage)
		{
			if (_isDying) return;

			// Passive 적: 피격 시 도발됨
			if (!_isProvoked && Stats.Behavior == EnemyBehavior.Passive)
				_isProvoked = true;

			Stats.CurrentHealth -= damage;
			AudioManager.Instance?.PlaySFX("enemy_hit.wav");
			if (_healthBar != null) _healthBar.Value = Stats.CurrentHealth;

			// 보스 HP 바 업데이트
			if (Stats.IsBoss)
				EventManager.TriggerBossHealthChanged(Stats.CurrentHealth, Stats.MaxHealth);

			// 플로팅 데미지 표시
			UIEffectManager.SpawnFloatingLabel(GlobalPosition, damage, false, false);

			// 피격 시 흰색 플래시 + 흔들림
			if (_animSprite != null)
			{
				_animSprite.Modulate = new Color(10f, 10f, 10f, 1f);
				var hitTween = CreateTween();
				hitTween.TweenProperty(_animSprite, "position:x", 3f, 0.03f);
				hitTween.TweenProperty(_animSprite, "position:x", -3f, 0.03f);
				hitTween.TweenProperty(_animSprite, "position:x", 2f, 0.03f);
				hitTween.TweenProperty(_animSprite, "position:x", 0f, 0.03f);
				hitTween.TweenProperty(_animSprite, "modulate",
					new Color(1f, 1f, 1f, 1f), 0.08f);
			}

			if (Stats.CurrentHealth <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			_isDying = true;

			// 진행 중인 공격 tween 즉시 종료 — IsValid 검사가 통과하는 동안에도
			// 데미지/투사체 콜백이 발사되지 않도록 차단. 콜백 안 _isDying 검사가
			// 2차 안전망이지만 Kill로 일찍 끊는 게 우선.
			if (_attackTween != null && _attackTween.IsValid())
				_attackTween.Kill();
			_attackTween = null;

			AudioManager.Instance?.PlaySFX("enemy_death.wav");

			EventManager.TriggerEnemyKilled();
			EventManager.TriggerEnemyKilledTyped(Stats.EnemyTypeName);

			// 골드 보상
			if (GameManager.Instance != null)
				GameManager.Instance.PlayerGold += Stats.GoldReward * BalanceData.Enemy.GoldMultiplier;

			// 경험치 지급 (이벤트 방식)
			EventManager.TriggerExpGained(Stats.ExperienceReward);

			// 아이템 드랍
			// 보스: 직접 인벤토리에 안전 지급 (앱 종료/재시작 시 드랍 손실 방지).
			//       인벤이 가득 차면 필드 드랍 fallback.
			// 일반: 기존대로 필드 드랍 (DropChance 적용).
			if (Stats.IsBoss)
			{
				// 트랜잭션 격리 — AddItem의 OnInventoryChanged 자동저장이 보스 키 기록 전에 끼면
				// "보상은 받았는데 보스는 미처치" 상태가 디스크에 박혀 재진입 시 중복 보상 위험.
				// dispose 후 SaveGame이 보상 + 보스 키 + 인벤 모두 한 시점에 영속화.
				using (GameTransaction.Begin())
				{
					if (Stats.PossibleDrops != null && Stats.PossibleDrops.Length > 0)
					{
						var inv = GameManager.Instance?.Player?.Inventory;
						foreach (var droppedItem in Stats.PossibleDrops)
						{
							var affixes = AffixGenerator.IsJewelry(droppedItem.Type) ? AffixGenerator.GenerateForJewelry(droppedItem.Rarity) : null;
							bool added = inv != null && inv.AddItem(droppedItem, 1, 0, true, affixes);
							if (!added)
								GameManager.Instance?.AddPendingReward(droppedItem, 1, 0, affixes);
						}
					}

					EventManager.TriggerBossDied();
					string bossKey = !string.IsNullOrEmpty(BossId) ? BossId : Stats.EnemyTypeName;
					GameManager.Instance?.RecordBossDefeat(bossKey);
				}
				SaveManager.SaveGame();
			}
			else
			{
				if (Stats.PossibleDrops != null && Stats.PossibleDrops.Length > 0 && GD.Randf() <= Stats.DropChance)
				{
					int index = Stats.PickDropIndex();
					SpawnFieldDrop(Stats.PossibleDrops[index], 50, 120);
				}
				SaveManager.RequestAutoSave();
			}

			// 사망 애니메이션 재생 후 QueueFree
			if (_animSprite != null)
			{
				if (_healthBar != null) _healthBar.Visible = false;
				SetPhysicsProcess(false);
				_animSprite.Play("death");
			}
			else
			{
				QueueFree();
			}
		}

		private void SpawnFieldDrop(ItemData item, float minForce, float maxForce)
		{
			var prefab = GD.Load<PackedScene>("res://Scenes/Objects/field_item.tscn");
			if (prefab == null)
			{
				GD.PrintErr("EnemyController: field_item.tscn을 찾을 수 없습니다.");
				return;
			}
			var fieldItem = prefab.Instantiate<FirstGame.Objects.FieldItem>();
			fieldItem.Item = item;
			fieldItem.Quantity = 1;
			// 장신구 드롭이면 rarity에 따라 affix 개수·값 차등 부여 — 같은 .tres라도 개체마다 달라짐.
			if (AffixGenerator.IsJewelry(item.Type))
				fieldItem.Affixes = AffixGenerator.GenerateForJewelry(item.Rarity);
			GetTree().CurrentScene.AddChild(fieldItem);
			Vector2 dropDir = new Vector2((float)GD.RandRange(-1, 1), -1).Normalized();
			fieldItem.Drop(GlobalPosition, dropDir, (float)GD.RandRange(minForce, maxForce));
		}

		private void OnAnimationFinished()
		{
			if (_animSprite == null) return;

			if (_animSprite.Animation == "death")
			{
				QueueFree();
			}
		}
	}
}
