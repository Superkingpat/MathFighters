using Godot;
using System;

public partial class Weapon : Node2D {
	[Export] public PackedScene ProjectileScene;
	[Export] public float FireCooldown = 0.5f;
	[Export] public string AttackAnimation = "attack";

	private float timeSinceLastShot = 0.0f;

	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;
	}

	public virtual Sprite2D GetPickupSprite() {
		return null;
	}

	public virtual void TryShoot(Vector2 targetPosition, Vector2 shooterPosition) {
		if (timeSinceLastShot < FireCooldown) return;

		if (ProjectileScene != null) {
			Bullet bullet = (Bullet)ProjectileScene.Instantiate();
			GetTree().CurrentScene.AddChild(bullet);
			bullet.Init(targetPosition, shooterPosition);
		}

		timeSinceLastShot = 0.0f;
	}
}
