using Godot;


public partial class LevelUpScreen : Control {
	public override void _Ready() {
		Visible = false;

		// Get the Player by its path if it's a sibling or within World_1
		// OR if Player is part of a group, keep using that
		Player player = GetNode<Player>("/root/World_1/Player"); // Assuming Player is also a direct child of World_1
		// OR (if you prefer groups and know it will be there)
		// var player = GetTree().GetNodesInGroup("player")[0] as Player;


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

	private void _on_Button_pressed() {
		Visible = false;
		GetTree().Paused = false;
	}
}
