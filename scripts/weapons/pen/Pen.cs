using Godot;
using System;

public partial class Pen : Weapon {
	[Export] public int PelletCount = 6;
	[Export] public float SpreadAngleDegrees = 15f;
	[Export] public float FireCooldown = 1.0f;

	private float timeSinceLastShot = 0f;

	public override void _Ready() {
		base._Ready();
		AttackAnimation = "attack_pen";
	}
	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;
	}

	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}

	public override void TryShoot(Vector2 targetPosition) {
		if (timeSinceLastShot < FireCooldown) return;
		timeSinceLastShot = 0f;

		Vector2 baseDirection = (targetPosition - Player.Instance.GlobalPosition).Normalized();
		float baseAngle = baseDirection.Angle();

		for (int i = 0; i < PelletCount; i++) {
			float spread = (float)GD.RandRange(-SpreadAngleDegrees / 2, SpreadAngleDegrees / 2);
			float angle = baseAngle + Mathf.DegToRad(spread);

			Vector2 spreadDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

			if (AttackScene != null) {
				Attack pellet = (Attack)AttackScene.Instantiate();
				GetTree().CurrentScene.AddChild(pellet);
				pellet.Init(Player.Instance.GlobalPosition + spreadDirection * 8, Player.Instance.GlobalPosition);
			}
		}
	}
}
