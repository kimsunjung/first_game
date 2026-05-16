using Godot;
using System.Collections.Generic;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Maps;

namespace FirstGame.Entities.Enemies
{
	public partial class EnemySpawner : Node2D
	{
		[Export] public PackedScene EnemyScene { get; set; }
		[Export] public EnemyStats[] StatVariants { get; set; } // 다양한 적 타입
		// 지역별 일반/희귀 비율 조절용 가중치. null이거나 길이가 StatVariants와 다르거나
		// 합계가 0 이하이면 기존 균등 랜덤으로 fallback. 설정 시 가중치 룰렛 선택.
		[Export] public float[] StatWeights { get; set; }
		[Export] public float SpawnInterval { get; set; } = 3.0f;
		[Export] public int MaxEnemies { get; set; } = 5;
		[Export] public float SpawnRadius { get; set; } = 300.0f;
		[Export] public bool SpawnAroundPlayer { get; set; } = true;
		[Export] public float MinSpawnDistance { get; set; } = 120.0f;
		[Export] public int SpawnEdgePaddingTiles { get; set; } = 4;
		[Export] public int TileSize { get; set; } = 16;
		[Export] public EnemyStats BossStatVariant { get; set; }

		/// <summary>
		/// 이 스포너의 보스 처치 키. 같은 boss_*.tres를 여러 던전이 공유할 때 충돌을 방지.
		/// 비워두면 BossStatVariant.EnemyTypeName을 사용 (단일 보스 또는 충돌 없는 경우).
		/// </summary>
		[Export] public string BossId { get; set; } = "";

		/// <summary>
		/// true면 GameManager.IsBossDefeated 기록과 무관하게 BossKillThreshold마다 보스를 재스폰.
		/// 메인 챕터 던전(dungeon_1/2/3)은 false로 두어 1회 처치 후 사라지게 하고,
		/// 야외/광산 Named 보스(field_5/field_6/mine_3/dungeon_4)는 true로 두어 파밍 가능하게 한다.
		/// 어느 경우든 GameManager.RecordBossDefeat은 항상 호출되어 first-kill 기록은 영구 유지된다.
		/// </summary>
		[Export] public bool RepeatableBoss { get; set; } = false;

		// 보스 스폰 카운터 (씬 재로드 시 초기화됨)
		private int _killCount = 0;
		private bool _bossAlive = false;

		private float _spawnTimer = 0f;
		private HashSet<Vector2I> _obstacleTiles = new();
		private HashSet<Vector2I> _groundTiles = new();
		private Vector2 _fieldOffset = Vector2.Zero;
		private Rect2I _mapBoundsCells = new(Vector2I.Zero, Vector2I.Zero);
		private bool _hasMapBounds = false;
		private bool _emptyGroundIsWalkable = false;
		private int _failedSpawnLogs = 0;

		public override void _Ready()
		{
			if (EnemyScene == null)
			{
				EnemyScene = GD.Load<PackedScene>("res://Scenes/Characters/enemy.tscn");
				if (EnemyScene == null)
					GD.PrintErr("EnemySpawner: Failed to load enemy scene");
			}

			EventManager.OnEnemyKilled += OnEnemyKilledHandler;
			EventManager.OnBossDied += OnBossDiedHandler;

			// 타일 캐시는 모든 _Ready() 완료 후 실행 (MapGenerator가 먼저 타일 생성하도록)
			CallDeferred(nameof(InitTileCache));
		}

