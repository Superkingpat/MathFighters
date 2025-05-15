using Godot;
using System;

public partial class Spawner : Node
{
	private Vector2 range= new Vector2(1920/3,1080/3);
	Random rnd = new Random();
	public static PackedScene ItemScene;
	private Player Player;
	
	private PackedScene EnemyScene = ResourceLoader.Load<PackedScene>("res://scenes/enemys/enemy.tscn");

	
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

	private Vector2 getRandPosition(Vector2 position){
		return  position +  new Vector2(rnd.Next((int)-range[0],(int)range[0]),rnd.Next((int)-range[1],(int)range[1]));
	}
	
	private void SpawnEnemy(Vector2 position)
	{
		var enemyInstance = EnemyScene.Instantiate<CharacterBody2D>();
		enemyInstance.GlobalPosition = getRandPosition(position); 
		GetTree().CurrentScene.AddChild(enemyInstance);
		GD.Print("Spawned enemy at: " + enemyInstance.GlobalPosition);
	}

	

	private void spawnItem(Vector2 position,String path,Action function){
		var newItem=ItemScene.Instantiate<Item>();
			AddChild(newItem);
			newItem.Initialize(0,position,GD.Load<Texture2D>(path),function);
	}
	

	public void Spawn(Vector2 position){ //ta funkcija bo skrbela za vse spawne pa chance
		float p_ItemSpawn=20f;
		p_ItemSpawn=100;
		if(rnd.Next(100)<p_ItemSpawn){
			int r=rnd.Next(10);
			switch (r){
				case 0:
				case 1:
				case 2:
					spawnItem(
						getRandPosition(position),
						"res://assets/items/Icon_DamageUp.png",
						()=>{_damageUp(2,2);}
					);
					break;
				case 3: 
				case 4:
				case 5:
					spawnItem(
						getRandPosition(position),
						"res://assets/items/Icon_Heal.png",
						()=>{_heal(50);}
					);
					break;
				case 6:
				case 7:
				case 8:
					for (int i = 0; i < 30; i++)
					{
						SpawnEnemy(getRandPosition(position));
					}
					break;
				case 9:
					spawnItem(
						getRandPosition(position),
						"res://assets/items/Icon_EnergyDrink.png",
						()=>{_speedUp(2,2);}
					);
					break;
				}
			}
	}
	//Definirani efekti razlicnih itemov
	private async void _speedUp(int duration, int multiplier){
		Player.Speed *= multiplier;
		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		Player.Speed /= multiplier;
	}
	
	private async void _damageUp(int duration, int multiplier){
		GD.Print("Damage up not implemented yet!");
		//ko bo player dobil dmg
		//Player.Speed *= multiplier;
		//await ToSignal(GetTree().CreateTimer(duration), "timeout");
		//Player.Speed /= multiplier;
	}
	private async void _heal(int amount){
		GD.Print("Heal not implemented yet!");
		//dodaj ko player dobi health 
	}



}
