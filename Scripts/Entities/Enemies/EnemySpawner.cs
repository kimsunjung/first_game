using Godot;

namespace FirstGame.Entities.Enemies
{
    public partial class EnemySpawner : Node2D
    {
        [Export] public PackedScene EnemyScene { get; set; }
        [Export] public float SpawnInterval { get; set; } = 3.0f;
        [Export] public int MaxEnemies { get; set; } = 5;
        [Export] public float SpawnRadius { get; set; } = 300.0f;

        private float _spawnTimer = 0f;

        public override void _Ready()
        {
            if (EnemyScene == null)
            {
                EnemyScene = GD.Load<PackedScene>("res://Scenes/Characters/enemy.tscn");
                if (EnemyScene == null)
                    GD.PrintErr("EnemySpawner: Failed to load res://Scenes/Characters/enemy.tscn");
                else
                    GD.Print("EnemySpawner: Manually loaded EnemyScene.");
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
            if (EnemyScene == null) 
            {
                GD.PrintErr("EnemySpawner: EnemyScene is null!");
                return;
            }

            int currentCount = GetTree().GetNodesInGroup("Enemy").Count;
            // GD.Print($"EnemySpawner: Current enemies: {currentCount}/{MaxEnemies}");

            if (currentCount >= MaxEnemies) return;

            var enemy = EnemyScene.Instantiate<Node2D>();
            // Position relative to spawner within random radius
            var randomOffset = new Vector2(
                (float)GD.RandRange(-SpawnRadius, SpawnRadius),
                (float)GD.RandRange(-SpawnRadius, SpawnRadius)
            );
            enemy.GlobalPosition = GlobalPosition + randomOffset;
            GetParent().AddChild(enemy);
            GD.Print($"EnemySpawner: Spawning enemy at {enemy.GlobalPosition}");
        }
    }
}