		private void InitTileCache()
		{
			var parent = GetParent();
			Node2D field = parent?.GetNodeOrNull<Node2D>("Field");
			MapGenerator mapGenerator = null;
			TileMapLayer obstacleLayer = null;
			TileMapLayer groundLayer = null;

			if (field != null)
			{
				_fieldOffset = field.GlobalPosition;
				obstacleLayer = field.GetNodeOrNull<TileMapLayer>("ObstacleLayer");
				groundLayer = field.GetNodeOrNull<TileMapLayer>("GroundLayer");
				mapGenerator = field as MapGenerator;
			}
			else if (parent != null)
			{
				foreach (var child in parent.GetChildren())
				{
					if (child is Node2D n2d)
					{
						var ol = n2d.GetNodeOrNull<TileMapLayer>("ObstacleLayer");
						var gl = n2d.GetNodeOrNull<TileMapLayer>("GroundLayer");
						if (ol != null || gl != null)
						{
							_fieldOffset = n2d.GlobalPosition;
							obstacleLayer = ol;
							groundLayer = gl;
							mapGenerator = n2d as MapGenerator;
							break;
						}
					}
				}
			}

			if (mapGenerator != null)
			{
				_hasMapBounds = true;
				_mapBoundsCells = new Rect2I(0, 0, mapGenerator.MapWidth, mapGenerator.MapHeight);
				// MapGenerator는 잔디를 TileMap 셀로 찍지 않고 Background ColorRect로 표현한다.
				// 따라서 빈 GroundLayer 셀도 이동/스폰 가능한 바닥으로 취급해야 한다.
				_emptyGroundIsWalkable = true;
			}
			else
			{
				// MapGenerator가 없는 단순 ColorRect 배경 맵(town/dungeon)에서도
				// 화면 밖(벽 너머)으로 스폰되지 않도록 Background ColorRect를 경계로 사용.
				var bg = parent?.GetNodeOrNull<ColorRect>("Background");
				if (bg != null && bg.Size.X > 0 && bg.Size.Y > 0)
				{
					int w = Mathf.FloorToInt(bg.Size.X / TileSize);
					int h = Mathf.FloorToInt(bg.Size.Y / TileSize);
					_hasMapBounds = true;
					_mapBoundsCells = new Rect2I(0, 0, w, h);
					_emptyGroundIsWalkable = true;
					_fieldOffset = bg.GlobalPosition;
				}
			}

			if (obstacleLayer != null)
				foreach (var cell in obstacleLayer.GetUsedCells())
					_obstacleTiles.Add(cell);

			if (groundLayer != null)
				foreach (var cell in groundLayer.GetUsedCells())
					_groundTiles.Add(cell);

			GD.Print($"[EnemySpawner] 초기화 완료 - 씬: {GetTree().CurrentScene?.Name}, " +
					 $"StatVariants: {StatVariants?.Length ?? 0}종, MaxEnemies: {MaxEnemies}, " +
					 $"바닥타일: {_groundTiles.Count}, 장애물타일: {_obstacleTiles.Count}, " +
					 $"플레이어주변스폰: {SpawnAroundPlayer}");
		}

		public override void _PhysicsProcess(double delta)
		{
			_spawnTimer -= (float)delta;

			if (_spawnTimer <= 0f)
			{
				_spawnTimer = SpawnInterval;
				TrySpawnEnemy();
			}
		}

		private void TrySpawnEnemy()
		{
			if (EnemyScene == null) return;

			int currentCount = GetTree().GetNodesInGroup("Enemy").Count;
			if (currentCount >= MaxEnemies) return;

			const int maxAttempts = 30;
			var spawnCenter = GetSpawnCenter();
			for (int i = 0; i < maxAttempts; i++)
			{
				var spawnPos = spawnCenter + GetRandomSpawnOffset();

				if (IsPositionBlocked(spawnPos)) continue;

				var enemy = EnemyScene.Instantiate<EnemyController>();

				// StatVariants가 있으면 랜덤으로 하나 선택 + zone 난이도 스케일 적용.
				// .tres 원본을 보호하기 위해 Duplicate 후 변형.
				bool isElite = false;
				EliteAffix affix = EliteAffix.None;
				EliteAffixUtil.AffixParams affixParams = default;
				if (StatVariants != null && StatVariants.Length > 0)
				{
					int idx = PickVariantIndex();
					var scaled = (EnemyStats)StatVariants[idx].Duplicate();
					ApplyZoneScaling(scaled);

					isElite = GD.Randf() < BalanceData.Elite.SpawnChance;
					if (isElite)
					{
						affix = EliteAffixUtil.RollRandom();
						affixParams = EliteAffixUtil.Get(affix);
						ApplyEliteBoost(scaled, affixParams);
					}
					enemy.Stats = scaled;
				}

				enemy.GlobalPosition = spawnPos;
				if (isElite)
				{
					enemy.Scale = new Vector2(BalanceData.Elite.ScaleMultiplier, BalanceData.Elite.ScaleMultiplier);
					enemy.Modulate = affixParams.Modulate;
					enemy.Affix = affix;
					// Vampiric 재생률을 EnemyController 내부 필드에 주입(public 재생률 setter 대신 reflection-free 경로).
					enemy.SetRegenRate(affixParams.HpRegenPerSec);
					enemy.AddToGroup("Elite");
				}
				enemy.AddToGroup("Enemy");
				GetParent().AddChild(enemy);
				return;
			}

			if (_failedSpawnLogs < 3)
			{
				_failedSpawnLogs++;
				GD.Print($"[EnemySpawner] 스폰 실패: {maxAttempts}회 모두 차단됨. center={spawnCenter}, " +
						 $"ground={_groundTiles.Count}, obstacle={_obstacleTiles.Count}, emptyGroundWalkable={_emptyGroundIsWalkable}");
			}
		}

