using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.UI;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController : CharacterBody2D, IDamageable
	{
		[Export] public PlayerStats Stats { get; set; }
		public Inventory Inventory { get; private set; }

		public bool IsDead { get; private set; } = false;
		private Vector2 _facingDirection = Vector2.Down;

		// 애니메이션
		private AnimatedSprite2D _animSprite;
		private bool _isAnimLocked = false;
		private const string AnimBasePath = "res://Resources/Tilesets/Pixel Crawler - Free Pack/Entities/Characters/Body_A/Animations/";

		// MP 재생
		private const float MpRegenPerSec = 2.0f;
		private float _mpRegenAccum = 0f;

		// 스킬 시스템
		private readonly float[] _skillCooldowns = new float[4]; // Q=0, W=1, E=2, R=3
		private bool _powerStrikeActive = false;
		private bool _dashActive = false;
		private float _dashTimer = 0f;
		private const float DashSpeedMultiplier = 2.5f;

		// 카메라 쉐이크
		private Camera2D _camera;
		private float _shakeIntensity = 0f;
		private float _shakeTimer = 0f;

		[Export] public float Acceleration { get; set; } = 800.0f;
		[Export] public float Friction { get; set; } = 1000.0f;

		// ─── IDamageable ────────────────────────────────────────────
		public void TakeDamage(int damage)
		{
			if (IsDead) return;
			Stats.CurrentHealth -= damage;
			AudioManager.Instance?.PlaySFX("player_hit.wav");
			SpawnFloatingLabel(GlobalPosition, damage, false, true);
			// 큰 피해 시 화면 흔들림 (최대 HP의 20% 이상)
			if (damage >= Stats.MaxHealth * 0.2f)
				TriggerCameraShake(5f, 0.3f);
			PlayHitAnimation();
			if (Stats.CurrentHealth <= 0) Die();
		}

		public void GainExp(int amount)
		{
			Stats.AddExp(amount);
		}

		private void Die()
		{
			IsDead = true;
			AudioManager.Instance?.PlaySFX("player_death.wav");
			EventManager.TriggerPlayerDeath();
			SetPhysicsProcess(false);
			PlayDeathAnimation();
		}

		// ─── _Ready ─────────────────────────────────────────────────
		public override void _Ready()
		{
			if (Stats != null)
				Stats = (PlayerStats)Stats.Duplicate();
			else
				Stats = new PlayerStats();

			Stats.OnLevelUp += OnLevelUpHandler;

			_camera = GetNodeOrNull<Camera2D>("Camera2D");
			if (_camera != null)
			{
				_camera.LimitLeft = 0; _camera.LimitTop = 0;
				_camera.LimitRight = 1280; _camera.LimitBottom = 960;
			}

			CollisionMask |= 4;
			Inventory = new Inventory();

			if (SaveManager.PendingLoadData != null)
			{
				var data = SaveManager.PendingLoadData;
				GlobalPosition = new Vector2(data.PlayerPosX, data.PlayerPosY);
				Stats.SetLevelFromSave(data.PlayerLevel, data.PlayerExp);
				Stats.SetStatPointsFromSave(data.StatPoints, data.StrPoints, data.ConPoints, data.IntPoints);
				Stats.MaxHealth = data.PlayerMaxHealth;
				Stats.CurrentHealth = data.PlayerHealth;
				Stats.CurrentMp = data.PlayerMp;
				GameManager.Instance.PlayerGold = data.PlayerGold;

				// 인벤토리 복원
				if (data.InventoryItems != null)
				{
					foreach (var savedSlot in data.InventoryItems)
					{
						var item = GD.Load<ItemData>(savedSlot.ItemPath);
						if (item != null) Inventory.AddItem(item, savedSlot.Quantity);
					}
				}

				ItemData loadedWeapon = null, loadedArmor = null;
				if (!string.IsNullOrEmpty(data.EquippedWeaponPath))
					loadedWeapon = GD.Load<ItemData>(data.EquippedWeaponPath);
				if (!string.IsNullOrEmpty(data.EquippedArmorPath))
					loadedArmor = GD.Load<ItemData>(data.EquippedArmorPath);
				Inventory.RestoreEquipment(loadedWeapon, loadedArmor, this);

				if (data.QuickSlotPaths != null)
				{
					for (int i = 0; i < data.QuickSlotPaths.Count && i < 4; i++)
					{
						if (!string.IsNullOrEmpty(data.QuickSlotPaths[i]))
						{
							var qsItem = GD.Load<ItemData>(data.QuickSlotPaths[i]);
							if (qsItem != null) Inventory.QuickSlots[i] = qsItem;
						}
					}
				}

				// 스킬 복원
				if (data.LearnedSkillPaths != null)
				{
					var skillsToLoad = new List<SkillData>();
					foreach (var path in data.LearnedSkillPaths)
					{
						if (!string.IsNullOrEmpty(path))
						{
							var sk = GD.Load<SkillData>(path);
							if (sk != null) skillsToLoad.Add(sk);
						}
					}
					Stats.LoadLearnedSkills(skillsToLoad);
				}

				SaveManager.PendingLoadData = null;
			}
			else if (!SaveManager.HasSave())
			{
				SaveManager.SaveGame();
			}

			IsDead = false;
			SetupAnimations();
		}

		public override void _ExitTree()
		{
			if (Stats != null) Stats.OnLevelUp -= OnLevelUpHandler;
		}

		private void OnLevelUpHandler(int newLevel)
		{
			EventManager.TriggerLevelUp(newLevel);
			AudioManager.Instance?.PlaySFX("level_up.wav");
		}

		// ─── Process ─────────────────────────────────────────────────
		public override void _PhysicsProcess(double delta)
		{
			GetInput(delta);
			MoveAndSlide();
			UpdateAnimation();
			RegenMp(delta);
			UpdateSkillCooldowns(delta);
			UpdateDash(delta);
			UpdateCameraShake(delta);
		}

		private void RegenMp(double delta)
		{
			if (Stats.CurrentMp >= Stats.MaxMp) return;
			_mpRegenAccum += MpRegenPerSec * (float)delta;
			if (_mpRegenAccum >= 1f)
			{
				int regen = (int)_mpRegenAccum;
				_mpRegenAccum -= regen;
				Stats.CurrentMp += regen;
			}
		}

		private void UpdateSkillCooldowns(double delta)
		{
			for (int i = 0; i < _skillCooldowns.Length; i++)
			{
				if (_skillCooldowns[i] > 0f)
					_skillCooldowns[i] = Mathf.Max(0f, _skillCooldowns[i] - (float)delta);
			}
		}

		private void UpdateDash(double delta)
		{
			if (!_dashActive) return;
			_dashTimer -= (float)delta;
			if (_dashTimer <= 0f) _dashActive = false;
		}

		// ─── Input ───────────────────────────────────────────────────
		public override void _UnhandledInput(InputEvent @event)
		{
			if (IsDead) return;
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				var key = k.Keycode != Key.None ? k.Keycode : k.PhysicalKeycode;
				if (key == Key.Key1) Inventory.UseQuickSlot(0, this);
				else if (key == Key.Key2) Inventory.UseQuickSlot(1, this);
				else if (key == Key.Key3) Inventory.UseQuickSlot(2, this);
				else if (key == Key.Key4) Inventory.UseQuickSlot(3, this);
				else if (key == Key.Q) UseSkillSlot(0);
				else if (key == Key.W && !IsMoving()) UseSkillSlot(1);
				else if (key == Key.E) UseSkillSlot(2);
				else if (key == Key.R) UseSkillSlot(3);
			}
		}

		private bool IsMoving() => Velocity.Length() > 10f;

		private void UseSkillSlot(int slot)
		{
			var skills = Stats.LearnedSkills;
			if (slot >= skills.Count) { GD.Print($"슬롯 {slot + 1}에 스킬이 없습니다."); return; }

			var skill = skills[slot];
			if (_skillCooldowns[slot] > 0f) { GD.Print($"{skill.SkillName} 쿨타임: {_skillCooldowns[slot]:F1}s"); return; }
			if (Stats.CurrentMp < skill.MpCost) { GD.Print("MP 부족!"); return; }

			Stats.CurrentMp -= skill.MpCost;
			_skillCooldowns[slot] = skill.Cooldown;
			ActivateSkill(skill);
		}

		private void ActivateSkill(SkillData skill)
		{
			switch (skill.Type)
			{
				case SkillType.PowerStrike:
					_powerStrikeActive = true;
					GD.Print($"파워 스트라이크! 다음 공격 {skill.BonusDamageMultiplier}배 데미지");
					break;
				case SkillType.HealSelf:
					Stats.CurrentHealth += skill.HealAmount;
					GD.Print($"힐! HP +{skill.HealAmount}");
					break;
				case SkillType.Dash:
					_dashActive = true;
					_dashTimer = skill.DurationSeconds > 0 ? skill.DurationSeconds : 2.0f;
					GD.Print("대시!");
					break;
				case SkillType.FireBolt:
					FireBoltAttack(skill);
					GD.Print("파이어볼트!");
					break;
			}
			AudioManager.Instance?.PlaySFX("skill_activate.wav");
		}

		private void FireBoltAttack(SkillData skill)
		{
			var enemies = GetTree().GetNodesInGroup("Enemy");
			float bestDist = 350f;
			IDamageable target = null;
			Node2D targetNode = null;

			foreach (Node2D e in enemies)
			{
				float d = GlobalPosition.DistanceTo(e.GlobalPosition);
				if (d < bestDist && e is IDamageable dam)
				{
					bestDist = d; target = dam; targetNode = e;
				}
			}
			if (target != null)
			{
				bool fbCrit = GD.Randf() < Stats.CritRate;
				int dmg = Stats.BaseDamage * (skill.BonusDamageMultiplier > 0 ? skill.BonusDamageMultiplier : 2);
				if (fbCrit) dmg = (int)(dmg * Stats.CritMultiplier);
				target.TakeDamage(dmg);
				TriggerCameraShake(7f, 0.3f);
				GD.Print($"파이어볼트 명중! ({dmg} 데미지{(fbCrit ? " CRIT!" : "")})");
			}
		}

		private void GetInput(double delta)
		{
			if (IsDead || _isAnimLocked)
			{
				Velocity = Vector2.Zero;
				return;
			}

			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");

			if (inputDir != Vector2.Zero)
			{
				inputDir = inputDir.Normalized();
				float speed = Stats.MoveSpeed * (_dashActive ? DashSpeedMultiplier : 1.0f);
				Velocity = Velocity.MoveToward(inputDir * speed, Acceleration * (float)delta);
				_facingDirection = inputDir;
			}
			else
			{
				Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
			}

			if (Input.IsActionJustPressed("attack")) Attack();
		}

		private void Attack()
		{
			AudioManager.Instance?.PlaySFX("player_attack.wav");

			int damage = Stats.BaseDamage;
			bool isCrit = GD.Randf() < Stats.CritRate;
			int multiplier = 1;
			if (_powerStrikeActive)
			{
				SkillData ps = null;
				foreach (var s in Stats.LearnedSkills)
					if (s.Type == SkillType.PowerStrike) { ps = s; break; }
				multiplier = ps != null ? ps.BonusDamageMultiplier : 2;
				damage *= multiplier;
				_powerStrikeActive = false;
				TriggerCameraShake(6f, 0.25f);
			}
			if (isCrit) damage = (int)(damage * Stats.CritMultiplier);

			PlayAttackAnimation();

			var enemies = GetTree().GetNodesInGroup("Enemy");
			foreach (Node2D enemyNode in enemies)
			{
				if (enemyNode is IDamageable damageableEnemy)
				{
					float distance = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
					if (distance <= Stats.AttackRange)
					{
						Vector2 dirToEnemy = (enemyNode.GlobalPosition - GlobalPosition).Normalized();
						if (_facingDirection.Dot(dirToEnemy) > 0.7f)
						{
							damageableEnemy.TakeDamage(damage);
							// 플로팅 데미지는 EnemyController.TakeDamage에서 처리
							if (isCrit) TriggerCameraShake(4f, 0.2f);
						}
					}
				}
			}
		}

		// ─── 애니메이션 시스템 ────────────────────────────────────────
		private void SetupAnimations()
		{
			_animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_animSprite == null) { GD.PrintErr("PlayerController: AnimatedSprite2D 없음"); return; }

			var frames = new SpriteFrames();
			if (frames.HasAnimation("default")) frames.RemoveAnimation("default");

			string[] dirs = { "Down", "Side", "Up" };
			foreach (var dir in dirs)
			{
				string d = dir.ToLower();
				AddSheetAnimation(frames, $"idle_{d}", $"Idle_Base/Idle_{dir}-Sheet.png", 4, 6, true);
				AddSheetAnimation(frames, $"walk_{d}", $"Walk_Base/Walk_{dir}-Sheet.png", 6, 10, true);
				AddSheetAnimation(frames, $"hit_{d}", $"Hit_Base/Hit_{dir}-Sheet.png", 4, 12, false);
				AddSheetAnimation(frames, $"death_{d}", $"Death_Base/Death_{dir}-Sheet.png", 8, 8, false);
				// 공격 애니메이션 3종
				AddSheetAnimation(frames, $"attack_slice_{d}", $"Slice_Base/Slice_{dir}-Sheet.png", 8, 15, false);
				AddSheetAnimation(frames, $"attack_crush_{d}", $"Crush_Base/Crush_{dir}-Sheet.png", 8, 15, false);
				// Pierce는 방향명이 약간 다를 수 있으므로 null-safe 처리
				string pierceDir = dir == "Up" ? "Top" : dir;
				AddSheetAnimation(frames, $"attack_pierce_{d}", $"Pierce_Base/Pierce_{pierceDir}-Sheet.png", 8, 15, false);
			}

			_animSprite.SpriteFrames = frames;
			_animSprite.Play("idle_down");
			_animSprite.AnimationFinished += OnAnimationFinished;
		}

		private string GetAttackAnimPrefix()
		{
			var weapon = Inventory?.EquippedWeapon;
			if (weapon == null) return "attack_slice";
			return weapon.AttackType switch
			{
				WeaponAttackType.Pierce => "attack_pierce",
				WeaponAttackType.Crush => "attack_crush",
				_ => "attack_slice"
			};
		}

		private void AddSheetAnimation(SpriteFrames frames, string animName, string sheetPath, int frameCount, int fps, bool loop)
		{
			frames.AddAnimation(animName);
			frames.SetAnimationSpeed(animName, fps);
			frames.SetAnimationLoop(animName, loop);

			var texture = GD.Load<Texture2D>(AnimBasePath + sheetPath);
			if (texture == null) return; // 파일 없으면 빈 애니메이션으로 둠

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

		private string GetDirectionSuffix()
		{
			float absX = Mathf.Abs(_facingDirection.X);
			float absY = Mathf.Abs(_facingDirection.Y);

			if (absY >= absX)
			{
				if (_animSprite != null) _animSprite.FlipH = false;
				return _facingDirection.Y >= 0 ? "down" : "up";
			}
			else
			{
				if (_animSprite != null) _animSprite.FlipH = _facingDirection.X < 0;
				return "side";
			}
		}

		private void UpdateAnimation()
		{
			if (_animSprite == null || _isAnimLocked) return;
			string dir = GetDirectionSuffix();
			PlayAnim(Velocity != Vector2.Zero ? $"walk_{dir}" : $"idle_{dir}");
		}

		private void PlayAnim(string animName)
		{
			if (_animSprite != null && _animSprite.Animation != animName)
			{
				// 해당 애니메이션이 없으면 idle로 폴백
				if (_animSprite.SpriteFrames.HasAnimation(animName))
					_animSprite.Play(animName);
			}
		}

		private void PlayAttackAnimation()
		{
			if (_animSprite == null) return;
			_isAnimLocked = true;
			Velocity = Vector2.Zero;
			string dir = GetDirectionSuffix();
			string prefix = GetAttackAnimPrefix();
			string animName = $"{prefix}_{dir}";
			// 애니메이션이 없으면 slice로 폴백
			if (!_animSprite.SpriteFrames.HasAnimation(animName))
				animName = $"attack_slice_{dir}";
			_animSprite.Play(animName);
		}

		private void PlayHitAnimation()
		{
			if (_animSprite == null || IsDead) return;
			_isAnimLocked = true;
			_animSprite.Play($"hit_{GetDirectionSuffix()}");
		}

		private void PlayDeathAnimation()
		{
			if (_animSprite == null) return;
			_isAnimLocked = true;
			_animSprite.Play($"death_{GetDirectionSuffix()}");
		}

		private void OnAnimationFinished()
		{
			if (_animSprite == null) return;
			var anim = _animSprite.Animation.ToString();
			if (anim.StartsWith("attack_") || anim.StartsWith("hit_"))
			{
				_isAnimLocked = false;
				UpdateAnimation();
			}
		}

		// ─── 플로팅 데미지 ────────────────────────────────────────
		private static readonly PackedScene FloatingLabelScene =
			GD.Load<PackedScene>("res://Scenes/UI/floating_label.tscn");

		public static void SpawnFloatingLabel(Vector2 worldPos, int damage, bool isCrit, bool isPlayerDamage = false)
		{
			if (FloatingLabelScene == null) return;
			var scene = Engine.GetMainLoop() as SceneTree;
			if (scene == null) return;
			var label = FloatingLabelScene.Instantiate<FloatingLabel>();
			label.GlobalPosition = worldPos + new Vector2(0, -20);
			scene.CurrentScene.AddChild(label);
			label.Init(damage, isCrit, isPlayerDamage);
		}

		// ─── 화면 흔들림 ──────────────────────────────────────────
		public void TriggerCameraShake(float intensity, float duration)
		{
			_shakeIntensity = intensity;
			_shakeTimer = duration;
		}

		private void UpdateCameraShake(double delta)
		{
			if (_shakeTimer <= 0f || _camera == null) return;
			_shakeTimer -= (float)delta;
			if (_shakeTimer <= 0f)
			{
				_shakeTimer = 0f;
				_camera.Offset = Vector2.Zero;
				return;
			}
			float ratio = _shakeTimer; // 시간이 지남에 따라 약해짐
			_camera.Offset = new Vector2(
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity) * ratio,
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity) * ratio
			);
		}
	}
}
