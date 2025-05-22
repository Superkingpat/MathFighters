using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D {
	public class Stats {
		// Public properties for direct access and modification
		public float BaseHealth { get; set; } = 100f; // Base part of health
		public int S_Health { get; set; } = 2;       // Skill part of health (e.g., 2 * S_Health)
		public float CurrentHealth { get; private set; } // Current health, set internally
		public float DamageMod { get; set; } = 1f;    // General damage multiplier (e.g., 1.1 for +10%)
		public float Speed { get; set; } = 200.0f;    // Player movement speed
		public float RangeMod { get; set; } = 1f;     // Weapon range multiplier (if applicable)

		private int _gold;
		public int Gold
		{
			get => _gold;
			set
			{
				if (_gold != value)
				{
					_gold = value;
					GoldChanged?.Invoke();
				}
			}
		}

		public event Action GoldChanged;

		// Constructor to initialize CurrentHealth
		public Stats()
		{
			UpdateCurrentHealthToMax();
		}

		// Calculates the maximum health based on BaseHealth and S_Health
		public float GetMaxHealth()
		{
			return BaseHealth + (2 * S_Health);
		}

		// Call this after changing BaseHealth or S_Health to update CurrentHealth
		public void UpdateCurrentHealthToMax()
		{
			float maxHealth = GetMaxHealth();
			// Ensure current health doesn't exceed new max, or set to max if previously 0
			CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
			if (CurrentHealth <= 0.01f) // If dead or just starting, set to max
			{
				CurrentHealth = maxHealth;
			}
		}

		public void TakeDamage(float amount) {
			CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
		}

		public void Heal(float amount) {
			CurrentHealth = Mathf.Min(CurrentHealth + amount, GetMaxHealth());
		}

		// --- New methods for upgrading stats ---

		public void UpgradeDamage(float amount) {
			DamageMod += amount;
			GD.Print($"Player DamageMod increased to: {DamageMod}");
		}

		public void UpgradeSpeed(float amount) {
			Speed += amount;
			GD.Print($"Player Speed increased to: {Speed}");
		}

		// Increases max health by modifying BaseHealth and S_Health
		public void UpgradeMaxHealth(float baseHealthIncrease, int sHealthIncrease) {
			BaseHealth += baseHealthIncrease;
			S_Health += sHealthIncrease;
			UpdateCurrentHealthToMax(); // Recalculate max health and adjust current health
			GD.Print($"Player Max Health increased. BaseHealth: {BaseHealth}, S_Health: {S_Health}. New Max: {GetMaxHealth()}. Current: {CurrentHealth}");
		}
	}

	public Stats PlayerStats { get; private set; } = new Stats();
	private AnimatedSprite2D animatedSprite;
	private int lastDir = 0;

	public float AttackSpeedAmp=1;
	private Weapon currentWeapon;
	private Node2D weaponHolder;
	public static Player Instance { get; private set; } // Static instance for easy access
	private Area2D screenBounds;
	private AudioStreamPlayer2D shootingSound;
	private AudioStreamPlayer2D weaponPickupSound;
	private AudioStreamPlayer2D walkingSound;

	// Removed redundant MaxHealth and CurrentHealth as they are now in PlayerStats
	// Removed int Damage = 1; as DamageMod in PlayerStats should handle this

	private List<Weapon> weaponInventory = new List<Weapon>();
	private int currentWeaponIndex = 0;


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


	public override void _Ready()
	{
		AddToGroup("player");
		GetNode<Spawner>("/root/Spawner").InitPlayer();
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		weaponHolder = GetNode<Node2D>("WeaponHolder");
		shootingSound = GetNode<AudioStreamPlayer2D>("ShootingSound");
		weaponPickupSound = GetNode<AudioStreamPlayer2D>("WeaponPickUpSound");
		walkingSound = GetNode<AudioStreamPlayer2D>("WalkingSound");
		Instance = this; // Set the static instance here

		// Ensure PlayerStats.CurrentHealth is correctly set at start
		PlayerStats.UpdateCurrentHealthToMax();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
		{
			if (eventKey.Keycode == Key.Z)
			{
				GainExperience(150);
				GD.Print("Added 150 EXP by pressing Z");
			}
		}
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
			// Use PlayerStats.Speed
			velocity = velocity.Normalized() * PlayerStats.Speed;
			UpdateAnimation(velocity);
		} else {
			PlayIdleAnimation();

		}

		Velocity = velocity;
		MoveAndSlide();

		if(Input.IsActionJustPressed("attack")) {
			GD.Print("Shooting");
			// Pass PlayerStats.DamageMod to weapon if it affects damage
			currentWeapon?.TryShoot(GetGlobalMousePosition(), AttackSpeedAmp); // You might want to pass PlayerStats.DamageMod here
		}

		if (Input.IsActionJustPressed("next_weapon")) {
			currentWeaponIndex = (currentWeaponIndex + 1) % weaponInventory.Count;
			EquipWeapon(currentWeaponIndex);
		} else if (Input.IsActionJustPressed("previous_weapon")) {
			currentWeaponIndex = (currentWeaponIndex - 1 + weaponInventory.Count) % weaponInventory.Count;
			EquipWeapon(currentWeaponIndex);
		}
	}
	private void PlayFootstepSound() {
		if (walkingSound != null && walkingSound.Stream != null) {
			if (!walkingSound.Playing) {
				walkingSound.Play();
			}
		} else {
			GD.PrintErr("WalkingSound node not properly configured");
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

		GD.Print("Trying to pick up weapon...");

		Weapon newWeapon = pickup.GetWeapon();
		Type newWeaponType = newWeapon.GetType();

		foreach (Weapon existingWeapon in weaponInventory) {
			if (existingWeapon.GetType() == newWeaponType) {
				GD.Print("Already have weapon of type " + newWeaponType.Name + ", leveling up.");
				existingWeapon.LevelUpWeapon();
				newWeapon.QueueFree();
				pickup.QueueFree();
				return;
			}
		}

		GD.Print("Picked up new weapon of type " + newWeaponType.Name);
		weaponHolder.AddChild(newWeapon);
		newWeapon.Position = Vector2.Zero;

		weaponInventory.Add(newWeapon);
		currentWeaponIndex = weaponInventory.Count - 1;
		EquipWeapon(currentWeaponIndex);
		pickup.QueueFree();
		string weaponName = newWeapon.Name.ToString().ToLower();
		string soundPath = "res://assets/audio/weapon_default.wav"; // fallback

		if (weaponName.Contains("geotriangle")) {
			soundPath = "res://assets/audio/weapon_geotriangle.wav";
		} else if (weaponName.Contains("pen")) {
			soundPath = "res://assets/audio/weapon_pen.wav";
		}

		var stream = GD.Load<AudioStream>(soundPath);
		weaponPickupSound.Stream = stream;
		weaponPickupSound.Play();
	}

	private void Die()
	{
		GD.Print("Player died!");
		// Disable movement, play animation, trigger game over, etc.
		QueueFree();
	}

	public void TakeDamage(float dmg) {
		PlayerStats.TakeDamage(dmg); // Use PlayerStats method
		GD.Print("Player took " + dmg + " damage. HP left: " + PlayerStats.CurrentHealth);
		if(PlayerStats.CurrentHealth <= 0) {
			GD.Print("Player is dead!");
			Die();
		}
	}

	public void Heal(float heal) {
		PlayerStats.Heal(heal); // Use PlayerStats method
		GD.Print("Player has been healed! Current health: " + PlayerStats.CurrentHealth);
	}

	public void GainExperience(int amount) {
		if (amount <= 0) return;

		Experience += amount;
		GD.Print($"Gained {amount} EXP! Total: {Experience}");
		CheckLevelUp();
	}

	private void CheckLevelUp() {
		while (Experience >= ExperienceToLevelUp) {
			Experience -= ExperienceToLevelUp;
			Level++;
			ExperienceToLevelUp = (int)(ExperienceToLevelUp * levelUpExperienceMultiplier); // Lets make this non-linear in the future
			GD.Print($"Player leveled up! New level: {Level}");
			EmitSignal(SignalName.LevelUp, Level);
		}
	}

	public int GetLevel() {
		return Level;
	}

	public int GetExperience() {
		return Experience;
	}

	public int GetExperienceToLevelUp() {
		return ExperienceToLevelUp;
	}
}
