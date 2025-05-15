using Godot;
using System;

public partial class SpawnEnemys : Node
{

	[Export] public bool ShouldSpawnEnemys = true;
	[Export] public NodePath SpawnAreaPath; // Path to the Path2D or PathFollow2D node
	private Area2D spawnArea;
	private CollisionShape2D spawnCollision;
	[Export] public float SpawnInterval = 2.0f; // Time interval between spawns
	[Export] public float ActivationDelay = 2.0f;

	private PackedScene enemy= ResourceLoader.Load<PackedScene>("res://scenes/Enemys/enemy.tscn");

	private Path2D spawnPath;
	private Timer spawnTimer;
	
	enemyInstance.AddToGroup("enemies");

	public override void _Ready()
	{
		if(ShouldSpawnEnemys)
		{
			// Get spawn area
			spawnArea = GetNodeOrNull<Area2D>(SpawnAreaPath);
			if (spawnArea == null)
			{
				GD.PrintErr("ERROR: spawnArea is null! Is the path assigned?");
				return;
			}

			// Get collision shape
			spawnCollision = spawnArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (spawnCollision == null)
			{
				GD.PrintErr("ERROR: spawnCollision is null! Is there a CollisionShape2D child?");
				return;
			}

			// Check shape type
			if (spawnCollision.Shape is not RectangleShape2D)
			{
				GD.PrintErr("ERROR: spawnCollision.Shape is not a RectangleShape2D!");
				return;
			}

			if (enemy == null)
			{
				GD.PrintErr("ERROR: Enemy scene not loaded! Check the path: res://scenes/Enemys/enemy.tscn");
				return;
			}

			// Set up timer
			spawnTimer = new Timer();
			spawnTimer.WaitTime = SpawnInterval;
			spawnTimer.OneShot = false;
			spawnTimer.Autostart = true;
			spawnTimer.Timeout += SpawnEnemy;
			AddChild(spawnTimer);
		}
	}

	private void SpawnEnemy()
	{
		if (spawnArea == null || spawnCollision == null || enemy == null)
		{
			GD.PrintErr("ERROR: Cannot spawn enemy. Ensure spawnArea, spawnCollision, and enemy are properly initialized.");
			return;
		}
	
		GD.Print("Spawning enemies...");
	
		var rectShape = (RectangleShape2D)spawnCollision.Shape;
		Vector2 extents = rectShape.Size * 0.5f;
	
		// Generate a random position near the edges of the rectangle
		float edgeOffset = 10.0f; // Distance from the edge
		float x, y;

		// Randomly decide whether to spawn along the horizontal or vertical edge
		if (GD.Randf() < 0.5f)
		{
			// Spawn along the top or bottom edge
			x = (float)GD.RandRange(-extents.X, extents.X);
			y = GD.Randf() < 0.5f ? -extents.Y + edgeOffset : extents.Y - edgeOffset;
		}
		else
		{
			// Spawn along the left or right edge
			x = GD.Randf() < 0.5f ? -extents.X + edgeOffset : extents.X - edgeOffset;
			y = (float)GD.RandRange(-extents.Y, extents.Y);
		}

		Vector2 localSpawnPos = new Vector2(x, y);
	
		// Convert local to global position
		Vector2 globalSpawnPos = spawnArea.GlobalPosition + localSpawnPos;
	
		// Spawn enemy
		var enemyInstance = (CharacterBody2D)enemy.Instantiate();


		GD.Print("Spawned instance type: " + enemyInstance.GetType().Name);
		enemyInstance.GlobalPosition = globalSpawnPos;
		GetTree().CurrentScene.AddChild(enemyInstance);
	}
}
