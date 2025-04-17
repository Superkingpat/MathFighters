using Godot;
using System;

//This is the player class.
public partial class Player : CharacterBody2D {
	//Export makes it so we can interact with the variable in the Godot UI
	[Export] public float Speed = 200.0f;
	[Export] public PackedScene BulletScene;
	private AnimatedSprite2D animatedSprite;
	private int lastDir = 0;
	
	private Area2D screenBounds;
	

	//With GetNode we get the instance of the AnimatedSprite2D that was addet in the Godot UI
	//_Ready is called when the root node (Player) entered the scene
	public override void _Ready() {
		//The AnimatedSprite2D handles animations
		AddToGroup("player"); //da ga lagka iz chunkov/spavnerjov najlaze najdemo GetTree().GetNodesInGroup("player")[0] as Player
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	//_PhysicsProcess updates the physics engine and animations in the background. MoveAndSlide() is used here, which means we don't need to care about delta time, it's handled automaticly
	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Vector2.Zero;

		//All Input.IsActionPressed are bound in the Godot UI under Project > Project Settings > Input Map
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

	//UpdateAnimation and PlayIdleAnimation handle the animations. Since AnimatedSprite2D::Play() is used here the animation speed is taken care of automaticly by Godot
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

	//If a scene is bound to the external BulletScene variable we create a new instance of Bullet,
	//position it at the player characters position and send it in the direction of the mouse cursor
	private void Shoot() {
		if (BulletScene == null) {
			return;
		}

		Bullet bullet = (Bullet)BulletScene.Instantiate();
		GetTree().CurrentScene.AddChild(bullet);

		bullet.Init(GetGlobalMousePosition(), GlobalPosition);
	}
}
