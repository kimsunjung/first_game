using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FirstGame.Data;
using FirstGame.Data.Skills;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.Entities.Player
{
	/// <summary>
	/// 플레이어 컨트롤러 (핵심 + 라이프사이클).
	/// 세부 로직은 partial class로 분리:
	///   Combat, Movement, Skills, Animation, Camera
	/// </summary>
	public partial class PlayerController : CharacterBody2D, IDamageable, ISkillTarget, ISaveable, IItemCollector, IPlayer
	{
		[Export] public PlayerStats Stats { get; set; }
		public Inventory Inventory { get; private set; }

		public bool IsDead { get; private set; } = false;
		private Vector2 _facingDirection = Vector2.Down;

		// 애니메이션
		private AnimatedSprite2D _animSprite;
		private bool _isAnimLocked = false;
		// Kenney 타일맵 기반 스프라이트 (정적 1프레임 + 프로그래밍 애니메이션)
		private Tween _walkBounceTween;

		// MP 재생
		private float _mpRegenAccum = 0f;

		// HP 재생 (비전투 시)
		private float _hpRegenAccum = 0f;
		private double _lastDamageTime = 0;

		// 넉백
		private Vector2 _knockbackVelocity = Vector2.Zero;

		// 스킬 시스템
		private readonly float[] _skillCooldowns = new float[4];
		private bool _powerStrikeActive = false;
		private bool _dashActive = false;
		private float _dashTimer = 0f;
		private const float DashSpeedMultiplier = 1.8f;

		// 카메라 쉐이크
		private Camera2D _camera;
		private float _shakeIntensity = 0f;
		private float _shakeTimer = 0f;
		private float _shakeDuration = 0f;

		[Export] public float Acceleration { get; set; } = 500.0f;
		[Export] public float Friction { get; set; } = 600.0f;

		// ─── ISkillTarget 구현 ──────────────────────────────────────
		int ISkillTarget.BaseDamage => Stats.BaseDamage;
		float ISkillTarget.CritRate => Stats.CritRate;
		float ISkillTarget.CritMultiplier => Stats.CritMultiplier;
		public void HealSelf(int amount) => Stats.CurrentHealth += amount;

		// ─── 라이프사이클 ────────────────────────────────────────────
		public override void _Ready()
		{
			AddToGroup("Player");

			if (Stats != null)
				Stats = (PlayerStats)Stats.Duplicate();
			else
				Stats = new PlayerStats();

			Stats.OnLevelUp += OnLevelUpHandler;
			EventManager.OnExpGained += GainExp;

			_camera = GetNodeOrNull<Camera2D>("Camera2D");
			if (_camera != null)
			{
				_camera.PositionSmoothingEnabled = true;
				_camera.PositionSmoothingSpeed = 8.0f;
				ApplyCameraZoom();
				ApplyCameraBounds();
			}

			CollisionMask |= 4;
			Inventory = new Inventory();

			if (SaveManager.PendingLoadData != null)
			{
				LoadFromSaveData(SaveManager.PendingLoadData);
			}
			else if (SaveManager.HasSave())
			{
				// PendingLoadData가 없지만 세이브 파일 존재 → 파일에서 복원 (폴백)
				SaveManager.LoadIntoPending();
				if (SaveManager.PendingLoadData != null)
					LoadFromSaveData(SaveManager.PendingLoadData);
			}
			else
			{
				SaveManager.SaveGame();
			}

			IsDead = false;

			// 포탈 이동 시 스폰 위치 적용 (세이브 위치보다 우선)
			if (SceneManager.Instance?.NextSpawnPosition != null)
			{
				GlobalPosition = SceneManager.Instance.NextSpawnPosition.Value;
				SceneManager.Instance.NextSpawnPosition = null;
			}

			SetupAnimations();

			if (GameManager.Instance != null)
				GameManager.Instance.Player = this;

		}

		public override void _ExitTree()
		{
			if (Stats != null) Stats.OnLevelUp -= OnLevelUpHandler;
			EventManager.OnExpGained -= GainExp;

			if (GameManager.Instance != null && GameManager.Instance.Player == this)
				GameManager.Instance.Player = null;
		}

		public override void _PhysicsProcess(double delta)
		{
			GetInput(delta);
			ApplyKnockbackDecay(delta);
			MoveAndSlide();
			UpdateAnimation();
			RegenMp(delta);
			RegenHp(delta);
			UpdateSkillCooldowns(delta);
			UpdateDash(delta);
			UpdateCameraShake(delta);
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			HandleZoomInput(@event);
			if (IsDead) return;
			if (@event is InputEventKey k2 && k2.Pressed && !k2.Echo)
			{
				var key = k2.Keycode != Key.None ? k2.Keycode : k2.PhysicalKeycode;
				if (key == Key.Key1) Inventory.UseQuickSlot(0, Stats);
				else if (key == Key.Key2) Inventory.UseQuickSlot(1, Stats);
				else if (key == Key.Key3) Inventory.UseQuickSlot(2, Stats);
				else if (key == Key.Key4) Inventory.UseQuickSlot(3, Stats);
				else if (key == Key.Q) UseSkillSlot(0);
				else if (key == Key.W && !IsMoving()) UseSkillSlot(1);
				else if (key == Key.E) UseSkillSlot(2);
				else if (key == Key.R) UseSkillSlot(3);
			}
		}

		private void OnLevelUpHandler(int newLevel)
		{
			EventManager.TriggerLevelUp(newLevel);
			AudioManager.Instance?.PlaySFX("level_up.wav");
		}

		public static void SpawnFloatingLabel(Vector2 worldPos, int damage, bool isCrit, bool isPlayerDamage = false)
		{
			UIEffectManager.SpawnFloatingLabel(worldPos, damage, isCrit, isPlayerDamage);
		}

		// ─── IItemCollector 구현 ─────────────────────────────────────
		public bool CollectItem(ItemData item, int quantity)
		{
			return Inventory.AddItem(item, quantity);
		}

		// ─── ISkillTarget.GetNearbyEnemies 구현 ─────────────────────
		public IEnumerable<(Node2D Node, IDamageable Target)> GetNearbyEnemies(float range)
		{
			var enemies = GameManager.Instance?.ActiveEnemies;
			if (enemies == null) return Enumerable.Empty<(Node2D, IDamageable)>();

			var result = new List<(Node2D, IDamageable)>();
			foreach (Node2D e in enemies)
			{
				if (e is IDamageable dam && GlobalPosition.DistanceTo(e.GlobalPosition) <= range)
					result.Add((e, dam));
			}
			return result;
		}

		// ─── 세이브 데이터 로드 ──────────────────────────────────────
		private void LoadFromSaveData(SaveData data)
		{
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
					if (item != null) Inventory.AddItem(item, savedSlot.Quantity, savedSlot.EnhancementLevel);
				}
			}

			ItemData loadedWeapon = null, loadedArmor = null, loadedAccessory = null;
			if (!string.IsNullOrEmpty(data.EquippedWeaponPath))
				loadedWeapon = GD.Load<ItemData>(data.EquippedWeaponPath);
			if (!string.IsNullOrEmpty(data.EquippedArmorPath))
				loadedArmor = GD.Load<ItemData>(data.EquippedArmorPath);
			if (!string.IsNullOrEmpty(data.EquippedAccessoryPath))
				loadedAccessory = GD.Load<ItemData>(data.EquippedAccessoryPath);
			Inventory.RestoreEquipment(loadedWeapon, loadedArmor, Stats, loadedAccessory,
				data.EquippedWeaponEnhancement, data.EquippedArmorEnhancement, data.EquippedAccessoryEnhancement);

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

			// 처치한 보스 목록 복원
			if (data.DefeatedBosses != null && data.DefeatedBosses.Count > 0)
				GameManager.Instance?.RestoreDefeatedBosses(data.DefeatedBosses);

			SaveManager.PendingLoadData = null;
		}
	}
}
