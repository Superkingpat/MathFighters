using Godot;
using System;

public partial class DividerProjectile : CharacterBody2D
{
	[Export] public float Speed = 150f;
	[Export] public float Lifetime = 2.5f;
	[Export] public int MaxGenerations = 2;
	[Export] public float SplitAngle = 20f;

	public Vector2 Direction = Vector2.Right;
	public float Damage = 10f;
	public int Generation = 0;

	private Timer lifeTimer;

	public override void _Ready()
	{
		lifeTimer = new Timer();
		lifeTimer.WaitTime = Lifetime;
		lifeTimer.OneShot = true;
		lifeTimer.Timeout += () => OnImpact();
		AddChild(lifeTimer);
		lifeTimer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 motion = Direction * Speed * (float)delta;
 		var collision = MoveAndCollide(motion);
		// var spaceState = GetWorld2D().DirectSpaceState;
		// var query = new PhysicsPointQueryParameters2D
		// {
		// 	Position = GlobalPosition,
		// 	CollideWithAreas = false,
		// 	CollideWithBodies = true
		// };
		// var result = spaceState.IntersectPoint(query);

		// foreach (var hit in result)
		// {
		// 	var collider = hit["collider"].AsGodotObject() as Node2D;
		// 	if (collider is Player p)
		// 	{
		// 		p.TakeDamage(Damage);
		// 		OnImpact();
		// 		break;
		// 	}
		// 	else if (collider is TileMapLayer || collider is StaticBody2D)
		// 	{
		// 		OnImpact();
		// 		break;
		// 	}
		// }

		if (collision != null)
		{
			var collider = collision.GetCollider();
			if (collider is Player p)
			{
				p.TakeDamage(Damage);
				OnImpact();
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

				newProjectile.Direction = Direction.Rotated(angleOffset);
				newProjectile.Damage *= 0.7f;
				newProjectile.Speed *= 1.2f;
				newProjectile.Generation = Generation + 1;

				GetTree().CurrentScene.AddChild(newProjectile);
			}
		}

		QueueFree();
	}
}