		// 가중치 룰렛으로 StatVariants 인덱스 선택. 조건 불충족 시 균등 랜덤 fallback.
		private int PickVariantIndex()
		{
			int n = StatVariants.Length;
			if (StatWeights == null || StatWeights.Length != n)
				return (int)(GD.Randi() % (uint)n);
			float total = 0f;
			foreach (var w in StatWeights) total += w > 0f ? w : 0f;
			if (total <= 0f)
				return (int)(GD.Randi() % (uint)n);
			float roll = (float)GD.RandRange(0.0, total);
			float cumulative = 0f;
			for (int i = 0; i < n; i++)
			{
				cumulative += StatWeights[i] > 0f ? StatWeights[i] : 0f;
				if (roll < cumulative) return i;
			}
			return n - 1;
		}

		private Vector2 GetSpawnCenter()
		{
			if (SpawnAroundPlayer)
			{
				var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
				if (player != null)
					return player.GlobalPosition;
			}

			return GlobalPosition;
		}

		private Vector2 GetRandomSpawnOffset()
		{
			float minDistance = Mathf.Clamp(MinSpawnDistance, 0f, SpawnRadius);
			float angle = (float)GD.RandRange(0.0, Mathf.Tau);
			float distance = (float)GD.RandRange(minDistance, SpawnRadius);
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
		}

		private bool IsPositionBlocked(Vector2 worldPos)
		{
			var localPos = worldPos - _fieldOffset;
			var tilePos = new Vector2I(
				Mathf.FloorToInt(localPos.X / TileSize),
				Mathf.FloorToInt(localPos.Y / TileSize)
			);

			if (_hasMapBounds && !IsInsideMapBounds(tilePos))
				return true;

			if (_groundTiles.Count > 0 && !_emptyGroundIsWalkable && !_groundTiles.Contains(tilePos))
				return true;

			if (_obstacleTiles.Contains(tilePos))
				return true;

			return false;
		}

		// 엘리트 부스트: 기본 multiplier(balance.elite)에 affix별 추가 배율 적용.
		private static void ApplyEliteBoost(EnemyStats stats, EliteAffixUtil.AffixParams affixParams)
		{
			var b = BalanceData.Elite;
			stats.MaxHealth = Mathf.Max(1, Mathf.RoundToInt(stats.MaxHealth * b.HpMultiplier * affixParams.HpMul));
			stats.CurrentHealth = stats.MaxHealth;
			stats.BaseDamage = Mathf.Max(1, Mathf.RoundToInt(stats.BaseDamage * b.AtkMultiplier * affixParams.AtkMul));
			stats.MoveSpeed *= affixParams.MoveSpeedMul;
			stats.AttackCooldown *= affixParams.AttackCooldownMul;
			stats.Defense += affixParams.DefenseBonus;
			stats.ExperienceReward = Mathf.RoundToInt(stats.ExperienceReward * b.ExpMultiplier);
			stats.DropChance = Mathf.Min(1f, stats.DropChance * b.DropMultiplier);

			string baseName = stats.EnemyTypeName;
			if (baseName.StartsWith("엘리트 ")) baseName = baseName.Substring(4);
			if (baseName.StartsWith("광폭한 ") || baseName.StartsWith("흡혈 ")
			    || baseName.StartsWith("민첩한 ") || baseName.StartsWith("강건한 "))
			{
				int sp = baseName.IndexOf(' ');
				if (sp > 0) baseName = baseName.Substring(sp + 1);
			}
			stats.EnemyTypeName = $"{affixParams.NamePrefix} {baseName}";
		}

