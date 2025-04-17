using Godot;
using System;



public partial class Item : Area2D
{
	[Export] public Vector2 ItemSize { get; set; } =new Vector2(50,50); // Default size
	[Export] private Sprite2D sprite;
	[Export] private CollisionShape2D collisionShape;
	//position ze ma by default or smthin
	public Player Player; 

	public void Initialize(int itemID,Vector2 itemposition,Texture2D icon) //irtemid se bo uporabu za izbiro ka item dela
	{
		//GD.Print($"Item at{itemposition}");
		Position  = itemposition;

		sprite.Texture=icon;
		var rect2d =new RectangleShape2D();
		rect2d.Size=ItemSize;
		collisionShape.Shape=rect2d;


		ZIndex=-1;
		//rect2=new Rect2(itemposition-chunkSize/2 , chunkSize);
		//GD.Print($"Item created");
	}
	
	public override void _Ready()
	{
		Player = GetTree().GetNodesInGroup("player")[0] as Player;
		BodyEntered += PlayerContact;
		//_list.Add(this);
	}

	private void PlayerContact(Node body){
		if(body==Player){
			GD.Print("Collected");
			
			QueueFree();
		}
	}

	
}
