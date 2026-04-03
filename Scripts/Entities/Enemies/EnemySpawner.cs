using Godot;
using System.Collections.Generic;
using FirstGame.Data;
using FirstGame.Core;

namespace FirstGame.Entities.Enemies
{
	public partial class EnemySpawner : Node2D
	{
		[Export] public PackedScene EnemyScene { get; set; }
		[Export] public EnemyStats[] StatVariants { get; set; } // 다양한 적 타입
		[Export] public float SpawnInterval { get; set; } = 3.0f;
		[Export] public int MaxEnemies { get; set; } = 5;
		[Export] public float SpawnRadius { get; set; } = 300.0f;
		[Export] public int TileSize { get; set; } = 16;
		[Export] public EnemyStats BossStatVariant { get; set; }

		// 보스 스폰 카운터 (씬 재로드 시 초기화됨)
		private int _killCount = 0;
		private bool _bossAlive = false;

		private float _spawnTimer = 0f;
		private HashSet<Vector2I> _obstacleTiles = new();
		private HashSet<Vector2I> _groundTiles = new();
		private Vector2 _fieldOffset = Vector2.Zero;

		public override void _Ready()
		{
			if (EnemyScene == null)
			{
				EnemyScene = GD.Load<PackedScene>("res://Scenes/Characters/enemy.tscn");
				if (EnemyScene == null)
					GD.PrintErr("EnemySpawner: Failed to load enemy scene");
			}

			// "Field" 노드 또는 TileMapLayer를 가진 아무 노드에서 타일 데이터 로드
			var parent = GetParent();
			Node2D field = parent?.GetNodeOrNull<Node2D>("Field");
			TileMapLayer obstacleLayer = null;
			TileMapLayer groundLayer = null;

			if (field != null)
			{
				_fieldOffset = field.GlobalPosition;
				obstacleLayer = field.GetNodeOrNull<TileMapLayer>("ObstacleLayer");
				groundLayer = field.GetNodeOrNull<TileMapLayer>("GroundLayer");
			}
			else if (parent != null)
			{
				// "Field" 노드가 없으면 부모에서 직접 TileMapLayer 탐색
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
							break;
						}
					}
				}
			}

			if (obstacleLayer != null)
			{
				foreach (var cell in obstacleLayer.GetUsedCells())
					_obstacleTiles.Add(cell);
			}
			if (groundLayer != null)
			{
				foreach (var cell in groundLayer.GetUsedCells())
					_groundTiles.Add(cell);
			}

			GD.Print($"[EnemySpawner] 초기화 완료 - 씬: {GetTree().CurrentScene?.Name}, " +
					 $"EnemyScene: {(EnemyScene != null ? "OK" : "NULL")}, " +
					 $"StatVariants: {StatVariants?.Length ?? 0}종, " +
					 $"MaxEnemies: {MaxEnemies}, " +
					 $"바닥타일: {_groundTiles.Count}, 장애물타일: {_obstacleTiles.Count}");

			EventManager.OnEnemyKilled += OnEnemyKilledHandler;
			EventManager.OnBossDied += OnBossDiedHandler;
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

			const int maxAttempts = 10;
			for (int i = 0; i < maxAttempts; i++)
			{
				var randomOffset = new Vector2(
					(float)GD.RandRange(-SpawnRadius, SpawnRadius),
					(float)GD.RandRange(-SpawnRadius, SpawnRadius)
				);
				var spawnPos = GlobalPosition + randomOffset;

				if (IsPositionBlocked(spawnPos)) continue;

				var enemy = EnemyScene.Instantiate<EnemyController>();

				// StatVariants가 있으면 랜덤으로 하나 선택
				if (StatVariants != null && StatVariants.Length > 0)
				{
					int idx = (int)(GD.Randi() % (uint)StatVariants.Length);
					enemy.Stats = StatVariants[idx];
				}

				enemy.GlobalPosition = spawnPos;
				enemy.AddToGroup("Enemy");
				GetParent().AddChild(enemy);
				return;
			}
		}

		private bool IsPositionBlocked(Vector2 worldPos)
		{
			var localPos = worldPos - _fieldOffset;
			var tilePos = new Vector2I(
				Mathf.FloorToInt(localPos.X / TileSize),
				Mathf.FloorToInt(localPos.Y / TileSize)
			);

			if (_groundTiles.Count > 0 && !_groundTiles.Contains(tilePos))
				return true;

			if (_obstacleTiles.Contains(tilePos))
				return true;

			return false;
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

		private void TrySpawnBoss()
		{
			if (EnemyScene == null) return;
			if (GameManager.Instance?.IsBossDefeated(BossStatVariant.EnemyTypeName) == true) return;
			_bossAlive = true;
			var boss = EnemyScene.Instantiate<EnemyController>();
			boss.Stats = (EnemyStats)BossStatVariant.Duplicate();
			// 보스는 스포너 근처에 스폰
			boss.GlobalPosition = GlobalPosition + new Vector2(0, -150);
			boss.Scale = new Vector2(2.0f, 2.0f);
			boss.AddToGroup("Boss");
			boss.AddToGroup("Enemy");
			GetParent().AddChild(boss);
			EventManager.TriggerBossSpawned(BossStatVariant.MaxHealth, BossStatVariant.EnemyTypeName);
			GD.Print($"보스 등장! {BossStatVariant.EnemyTypeName}");
		}
	}
}
