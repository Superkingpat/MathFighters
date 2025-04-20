using Godot;
using System;

public partial class PenAttack : Attack {
	[Export] public float ExpandScale = 5.0f;
	[Export] public float TravelTime = 1.0f;
	[Export] public float ExpandedTime = 1.0f;

	private float timer = 0f;
	private Vector2 originalScale;
	private bool hasExpanded = false;
	private bool hasStopped = false;
	private CollisionShape2D collisionShape;
	private Shape2D originalCollisionShape;
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

	public override void _PhysicsProcess(double delta) {
		timer += (float)delta;

		if (!hasStopped && timer >= TravelTime) {
			hasStopped = true;
			Speed = 0f;
			timer = 0f;
		}

		if (hasStopped && !hasExpanded) {
			Scale = originalScale * ExpandScale;

			if (collisionShape != null && originalCollisionShape != null) {
				if (originalCollisionShape is CircleShape2D circleShape) {
					var newShape = new CircleShape2D();
					newShape.Radius = circleShape.Radius * ExpandScale;
					collisionShape.Shape = newShape;
				}
			}

			hasExpanded = true;
		} else if (hasExpanded && timer >= ExpandedTime) {
			QueueFree();
			GD.Print("Pen bullet was deleted!");
		}

		if (!hasStopped) {
			base._PhysicsProcess(delta);
		}
	}
}
