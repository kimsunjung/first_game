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

		// 엘리트 affix(스폰 시 EnemySpawner가 주입). Vampiric 자가 재생 tick에 필요.
		public EliteAffix Affix { get; set; } = EliteAffix.None;
		private float _regenRatePerSec = 0f; // MaxHealth의 비율
		private float _regenAccum = 0f;
		public void SetRegenRate(float ratePerSec) => _regenRatePerSec = Mathf.Max(0f, ratePerSec);

		private Node2D _target;
		private ProgressBar _healthBar;
		private AnimatedSprite2D _animSprite;
		private bool _isDying = false;
		private float _attackTimer = 0f;
		private EnemyState _state = EnemyState.Idle;
		private Vector2 _knockbackVelocity = Vector2.Zero;
		private bool _isAttacking = false;
		private bool _isProvoked = false; // Passive 행동: 피격 전까지 비공격

		// AI stuck 회피 — Chase 중 일정 시간 위치 변화가 없으면 perpendicular nudge로 우회.
		private Vector2 _lastPosition = Vector2.Zero;
		private float _stuckTimer = 0f;
		private Vector2 _avoidanceNudge = Vector2.Zero;
		private float _avoidanceTimer = 0f;

		// 진행 중인 공격 tween 참조 — 사망 시 Kill해 죽은 적이 데미지를 주거나
		// 투사체를 발사하는 결함 차단. CreateTween()이 반환한 SceneTreeTween을 보관.
		private Tween _attackTween;
		// 진행 중인 피격 시각 tween — 연타 시 새 tween을 만들기 전에 이전 것을 Kill해
		// position:x 흔들기/modulate 페이드가 누적되지 않도록 dedupe.
		private Tween _hitTween;

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

			// CollisionScale은 CollisionShape2D만 — root Scale 미사용이라 elite scale과 독립.
			// 큰 적(rock_golem 등)의 시각/충돌 정합성 회복용.
			if (Stats.CollisionScale > 0f && !Mathf.IsEqualApprox(Stats.CollisionScale, 1.0f))
			{
				var col = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
				if (col != null)
					col.Scale = new Vector2(Stats.CollisionScale, Stats.CollisionScale);
			}

			GameManager.Instance?.RegisterEnemy(this);
			SetupAnimations();

			// 보스 패턴 브레인 attach — IsBoss + Patterns 배열 모두 있을 때만.
			if (Stats.IsBoss && Stats.Patterns != null && Stats.Patterns.Length > 0)
			{
				var brain = new BossController();
				brain.Attach(this);
				AddChild(brain);
			}
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

			// Stats.Sprite 지정 시 단일 PNG로 1프레임 애니 구성 — mine_* 적처럼 정면 1포즈
			// 시트만 있는 신규 적용. 미지정 시 기존 Kenney 타일맵 fallback.
			if (Stats.Sprite != null)
			{
				AnimationHelper.AddSinglePngAnimation(frames, "idle", Stats.Sprite, 6, true);
				AnimationHelper.AddSinglePngAnimation(frames, "run", Stats.Sprite, 6, true);
				AnimationHelper.AddSinglePngAnimation(frames, "death", Stats.Sprite, 8, false);
			}
			else
			{
				var tilemap = AnimationHelper.KenneyTilemap;
				if (tilemap == null) { GD.PrintErr("EnemyController: Kenney 타일맵 로드 실패"); return; }

				int ts = KenneyTiles.TileSize;
				var coords = Stats.SpriteAtlasCoords;

				// Kenney 정적 타일: 1프레임 애니메이션
				AnimationHelper.AddSingleTileAnimation(frames, "idle", tilemap, coords, ts, 6, true);
				AnimationHelper.AddSingleTileAnimation(frames, "run", tilemap, coords, ts, 6, true);
				AnimationHelper.AddSingleTileAnimation(frames, "death", tilemap, coords, ts, 8, false);
			}

			_animSprite.SpriteFrames = frames;
			// SpriteScale은 시각만 — root CharacterBody2D.Scale에 손대지 않아 콜리전/체력바/elite scale 보호.
			if (Stats.SpriteScale > 0f && !Mathf.IsEqualApprox(Stats.SpriteScale, 1.0f))
				_animSprite.Scale = new Vector2(Stats.SpriteScale, Stats.SpriteScale);
			_animSprite.Play("idle");
			_animSprite.AnimationFinished += OnAnimationFinished;
		}

		/// <summary>주변 다른 적과 너무 가까우면 분리 방향으로 push velocity 반환.</summary>
		private Vector2 ComputeSeparation()
		{
			const float separationRadius = 38f;
			const float pushStrength = 80f;
			var enemies = GameManager.Instance?.ActiveEnemies;
			if (enemies == null) return Vector2.Zero;
			Vector2 push = Vector2.Zero;
			int neighbors = 0;
			foreach (Node2D other in enemies)
			{
				if (other == this || !IsInstanceValid(other)) continue;
				Vector2 diff = GlobalPosition - other.GlobalPosition;
				float d = diff.Length();
				if (d > 0.01f && d < separationRadius)
				{
					push += diff.Normalized() * (separationRadius - d);
					neighbors++;
				}
			}
			if (neighbors == 0) return Vector2.Zero;
			return (push / neighbors) * (pushStrength / separationRadius);
		}

		public void ApplyKnockback(Vector2 direction, float force)
		{
			// 공격 사이클 진입 후에는 knockback 면역 — 자기 사거리 진입 후 바로 밀려나
			// 한 번도 공격 못 끝내는 결함 차단. 사이클 종료 콜백에서 _isAttacking=false로 해제.
			if (_isAttacking) return;
			_knockbackVelocity = direction * force;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_isDying) return;

			// 상태이상 tick — 독 데미지 자체 적용
			int poisonDmg = Stats.TickStatuses((float)delta);
			if (poisonDmg > 0)
			{
				Stats.CurrentHealth -= poisonDmg;
				if (_healthBar != null) _healthBar.Value = Stats.CurrentHealth;
				if (Stats.CurrentHealth <= 0) { Die(); return; }
			}
			UpdateStatusTint();

			// Vampiric elite 자가 재생 — MaxHealth 비율로 매초 누적, 1HP 단위로 적용.
			if (_regenRatePerSec > 0f && Stats.CurrentHealth < Stats.MaxHealth)
			{
				_regenAccum += Stats.MaxHealth * _regenRatePerSec * (float)delta;
				if (_regenAccum >= 1f)
				{
					int heal = (int)_regenAccum;
					_regenAccum -= heal;
					Stats.CurrentHealth = Mathf.Min(Stats.MaxHealth, Stats.CurrentHealth + heal);
					if (_healthBar != null) _healthBar.Value = Stats.CurrentHealth;
				}
			}

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
				_animSprite.FlipH = direction.X > 0;

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

			// AI stuck 회피 — Chase 중 기대 이동량의 30% 미만이면 stuck으로 판정해 perpendicular nudge.
			// 고정 임계값(예: 1px) 대신 Stats.MoveSpeed*delta를 기준으로 동적 스케일 — 느린 적(슬라임 등)을
			// 정상 이동 중인데 stuck으로 오인식하던 결함(Codex P2) 차단.
			if (_state == EnemyState.Chase)
			{
				float movedSq = (GlobalPosition - _lastPosition).LengthSquared();
				float expected = Stats.MoveSpeed * (float)delta * 0.3f;
				float thresholdSq = expected * expected;
				if (movedSq < thresholdSq)
				{
					_stuckTimer += (float)delta;
					if (_stuckTimer > 0.7f && _avoidanceTimer <= 0f)
					{
						Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
						if (GD.Randf() < 0.5f) perpendicular = -perpendicular;
						_avoidanceNudge = perpendicular * Stats.MoveSpeed;
						_avoidanceTimer = 0.4f;
						_stuckTimer = 0f;
					}
				}
				else _stuckTimer = 0f;
			}
			else
			{
				// Chase 떠날 때 stuck/avoidance 상태 즉시 해제 — 공격/대기/도주 중 nudge가 의도된
				// velocity에 더해져 슬라이드되는 결함(Codex P3) 차단.
				_stuckTimer = 0f;
				_avoidanceTimer = 0f;
			}
			_lastPosition = GlobalPosition;

			if (_avoidanceTimer > 0f && _state == EnemyState.Chase)
			{
				_avoidanceTimer -= (float)delta;
				Velocity += _avoidanceNudge;
			}

			// 적 분리(separation) — 다른 적과 너무 가까우면 반대 방향으로 밀어내기.
			// MoveAndSlide만으론 겹친 상태 해소 안 됨 → 명시적 push가 필요.
			Velocity += ComputeSeparation();

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
				_attackTimer = Stats.AttackCooldown * (Stats.HasStatus(FirstGame.Data.StatusEffect.Shock) ? 1.5f : 1.0f);

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
							TryInflictStatus(_target);
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
				_attackTimer = Stats.AttackCooldown * (Stats.HasStatus(FirstGame.Data.StatusEffect.Shock) ? 1.5f : 1.0f);

				Vector2 attackDir = (_target.GlobalPosition - GlobalPosition).Normalized();
				if (attackDir == Vector2.Zero) attackDir = Vector2.Right;

				// 시전 연출: 파란/보라 톤 + 몸 부풀리기.
				// baseScale 저장 — SetupAnimations에서 적용된 Stats.SpriteScale을 연출 종료 시 보존.
				// (Vector2.One으로 복구하면 mine_* 처럼 SpriteScale<1 인 적이 원본 PNG 크기로 튄다.)
				if (_animSprite != null)
				{
					Vector2 baseScale = _animSprite.Scale;
					_animSprite.Modulate = new Color(0.7f, 0.7f, 1.4f, 1f);
					var tween = CreateTween();
					_attackTween = tween;
					tween.TweenProperty(_animSprite, "scale", baseScale * new Vector2(1.2f, 0.8f), 0.2f);
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
					tween.TweenProperty(_animSprite, "scale", baseScale, 0.15f);
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
			proj.ProjectileColor = new Color(0.6f, 0.2f, 1f); // 보라색 마법탄 (텍스처 없을 때 폴백)
			if (Stats.ProjectileTexture != null)
			{
				proj.Texture = Stats.ProjectileTexture;
				proj.TextureScale = Stats.ProjectileScale;
			}
			// Stats의 상태이상 값을 투사체에 위임 — Ranged 적도 적중 시 상태이상 부여 가능.
			proj.InflictedStatus = Stats.InflictedStatus;
			proj.InflictedStatusDuration = Stats.InflictedStatusDuration;
			proj.InflictedStatusChance = Stats.InflictedStatusChance;
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

		private void UpdateStatusTint()
		{
			if (_animSprite == null) return;
			// hit flash 중(R>1.2)이면 상태 색조 덮어쓰기 생략
			if (_animSprite.Modulate.R > 1.2f) return;
			_animSprite.Modulate = Stats.GetStatusModulate();
		}

		private void TryInflictStatus(Node2D target)
		{
			if (Stats.InflictedStatus == FirstGame.Data.StatusEffect.None) return;
			if (GD.Randf() > Stats.InflictedStatusChance) return;
			var player = target as FirstGame.Entities.Player.PlayerController;
			if (player == null) return;
			player.Stats?.ApplyStatus(Stats.InflictedStatus, Stats.InflictedStatusDuration);
			GD.Print($"[상태이상] {Stats.EnemyTypeName} → 플레이어에게 {Stats.InflictedStatus} {Stats.InflictedStatusDuration}초 부여");
		}

		private void FindTarget()
		{
			_target = GameManager.Instance?.Player as Node2D;
		}

		public void TakeDamage(int damage) => TakeDamage(damage, FirstGame.Data.ElementType.None);

		// 플레이어 스킬/투사체가 적에게 상태이상을 거는 통합 경로.
		public void ApplyStatusEffect(FirstGame.Data.StatusEffect status, float duration)
		{
			if (_isDying || Stats == null) return;
			Stats.ApplyStatus(status, duration);
		}

		// 속성 인식 오버로드 — 공격자 속성이 적 Weakness면 1.5x, Element와 같으면 0.75x.
		public void TakeDamage(int damage, FirstGame.Data.ElementType attackerElement)
		{
			if (_isDying) return;

			// Passive 적: 피격 시 도발됨
			if (!_isProvoked && Stats.Behavior == EnemyBehavior.Passive)
				_isProvoked = true;

			// 속성 보정
			if (attackerElement != FirstGame.Data.ElementType.None)
			{
				if (Stats.Weakness != FirstGame.Data.ElementType.None && attackerElement == Stats.Weakness)
					damage = (int)(damage * 1.5f);
				else if (Stats.Element != FirstGame.Data.ElementType.None && attackerElement == Stats.Element)
					damage = (int)(damage * 0.75f);
				if (damage < 1) damage = 1;
			}

			// Defense 적용 — Tough 엘리트 affix가 DefenseBonus를 +5 부여하므로 실제 피해 감소로 반영.
			// 일반 적은 .tres Defense 기본 0이라 영향 없음. PlayerController.TakeDamage와 동일 패턴.
			damage = Math.Max(1, damage - Stats.Defense);

			Stats.CurrentHealth -= damage;
			AudioManager.Instance?.PlaySFX("enemy_hit.wav");
			if (_healthBar != null) _healthBar.Value = Stats.CurrentHealth;

			// 보스 HP 바 업데이트
			if (Stats.IsBoss)
				EventManager.TriggerBossHealthChanged(Stats.CurrentHealth, Stats.MaxHealth);

			// 플로팅 데미지 표시
			UIEffectManager.SpawnFloatingLabel(GlobalPosition, damage, false, false);

			// 피격 시 흰색 플래시 + 흔들림 — 연타 시 이전 hitTween을 Kill해 누적 차단.
			if (_animSprite != null)
			{
				if (_hitTween != null && _hitTween.IsValid()) _hitTween.Kill();
				_animSprite.Modulate = new Color(10f, 10f, 10f, 1f);
				_hitTween = CreateTween();
				_hitTween.TweenProperty(_animSprite, "position:x", 3f, 0.03f);
				_hitTween.TweenProperty(_animSprite, "position:x", -3f, 0.03f);
				_hitTween.TweenProperty(_animSprite, "position:x", 2f, 0.03f);
				_hitTween.TweenProperty(_animSprite, "position:x", 0f, 0.03f);
				_hitTween.TweenProperty(_animSprite, "modulate",
					Stats.GetStatusModulate(), 0.08f);
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

			// 골드 보상 — 일반 적은 바닥 드롭(파밍 연출), 보스는 트랜잭션 일관성을 위해 직접 지급 + 즉시 표시
			if (GameManager.Instance != null)
			{
				int goldAmount = Stats.GoldReward * BalanceData.Enemy.GoldMultiplier;
				if (goldAmount > 0)
				{
					if (Stats.IsBoss)
					{
						GameManager.Instance.PlayerGold += goldAmount;
						UIEffectManager.SpawnGoldLabel(GlobalPosition, goldAmount);
					}
					else
					{
						SpawnGoldDrop(goldAmount);
					}
				}
			}

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

		private void SpawnGoldDrop(int amount)
		{
			var prefab = GD.Load<PackedScene>("res://Scenes/Objects/gold_pickup.tscn");
			if (prefab == null)
			{
				// 씬 로드 실패 시 안전망: 즉시 지급 + 표시
				GameManager.Instance.PlayerGold += amount;
				UIEffectManager.SpawnGoldLabel(GlobalPosition, amount);
				return;
			}
			var pickup = prefab.Instantiate<FirstGame.Objects.GoldPickup>();
			pickup.Amount = amount;
			GetTree().CurrentScene.AddChild(pickup);
			Vector2 dropDir = new Vector2((float)GD.RandRange(-1, 1), -1).Normalized();
			pickup.Drop(GlobalPosition, dropDir, (float)GD.RandRange(60, 130));
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
