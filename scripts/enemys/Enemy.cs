using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : CharacterBody2D
{
	[Export] public string EnemyName { get; protected set; } = "Enemy";
	
	[Export] public float Damage { get; protected set; } = 3.0f;
	[Export] public float AttackCooldown { get; protected set; } = 1.0f;
	[Export] public float Speed { get; protected set; } = 40.0f;
	[Export] public float MaxHealth { get; protected set; } = 100.0f;
	[Export] public float Armor { get; protected set; } = 50.0f;
	
	// New properties for drops
	[Export] public PackedScene[] PossibleDrops { get; protected set; }
	[Export] public float[] DropChances { get; protected set; }
	[Export] public int MaxDrops { get; protected set; } = 3;
	[Export] public float CurrentHealth { get; protected set; }

	// Exp reward
	[Export] public int ExpReward { get; protected set; } = 10;
	
	protected bool isAttacking = false;
	protected bool isAggroed = false;
	protected bool isDead = false;
	protected bool isMoving = false;

	protected Player player;
	protected AnimatedSprite2D animatedSprite;
	protected Area2D detectionArea;
	protected CollisionShape2D detectionCollision;
	protected CollisionShape2D bodyCollision;
	protected bool isStunned = false;
	protected bool isSlowed = false;
	private Color? storedOriginalColor = null;
	private int tintStackCount = 0;

	
	protected Vector2 externalForce = Vector2.Zero;
	
	// List to store configured drops
	protected List<Drop> dropTable = new List<Drop>();
	
	// Random number generator
	protected RandomNumberGenerator rng = new RandomNumberGenerator();

	private Timer attackTimer;

	public override void _Ready()
	{
		rng.Randomize();

		// Access player
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null)
			GD.PrintErr("Player not found in group 'Player'");

		// Access sprite
		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite == null)
			GD.PrintErr("AnimatedSprite2D not found!");

		// Access detection area and its collision
		detectionArea = GetNodeOrNull<Area2D>("detection_area");
		if (detectionArea == null)
			GD.PrintErr("detection_area not found!");
		else
		{
			detectionCollision = detectionArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (detectionCollision == null)
				GD.PrintErr("Detection CollisionShape2D not found!");
		}

		// Access main body collision shape
		bodyCollision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (bodyCollision == null)
			GD.PrintErr("Body CollisionShape2D not found!");

		CurrentHealth = MaxHealth;
		
		AddToGroup("enemies");
		InitializeDropTable();

		detectionArea.BodyEntered += OnBodyEntered;
		detectionArea.BodyExited += OnBodyExited;

		// Setup attack timer
		attackTimer = new Timer();
		attackTimer.WaitTime = AttackCooldown;
		attackTimer.OneShot = false;
		attackTimer.Autostart = false;
		attackTimer.Timeout += Attack;

		AddChild(attackTimer);
	}

	private void OnBodyEntered(Node body)
	{
		if (player == body) // use groups to avoid hardcoding types
		{
			GD.Print("Player entered detection area!");
			isAggroed = true;
			attackTimer.Start(); // start attacking
		}
	}

	private void OnBodyExited(Node body)
	{
		if (player == body)
		{
			GD.Print("Player left detection area.");
			isAggroed = false;
			attackTimer.Stop();
			isAttacking = false;
		}
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
			if(isAggroed) isMoving = false;
			else isMoving = true;

			if(isMoving)
				MoveEnemy((float)delta);
		}
	}
	
	protected virtual void MoveEnemy(float delta)
	{
		if (isDead || player == null)
			return;

		if(isAggroed || (isAggroed && isAttacking))
			return;

		if(isStunned) return;

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
		// Position += direction * Speed * delta;
		// Position += (player.Position - Position)/Speed;
		float moveSpeed = isSlowed ? Speed * 0.7f : Speed;
		Vector2 moveVector = direction * moveSpeed + externalForce;
		Velocity = moveVector;
		MoveAndSlide();

		externalForce = externalForce.MoveToward(Vector2.Zero, 100f * delta);
	}	

	protected virtual void Attack()
	{
		if (isAggroed && player != null)
		{
			isAttacking = true;
			GD.Print("Attacking player...");
			player.TakeDamage(Damage);
		}
	}

	public virtual void TakeDamage(float amount)
	{
		CurrentHealth -= amount;
		GD.Print($"[Enemy {EnemyName}] Hit for {amount} damage");
		
		Tint(new Color(1, 0.3f, 0.3f), 0.1f);
		
		if (CurrentHealth <= 0)
		{
			Die();
		}
	}
	
	protected virtual void Die()
	{
		isDead = true;

		if (player != null && IsInstanceValid(player))
		{
			player.GainExperience(ExpReward);
		}
		
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
	
	public async void Stun(float dur)
	{
		if(isStunned) return;
		isStunned = true;
		await ToSignal(GetTree().CreateTimer(dur), "timeout");
		isStunned = false;
	}
	
	public async void Slow(float dur, float slowFactor = 0.7f)
	{
		if (isSlowed) return;
		isSlowed = true;
		GD.Print($"{EnemyName} is slowed by factor {slowFactor} for {dur} seconds");
		await ToSignal(GetTree().CreateTimer(dur), "timeout");
		isSlowed = false;
		GD.Print($"{EnemyName} is no longer slowed");
	}
	
	public void ApplyForce(Vector2 force)
	{
		externalForce += force;
	}
	
	public async void Tint(Color color, float duration)
	{
		if (animatedSprite == null) return;

		if (storedOriginalColor == null)
			storedOriginalColor = animatedSprite.Modulate;

		tintStackCount++;
		animatedSprite.Modulate = color;

		await ToSignal(GetTree().CreateTimer(duration), "timeout");

		tintStackCount--;
		if (tintStackCount <= 0 && IsInstanceValid(this) && storedOriginalColor.HasValue)
		{
			animatedSprite.Modulate = storedOriginalColor.Value;
			storedOriginalColor = null;
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
