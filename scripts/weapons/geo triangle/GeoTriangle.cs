using Godot;
using System;

public partial class GeoTriangle : Weapon {
	[Export] public int maxTriangleCount = 1;
	public int currTriangleCount = 0;
	private float timeSinceLastShot = 0f;
	[Export] public float AttackCooldown = 1f;
	[Export] private AudioStreamPlayer2D returnSound;
	public override void _Ready()
	{
		base._Ready();
		AttackAnimation = "attack_geo";
	}
	public override void _Process(double delta)
	{
		timeSinceLastShot += (float)delta;
		
		if (returnSound != null && currTriangleCount > 0 && Input.IsActionJustPressed("right_attack")) {
			returnSound.PitchScale = (float)GD.RandRange(0.9f, 1.1f);
			returnSound.Play();
		}
	}
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}
	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		
		switch (WeaponLevel) {
			case 1:
				maxTriangleCount = 1;
				AttackCooldown = 1f;
				break;
			case 2:
				maxTriangleCount = 3;
				AttackCooldown = 0.5f;
				break;
			case 3:
				maxTriangleCount = 6;
				AttackCooldown = 0.25f;
				break;
		}

		if ((timeSinceLastShot < AttackCooldown / attackSpeedAmp) || (currTriangleCount >= maxTriangleCount)) return;
		timeSinceLastShot = 0f;
		currTriangleCount++;

		if (AttackScene != null) {
			GeoTriangleAttack triangle = (GeoTriangleAttack)AttackScene.Instantiate();
			GetTree().CurrentScene.AddChild(triangle);
			triangle.Init(this, GetGlobalMousePosition(), Player.Instance.GlobalPosition, WeaponLevel);
		}
	}

	public void NotifyTriangleDestroyed() {
		currTriangleCount = Mathf.Max(0, currTriangleCount - 1);
	}
}
