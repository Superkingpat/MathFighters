using Godot;
using System;

public partial class Pencil : Weapon {
	[Export] public float AttackRange = 300f;
	[Export] public float AttackWidth = 20f;
	[Export] public int PierceCount = 3;
	[Export] public float FireCooldown = 1.5f;
	[Export] public int ConsecutiveShots = 1;
	
	private Sprite2D weaponSprite;
	private float timeSinceLastShot = 0f;
	private float radius = 50f;
	private Vector2 lastMovementDirection = Vector2.Right;
	private int shotsFired = 0;
	private float timeBetweenShots = 0.1f;
	private float shotTimer = 0f;

	public override void _Ready() {
		base._Ready();
		weaponSprite = GetNode<Sprite2D>("Sprite2D");
		AttackAnimation = "attack_pencil";
	}

	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;
		shotTimer += (float)delta;
		Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		// Update direction based on player movement or nearest enemy
		if (inputDirection.LengthSquared() > 0) {
			lastMovementDirection = inputDirection.Normalized();
		}

		// Try to find nearest enemy if available
		Enemy nearestEnemy = FindNearestEnemy();
		if (nearestEnemy != null) {
			lastMovementDirection = (nearestEnemy.GlobalPosition - Player.Instance.GlobalPosition).Normalized();
		}

		weaponSprite.Rotation = lastMovementDirection.Angle() + (float)Math.PI / 2;
		weaponSprite.GlobalPosition = Player.Instance.GlobalPosition + lastMovementDirection * radius;
	}

	private Enemy FindNearestEnemy() {
		Enemy nearest = null;
		float nearestDistance = float.MaxValue;

		foreach (Node node in GetTree().CurrentScene.GetChildren()) {
			if (node is Enemy enemy) {
				float distance = Player.Instance.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearest = enemy;
				}
			}
		}

		return nearest;
	}

	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}

	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		// Apply weapon upgrades
		switch (WeaponLevel) {
			case 1:
				AttackRange = 300f;
				PierceCount = 3;
				ConsecutiveShots = 1;
				FireCooldown = 1.5f;
				break;
			case 2:
				AttackRange = 450f;
				PierceCount = 5;
				ConsecutiveShots = 2;
				FireCooldown = 1.2f;
				break;
			case 3:
				AttackRange = 600f;
				PierceCount = 7;
				ConsecutiveShots = 3;
				FireCooldown = 0.9f;
				break;
		}

		if (timeSinceLastShot < FireCooldown / attackSpeedAmp) return;
		if (shotsFired >= ConsecutiveShots && shotTimer < timeBetweenShots) return;

		if (shotsFired >= ConsecutiveShots) {
			// Reset after all consecutive shots are fired
			shotsFired = 0;
			timeSinceLastShot = 0f;
			return;
		}

		if (shotTimer >= timeBetweenShots) {
			shotTimer = 0f;
			shotsFired++;

			// Determine attack direction
			Vector2 attackDirection = lastMovementDirection;
			Enemy nearestEnemy = FindNearestEnemy();
			if (nearestEnemy != null) {
				attackDirection = (nearestEnemy.GlobalPosition - Player.Instance.GlobalPosition).Normalized();
			}

			if (AttackScene != null) {
				PencilAttack attack = (PencilAttack)AttackScene.Instantiate();
				GetTree().CurrentScene.AddChild(attack);
				attack.Init(
					Player.Instance.GlobalPosition + attackDirection * AttackRange,
					Player.Instance.GlobalPosition,
					WeaponLevel,
					attackDirection
				);
			}
		}
	}
}