		private void ApplyZoneScaling(EnemyStats stats)
		{
			var zone = BalanceData.GetZone(GetCurrentZoneName());
			if (zone.HpMultiplier == 1f && zone.AtkMultiplier == 1f && zone.ExpMultiplier == 1f)
				return;

			stats.MaxHealth = Mathf.Max(1, Mathf.RoundToInt(stats.MaxHealth * zone.HpMultiplier));
			stats.CurrentHealth = stats.MaxHealth;
			stats.BaseDamage = Mathf.Max(1, Mathf.RoundToInt(stats.BaseDamage * zone.AtkMultiplier));
			stats.ExperienceReward = Mathf.RoundToInt(stats.ExperienceReward * zone.ExpMultiplier);
		}

		private string GetCurrentZoneName()
		{
			var path = GetTree()?.CurrentScene?.SceneFilePath;
			if (string.IsNullOrEmpty(path)) return "";
			int lastSlash = path.LastIndexOf('/');
			string name = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
			return name.EndsWith(".tscn") ? name.Substring(0, name.Length - 5) : name;
		}

		private bool IsInsideMapBounds(Vector2I tilePos)
		{
			int padding = Mathf.Max(0, SpawnEdgePaddingTiles);
			return tilePos.X >= _mapBoundsCells.Position.X + padding
				&& tilePos.Y >= _mapBoundsCells.Position.Y + padding
				&& tilePos.X < _mapBoundsCells.Position.X + _mapBoundsCells.Size.X - padding
				&& tilePos.Y < _mapBoundsCells.Position.Y + _mapBoundsCells.Size.Y - padding;
		}

		public override void _ExitTree()
		{
			EventManager.OnEnemyKilled -= OnEnemyKilledHandler;
			EventManager.OnBossDied -= OnBossDiedHandler;
		}

		private void OnEnemyKilledHandler()
		{
			_killCount++;
			if (!_bossAlive && BossStatVariant != null && _killCount % BalanceData.Enemy.BossKillThreshold == 0)
			{
				TrySpawnBoss();
			}
		}

		private void OnBossDiedHandler()
		{
			_bossAlive = false;
		}

		private string ResolveBossId() =>
			!string.IsNullOrEmpty(BossId) ? BossId : BossStatVariant?.EnemyTypeName ?? "";

		private void TrySpawnBoss()
		{
			if (EnemyScene == null) return;
			string bossKey = ResolveBossId();
			// 1회용 보스만 처치 기록으로 재스폰 차단. 반복 보스는 기록과 무관하게 재출현.
			if (!RepeatableBoss && GameManager.Instance?.IsBossDefeated(bossKey) == true) return;
			_bossAlive = true;
			var boss = EnemyScene.Instantiate<EnemyController>();
			var bossStats = (EnemyStats)BossStatVariant.Duplicate();
			ApplyZoneScaling(bossStats);
			boss.Stats = bossStats;
			boss.BossId = bossKey;
			// 보스는 스포너 근처에 스폰
			boss.GlobalPosition = GlobalPosition + new Vector2(0, -150);
			boss.Scale = new Vector2(2.0f, 2.0f);
			boss.AddToGroup("Boss");
			boss.AddToGroup("Enemy");
			GetParent().AddChild(boss);
			// zone scaling이 반영된 실제 HP를 UI에 전달
			EventManager.TriggerBossSpawned(bossStats.MaxHealth, bossStats.EnemyTypeName);
			GD.Print($"보스 등장! {bossStats.EnemyTypeName} (HP {bossStats.MaxHealth}, BossId={bossKey})");
		}
	}
}
