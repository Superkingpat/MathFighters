using Godot;
using System;

public partial class Divider : Enemy
{
	[Export] public PackedScene DividerProjectileScene { get; set; }
	[Export] public float PreferredDistance = 500f;
	[Export] public float RetreatThreshold = 200f;
	[Export] public float HoverSpeed = 40f;
	[Export] public float FireInterval = 1.5f;

	private Timer fireTimer;

	public override void _Ready()
	{
		base._Ready();

		Speed = HoverSpeed;
		MaxHealth = 80f;
		Damage = 10f;
		AttackCooldown = FireInterval;

		CurrentHealth = MaxHealth;

		fireTimer = new Timer();
		fireTimer.WaitTime = FireInterval;
		fireTimer.OneShot = false;
		fireTimer.Autostart = true;
		fireTimer.Timeout += FireProjectile;

		AddChild(fireTimer);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (player == null || isDead) return;

		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
		float distance = toPlayer.Length();
		Vector2 direction;

		if (distance < RetreatThreshold)
		{
			// Umik, če je igralec preblizu
			direction = -toPlayer.Normalized();
		}
		else if (distance > PreferredDistance)
		{
			// Premik proti igralcu, če je predaleč
			direction = toPlayer.Normalized();
		}
		else
		{
			// Ostani približno na mestu (rahlo lebdi)
			direction = Vector2.Zero;
		}

		if (direction != Vector2.Zero)
			Position += direction * Speed * (float)delta;
	}

	private void FireProjectile()
	{
		if (DividerProjectileScene == null || player == null)
			return;

		var projectile = DividerProjectileScene.Instantiate<DividerProjectile>();
		projectile.GlobalPosition = GlobalPosition;
		projectile.Direction = (player.GlobalPosition - GlobalPosition).Normalized();
		projectile.Damage = Damage;
		projectile.Generation = 0; // prva generacija

		GetTree().CurrentScene.AddChild(projectile);
		GD.Print("Divider fired primary projectile");
	}
}
