using Godot;

public partial class EraserParticle : Attack {
	[Export] public float MaxLifetime = 0.8f;
	[Export] public float MinDamage = 5f;
	[Export] public float MaxDamage = 15f;
	[Export] public float KnockbackForce = 0f;
	
	private Vector2 direction;
	private float lifetime = 0f;
	private float initialDistance;

	public void Init(Vector2 targetPosition, Vector2 startPosition, int weaponLevel, Vector2 dir, float knockback) {
		base.Init(targetPosition, startPosition, weaponLevel);
		direction = dir;
		KnockbackForce = knockback;
		initialDistance = startPosition.DistanceTo(targetPosition);
		
		// Apply upgrades
		switch (weaponLevel) {
			case 2:
				MaxDamage += 3f;
				break;
			case 3:
				MaxDamage += 6f;
				MaxLifetime *= 1.2f; // Longer range
				break;
		}

		Rotation = dir.Angle();
	}

	public override void _PhysicsProcess(double delta) {
		lifetime += (float)delta;
		
		if (lifetime >= MaxLifetime) {
			QueueFree();
			return;
		}

		// Move particle
		Velocity = direction * Speed;
		var collision = MoveAndCollide(Velocity * (float)delta);

		if (collision != null && collision.GetCollider() is Enemy enemy) {
			// Damage falls off with distance
			float distanceTravelled = GlobalPosition.DistanceTo(Player.Instance.GlobalPosition);
			float damageFalloff = 1f - (distanceTravelled / initialDistance);
			float damage = Mathf.Lerp(MinDamage, MaxDamage, damageFalloff);
			
			enemy.TakeDamage((int)damage);
			
			// Apply knockback
			if (KnockbackForce > 0) {
				enemy.GlobalPosition += direction * KnockbackForce * 0.1f;
			}

			QueueFree();
		}
	}

	protected override void PlayAttackSound() {
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlayShootSound("eraser");
		}
	}
}
