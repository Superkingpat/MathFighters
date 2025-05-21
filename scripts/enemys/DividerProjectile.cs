using Godot;
using System;

public partial class DividerProjectile : Attack
{
	[Export] public float Lifetime = 0.8f;
	[Export] public int MaxGenerations = 2;
	[Export] public float SplitAngle = 20f;

	public float Damage = 10.0f;
	public int Generation = 0;

	private Timer lifeTimer;

	public override void _Ready()
	{
		base._Ready();
		Speed = 150.0f;

		lifeTimer = new Timer();
		lifeTimer.WaitTime = Lifetime;
		lifeTimer.OneShot = true;
		lifeTimer.Timeout += () => OnImpact();
		AddChild(lifeTimer);
		lifeTimer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 motion = direction * Speed * (float)delta;
 		var collision = MoveAndCollide(motion);

		if (collision != null)
		{
			var collider = collision.GetCollider();
			if (collider is Player p)
			{
				p.TakeDamage(Damage);
				// OnImpact();
				QueueFree();
			}
		}
		else
		{
			Position += motion; // Move if nothing is hit
		}
	}

	private void OnImpact()
	{
		if (Generation < MaxGenerations)
		{
			int splitCount = GD.RandRange(2, 3);

			for (int i = 0; i < splitCount; i++)
			{
				var newProjectile = (DividerProjectile)Duplicate();
				float angleOffset = Mathf.DegToRad(SplitAngle) * (i - (splitCount - 1) / 2f);

				newProjectile.direction = direction.Rotated(angleOffset);
				newProjectile.Damage *= 0.7f;
				newProjectile.Speed *= 1.2f;
				newProjectile.Generation = Generation + 1;

				GetTree().CurrentScene.AddChild(newProjectile);
			}
		}
		QueueFree();
	}
}
