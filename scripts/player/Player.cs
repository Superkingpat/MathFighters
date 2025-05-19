using Godot;
using System;
using System.Collections.Generic;

//This is the player class.
public partial class Player : CharacterBody2D {
	//Export makes it so we can interact with the variable in the Godot UI

	public class Stats {
		public float BaseHealth { get; set; } = 100f;
		public float CurrentHealth { get; set; }
		public int S_Health { get; set; } = 2;

		public Stats() {
			CurrentHealth = BaseHealth + 2*S_Health;
		}

		public void TakeDamage(float amount) {
			CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
		}

		public void Heal(float amount) {
			CurrentHealth = Mathf.Min(CurrentHealth + amount, BaseHealth + 2*S_Health);
		}
	}

	public Stats PlayerStats { get; private set; } = new Stats();

	[Export] public float Speed = 200.0f;
	private AnimatedSprite2D animatedSprite;
	private int lastDir = 0;

	public float AttackSpeedAmp=1;
	private Weapon currentWeapon;
	private Node2D weaponHolder;
	public static Player Instance { get; private set; }
	private Area2D screenBounds;
	[Export] public float MaxHealth = 100f;
	public float CurrentHealth { get; private set; }
	

	public int Damage=1;

	private List<Weapon> weaponInventory = new List<Weapon>();
	private int currentWeaponIndex = 0;

	//With GetNode we get the instance of the AnimatedSprite2D that was addet in the Godot UI
	//_Ready is called when the root node (Player) entered the scene
	public override void _Ready() {
		//The AnimatedSprite2D handles animations
		AddToGroup("player"); //da ga lagka iz chunkov/spavnerjov najlaze najdemo GetTree().GetNodesInGroup("player")[0] as Player
		GetNode<Spawner>("/root/Spawner").InitPlayer();
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		weaponHolder = GetNode<Node2D>("WeaponHolder");
		Instance = this;
		CurrentHealth = MaxHealth;
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
			currentWeapon?.TryShoot(GetGlobalMousePosition(),AttackSpeedAmp);
		}

		if (Input.IsActionJustPressed("next_weapon")) {
			currentWeaponIndex = (currentWeaponIndex + 1) % weaponInventory.Count;
			EquipWeapon(currentWeaponIndex);
		} else if (Input.IsActionJustPressed("previous_weapon")) {
			currentWeaponIndex = (currentWeaponIndex - 1 + weaponInventory.Count) % weaponInventory.Count;
			EquipWeapon(currentWeaponIndex);
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

	private void EquipWeapon(int index) {
		if (weaponInventory.Count == 0) return;

		foreach (var weapon in weaponInventory) {
			weapon.Visible = false;
		}

		currentWeaponIndex = index % weaponInventory.Count;
		weaponInventory[currentWeaponIndex].Visible = true;
		currentWeapon = weaponInventory[currentWeaponIndex];
	}

	public void TryPickupWeapon(WeaponPickUp pickup) {
		if (pickup == null) return;

		GD.Print("Picked up new weapon!");

		Weapon newWeapon = pickup.GetWeapon();
		weaponHolder.AddChild(newWeapon);
		newWeapon.Position = Vector2.Zero;

		weaponInventory.Add(newWeapon);
		currentWeaponIndex = weaponInventory.Count - 1;
		EquipWeapon(currentWeaponIndex);

		pickup.QueueFree();
	}
	
	public void TakeDamage(float amount)
	{
		CurrentHealth -= amount;
		GD.Print("Player took " + amount + " damage. HP left: " + CurrentHealth);

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Player died!");
		// Disable movement, play animation, trigger game over, etc.
		QueueFree();
	}

	public void TakeDamage(float dmg) {
		PlayerStats.TakeDamage(dmg);

		if(PlayerStats.CurrentHealth <= 0) {
			//GD.Print("Player is dead!");
		}
	}

	public void Heal(float heal) {
		PlayerStats.Heal(heal);

		GD.Print("Player has been healed! Current health: " + PlayerStats.CurrentHealth);
	}

	//If a scene is bound to the external BulletScene variable we create a new instance of Bullet,
	//position it at the player characters position and send it in the direction of the mouse cursor
}
