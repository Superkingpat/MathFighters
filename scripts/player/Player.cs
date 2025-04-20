using Godot;
using System;

//This is the player class.
public partial class Player : CharacterBody2D {
	//Export makes it so we can interact with the variable in the Godot UI
	[Export] public float Speed = 200.0f;
	private AnimatedSprite2D animatedSprite;
	private int lastDir = 0;
	private Weapon currentWeapon;
	private Node2D weaponHolder;
	//With GetNode we get the instance of the AnimatedSprite2D that was addet in the Godot UI
	//_Ready is called when the root node (Player) entered the scene
	public override void _Ready() {
		//The AnimatedSprite2D handles animations
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		weaponHolder = GetNode<Node2D>("WeaponHolder");
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
			GD.Print("Shooting");
			currentWeapon?.TryShoot(GetGlobalMousePosition(), GlobalPosition);
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

	public void TryPickupWeapon(WeaponPickUp pickup) {
		if (pickup == null) return;

		GD.Print("Picked up new weapon!");
		// Remove old weapon
		if (currentWeapon != null)
		{
			currentWeapon.QueueFree();
		}
		// Get the new weapon from pickup
		Weapon newWeapon = pickup.GetWeapon();

		// Add it to the weapon holder
		weaponHolder.AddChild(newWeapon);
		newWeapon.Position = Vector2.Zero; // Reset local position
		GD.Print("WeaponHolder children: ", weaponHolder.GetChildCount());
		currentWeapon = newWeapon;
		// Remove pickup from scene
		pickup.QueueFree();
	}

	//If a scene is bound to the external BulletScene variable we create a new instance of Bullet,
	//position it at the player characters position and send it in the direction of the mouse cursor
}
