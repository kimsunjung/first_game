using Godot;
using System.Collections.Generic;
using FirstGame.Data;

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

		private float _spawnTimer = 0f;
		private HashSet<Vector2I> _obstacleTiles = new();
		private HashSet<Vector2I> _groundTiles = new();

		public override void _Ready()
		{
			if (EnemyScene == null)
			{
				EnemyScene = GD.Load<PackedScene>("res://Scenes/Characters/enemy.tscn");
				if (EnemyScene == null)
					GD.PrintErr("EnemySpawner: Failed to load enemy scene");
			}

			var field = GetParent()?.GetNodeOrNull("Field");
			if (field != null)
			{
				var obstacleLayer = field.GetNodeOrNull<TileMapLayer>("ObstacleLayer");
				var groundLayer = field.GetNodeOrNull<TileMapLayer>("GroundLayer");

				if (obstacleLayer != null)
				{
					foreach (var cell in obstacleLayer.GetUsedCells())
						_obstacleTiles.Add(cell);
					GD.Print($"EnemySpawner: 장애물 타일 {_obstacleTiles.Count}개 로드");
				}

				if (groundLayer != null)
				{
					foreach (var cell in groundLayer.GetUsedCells())
						_groundTiles.Add(cell);
					GD.Print($"EnemySpawner: 바닥 타일 {_groundTiles.Count}개 로드");
				}
			}
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
				GetParent().AddChild(enemy);
				return;
			}
		}

		private bool IsPositionBlocked(Vector2 worldPos)
		{
			var tilePos = new Vector2I(
				Mathf.FloorToInt(worldPos.X / TileSize),
				Mathf.FloorToInt(worldPos.Y / TileSize)
			);

			if (_groundTiles.Count > 0 && !_groundTiles.Contains(tilePos))
				return true;

			if (_obstacleTiles.Contains(tilePos))
				return true;

			return false;
		}
	}
}
