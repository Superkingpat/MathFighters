using Godot;
using System;

public partial class GeoTriangle : Weapon {
	[Export] public int maxTriangleCount = 1;
	public int currTriangleCount = 0;
	private float timeSinceLastShot = 0f;
	[Export] public float AttackCooldown = 0.5f;
	public override void _Ready() {
		base._Ready();
		AttackAnimation = "attack_geo";
	}
	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;
	}
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}
	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		if ((timeSinceLastShot < AttackCooldown /attackSpeedAmp) || (currTriangleCount >= maxTriangleCount)) return;
		timeSinceLastShot = 0f;
		currTriangleCount++;
		//Vector2 baseDirection = (targetPosition - Player.Instance.GlobalPosition).Normalized();

		if (AttackScene != null) {
			GeoTriangleAttack triangle = (GeoTriangleAttack)AttackScene.Instantiate();
			GetTree().CurrentScene.AddChild(triangle);
			triangle.Init(this, GetGlobalMousePosition(), Player.Instance.GlobalPosition);
		}
	}

	public void NotifyTriangleDestroyed() {
		currTriangleCount = Mathf.Max(0, currTriangleCount - 1);
	}
}
