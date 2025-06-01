using Godot;
using System;

public partial class Pen : Weapon {
	[Export] public int PelletCount = 1;
	[Export] public float SpreadAngleDegrees = 15f;
	[Export] public float FireCooldown = 1.0f;
	private Sprite2D weaponSprite;
	private float timeSinceLastShot = 0f;
	private float radius = 50f;
	private Vector2 dir;

	public override void _Ready()
	{
		base._Ready();
		weaponSprite = GetNode<Sprite2D>("Sprite2D");
		AttackAnimation = "attack_pen";
	}
	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;

		dir = (GetGlobalMousePosition() - Player.Instance.GlobalPosition).Normalized();

		weaponSprite.Rotation = dir.Angle() + (float)Math.PI / 2;

		weaponSprite.GlobalPosition = Player.Instance.GlobalPosition + dir * radius;
	}
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}

	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {

		switch (WeaponLevel) {
			case 1:
				FireCooldown = 1f;
				PelletCount = 1;
				SpreadAngleDegrees = 5f;
				break;
			case 2:
				FireCooldown = 0.7f;
				PelletCount = 3;
				SpreadAngleDegrees = 15f;
				break;
			case 3:
				FireCooldown = 0.4f;
				PelletCount = 6;
				SpreadAngleDegrees = 25f;
				break;
		}

		if (timeSinceLastShot < FireCooldown / attackSpeedAmp) return;
		timeSinceLastShot = 0f;

		Vector2 baseDirection = (targetPosition - Player.Instance.GlobalPosition).Normalized();
		float baseAngle = baseDirection.Angle();

		for (int i = 0; i < PelletCount; i++) {
			float spread = (float)GD.RandRange(-SpreadAngleDegrees / 2, SpreadAngleDegrees / 2);
			float angle = baseAngle + Mathf.DegToRad(spread);

			Vector2 spreadDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

			if (AttackScene != null) {
				PenAttack pellet = (PenAttack)AttackScene.Instantiate();
				GetTree().CurrentScene.AddChild(pellet);
				pellet.Init(Player.Instance.GlobalPosition + spreadDirection * 8, Player.Instance.GlobalPosition, WeaponLevel);
			}
		}
	}
}
