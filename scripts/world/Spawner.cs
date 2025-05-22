using Godot;
using System;
using System.Collections.Generic;

public partial class Spawner : Node
{
	private static Vector2 range = new Vector2(1056/3, 1056/3);
	private static Random rnd = new Random();
	
	[Export] public PackedScene ItemScene;
	
	private static List<Item> _list = new List<Item>();
	private static List<CharacterBody2D> _list_e= new List<CharacterBody2D>();
	private static PackedScene EnemyScene = ResourceLoader.Load<PackedScene>("res://scenes/enemys/enemy.tscn");
	// Singleton pattern
	public static Spawner Instance { get; private set; }
	
	public override void _EnterTree()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			GD.PrintErr("Multiple instances of Spawner detected!");
		}
	}
	
	public override void _Ready()
	{
		if (ItemScene == null)
			ItemScene = ResourceLoader.Load<PackedScene>("res://scenes/item.tscn");
		GD.Print("Spawner ready");
	}
	
	public static void Test()
	{
		GD.Print("Accessible");
	}
	
	private static Vector2 GetRandPosition(Vector2 position)
	{
		return position+ new Vector2(rnd.Next((int)-range[0], (int)range[0]), rnd.Next((int)-range[1], (int)range[1]));
	}
	
	public static void Reset()
	{
		foreach (var item in _list)
		{
			item.QueueFree();
		}
		_list.Clear();
		
		
		foreach (var enemy in _list_e)
		{
			enemy.QueueFree();
		}
		_list_e.Clear();
	}
	
	private static void SpawnItem(Vector2 position, String path, Action function)
	{
		//GD.Print(position);
		var newItem = Instance.ItemScene.Instantiate<Item>();
		_list.Add(newItem);
		Instance.GetTree().CurrentScene.AddChild(newItem);
		newItem.Initialize(0, position, GD.Load<Texture2D>(path), function);
	}
	
	private static void SpawnEnemy(Vector2 position)
	{
		var enemyInstance = EnemyScene.Instantiate<CharacterBody2D>();
		_list_e.Add(enemyInstance);
		enemyInstance.GlobalPosition = GetRandPosition(position); 
		Instance.GetTree().CurrentScene.AddChild(enemyInstance);
		GD.Print("Spawned enemy at: " + enemyInstance.GlobalPosition);
	}
	
	public static void Spawn(Vector2 position)
	{
		// Ensure we have instances of required resources
		if (Instance == null || ChunkManager.Instance == null || ChunkManager.Instance.Player == null)
		{
			GD.PrintErr("Cannot spawn items: Missing required references");
			return;
		}

		float p_ItemSpawn = 100; // Item spawn chance set to 100% for testing

		SpawnItem(
						GetRandPosition(position),
						"res://assets/items/Icon_Coin.png",
						() => { GetGold(100); }
					);


		if (rnd.Next(100) < p_ItemSpawn)
		{
			int r = rnd.Next(10);
			switch (r)
			{
				case 0:
				case 1:
				case 2:
					SpawnItem(
						GetRandPosition(position),
						"res://assets/items/Icon_DamageUp.png",
						() => { DamageUp(20, 2); }
					);
					break;
				case 3:
				case 4:
				case 5:
					SpawnItem(
						GetRandPosition(position),
						"res://assets/items/Icon_Heal.png",
						() => { Heal(50); }
					);
					break;
				case 6:
				case 7:
				case 8:
					for (int i = 0; i < 30; i++)
					{
						SpawnEnemy(position);
					}
					break;
				case 9:
					SpawnItem(
						GetRandPosition(position),
						"res://assets/items/Icon_EnergyDrink.png",
						() => { SpeedUp(10, 2); }
					);
					break;
			}
		}
	}
	
	// Item effect methods
	private static async void SpeedUp(int duration, int multiplier)
	{
		var player = ChunkManager.Instance.Player;
		player.PlayerStats.Speed *= multiplier;
		player.AttackSpeedAmp *= 2;
		
		await Instance.ToSignal(Instance.GetTree().CreateTimer(duration), "timeout");
		
		player.PlayerStats.Speed /= multiplier;
		player.AttackSpeedAmp /= 2;
	}
	
	private static async void DamageUp(int duration, int multiplier)
	{
		var player = ChunkManager.Instance.Player;
		player.PlayerStats.DamageMod += multiplier;
		
		await Instance.ToSignal(Instance.GetTree().CreateTimer(duration), "timeout");
		
		player.PlayerStats.DamageMod -= multiplier;
	}
	
	private static void Heal(int amount)
	{
		var player = ChunkManager.Instance.Player;
		player.PlayerStats.CurrentHealth += amount;
	}

	private static void GetGold(int amount)
	{
		var player = ChunkManager.Instance.Player;
		player.PlayerStats.Gold += amount;
	}



}
