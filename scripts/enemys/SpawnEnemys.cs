using Godot;
using System;

public partial class SpawnEnemys : Node
{
    [Export] public PackedScene EnemyScene; // Drag and drop your enemy scene here in the editor
    [Export] public NodePath SpawnPathNodePath; // Path to the Path2D or PathFollow2D node
    [Export] public float SpawnInterval = 2.0f; // Time interval between spawns
	[Export] public float ActivationDelay = 2.0f;

	private PackedScene enemy= ResourceLoader.Load<PackedScene>("res://scenes/EnemySpawnerHandler.tscn");

    private Path2D spawnPath;
    private Timer spawnTimer;

	public override void _Ready()
	{
		if (EnemyScene == null)
        {
            GD.PrintErr("EnemyScene is not assigned!");
            return;
        }

        // spawnPath = GetNode<Path2D>(SpawnPathNodePath);
        // if (spawnPath == null)
        // {
        //     GD.PrintErr("SpawnPath node is not assigned or invalid!");
        //     return;
        // }

        // Create and configure the spawn timer
		GD.Print("START");
        spawnTimer = new Timer();
        spawnTimer.WaitTime = SpawnInterval;
        spawnTimer.OneShot = false;
        spawnTimer.Autostart = true;
        spawnTimer.Timeout += SpawnEnemy;
        AddChild(spawnTimer);
	}

    private void SpawnEnemy()
    {
        if (EnemyScene == null || spawnPath == null)
            return;

		GD.Print("Spawning enemys...");

        // Create a new enemy instance
        var enemyInstance = enemy.Instantiate<BasicEnemy>();
		enemyInstance.Position = new Vector2(0,0);
		AddChild(enemyInstance);

        // // Choose a random position along the path
        // var pathFollow = spawnPath.GetNode<PathFollow2D>("PathFollow2D");
        // pathFollow.HOffset = GD.Randf(); // Randomize the position along the path
        // enemyInstance.GlobalPosition = pathFollow.GlobalPosition;

        // // Add the enemy to the scene
        // GetTree().CurrentScene.AddChild(enemyInstance);
    }
}
