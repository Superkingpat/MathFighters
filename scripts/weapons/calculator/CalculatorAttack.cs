using Godot;
using System;

public partial class CalculatorAttack : Attack {
	[Export] public float MaxExpandScale = 5.0f;
	[Export] public float ExpandedTime = 1.0f;

	private float timer = 0f;
	private Vector2 originalScale;
	private CollisionShape2D collisionShape;
	private Shape2D originalCollisionShape;
	private float expandScale = 1.0f;

	public override void _Ready() {
		base._Ready();
		originalScale = Scale;
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		if (collisionShape != null) {
			originalCollisionShape = collisionShape.Shape;
		} else {
			GD.PrintErr("CollisionShape2D not found!");
		}
	}

	public void Init(Vector2 targetPosition, Vector2 startPosition, int weaponLevel, double currChargeTime) {
		GD.Print("CALC ATTACK IS INIT");
		base.Init(targetPosition, startPosition, weaponLevel);

		expandScale = Mathf.Lerp(1.0f, MaxExpandScale, (float)(currChargeTime / 0.5f));
		Scale = originalScale * expandScale;

		if (collisionShape != null && originalCollisionShape is CircleShape2D circleShape) {
			CircleShape2D newShape = new CircleShape2D();
			newShape.Radius = circleShape.Radius * expandScale;
			collisionShape.Shape = newShape;
		}

		PlayAttackSound();
	}

	public override void _PhysicsProcess(double delta) {
		timer += (float)delta;

		if (timer >= ExpandedTime) {
			QueueFree();
		}
	}

	protected override void PlayAttackSound() {
		AudioManager.Instance?.PlayShootSound("pen");
	}
}
