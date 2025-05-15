using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ExponentialLeaper : Enemy
{
	[Export] public float baseSpeed = 10.0f;
	[Export] public float baseDamage = 5.0f;
	[Export] public float baseCooldown = 2.0f;

	private int speedIncreaseCount = 0;
	private int AttackIncreaseCount = 0;
	private float scale = 4;

	float minCooldown = 0.3f;

	public void Initialize(){
		EnemyName = "Exponential Leaper";
		Speed = baseSpeed;
		Damage = baseDamage;
	}

	public override void _Ready()
	{
		base._Ready();

		var speedTimer = new Timer();
		speedTimer.WaitTime = 5.0f;
		speedTimer.OneShot = false;
		speedTimer.Autostart = true;
		speedTimer.Timeout += IncreaseSpeed;
		AddChild(speedTimer);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);  
	}

	private void IncreaseSpeed()
	{
		speedIncreaseCount++;
		Speed = baseSpeed * (float)Math.Pow(2, speedIncreaseCount);;
		GD.Print($"Run #{speedIncreaseCount} — Speed is now: {Speed}");
	}

	private void IncreaseDamage(int count)
	{
		Damage = baseDamage + (float)Math.Sqrt(AttackIncreaseCount) * scale;
		GD.Print($"Attack #{count} — Damage is now: {Damage}");
	}

	private void IncreaseAttackSpeed(int count)
	{
		AttackCooldown = Math.Max(minCooldown, baseCooldown / (1 + (float)Math.Sqrt(count)));
		GD.Print($"Attack #{count} — Attack Cooldown is now: {AttackCooldown}");
	}
	protected override void Attack()
	{
		if (isAggroed && player != null)
		{
			isAttacking = true;
			GD.Print($"[Enemy {EnemyName}] Attacking player");
			player.TakeDamage(Damage);
			// Do damage logic or animation
			AttackIncreaseCount++;
			IncreaseDamage(AttackIncreaseCount);
			IncreaseAttackSpeed(AttackIncreaseCount);
		}
	}
}
