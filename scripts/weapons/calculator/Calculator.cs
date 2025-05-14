using Godot;
using System;

public partial class Calculator : Weapon {
	private float timeSinceLastShot = 0f;
	[Export] public float AttackCooldown = 0.5f;
	public override void _Ready() {
		base._Ready();
		AttackAnimation = "attack_calc";
	}
	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;
	}
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}
	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		
	}
}
