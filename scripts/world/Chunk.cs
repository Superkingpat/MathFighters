using Godot;
using System;
using System.Collections.Generic;

public partial class Chunk : Node2D
{
	[Export] public PackedScene ZoneScene;
	[Export] public Vector2 ChunkSize;
	private Rect2 rect2;
	private bool[] playerIsIn = [false, false];
	
	private static readonly Vector2[] _pos_list = new Vector2[] {
		new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1),
		new Vector2(-1, 0),                      new Vector2(1, 0),
		new Vector2(-1, 1),  new Vector2(0, 1),  new Vector2(1, 1)
	};
	
	public void Initialize(Vector2 chunkPosition, Vector2 chunkSize, Player player)
	{
		GD.Print(chunkPosition);
		this.Position = chunkPosition;
		ZIndex = -100;
		ChunkSize = chunkSize;
		rect2 = new Rect2(chunkPosition - chunkSize / 2, chunkSize);
	}
	
	public override void _Ready()
	{
		// Add to ChunkManager's list
		ChunkManager.Instance.Chunks.Add(this);
		
		// Spawn items in this chunk
		Spawner.Spawn(Position);
		GD.Print(Position);
		// Create zone
		SpawnZone();
	}
	
	public override void _Process(double delta)
	{
		// Check if player is null
		if (ChunkManager.Instance.Player == null)
			return;
		
		// Check if player entered the chunk
		playerIsIn[0] = IsPlayerInChunk();
		if (playerIsIn[0] != playerIsIn[1] && playerIsIn[0])
		{
			LoadChunks(); // Generate surrounding chunks
		}
		playerIsIn[1] = playerIsIn[0];
		
		// Check if chunk should be destroyed
		if (!IsChunkNearPlayer())
		{
			ChunkManager.Instance.Chunks.Remove(this);
			QueueFree();
			GD.Print("Deleted chunk...");
		}
	}
	
	private bool IsPlayerInChunk()
	{
		return rect2.HasPoint(ChunkManager.Instance.Player.GlobalPosition);
	}
	
	private void LoadChunks()
	{
		foreach (var offset in _pos_list)
		{
			Vector2 newPos = Position + offset * ChunkSize;
			bool exists = ChunkManager.Instance.Chunks.Exists(c => c.Position == newPos);
			
			if (!exists)
			{
				ChunkManager.Instance.CreateChunk(newPos);
			}
		}
	}
	
	private bool IsChunkNearPlayer()
	{
		return Position.DistanceTo(ChunkManager.Instance.Player.GlobalPosition) <= 4000;
	}
	
	private void SpawnZone()
	{
		if (ZoneScene == null)
		{
			// Try to get the zone scene from ChunkManager
			ZoneScene = ChunkManager.Instance.ZoneScene;
			
			if (ZoneScene == null)
			{
				GD.Print("=>Error: Zone Scene is null");
				return;
			}
		}
		
		var zone = ZoneScene.Instantiate<Area2D>();
		
		Vector2 localOffset = new Vector2(GD.Randf() * ChunkSize.X, GD.Randf() * ChunkSize.Y);
		Vector2 globalPos = GlobalPosition - ChunkSize / 2 + localOffset;
		
		// Add to root scene to avoid limitations of being a child of the chunk
		GetTree().Root.AddChild(zone);
		zone.GlobalPosition = globalPos;
		
		//GD.Print("Spawned zone at global position: ", globalPos);
	}
}
