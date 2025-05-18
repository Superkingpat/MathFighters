using Godot;
using System;

public partial class WaveProjectile : Area2D
{
	[Export] public float Speed = 300f;
	[Export] public float LifeTime = 2f;
	[Export] public int Damage = 20;

	private Vector2 direction;
	private Timer lifeTimer;
	private Sprite2D sprite;

	public override void _Ready()
	{
		lifeTimer = GetNode<Timer>("LifeTimer");
		sprite = GetNode<Sprite2D>("Sprite2D");

		lifeTimer.WaitTime = LifeTime;
		lifeTimer.OneShot = true;
		lifeTimer.Start();
		lifeTimer.Timeout += OnLifeTimeout;

		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += direction * Speed * (float)delta;
	}

	public void Launch(Vector2 dir)
	{
		direction = dir.Normalized();
		Rotation = direction.Angle();
		GD.Print("Launched with dir: " + direction);
	}

	private void OnLifeTimeout()
	{
		QueueFree();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.TakeDamage(Damage);
			QueueFree();
		}
	}
}
