using Godot;
using System;

public partial class LimitPhantom : Enemy
{
	[Export] public float KnockbackDistance = 50f;
	[Export] public float AoeRadius = 50f;
	[Export] public float ResistancePerStack = 0.03f;
	[Export] public int MaxStacks = 30;
	[Export] public float StackDecayDelay = 8f; //sekunde
	[Export] public float StackDecayRate = 1f; // stacks per second

	private int limitStacks = 0;
	private float timeSinceLastHit = 0f;

	private Timer attackTimer;
	private Area2D aoeArea;

	public override void _Ready()
	{
		base._Ready();

		Speed = 30f;
		Damage = 20f;
		MaxHealth = 300f;
		AttackCooldown = 2.5f;

		CurrentHealth = MaxHealth;

		aoeArea = new Area2D();
		var shape = new CircleShape2D { Radius = AoeRadius };
		var collision = new CollisionShape2D { Shape = shape };
		aoeArea.AddChild(collision);
		AddChild(aoeArea);

		attackTimer = new Timer();
		attackTimer.WaitTime = AttackCooldown;
		attackTimer.OneShot = false;
		attackTimer.Timeout += Attack;
		AddChild(attackTimer);
		attackTimer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (player == null || isDead) return;

		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		Position += direction * Speed * (float)delta;

		timeSinceLastHit += (float)delta;
		if (timeSinceLastHit >= StackDecayDelay && limitStacks > 0)
		{
			limitStacks = Mathf.Max(limitStacks - Mathf.CeilToInt(StackDecayRate * (float)delta), 0);
		}
	}

	public override void TakeDamage(int amount)
	{
		timeSinceLastHit = 0f;

		float resistance = Mathf.Min(limitStacks * ResistancePerStack, 0.95f);
		float effectiveDamage = amount * (1f - resistance);

		CurrentHealth -= effectiveDamage;
		limitStacks = Mathf.Min(limitStacks + 1, MaxStacks);

		GD.Print($"Limit Phantom hit! Stacks: {limitStacks}, Resistance: {resistance * 100f:F1}%, Damage Taken: {effectiveDamage:F1}");

		if (CurrentHealth <= 0)
			Die();
	}

	protected override void Attack()
	{
		if (player == null || isDead) return;

		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
		if (distance <= AoeRadius)
		{
			GD.Print("Limit Phantom AOE slam!");
			player.TakeDamage(Damage);

			Vector2 knockbackDirection = (player.GlobalPosition - GlobalPosition).Normalized();
			player.GlobalPosition += knockbackDirection * KnockbackDistance;
		}
	}
}
