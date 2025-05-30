using Godot;
using System;

public partial class Eraser : Weapon {
	[Export] public int ParticleCount = 5;
	[Export] public float SpreadDegrees = 45f;
	[Export] public float MaxRange = 200f;
	[Export] public float KnockbackForce = 0f;
	[Export] public float FireCooldown = 1.2f;
	
	private Sprite2D weaponSprite;
	private float timeSinceLastShot = 0f;
	private float radius = 50f;

	public override void _Ready() {
		base._Ready();
		weaponSprite = GetNode<Sprite2D>("Sprite2D");
		AttackAnimation = "attack_eraser";
	}

	public override void _Process(double delta) {
		timeSinceLastShot += (float)delta;
		
		Vector2 dir = (GetGlobalMousePosition() - Player.Instance.GlobalPosition).Normalized();
		weaponSprite.Rotation = dir.Angle() + (float)Math.PI / 2;
		weaponSprite.GlobalPosition = Player.Instance.GlobalPosition + dir * radius;
	}

	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}

	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		if (timeSinceLastShot < FireCooldown / attackSpeedAmp) return;
		timeSinceLastShot = 0f;

		// Apply upgrades
		switch (WeaponLevel) {
			case 1:
				ParticleCount = 5;
				SpreadDegrees = 45f;
				KnockbackForce = 0f;
				break;
			case 2:
				ParticleCount = 8;
				SpreadDegrees = 60f;
				KnockbackForce = 50f;
				break;
			case 3:
				ParticleCount = 12;
				SpreadDegrees = 75f;
				KnockbackForce = 100f;
				break;
		}

		Vector2 baseDirection = (targetPosition - Player.Instance.GlobalPosition).Normalized();
		float baseAngle = baseDirection.Angle();

		for (int i = 0; i < ParticleCount; i++) {
			float spread = Mathf.Lerp(-SpreadDegrees/2, SpreadDegrees/2, (float)i/(ParticleCount-1));
			float angle = baseAngle + Mathf.DegToRad(spread);

			Vector2 particleDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			float distanceVariation = GD.Randf() * 0.3f + 0.7f; // 70-100% of max range

			if (AttackScene != null) {
				EraserParticle particle = (EraserParticle)AttackScene.Instantiate();
				GetTree().CurrentScene.AddChild(particle);
				particle.Init(
					Player.Instance.GlobalPosition + particleDirection * MaxRange * distanceVariation,
					Player.Instance.GlobalPosition,
					WeaponLevel,
					particleDirection,
					KnockbackForce
				);
			}
		}
	}
}
