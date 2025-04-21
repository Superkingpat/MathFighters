using Godot;

public partial class ChunkManager : Node
{
	public static PackedScene ZoneScene;
	public static PackedScene ChunkScene;
	
	[Export] public Vector2 ChunkSize { get; set; } =new Vector2(1920,1080); // Default size

	public override void _Ready()
	{
		ChunkScene = ResourceLoader.Load<PackedScene>("res://scenes/sample_chunk.tscn");
		ZoneScene = ResourceLoader.Load<PackedScene>("res://scenes/ZoneHandler.tscn");
		
		//GD.Print("Happened");
		var newChunk = ChunkScene.Instantiate<Chunk>();
		AddChild(newChunk);
		newChunk.ZoneScene = ZoneScene;
		newChunk.Initialize(new Vector2(0,0), ChunkSize);

		

		



	}
}
