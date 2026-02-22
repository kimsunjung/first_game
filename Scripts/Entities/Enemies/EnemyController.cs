using Godot;
using System;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;
using FirstGame.Entities.Player;
using FirstGame.UI;

namespace FirstGame.Entities.Enemies
{
	public partial class EnemyController : CharacterBody2D, IDamageable
	{
		[Export] public EnemyStats Stats { get; set; }

		private Node2D _target;
		private ProgressBar _healthBar;
		private AnimatedSprite2D _animSprite;
		private bool _isDying = false;
		private float _attackTimer = 0f;

		public override void _Ready()
		{
			if (Stats != null)
				Stats = (EnemyStats)Stats.Duplicate();
			else
				Stats = new EnemyStats();

			Stats.CurrentHealth = Stats.MaxHealth;

			_healthBar = GetNode<ProgressBar>("HealthBar");
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

			CollisionMask |= 4;

			SetupAnimations();
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

			string basePath = Stats.AnimBasePath;
			AddSheetAnimation(frames, "idle", Stats.AnimIdleFile, Stats.AnimIdleFrames, 6, true, basePath);
			AddSheetAnimation(frames, "run", Stats.AnimRunFile, Stats.AnimRunFrames, 10, true, basePath);
			AddSheetAnimation(frames, "death", Stats.AnimDeathFile, Stats.AnimDeathFrames, 8, false, basePath);

			_animSprite.SpriteFrames = frames;
			_animSprite.Play("idle");
			_animSprite.AnimationFinished += OnAnimationFinished;
		}

		private void AddSheetAnimation(SpriteFrames frames, string animName, string sheetFile, int frameCount, int fps, bool loop, string basePath)
		{
			frames.AddAnimation(animName);
			frames.SetAnimationSpeed(animName, fps);
			frames.SetAnimationLoop(animName, loop);

			var texture = GD.Load<Texture2D>(basePath + sheetFile);
			if (texture == null)
			{
				GD.PrintErr($"EnemyController: 텍스처 로드 실패 - {basePath}{sheetFile}");
				return;
			}

			int frameWidth = texture.GetWidth() / frameCount;
			int frameHeight = texture.GetHeight();

			for (int i = 0; i < frameCount; i++)
			{
				var atlas = new AtlasTexture();
				atlas.Atlas = texture;
				atlas.Region = new Rect2(i * frameWidth, 0, frameWidth, frameHeight);
				frames.AddFrame(animName, atlas);
			}
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

			if (_animSprite != null)
				_animSprite.FlipH = direction.X < 0;

			if (distance <= Stats.AttackRange)
			{
				Velocity = Vector2.Zero;
				TryAttack();
				PlayAnim("idle");
			}
			else if (distance <= Stats.DetectionRange)
			{
				float stopBuffer = 30.0f;

				if (distance <= Stats.AttackRange + stopBuffer)
				{
					float ratio = Mathf.Max(0.15f, (distance - Stats.AttackRange) / stopBuffer);
					Velocity = direction * Stats.MoveSpeed * ratio;
				}
				else
				{
					Velocity = direction * Stats.MoveSpeed;
				}
				PlayAnim("run");
			}
			else
			{
				Velocity = Vector2.Zero;
				PlayAnim("idle");
			}

			MoveAndSlide();
		}

		private void PlayAnim(string animName)
		{
			if (_animSprite != null && _animSprite.Animation != animName)
				_animSprite.Play(animName);
		}

		private void TryAttack()
		{
			if (_attackTimer <= 0f && IsInstanceValid(_target) && _target is IDamageable target)
			{
				target.TakeDamage(Stats.BaseDamage);
				_attackTimer = Stats.AttackCooldown;
				AudioManager.Instance?.PlaySFX("enemy_attack.wav");
			}
		}

		private void FindTarget()
		{
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
			{
				_target = players[0] as Node2D;
			}
		}

		public void TakeDamage(int damage)
		{
			if (_isDying) return;

			Stats.CurrentHealth -= damage;
			AudioManager.Instance?.PlaySFX("enemy_hit.wav");
			_healthBar.Value = Stats.CurrentHealth;

			// 보스 HP 바 업데이트
			if (Stats.IsBoss)
				EventManager.TriggerBossHealthChanged(Stats.CurrentHealth, Stats.MaxHealth);

			// 플로팅 데미지 표시
			PlayerController.SpawnFloatingLabel(GlobalPosition, damage, false, false);

			// 피격 시 흰색 플래시
			if (_animSprite != null)
			{
				var originalColor = _animSprite.Modulate;
				_animSprite.Modulate = Colors.White;
				GetTree().CreateTimer(0.1).Timeout += () =>
				{
					if (IsInstanceValid(_animSprite)) _animSprite.Modulate = originalColor;
				};
			}

			if (Stats.CurrentHealth <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			_isDying = true;
			AudioManager.Instance?.PlaySFX("enemy_death.wav");

			// 보스/퀘스트 이벤트
			EventManager.TriggerEnemyKilled();
			QuestManager.ReportKill(Stats.EnemyTypeName);
			if (Stats.IsBoss) EventManager.TriggerBossDied();

			// 골드 보상
			GameManager.Instance.PlayerGold += 10;

			// 경험치 지급
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0 && players[0] is PlayerController player)
			{
				player.GainExp(Stats.ExperienceReward);
			}

			// 아이템 드롭 (물리 드랍)
			if (Stats.PossibleDrops != null && Stats.PossibleDrops.Length > 0)
			{
				if (GD.Randf() <= Stats.DropChance)
				{
					int index = (int)(GD.Randi() % Stats.PossibleDrops.Length);
					var droppedItem = Stats.PossibleDrops[index];

					// FieldItem 프리팹 로드 (경로는 프로젝트 트리에 맞게 설정해야 함)
					// 지금은 임시로 Load 사용. 향후 EnemySpawner나 GameManager 등을 통해 캐싱 권장
					var fieldItemPrefab = GD.Load<PackedScene>("res://Scenes/Objects/field_item.tscn");
					if (fieldItemPrefab != null)
					{
						var fieldItem = fieldItemPrefab.Instantiate<FirstGame.Objects.FieldItem>();
						fieldItem.Item = droppedItem;
						fieldItem.Quantity = 1;
						
						// 부모 노드(필드)에 추가 (몬스터가 지워지더라도 아이템은 남아야 하므로 GetParent)
						GetParent().AddChild(fieldItem);
						
						// 물리 드랍 연출: 랜덤한 방향으로 통통 튀게 던짐
						Vector2 dropDir = new Vector2((float)GD.RandRange(-1, 1), -1).Normalized();
						fieldItem.Drop(GlobalPosition, dropDir, (float)GD.RandRange(200, 400));
					}
					else
					{
						GD.PrintErr("EnemyController: field_item.tscn을 찾을 수 없습니다.");
					}
				}
			}

			// 자동 저장
			SaveManager.SaveGame();

			// 사망 애니메이션 재생 후 QueueFree
			if (_animSprite != null)
			{
				_healthBar.Visible = false;
				SetPhysicsProcess(false);
				_animSprite.Play("death");
			}
			else
			{
				QueueFree();
			}
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
