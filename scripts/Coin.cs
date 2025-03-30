using Godot;
using System;

public partial class Coin : Area2D {
	// Called when the node enters the scene tree for the first time.
	private AnimationPlayer player;
	public override void _Ready() {
		GD.Print("A coin appeard!");
		BodyEntered += OnBodyEntered;

		player = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private void OnBodyEntered(Node body) {
		if (body is Player) {
			GameManager.Instance.pickedUpCoin();
			player.Play("pickup");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

	}
}
