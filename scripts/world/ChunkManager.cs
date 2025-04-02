using Godot;

public partial class ChunkManager : Node
{
	public static PackedScene ChunkScene;

	public override void _Ready()
	{
		ChunkScene = ResourceLoader.Load<PackedScene>("res://scenes/sample_chunk.tscn");
	}
}
