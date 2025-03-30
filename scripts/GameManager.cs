using Godot;
using System;
using System.Runtime.InteropServices;
using static System.Formats.Asn1.AsnWriter;

public partial class GameManager : Node {
	private static GameManager _instance;
	private Label scoreLabel;

	public static GameManager Instance {
		get {
			return _instance;
		}
	}

	public int coins = 0;

	private void Initialize() {
		Node uiScene = GetTree().Root.FindChild("UI", true, false);
		if (uiScene != null) {
			scoreLabel = uiScene.GetNode<Label>("Label");
			GD.Print("Label found!");
		} else {
			GD.PrintErr("UI scene not found!");
		}
	}

	public override void _Ready() {
		if (_instance == null) {
			_instance = this;
		} else {
			QueueFree();
		}

		CallDeferred(nameof(Initialize));
	}
	public void pickedUpCoin() {
		coins++;
		scoreLabel.Text = "COLLECTED COINS: " + coins.ToString();
		GD.Print("Picked up coin! New score: " + coins);
	}
}
