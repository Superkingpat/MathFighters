using Godot;
using System;

public partial class MainMenu : Control
{
	private void _on_start_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/world_1.tscn");
		GD.Print("Start pressed");
	}

	private void _on_settings_button_pressed()
	{
		GD.Print("Settings pressed");
	}

	private void _on_exit_button_pressed()
	{
		GetTree().Quit();
		GD.Print("Exit pressed");
	}
}
