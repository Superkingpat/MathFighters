using Godot;
using System;
using System.Runtime.InteropServices;

public partial class GameManager : Node {
	private static GameManager _instance;
	public static GameManager Instance {
		get {
			return _instance;
		}
	}

	public int coins = 0;

	public override void _Ready() {
		if (_instance == null) {
			_instance = this;
		} else {
			QueueFree();
		}
	}

	public void pickedUpCoin() {
		coins++;
		GD.Print("Picked up coin! New score: " + coins);
	}
}
