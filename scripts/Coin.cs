using Godot;
using System;

public partial class Coin : Area2D {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GD.Print("A coin appeard!");
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body) {
		if (body is Player player) {
			player.CollectCoin();
			QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

	}
}
