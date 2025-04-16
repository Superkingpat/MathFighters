using Godot;
using System;
using System.Collections.Generic;


public partial class Chunk : Node2D
{
	[Export] public PackedScene ZoneScene;
	[Export] public Vector2 ChunkPosition { get; set; }
	[Export] public Vector2 ChunkSize { get; set; } =new Vector2(1920,1080); // Default size
	public Player Player;  // Reference to player
	private Rect2 rect2;
	private bool []playerIsIn=[false,false];
	
	
	private static List<Chunk> _list =new List<Chunk>();
	private static Vector2[] _pos_list = new Vector2[] {
		new Vector2(-1,-1),new Vector2(0,-1),new Vector2(1,-1),
		new Vector2(-1,0),                   new Vector2(1,0),
		new Vector2(-1,1), new Vector2(0,1), new Vector2(1,1)};
		
		
	public void Initialize(Vector2 chunkPosition,Vector2 chunkSize)
	{
		GD.Print($"making chunk at {chunkPosition}");
		Position = ChunkPosition = chunkPosition;
		ZIndex=-1;
		ChunkSize=chunkSize;
		rect2=new Rect2(chunkPosition-chunkSize/2 , chunkSize);
		GD.Print($"chunk created");
	}
	
	public override void _Ready()
	{
		Player = GetNode<Player>("../Player");
		_list.Add(this);
		SpawnZone();
	}
	
	public override void _Process(double delta)
	{
		if(Player==null)
			throw new NullReferenceException("Player is null");
		
		//preverja ko pridemo v chunk
		playerIsIn[0]=IsPlayerInChunk();
		if(playerIsIn[0]!=playerIsIn[1]&&playerIsIn[0]){
			GD.Print("Player entered");
			LoadChunks(); //spawna druge
		}
		playerIsIn[1]=playerIsIn[0];
			

		if (!IsChunkNearPlayer())
		{
			_list.Remove(this);
			QueueFree(); //odstrani chunk iz scene
			GD.Print("Deleted chunk...");
		}
			
		
	}
	
	private bool IsPlayerInChunk()
	{
		return rect2.HasPoint(Player.GlobalPosition);
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
				newChunk.ZoneScene = this.ZoneScene;
				newChunk.Initialize(newPos, ChunkSize);
				GetParent().AddChild(newChunk);  // Important: Add to scene tree
			}
		}
		
		GD.Print(_list.Count);
	}
	
	private bool IsChunkNearPlayer()
	{
		return ChunkPosition.DistanceTo(Player.GlobalPosition) <= 4000;
	}
	
	private void SpawnZone()
	{
		if(ZoneScene == null){
			GD.Print("=>Error: Zone Scene is null");
			return;
		}
		var zone = ZoneScene.Instantiate<Area2D>();
		zone.Position = new Vector2(GD.Randf() * ChunkSize.X, GD.Randf() * ChunkSize.Y);
		AddChild(zone);
		GD.Print("Spawned zone");
	}
	
}
