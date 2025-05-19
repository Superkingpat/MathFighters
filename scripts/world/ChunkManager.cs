using Godot;
using System.Collections.Generic;

public partial class ChunkManager : Node
{
	// Resources
	[Export] public PackedScene ZoneScene { get; set; }
	[Export] public PackedScene ChunkScene { get; set; }
	[Export] public Vector2 ChunkSize { get; set; } = new Vector2(1920, 1080);
	
	// References
	public Player Player { get; set; }
	public static ChunkManager Instance { get; private set; }
	
	// State
	public List<Chunk> Chunks { get; private set; } = new List<Chunk>();
	
	public override void _EnterTree()
	{
		// Set up singleton pattern
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			GD.PrintErr("Multiple instances of ChunkManager detected!");
		}
	}
	
	public override void _Ready()
	{
		// Load resources if not exported
		if (ChunkScene == null)
			ChunkScene = ResourceLoader.Load<PackedScene>("res://scenes/sample_chunk.tscn");
		
		if (ZoneScene == null)
			ZoneScene = ResourceLoader.Load<PackedScene>("res://scenes/ZoneHandler.tscn");
		
		// Wait until we have a Player reference before initializing chunks
		GetTree().CreateTimer(0.1).Timeout += InitializeFirstChunk;
	}
	
	public void InitializeFirstChunk()
	{
		// Find player if not assigned
		if (Player == null)
		{
			var players = GetTree().GetNodesInGroup("player");
			if (players.Count > 0)
				Player = players[0] as Player;
			else
			{
				GD.PrintErr("No player found! Make sure your player is in the 'player' group.");
				return;
			}
		}
		
		// Create initial chunk
		CreateChunk(new Vector2(0, 0));
	}
	
	public Chunk CreateChunk(Vector2 position)
	{
		var newChunk = ChunkScene.Instantiate<Chunk>();
		newChunk.Initialize(position, ChunkSize, Player);
		GetTree().CurrentScene.AddChild(newChunk);
		return newChunk;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// Run spawner test if needed
		// Spawner.Test();
	}
	
	public void Reset()
	{
		foreach (var chunk in Chunks)
		{
			chunk.QueueFree();
		}
		Chunks.Clear();
		
		// Recreate initial chunk
		InitializeFirstChunk();
	}
}
