using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Enemy : Node2D
{
	[Export] public string EnemyName { get; protected set; } = "Enemy";
	
	[Export] public float Damage { get; protected set; } = 10.0f;
	[Export] public float AttackRange { get; protected set; } = 10.0f;
	[Export] public float AggroRange { get; protected set; } = 25.0f;
	[Export] public float AttackCooldown { get; protected set; } = 1.0f;
	
	[Export] public float Speed { get; protected set; } = 40.0f;
	[Export] public float MaxHealth { get; protected set; } = 100.0f;
	[Export] public float Armor { get; protected set; } = 50.0f;
	
	// New properties for drops
	[Export] public PackedScene[] PossibleDrops { get; protected set; }
	[Export] public float[] DropChances { get; protected set; }
	[Export] public int MaxDrops { get; protected set; } = 3;
	
	public float CurrentHealth { get; protected set; }
	protected Player player;
	
	protected AnimatedSprite2D animatedSprite;
	
	protected bool isAttacking = false;
	protected bool isAggroed = false;
	protected bool isDead = false;
	protected bool isMoving = false;
	
	// List to store configured drops
	protected List<Drop> dropTable = new List<Drop>();
	
	// Random number generator
	protected RandomNumberGenerator rng = new RandomNumberGenerator();
	
	public override void _Ready()
	{
		rng.Randomize();
		
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		
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
			if(isAttacking) isMoving = false;
			else isMoving = true;

			if(isMoving)
				MoveEnemy((float)delta);
		}
	}
	
	protected virtual void MoveEnemy(float delta)
	{
		if (isDead || player == null)
			return;

		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();

		// Cast a ray in the direction of movement to check for obstacles
		Vector2 rayOrigin = GlobalPosition;
		Vector2 rayEnd = rayOrigin + direction * 20f; // Adjust length as needed

		// Use Godot's direct space state to perform the raycast
		var spaceState = GetWorld2D().DirectSpaceState;
		var k = PhysicsRayQueryParameters2D.Create(rayOrigin, rayEnd);
		var result = GetWorld2D().DirectSpaceState.IntersectRay(k);

		// if (result.Count > 0)
		// {
		// 	// Obstacle detected - try to go around it by changing direction
		// 	// Simple avoidance: try rotating the direction slightly
		// 	float avoidAngle = Mathf.Pi / 4; // 45 degrees

		// 	// Try to go around to the right first
		// 	Vector2 altDirection = direction.Rotated(avoidAngle);
		// 	rayEnd = rayOrigin + altDirection * 20f;
		// 	k = PhysicsRayQueryParameters2D.Create(rayOrigin, rayEnd);
		// 	result = GetWorld2D().DirectSpaceState.IntersectRay(k);

		// 	if (result.Count == 0)
		// 	{
		// 		direction = altDirection;
		// 	}
		// 	else
		// 	{
		// 		// Try to go around to the left
		// 		altDirection = direction.Rotated(-avoidAngle);
		// 		rayEnd = rayOrigin + altDirection * 20f;
		// 		k = PhysicsRayQueryParameters2D.Create(rayOrigin, rayEnd);
		// 		result = GetWorld2D().DirectSpaceState.IntersectRay(k);

		// 		if (result.Count == 0)
		// 		{
		// 			direction = altDirection;
		// 		}
		// 		else
		// 		{
		// 			// If both directions are blocked, stop movement
		// 			return;
		// 		}
		// 	}
		// }

		// Move in the (possibly adjusted) direction
		Position += direction * Speed * delta;
		// Position += (player.Position - Position)/Speed;
	}	

	protected virtual void Attack()
	{
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		
		if (distanceToPlayer <= AggroRange)
		{
			isAggroed = true;
			
			if (distanceToPlayer <= AttackRange && !isAttacking)
			{
				isAttacking = true;
				GD.Print("Enemy attacking player");

				GetTree().CreateTimer(AttackCooldown).Timeout += () =>
				{
					isAttacking = false;
				};
			}
		}
		else
		{
			isAggroed = false;
		}

		// You'll need to add a TakeDamage method to your Player class
		// player.TakeDamage(Damage);
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
