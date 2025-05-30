using Godot;
using System;
using System.Collections.Generic;

public partial class PencilAttack : Attack {
	[Export] public float MaxLength = 300f;
	[Export] public float Width = 20f;
	[Export] public int PierceCount = 3;
	[Export] public float Lifetime = 0.5f;
	[Export] public int Damage = 10;
	
	private Line2D attackLine;
	private List<Enemy> piercedEnemies = new List<Enemy>();
	private Vector2 attackDirection;
	private float currentLength = 0f;
	private float timer = 0f;

	public override void _Ready() {
		base._Ready();
		attackLine = GetNode<Line2D>("Line2D");
		attackLine.Width = Width;
	}

	public void Init(Vector2 targetPosition, Vector2 startPosition, int weaponLevel, Vector2 direction) {
		base.Init(targetPosition, startPosition, weaponLevel);
		this.attackDirection = direction;
		
		// Apply weapon level upgrades
		switch (weaponLevel) {
			case 1:
				MaxLength = 300f;
				PierceCount = 3;
				Width = 20f;
				break;
			case 2:
				MaxLength = 450f;
				PierceCount = 5;
				Width = 25f;
				break;
			case 3:
				MaxLength = 600f;
				PierceCount = 7;
				Width = 30f;
				break;
		}
		
		// Set initial position and rotation
		GlobalPosition = startPosition;
		Rotation = direction.Angle();
	}

	public override void _PhysicsProcess(double delta) {
		if (currentLength < MaxLength) {
			// Extend the pencil attack
			currentLength += Speed * (float)delta;
			currentLength = Mathf.Min(currentLength, MaxLength);
			
			// Update the line points
			Vector2 startPoint = Vector2.Zero;
			Vector2 endPoint = attackDirection * currentLength;
			attackLine.Points = new Vector2[] { startPoint, endPoint };
			
			// Check for collisions along the line
			CheckLineCollisions();
		} else {
			// Attack has reached full length, wait a moment then disappear
			timer += (float)delta;
			if (timer >= Lifetime) {
				QueueFree();
			}
		}
	}

	private void CheckLineCollisions() {
		// Check for enemies along the attack line
		foreach (Node node in GetTree().CurrentScene.GetChildren()) {
			if (node is Enemy enemy && !piercedEnemies.Contains(enemy)) {
				// Check if enemy is within the attack line
				Vector2 enemyPos = enemy.GlobalPosition - GlobalPosition;
				float projection = enemyPos.Dot(attackDirection);
				
				if (projection >= 0 && projection <= currentLength) {
					// Check distance from line
					float distance = Mathf.Abs(enemyPos.Cross(attackDirection));
					if (distance <= Width / 2) {
						enemy.TakeDamage(Damage);
						piercedEnemies.Add(enemy);
						
						if (piercedEnemies.Count >= PierceCount) {
							QueueFree();
							return;
						}
					}
				}
			}
		}
	}

	protected override void PlayAttackSound() {
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlayShootSound("pencil");
		}
	}
}
