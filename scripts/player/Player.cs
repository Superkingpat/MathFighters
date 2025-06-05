using Godot;
using System;
using System.Collections.Generic;

//This is the player class.
public partial class Player : CharacterBody2D 
{
	//Export makes it so we can interact with the variable in the Godot UI

	public class Stats {
		// Public properties for direct access and modification
		public float BaseHealth { get; set; } = 100f; // Base part of health
		public int S_Health { get; set; } = 2;       // Skill part of health (e.g., 2 * S_Health)
		public float CurrentHealth { get; set; } // Current health, set internally
		public float DamageMod { get; set; } = 1f;    // General damage multiplier (e.g., 1.1 for +10%)
		public float Speed { get; set; } = 200.0f;    // Player movement speed
		public float RangeMod { get; set; } = 1f;     // Weapon range multiplier (if applicable)

		public float CurrentHealth
		{
			get=>_currentHealth;
			set
			{
				_currentHealth = value;
				if (_currentHealth > BaseHealth)
					_currentHealth = BaseHealth;
			}
		} // Current health, set internally


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

	public float AttackSpeedAmp = 1;
	private Weapon currentWeapon;
	private Node2D weaponHolder;
	public static Player Instance { get; private set; } // Static instance for easy access
	private Area2D screenBounds;
	private AudioStreamPlayer2D shootingSound;
	private AudioStreamPlayer2D weaponPickupSound;
	private AudioStreamPlayer2D walkingSound;
	private AudioStreamPlayer2D losingLifeSound;
	private AudioStreamPlayer2D winningSound;
	private float kickRange = 100f; 
	private float kickDamage = 25f; 
	private float kickCooldown = 0.5f; 
	private float lastKickTime = 0f; 
	[Export] public float MaxHealth = 100f;
	public float CurrentHealth { get; private set; }
	

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
	private bool walkingSoundErrorShown = false;
	public override void _Ready()
	{
		//The AnimatedSprite2D handles animations
		AddToGroup("player"); //da ga lagka iz chunkov/spavnerjov najlaze najdemo GetTree().GetNodesInGroup("player")[0] as Player
		ChunkManager.Instance.Player = this;
		//GetNode<Spawner>("/root/Spawner").InitPlayer();
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		weaponHolder = GetNode<Node2D>("WeaponHolder");
		shootingSound = GetNode<AudioStreamPlayer2D>("ShootingSound");
		weaponPickupSound = GetNode<AudioStreamPlayer2D>("WeaponPickUpSound");

				try { walkingSound = GetNode<AudioStreamPlayer2D>("WalkingSound"); GD.Print("WalkingSound node found!"); } 
		catch { GD.PrintErr("WalkingSound node not found!"); }
		
		try { winningSound = GetNode<AudioStreamPlayer2D>("WinningSound"); GD.Print("WinningSound node found!"); } 
		catch { GD.PrintErr("WinningSound node not found!"); }
		
		if (winningSound != null)
		{
			string[] winningSoundPaths = {
				"res://assets/audio/winning.wav"
			};
			
			bool winningSoundLoaded = false;
			foreach (string path in winningSoundPaths)
			{
				try 
				{
					var audioStream = GD.Load<AudioStream>(path);
					if (audioStream != null)
					{
						winningSound.Stream = audioStream;
						GD.Print($"WinningSound loaded successfully from: {path}");
						winningSoundLoaded = true;
						break;
					}
				}
				catch (Exception e)
				{
				}
			}
			
			if (!winningSoundLoaded)
			{
				GD.PrintErr("Could not load winning sound from any of the tried paths!");
			}
		}
		
		losingLifeSound = GetNodeOrNull<AudioStreamPlayer2D>("LosingLifeSound");
		if (losingLifeSound != null)
		{
			GD.Print("LosingLifeSound node found successfully!");
			
			string[] possiblePaths = {
				"res://assets/audio/losing_life.wav"
			};
			
			bool streamLoaded = false;
			foreach (string path in possiblePaths)
			{
				try 
				{
					var audioStream = GD.Load<AudioStream>(path);
					if (audioStream != null)
					{
						losingLifeSound.Stream = audioStream;
						GD.Print($"LosingLifeSound loaded successfully from: {path}");
						
						GD.Print($"Audio stream details - Length: {audioStream.GetLength()}, Valid: {audioStream != null}");
						
						streamLoaded = true;
						break;
					}
					else
					{
						GD.Print($"Failed to load audio from {path} - stream was null");
					}
				}
				catch (Exception e)
				{
					GD.Print($"Could not load audio from {path}: {e.Message}");
				}
			}
			
			if (!streamLoaded)
			{
				GD.PrintErr("Could not load losing life audio file from any of the tried paths!");
			}
		}
		else
		{
			GD.PrintErr("CRITICAL: LosingLifeSound node not found!");
			GD.PrintErr("Please add an AudioStreamPlayer2D node named 'LosingLifeSound' as a child of the Player node.");
		}
		Instance = this; 
		
		// Load walking sound
		if (walkingSound != null)
		{
			string[] walkingSoundPaths = {
				"res://assets/audio/walking.wav"
			};
			
			bool walkingSoundLoaded = false;
			foreach (string path in walkingSoundPaths)
			{
				try 
				{
					var audioStream = GD.Load<AudioStream>(path);
					if (audioStream != null)
					{
						walkingSound.Stream = audioStream;
						GD.Print($"WalkingSound loaded successfully from: {path}");
						walkingSoundLoaded = true;
						break;
					}
				}
				catch (Exception e)
				{
				}
			}
			
			if (!walkingSoundLoaded)
			{
				GD.PrintErr("Could not load walking sound from any of the tried paths!");
			}
		}
		
		GD.Print("=== Listing all AudioStreamPlayer2D children ===");
		foreach (Node child in GetChildren())
		{
			if (child is AudioStreamPlayer2D)
			{
				GD.Print($"Found AudioStreamPlayer2D: {child.Name}");
			}
		}
		GD.Print("=== End of AudioStreamPlayer2D list ===");
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

	//_PhysicsProcess updates the physics engine and animations in the background. MoveAndSlide() is used here, which means we don't need to care about delta time, it's handled automaticly
	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Vector2.Zero;

		//All Input.IsActionPressed are bound in the Godot UI under Project > Project Settings > Input Map
		if (Input.IsActionPressed("move_up")) {
			velocity.Y -= 1;
			lastDir = 0;
		}
		else if (Input.IsActionPressed("move_down"))
		{
			velocity.Y += 1;
			lastDir = 1;
		}
		else if (Input.IsActionPressed("move_left"))
		{
			velocity.X -= 1;
			lastDir = 2;
		}
		else if (Input.IsActionPressed("move_right"))
		{
			velocity.X += 1;
			lastDir = 3;
		}

		if (velocity.Length() > 0) {
			// Use PlayerStats.Speed
			velocity = velocity.Normalized() * PlayerStats.Speed;
			UpdateAnimation(velocity);
			PlayFootstepSound(); // Play walking sound when moving
		}
		else
		{
			PlayIdleAnimation();
			// Stop walking sound when not moving
			if (walkingSound != null && walkingSound.Playing)
			{
				walkingSound.Stop();
			}

		}

		Velocity = velocity;
		MoveAndSlide();

		if (Input.IsActionJustPressed("attack"))
		{
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
				GD.Print("Playing walking sound...");
				walkingSound.Play();
			}
		} else {
			if (!walkingSoundErrorShown)
			{
				if (walkingSound == null)
				{
					GD.PrintErr("WalkingSound node is null");
				}
				else if (walkingSound.Stream == null)
				{
					GD.PrintErr("WalkingSound stream is null - no audio file loaded");
				}
				walkingSoundErrorShown = true;
			}
		}
	}
	//za winning sound samo
	public void PlayWinningSound()
	{
		GD.Print("=== ATTEMPTING TO PLAY WINNING SOUND ===");
		
		if (winningSound == null)
		{
			GD.PrintErr("WinningSound node is NULL!");
			winningSound = GetNodeOrNull<AudioStreamPlayer2D>("WinningSound");
			if (winningSound == null)
			{
				GD.PrintErr("Could not find WinningSound node in scene tree!");
				return;
			}
			else
			{
				GD.Print("Found WinningSound node on retry!");
			}
		}
		
		if (winningSound.Stream == null)
		{
			GD.PrintErr("WinningSound stream is NULL! Attempting to load winning.wav...");
			
			// Try to load the winning.wav file we can see in your project
			try
			{
				var audioStream = GD.Load<AudioStream>("res://assets/audio/winning.wav");
				if (audioStream != null)
				{
					winningSound.Stream = audioStream;
					GD.Print("Successfully loaded winning.wav!");
				}
				else
				{
					GD.PrintErr("Failed to load winning.wav - file exists but returned null");
					return;
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"Exception loading winning.wav: {e.Message}");
				return;
			}
		}
		
		try
		{
			GD.Print("Playing winning sound...");
			
			// Stop any currently playing sound
			if (winningSound.Playing)
			{
				winningSound.Stop();
			}
			
			winningSound.VolumeDb = 0.0f;  // Full volume
			winningSound.PitchScale = 1.0f; // Normal pitch
			
			// Play the sound
			winningSound.Play();
			//AudioLogHelper.LogSoundPlayed("WinningSound");

			GD.Print($"Sound playing status: {winningSound.Playing}");
			GD.Print($"Sound stream length: {winningSound.Stream?.GetLength() ?? 0}");
			GD.Print($"Sound volume: {winningSound.VolumeDb} dB");
			
			if (!winningSound.Playing)
			{
				GD.PrintErr("Sound is not playing even after Play() call!");
				
				winningSound.Play(0.0f);
				GD.Print($"Retry play status: {winningSound.Playing}");
			}
			else
			{
				GD.Print("Winning sound is successfully playing!");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Exception when trying to play winning sound: {e.Message}");
			GD.PrintErr($"Stack trace: {e.StackTrace}");
		}
		
		GD.Print("=== END WINNING SOUND ATTEMPT ===");
	}

	public void ForcePlayWinningSound()
	{
		PlayWinningSound();
	}

	private void CheckLevelUp() {
		if (Experience >= ExperienceToLevelUp) {
			Experience -= ExperienceToLevelUp;
			Level++;
			ExperienceToLevelUp = (int)(ExperienceToLevelUp * levelUpExperienceMultiplier);
			GD.Print($"Player leveled up! New level: {Level}");
			
			PlayWinningSound();
			
			GetTree().CreateTimer(0.1).Timeout += () => {
				EmitSignal(SignalName.LevelUp, Level);
			};
			
			if (Experience >= ExperienceToLevelUp) {
				GetTree().CreateTimer(0.5).Timeout += CheckLevelUp;
			}
		}
	}

	//UpdateAnimation and PlayIdleAnimation handle the animations. Since AnimatedSprite2D::Play() is used here the animation speed is taken care of automaticly by Godot
	private void UpdateAnimation(Vector2 velocity)
	{
		if (velocity.Y < 0)
		{
			animatedSprite.Play("walk_down2");
		} else if (velocity.Y > 0) {
			animatedSprite.Play("walk_down2");
		} else if (velocity.X < 0) {
			animatedSprite.Play("walk_left2");
		} else if (velocity.X > 0) {
			animatedSprite.Play("walk_right2");
		}
	}

	private void PlayIdleAnimation()
	{
		if (lastDir == 0)
		{
			animatedSprite.Play("still_down2");
		} else if(lastDir == 1) {
			animatedSprite.Play("still_down2");
		} else if(lastDir == 2) {
			animatedSprite.Play("still_left2");
		} else if(lastDir == 3) {
			animatedSprite.Play("still_right2");
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
		PlayLosingLifeSound();

		// Disable movement, play animation, trigger game over, etc.
		ResetMap();
		//QueueFree();a
		this.PlayerStats.UpdateCurrentHealthToMax();
		this.Position = new Vector2(0, 0);
	}

	private void PlayLosingLifeSound()
	{
		if (losingLifeSound != null)
		{
			if (losingLifeSound.Stream != null)
			{
				try
				{
					GD.Print("Player died - playing losing life sound...");
					
					if (losingLifeSound.Playing)
					{
						losingLifeSound.Stop();
					}
					
					losingLifeSound.Play();
					//AudioLogHelper.LogSoundPlayed("LosingLifeSound");

					
					if (losingLifeSound.Playing)
					{
						GD.Print("Losing life sound is now playing!");
					}
					else
					{
						GD.PrintErr("Sound failed to play - Playing property is still false");
					}
				}
				catch (Exception e)
				{
					GD.PrintErr($"Exception when trying to play losing life sound: {e.Message}");
				}
			}
			else
			{
				GD.PrintErr("LosingLifeSound stream is null - audio file not loaded properly");
			}
		}
		else
		{
			GD.PrintErr("LosingLifeSound node is null - node not found in scene tree");
		}
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

	public void ResetMap(){
		ChunkManager.Instance.Reset();
		Spawner.Reset();
	}

	//If a scene is bound to the external BulletScene variable we create a new instance of Bullet,
	//position it at the player characters position and send it in the direction of the mouse cursor
	public void GainExperience(int amount) {
		if (amount <= 0) return;

		Experience += amount;
		GD.Print($"Gained {amount} EXP! Total: {Experience}");
		CheckLevelUp();
	}

	/*private void CheckLevelUp() {
		while (Experience >= ExperienceToLevelUp) {
			Experience -= ExperienceToLevelUp;
			Level++;
			ExperienceToLevelUp = (int)(ExperienceToLevelUp * levelUpExperienceMultiplier); // Lets make this non-linear in the future
			GD.Print($"Player leveled up! New level: {Level}");
			EmitSignal(SignalName.LevelUp, Level);
		}
	}*/

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


/*public static class AudioLogHelper
{
	//treba popravek
	private static readonly string LogFilePath = "user://audio_log.txt"; 
	private static readonly string EncryptionKey = "secureKey";

	public static void LogSoundPlayed(string soundName)
	{
		string message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Sound played: {soundName}";
		string encrypted = Encrypt(message);

		string globalPath = ProjectSettings.GlobalizePath(LogFilePath);
		
		string directory = Path.GetDirectoryName(globalPath);// problem ne se zapisva 
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		//za preverka
		//GD.Print("Zapisano v: " + ProjectSettings.GlobalizePath(LogFilePath));

		File.AppendAllText(globalPath, encrypted + System.Environment.NewLine);
	}

	private static string Encrypt(string input)
	{
		var output = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			char encryptedChar = (char)(input[i] ^ EncryptionKey[i % EncryptionKey.Length]);
			output.Append(encryptedChar);
		}
		return output.ToString();
	}

	public static string Decrypt(string encrypted)
	{
		return Encrypt(encrypted); 
	}
}*/
