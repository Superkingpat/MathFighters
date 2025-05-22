using Godot;


public partial class LevelUpScreen : Control {
	[Export] public Button DamageUpgradeButton { get; set; }
	[Export] public Button SpeedUpgradeButton { get; set; }
	[Export] public Button HealthUpgradeButton { get; set; }

	public override void _Ready() {
		Visible = false;

		ConnectButtonSignals(DamageUpgradeButton, nameof(_on_DamageUpgradeButton_pressed), "DamageUpgradeButton");
		ConnectButtonSignals(SpeedUpgradeButton, nameof(_on_SpeedUpgradeButton_pressed), "SpeedUpgradeButton");
		ConnectButtonSignals(HealthUpgradeButton, nameof(_on_HealthUpgradeButton_pressed), "HealthUpgradeButton");

		Player player = GetNode<Player>("/root/World_1/Player");

		if (player != null) {
			
			if (!player.IsConnected("LevelUp", new Callable(this, nameof(OnPlayerLevelUp))))
			{
				player.Connect("LevelUp", new Callable(this, nameof(OnPlayerLevelUp)));
			}
			
		}
	}

	private void OnPlayerLevelUp(int newLevel) {
		GD.Print($"LevelUpScreen: Received LevelUp signal! Player leveled up to level {newLevel}!");
		Visible = true;
		GetTree().Paused = true;
	}

	private void ConnectButtonSignals(Button button, string methodName, string buttonNameForDebug)
	{
		if (button != null)
		{
			if (!button.IsConnected("pressed", new Callable(this, methodName)))
			{
				button.Connect("pressed", new Callable(this, methodName));
				GD.Print($"DEBUG: {buttonNameForDebug} signal connected via code.");
			}
			else
			{
				GD.Print($"DEBUG: {buttonNameForDebug} signal already connected.");
			}
		}
		else
		{
			GD.PrintErr($"DEBUG ERROR: {buttonNameForDebug} is null! Make sure it's linked in the Inspector for LevelUpScreen.");
		}
	}


	private void _on_DamageUpgradeButton_pressed()
	{ 
		GD.Print("Damage upgrade button pressed!");
		if (Player.Instance != null)
		{
			Player.Instance.PlayerStats.UpgradeDamage(0.1f); // Example: +10% damage multiplier
			GD.Print("Damage upgrade selected and applied!");
			ContinueGame();
		}
		else
		{
			GD.PrintErr("Player instance not found for damage upgrade!");
		}
	} 


	private void _on_SpeedUpgradeButton_pressed() 
	{
		if (Player.Instance != null)
		{
			Player.Instance.PlayerStats.UpgradeSpeed(50f); // Example: +50 speed
			GD.Print("Speed upgrade selected and applied!");
			ContinueGame();
		}
		else
		{
			GD.PrintErr("Player instance not found for speed upgrade!");
		}
	}

	private void _on_HealthUpgradeButton_pressed() 
	{
		if (Player.Instance != null)
		{
			Player.Instance.PlayerStats.UpgradeMaxHealth(20f, 0);;
			GD.Print("Health upgrade selected and applied!");
			ContinueGame();
		}
		else
		{
			GD.PrintErr("Player instance not found for health upgrade!");
		}
	}

	private void ContinueGame()
	{
		Visible = false;
		GetTree().Paused = false;
	}
}
