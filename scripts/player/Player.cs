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
	public static Player Instance { get; private set; }
	private Area2D screenBounds;

	// Leveling system variables
	private int level = 1;
	private int experience = 0;
	private int experienceToLevelUp = 100;
	private float levelUpExperienceMultiplier = 1.5f;

	 // Signals for level up and experience change
	[Signal]
	public delegate void LevelUpEventHandler(int newLevel);

	[Signal]
	public delegate void ExperienceChangedEventHandler(int currentExperience, int experienceNeeded);

	public int Level { get { return level; } private set { level = value; } }
	public int Experience { get { return experience; } private set { experience = value; EmitSignal(SignalName.ExperienceChanged, experience, experienceToLevelUp); } }
	public int ExperienceToLevelUp { get { return experienceToLevelUp; } private set { experienceToLevelUp = value; EmitSignal(SignalName.ExperienceChanged, experience, experienceToLevelUp); } }

	
	//With GetNode we get the instance of the AnimatedSprite2D that was addet in the Godot UI
	//_Ready is called when the root node (Player) entered the scene
	public override void _Ready() {
		//The AnimatedSprite2D handles animations
		AddToGroup("player"); //da ga lagka iz chunkov/spavnerjov najlaze najdemo GetTree().GetNodesInGroup("player")[0] as Player
		GetNode<Spawner>("/root/Spawner").InitPlayer();
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		weaponHolder = GetNode<Node2D>("WeaponHolder");
		Instance = this;

		// Init UI elements for XP and Level
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
			currentWeapon?.TryShoot(GetGlobalMousePosition());
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

	public void GainExperience(int amount) {
		if (amount <= 0) return;

		Experience += amount;
		CheckLevelUp();
	}

	private void CheckLevelUp() {
		while (Experience >= ExperienceToLevelUp) {
			Experience -= ExperienceToLevelUp;
			Level++;
			ExperienceToLevelUp = (int)(ExperienceToLevelUp * levelUpExperienceMultiplier); // Lets make this non-linear in the future
			GD.Print($"Player leveled up! New level: {Level}");
			EmitSignal(SignalName.LevelUp, Level);

			// Show level up screen to choose a perk
		}
	}

	// Function to get current level (can be accessed from other scripts)
	public int GetLevel() {
		return Level;
	}

	// Function to get the current experience (can be accessed from other scripts)
	public int GetExperience() {
		return Experience;
	}

	// Function to get the experience needed to level up
	public int GetExperienceToLevelUp() {
		return ExperienceToLevelUp;
	}
}
