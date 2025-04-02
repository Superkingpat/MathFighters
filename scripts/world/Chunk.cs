using Godot;
using System;
using System.Collections.Generic;

public partial class Chunk : Node2D
{
	[Export] public Vector2 ChunkPosition { get; set; }
	[Export] public Vector2 ChunkSize { get; set; } =new Vector2(1920,1080); // Default size
	public Player Player;  // Reference to player
	
	
	private static List<Chunk> _list =new List<Chunk>();
	private static Vector2[] _pos_list = new Vector2[] {
		new Vector2(-1,-1),new Vector2(0,-1),new Vector2(1,-1),
		new Vector2(-1,0),                   new Vector2(1,0),
		new Vector2(-1,1), new Vector2(0,1), new Vector2(1,1)};
		
		
	public void Initialize(Vector2 chunkPosition,Vector2 chunkSize,Player player = null)
	{
		GD.Print($"making chunk at {chunkPosition}");
		Position = ChunkPosition = chunkPosition;
		ZIndex=-1;
		ChunkSize=chunkSize;
		this.Player = player;
		GD.Print("chunk created");
	}
	
	public override void _Ready()
	{
		Player = GetNode<Player>("../Player");
		
		GD.Print("Chunk generated!");
		_list.Add(this);
	}
	
	public override void _Process(double delta)
	{
		// Your chunk-specific logic here
		if (Player != null)
		{
			// Example: Check if player is inside this chunk
			if (IsPlayerInChunk())
			{
				//GD.Print("in chunk");
				HandlePlayerInChunk();
			}
		}
	}
	
	private bool IsPlayerInChunk()
	{
		// Implement your chunk boundary checking here
		return GetRect().HasPoint(Player.GlobalPosition);
	}
	
	private bool entered=false;
	private void HandlePlayerInChunk()
	{
		if (entered==false){
			entered=true;
			GD.Print("player entered chunk!");
			LoadChunks();
			
		}
	}
	
	private Rect2 GetRect()
	{
		// Return the chunk's area as a Rect2
		// Adjust based on your chunk's actual size
		return new Rect2(GlobalPosition, ChunkSize);
	}
		
	private void LoadChunks()
	{
		foreach (var offset in _pos_list)
		{
			Vector2 newPos = ChunkPosition + offset * ChunkSize;
			bool exists = _list.Exists(c => c.ChunkPosition == newPos);
			if (!exists)
			{
				var newChunk = ChunkManager.ChunkScene.Instantiate<Chunk>();
				newChunk.Initialize(newPos, ChunkSize, Player);
				GetParent().AddChild(newChunk);  // Important: Add to scene tree
			}
		}
		
		GD.Print("4");
	}
	
	
}
