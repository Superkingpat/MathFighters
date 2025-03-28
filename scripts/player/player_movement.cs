using Godot;
using System;

public partial class player_movement : CharacterBody2D {
	[Export] public float Speed = 200.0f;
	private AnimatedSprite2D animatedSprite;
	private int lastDir = 0;
	[Export] public PackedScene BulletScene;

	public override void _Ready() {
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("move_up")) {
			velocity.Y -= 1;
			lastDir = 0;
		} else if (Input.IsActionPressed("move_down")) {
			velocity.Y += 1;
			lastDir = 1;
		} else if (Input.IsActionPressed("move_left")) {
			velocity.X -= 1;
			lastDir = 2;
		} else if (Input.IsActionPressed("move_right")) {
			velocity.X += 1;
			lastDir = 3;
		}

		if (velocity.Length() > 0) {
			velocity = velocity.Normalized() * Speed;
			UpdateAnimation(velocity);
		} else {
			PlayIdleAnimation();

		}

		Velocity = velocity;
		MoveAndSlide();

		if(Input.IsActionJustPressed("attack")) {
			Shoot();
		}
	}

	private void UpdateAnimation(Vector2 velocity) {
		if (velocity.Y < 0) {
			animatedSprite.Play("walk_up");
		} else if (velocity.Y > 0) {
			animatedSprite.Play("walk_down");
		} else if (velocity.X < 0) {
			animatedSprite.Play("walk_left");
		} else if (velocity.X > 0) {
			animatedSprite.Play("walk_right");
		}
	}

	private void PlayIdleAnimation() {
		if(lastDir == 0) {
			animatedSprite.Play("still_up");
		} else if(lastDir == 1) {
			animatedSprite.Play("still_down");
		} else if(lastDir == 2) {
			animatedSprite.Play("still_left");
		} else if(lastDir == 3) {
			animatedSprite.Play("still_right");
		} 

	}

	private void Shoot() {
		if (BulletScene == null) {
			return;
		}

		Bullet bullet = (Bullet)BulletScene.Instantiate();
		GetTree().CurrentScene.AddChild(bullet);

		bullet.Init(GetGlobalMousePosition(), GlobalPosition);
	}
}
