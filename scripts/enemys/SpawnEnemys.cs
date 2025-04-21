using Godot;
using System;

public partial class SpawnEnemys : Node
{
	// Drag and drop your enemy scene into this field in the editor
	[Export]
	public PackedScene EnemyScene;

	// You can also expose a position for spawning (optional)
	[Export]
	public Vector2 SpawnPosition = new Vector2(100, 100);

	public override void _Ready()
	{
		// You can call it here to test
		SpawnEnemy();
	}

	public void SpawnEnemy()
	{
		if (EnemyScene == null)
		{
			GD.PrintErr("EnemyScene is not assigned!");
			return;
		}

		/*
		Enemy enemyInstance = (Enemy)EnemyScene.Instance();
		enemyInstance.Position = SpawnPosition;
		AddChild(enemyInstance);
		*/
	}
}
