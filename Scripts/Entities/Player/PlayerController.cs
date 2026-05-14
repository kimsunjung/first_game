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
		// 피격 Tween(modulate/흔들림) 진행 중 표시. attack 애니가 같은 시점 끝나도 hit이 lock을
		// 풀어주기 전엔 walk/idle로 튀지 않도록 가드 (Codex P2).
		private bool _isHitFlashing = false;
		// Kenney 타일맵 기반 스프라이트 (정적 1프레임 + 프로그래밍 애니메이션)
		private Tween _walkBounceTween;
		// 진행 중 피격 시각 tween — 연타로 피격 시 새 tween 만들기 전 Kill해 흔들기/페이드 누적 차단.
		private Tween _hitTween;

		// MP 재생
		private float _mpRegenAccum = 0f;

		// HP 재생 (비전투 시)
		private float _hpRegenAccum = 0f;
		private double _lastDamageTime = 0;

		// 넉백
		private Vector2 _knockbackVelocity = Vector2.Zero;

		// 스킬 시스템 — 쿨타임은 SkillType 기준. 슬롯 인덱스에 묶지 않으므로 슬롯 스왑/재배치 시
		// 쿨타임이 다른 스킬에 잘못 적용되거나 우회되는 일이 없다.
		private readonly Dictionary<SkillType, float> _skillCooldowns = new();
		private bool _powerStrikeActive = false;
		private bool _dashActive = false;
		private float _dashTimer = 0f;
		private const float DashSpeedMultiplier = 1.8f;

		// 머리 위 HP바 (적과 동일 패턴, 녹색 fill로 아군 구분)
		private ProgressBar _headHealthBar;

		// 공격 cooldown — Stats.AttackSpeed가 베이스 1.0. 장비/affix가 += 누적해
		// 실제 cooldown = AutoAttackInterval / AttackSpeed로 적용.
		private float _attackCooldown = 0f;

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
		Vector2 ISkillTarget.FacingDirection => _facingDirection;
		public void HealSelf(int amount) => Stats.CurrentHealth += amount;

		/// <summary>스킬 전략용 — PlayerProjectile 생성·발사. 닿을 때 데미지 적용.</summary>
		public void FireProjectile(int damage, FirstGame.Data.ElementType element, Color color, float speed = 460f)
		{
			var proj = new PlayerProjectile
			{
				Damage = damage,
				Speed = speed,
				Direction = _facingDirection != Vector2.Zero ? _facingDirection.Normalized() : Vector2.Right,
				Element = element,
				ProjectileColor = color,
				SingleHit = true
			};
			GetParent().AddChild(proj);
			proj.GlobalPosition = GlobalPosition;
		}

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

			SetupHeadHealthBar();

			CollisionMask |= 4;
			Inventory = new Inventory();
			Inventory.OnItemPickedUp += item =>
				GameManager.Instance?.QuestManager.NotifyItemAcquired(item, this);
			// OnInventoryChanged → TryClaimPendingRewards 구독은 복원 완료 후에 추가한다.
			// 복원 중 AddItem이 발생시키는 OnInventoryChanged가 pending claim을 트리거하면
			// 부분 복원 상태에서 SaveGame이 호출돼 저장 파일이 망가질 수 있다.

			// LoadFromSaveData 내부에서 GameManager.PlayerGold 등을 갱신하므로 사전 등록 필요.
			if (GameManager.Instance != null)
				GameManager.Instance.Player = this;

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
			// 신규 게임: GameManager 등록은 위에서 완료 → 아래에서 즉시 저장

			IsDead = false;

			// 포탈 이동 시 스폰 위치 적용 (세이브 위치보다 우선)
			if (SceneManager.Instance?.NextSpawnPosition != null)
			{
				GlobalPosition = SceneManager.Instance.NextSpawnPosition.Value;
				SceneManager.Instance.NextSpawnPosition = null;
			}

			SetupAnimations();

			// 신규 게임: 클래스 선택값 + 시작 무기 + 시작 스킬 부여 후 첫 자동 저장.
			if (SaveManager.PendingLoadData == null && !SaveManager.HasSave())
			{
				ApplyNewGameClassSetup();
				SaveManager.SaveGame();
			}

			// 인벤에 변동 생길 때마다 pending reward 재시도. 복원 중에는 GameManager의
			// IsRestoringState 가드가 막아주므로 구독 위치는 자유.
			Inventory.OnInventoryChanged += () => GameManager.Instance?.TryClaimPendingRewards();

			// 상태 변경 dirty 마킹 — 인벤(상점/획득/소모) + 장비(장착/해제/강화 적용)에만
			// RequestAutoSave를 건다. 골드 변동에는 의도적으로 걸지 않음 — 상점 트랜잭션은
			// "골드 차감 → AddItem"처럼 두 단계인데 OnGoldChanged 시점에 즉시 저장되면
			// 인벤이 반영되기 전 상태가 디스크에 떨어져, 직후 OnInventoryChanged는 throttle에
			// 막혀 dirty만 남는다. 이 사이에 OS kill 시 골드만 변경된 손상 상태가 된다.
			// 보스 골드는 EnemyController.Die가 별도로 SaveGame을 호출하므로 손실 없음.
			Inventory.OnInventoryChanged += () => SaveManager.RequestAutoSave();
			Inventory.OnEquipmentChanged += () => SaveManager.RequestAutoSave();
		}

		public override void _ExitTree()
		{
			if (Stats != null)
			{
				Stats.OnLevelUp -= OnLevelUpHandler;
				Stats.OnHealthChanged -= UpdateHeadHealthBar;
			}
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
			if (_attackCooldown > 0f) _attackCooldown -= (float)delta;
			Stats?.TickBuffs((float)delta);
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
				else if (key == Key.Key5) Inventory.UseQuickSlot(4, Stats);
				else if (key == Key.Key6) Inventory.UseQuickSlot(5, Stats);
				else if (key == Key.Q) UseSkillSlot(0);
				else if (key == Key.W && !IsMoving()) UseSkillSlot(1);
				else if (key == Key.E) UseSkillSlot(2);
				else if (key == Key.T) UseSkillSlot(4);
				else if (key == Key.Y) UseSkillSlot(5);
				else if (key == Key.R) UseSkillSlot(3);
			}
		}

		private void SetupHeadHealthBar()
		{
			_headHealthBar = GetNodeOrNull<ProgressBar>("HealthBar");
			if (_headHealthBar == null) return;

			_headHealthBar.MaxValue = Stats.MaxHealth;
			_headHealthBar.Value = Stats.CurrentHealth;

			var bgStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
				CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2,
				CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2
			};
			_headHealthBar.AddThemeStyleboxOverride("background", bgStyle);

			var fillStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.2f, 0.85f, 0.25f, 1.0f),
				CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2,
				CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2
			};
			_headHealthBar.AddThemeStyleboxOverride("fill", fillStyle);

			Stats.OnHealthChanged += UpdateHeadHealthBar;
		}

		private void UpdateHeadHealthBar(int cur, int max)
		{
			if (_headHealthBar == null) return;
			_headHealthBar.MaxValue = max;
			_headHealthBar.Value = cur;
		}

		// 신규 게임 시작 시 호출. SaveManager.PendingNewGameClass(MainMenu가 설정)을 읽어
		// 플레이어 클래스, 시작 무기(starter_*), 시작 스킬을 결정. 누락 시 Warrior 기본.
		private void ApplyNewGameClassSetup()
		{
			var cls = SaveManager.PendingNewGameClass ?? FirstGame.Data.PlayerClass.Warrior;
			SaveManager.PendingNewGameClass = null;
			Stats.PlayerClass = cls;
			FirstGame.Core.DayNightCycle.ResetForNewGame();

			string weaponPath = cls switch
			{
				FirstGame.Data.PlayerClass.Mage   => "res://Resources/Items/starter_staff.tres",
				FirstGame.Data.PlayerClass.Archer => "res://Resources/Items/starter_bow.tres",
				_ => "res://Resources/Items/starter_sword.tres"
			};
			string skillPath = cls switch
			{
				FirstGame.Data.PlayerClass.Mage   => "res://Resources/Skills/fire_bolt.tres",
				FirstGame.Data.PlayerClass.Archer => "res://Resources/Skills/arrow_shot.tres",
				_ => "res://Resources/Skills/power_strike.tres"
			};

			var weapon = GD.Load<ItemData>(weaponPath);
			if (weapon != null)
			{
				if (Inventory.AddItem(weapon, 1, 0, fireAcquired: false))
				{
					// ResourcePath 비교 — GD.Load 캐싱이 깨지거나(테스트 빌드) 같은 ItemData가
					// 두 슬롯에 분할된 가상 케이스에도 안정적. 참조 동등성 의존 제거.
					int idx = Inventory.Slots.FindIndex(s => s.Item?.ResourcePath == weaponPath);
					if (idx >= 0) Inventory.EquipItem(idx, Stats);
				}
			}

			var skill = GD.Load<SkillData>(skillPath);
			if (skill != null) Stats.LearnSkill(skill);
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
		public bool CollectItem(ItemData item, int quantity, List<ItemAffix> affixes = null)
		{
			return Inventory.AddItem(item, quantity, 0, true, affixes);
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
			// 전체 복원 구간을 IsRestoringState=true로 감싸 TryClaimPendingRewards가 부분 복원
			// 상태에서 SaveGame을 호출하지 못하게 한다. 끝나면 1회만 명시 claim.
			GameManager.Instance?.BeginRestoreState();
			try
			{
				GlobalPosition = new Vector2(data.PlayerPosX, data.PlayerPosY);
				// 게임 시간 복원 — v10 이하 누락 시 기본(06:00 / Day 1)
				FirstGame.Core.DayNightCycle.RestoreFromSave(data.DayTime, data.GameDay);

				// 스탯 재계산: 레벨 → STR/CON/INT → 장비(RestoreEquipment) 순서로 결정론적 재계산
				// data.PlayerMaxHealth 직접 신뢰 금지: RestoreEquipment가 장비 보너스를 더하므로 이중 카운팅 발생
				Stats.SetLevelFromSave(data.PlayerLevel, data.PlayerExp);
				Stats.SetStatPointsFromSave(data.StatPoints, data.StrPoints, data.ConPoints, data.IntPoints, data.DexPoints);
				Stats.PlayerClass = (FirstGame.Data.PlayerClass)data.PlayerClassId;
				Stats.ApplyStatPointBonuses();
				GameManager.Instance?.RestoreDefeatedBosses(data.DefeatedBosses ?? new());
				GameManager.Instance.PlayerGold = data.PlayerGold;

				// 인벤토리 복원 — 직렬화된 슬롯 순서대로 그대로 추가, IsEquipped 보존.
				if (data.InventoryItems != null)
				{
					foreach (var savedSlot in data.InventoryItems)
					{
						var item = GD.Load<ItemData>(savedSlot.ItemPath);
						if (item == null) continue;
						bool added = Inventory.AddItem(item, savedSlot.Quantity, savedSlot.EnhancementLevel,
							fireAcquired: false, savedSlot.Affixes);
						// AddItem 직후 마지막 슬롯에 IsEquipped 마킹 (AddItem이 stack했을 경우 마킹 안 됨)
						if (added && savedSlot.IsEquipped && Inventory.Slots.Count > 0)
						{
							var last = Inventory.Slots[Inventory.Slots.Count - 1];
							if (last.Item.ResourcePath == savedSlot.ItemPath)
								last.IsEquipped = true;
						}
					}
				}

				// 9개 장비 슬롯 + v3→v4 Accessory 재분류 마이그 일괄 처리
				var report = Inventory.RestoreFromSaveData(data, Stats);
				if (report.MigratedItem != null)
				{
					if (report.MigratedToInventory)
						GD.Print($"[마이그] 강화 +{report.MigratedEnhancement} {report.MigratedItem.ItemName} " +
								 "→ 인벤토리로 반환 (신규 슬롯은 강화 미지원)");
					else
						GD.PrintErr($"[마이그] 인벤 가득 — {report.MigratedItem.ItemName} " +
									$"강화 +{report.MigratedEnhancement} 손실하며 신규 슬롯에 강제 장착");
				}

				// 퀘스트 복원
				GameManager.Instance?.QuestManager.RestoreFromSave(
					data.CurrentQuestPath, data.QuestKillProgress, data.CompletedQuestIds);

				// 장비 보너스 적용 후 현재 HP/MP 복원 (MaxHealth/MaxMp가 확정된 뒤에 설정)
				Stats.CurrentHealth = Mathf.Min(data.PlayerHealth, Stats.MaxHealth);
				Stats.CurrentMp = Mathf.Min(data.PlayerMp, Stats.MaxMp);

				if (data.QuickSlotPaths != null)
				{
					var qsItems = new ItemData[6];
					for (int i = 0; i < data.QuickSlotPaths.Count && i < 6; i++)
					{
						if (!string.IsNullOrEmpty(data.QuickSlotPaths[i]))
							qsItems[i] = GD.Load<ItemData>(data.QuickSlotPaths[i]);
					}
					Inventory.RestoreQuickSlots(qsItems);
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
					// 패시브 스킬 효과를 SetLevelFromSave 베이스 리셋 이후에 재적용 — 매 로드마다 결정론.
					Stats.ApplyPassiveBonuses();
				}

				// 처치한 보스 목록 복원
				if (data.DefeatedBosses != null && data.DefeatedBosses.Count > 0)
					GameManager.Instance?.RestoreDefeatedBosses(data.DefeatedBosses);

				// 보류 보상은 복원만 하고 TryClaim은 EndRestoreState 후로 미룬다.
				GameManager.Instance?.RestorePendingRewards(data.PendingRewardItems);

				// 절차적 필드맵 seed 복원 — 같은 씬 재진입 시 동일 지형이 보장돼
				// 저장 좌표가 장애물 안으로 들어가는 결함 차단.
				GameManager.Instance?.RestoreFieldSeeds(data.FieldSeeds);
				GameManager.Instance?.RestoreVisitedScenes(data.VisitedScenes);
				GameManager.Instance?.RestoreMinedNodes(data.MinedNodes);
				GameManager.Instance?.RestoreChapterFlags(data.ChapterFlags);
				// 현재 씬을 visited에 추가 (로드된 직후의 시작 씬도 방문 처리).
				GameManager.Instance?.RecordSceneVisit(GetTree().CurrentScene.SceneFilePath);

				SaveManager.PendingLoadData = null;
			}
			finally
			{
				GameManager.Instance?.EndRestoreState();
			}

			// 모든 복원이 끝난 뒤 단 한 번 claim 시도. 인벤에 자리 있으면 즉시 지급되고
			// queueMutated 경로에서 SaveGame이 호출돼 stale pending 재지급을 차단한다.
			GameManager.Instance?.TryClaimPendingRewards();
		}
	}
}
