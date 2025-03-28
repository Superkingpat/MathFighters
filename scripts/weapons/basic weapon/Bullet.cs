using Godot;
using System;

public partial class Bullet : CharacterBody2D {
	[Export] public float Speed = 500.0f;
	private Vector2 direction;

	public void Init(Vector2 targetPosition, Vector2 startPosition) {
		Position = startPosition;
		direction = (targetPosition - startPosition).Normalized();
		Rotation = direction.Angle();
	}

	public override void _PhysicsProcess(double delta) {
		Velocity = direction * Speed;
		MoveAndSlide();
	}

	private void OnScreenExited() {
		QueueFree();
	}
}
