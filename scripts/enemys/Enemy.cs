using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Enemy : Node2D
{
	[Export] public string EnemyName { get; private set; } = "Enemy";
	[Export] public int Damage { get; private set; } = 10;
	[Export] public float AttackRange { get; private set; } = 50.0f;
	[Export] public float AttackCooldown { get; private set; } = 1.0f;
	[Export] public float DetectionRange { get; private set; } = 200.0f;
	[Export] public float AggroRange { get; private set; } = 300.0f;
	[Export] public float FleeRange { get; private set; } = 100.0f;
	[Export] public float Speed { get; private set; } = 200.0f;
	[Export] public float MaxHealth { get; private set; } = 100.0f;
	[Export] public float Armor { get; private set; } = 50.0f;
	
	// New properties for drops
	[Export] public PackedScene[] PossibleDrops { get; private set; }
	[Export] public float[] DropChances { get; private set; }
	[Export] public int MaxDrops { get; private set; } = 3;
	
	public float CurrentHealth { get; protected set; }
	protected Player player;
	protected AnimatedSprite2D animatedSprite;
	protected bool isAttacking = false;
	protected bool isFleeing = false;
	protected bool isAggroed = false;
	protected bool isDead = false;
	
	// List to store configured drops
	protected List<Drop> dropTable = new List<Drop>();
	
	// Random number generator
	protected RandomNumberGenerator rng = new RandomNumberGenerator();
	
	public override void _Ready()
	{
		rng.Randomize();
		
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null)
		{
			player = GetTree().Root.GetNode<Player>("World_1/Player");
		}
		
		CurrentHealth = MaxHealth;
		
		InitializeDropTable();
	}
	
	protected virtual void InitializeDropTable()
	{
		if (PossibleDrops != null && DropChances != null && PossibleDrops.Length == DropChances.Length)
		{
			for (int i = 0; i < PossibleDrops.Length; i++)
			{
				if (PossibleDrops[i] != null)
				{
					dropTable.Add(new Drop($"Item{i}", PossibleDrops[i], DropChances[i]));
				}
			}
		}
	}

	protected void AddDrop(string itemName, PackedScene itemScene, float dropChance)
	{
		dropTable.Add(new Drop(itemName, itemScene, dropChance));
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (player != null && IsInstanceValid(player))
		{
			MoveEnemy((float)delta);
		}
	}
	
	protected virtual void MoveEnemy(float delta)
	{
		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		Position += direction * Speed * delta;
	}
	
	public virtual void TakeDamage(int amount)
	{
		CurrentHealth -= amount;
		if (CurrentHealth <= 0)
		{
			Die();
		}
	}
	
	protected virtual void Die()
	{
		isDead = true;
		
		DropItems();
		
		QueueFree();
	}
	
	protected virtual void DropItems()
	{
		if (dropTable.Count == 0)
			return;
			
		int dropsCount = 0;
		
		foreach (var drop in dropTable)
		{
			float roll = rng.Randf();
			if (roll <= drop.DropChance && dropsCount < MaxDrops)
			{
				if (drop.ItemScene != null)
				{
					Node2D item = drop.ItemScene.Instantiate<Node2D>();
					GetTree().CurrentScene.AddChild(item);
					
					float offsetX = rng.RandfRange(-20, 20);
					float offsetY = rng.RandfRange(-20, 20);
					item.GlobalPosition = GlobalPosition + new Vector2(offsetX, offsetY);
					
					dropsCount++;
				}
			}
		}
	}
}

public class Drop
{
	public string ItemName { get; set; }
	public PackedScene ItemScene { get; set; }
	public float DropChance { get; set; } 
	
	public Drop(string itemName, PackedScene itemScene, float dropChance)
	{
		ItemName = itemName;
		ItemScene = itemScene;
		DropChance = Mathf.Clamp(dropChance, 0.0f, 1.0f);
	}
}
