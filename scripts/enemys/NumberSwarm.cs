using Godot;
using System;
using System.Collections.Generic;

public partial class NumberSwarm : Enemy
{
	[Export] public int EnemyValue { get; set; } = 0;
	private ProgressBar healthBar;

	public override void _Ready()
	{
		base._Ready();
		EnemyName = $"NumberSwarm[{EnemyValue}]";
		animatedSprite.Play(EnemyValue.ToString());
		ConfigureStats();
		healthBar = GetNode<ProgressBar>("HealthBar");
	}

	private void ConfigureStats()
	{
		if (EnemyValue <= 5)
		{
			Speed = 120f;
			Damage = 5f;
			AttackCooldown = 0.5f;
			MaxHealth = 20f;
		}
		else if (EnemyValue <= 9) 
		{
			Speed = 70f;
			Damage = 15f;
			AttackCooldown = 1.2f;
			MaxHealth = 50f;
		}
		else
		{
			Speed = 30f;
			Damage = 30f;
			AttackCooldown = 2.5f;
			MaxHealth = 200f;
		}

		CurrentHealth = MaxHealth;
		GD.Print($"[Number {EnemyValue}] Spawned!");
	}

	protected override void Attack()
	{
		if (!isAggroed || player == null)
			return;

		isAttacking = true;

		if (EnemyValue <= 5)
		{
			GD.Print($"[Number {EnemyValue}] Quick attack!");
			player.TakeDamage(Damage);
		}
		else if (EnemyValue <= 9)
		{
			GD.Print($"[Number {EnemyValue}] Strong melee attack!");
			player.TakeDamage(Damage * 1.5f);
		}
		else
		{
			GD.Print($"[Number {EnemyValue}] CHARGED AOE attack!");
			AreaAttack();
		}
	}

	private void AreaAttack()
	{
		float aoeRadius = 100f;
		foreach (var body in GetTree().GetNodesInGroup("player"))
		{
			if (body is Player p && GlobalPosition.DistanceTo(p.GlobalPosition) <= aoeRadius)
			{
				p.TakeDamage(Damage * 2f);
			}
		}
	}

	public override void TakeDamage(float amount)
	{
		base.TakeDamage(amount);
		GD.Print($"[Number {EnemyValue}] took {amount} damage. Remaining: {CurrentHealth}");
		if (healthBar != null)
		{
			healthBar.Value = CurrentHealth;
		}
	}

	protected override void Die()
	{
		GD.Print($"[Number {EnemyValue}] has died.");
		base.Die();
	}
}
