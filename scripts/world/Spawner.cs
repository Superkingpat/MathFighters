using Godot;
using System;



public partial class Spawner : Node
{
	Random rnd = new Random();
	public static PackedScene ItemScene;
	private Player Player;
	
	public override void _Ready(){
		ItemScene = ResourceLoader.Load<PackedScene>("res://scenes/item.tscn");
		
		GD.Print("spawner ready");
	}

	public void Test(){
		GD.Print("Accesable");
	}
	
	public void InitPlayer(){
		Player = GetTree().GetNodesInGroup("player")[0] as Player;
		GD.Print(Player.GlobalPosition);
	}



	public void Spawn(Vector2 position){ //ta funkcija bo skrbela za vse spawne pa chance
		float p_ItemSpawn=0.2f;
		p_ItemSpawn=1;
		if(rnd.Next(100)<p_ItemSpawn*100){
			var newItem=ItemScene.Instantiate<Item>();
			AddChild(newItem);
			newItem.Initialize(0,position + new Vector2(rnd.Next(-480,480),rnd.Next(-270,270)),GD.Load<Texture2D>("res://assets/items/Icon_EnergyDrink.png"),
			async () => {
				Player.Speed *= 2;
				await ToSignal(GetTree().CreateTimer(2.0), "timeout");
				Player.Speed /= 2;
			});
			}
	}
}
