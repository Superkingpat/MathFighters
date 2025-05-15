using Godot;
using System;
using System.Collections.Generic;

public partial class NumberSwarm : Enemy
{
	[Export] public int EnemyValue { get; private set; } = 1;

	public override void _Ready()
	{
		base._Ready();

		// Nastavi lastnosti glede na vrednost
		ConfigureStats();
	}

	private void ConfigureStats()
	{
		if (EnemyValue <= 5) // nizke številke
		{
			Speed = 120f;
			Damage = 5f;
			AttackCooldown = 0.5f;
			MaxHealth = 20f;
		}
		else if (EnemyValue <= 9) // srednje številke
		{
			Speed = 70f;
			Damage = 15f;
			AttackCooldown = 1.2f;
			MaxHealth = 50f;
		}
		else // visoke številke (10+)
		{
			Speed = 30f;
			Damage = 30f;
			AttackCooldown = 2.5f;
			MaxHealth = 200f;
		}

		CurrentHealth = MaxHealth;
	}

	protected override void MoveEnemy(float delta)
	{
		if (isDead || player == null)
			return;

		if (EnemyValue >= 10)
		{
			// Visoke številke ignorirajo ovire in samo napredujejo proti igralcu
			Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
			Position += direction * Speed * delta;
			return;
		}

		if (isAggroed || (isAggroed && isAttacking))
			return;

		Vector2 directionLow = (player.GlobalPosition - GlobalPosition).Normalized();

		// Nizke in srednje številke imajo lahko izogibanje (odkomentiraj, če želiš)
		// ali samo direktno gibanje:
		Position += directionLow * Speed * delta;
	}

	protected override void Attack()
	{
		if (!isAggroed || player == null)
			return;

		isAttacking = true;

		if (EnemyValue <= 5)
		{
			// Hitri, šibki napadi
			GD.Print($"[Number {EnemyValue}] Quick attack!");
			player.TakeDamage(Damage);
		}
		else if (EnemyValue <= 9)
		{
			// Srednje močni napadi
			GD.Print($"[Number {EnemyValue}] Strong melee attack!");
			player.TakeDamage(Damage * 1.5f);
		}
		else
		{
			// Težki, počasni napadi z AoE
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

	public override void TakeDamage(int amount)
	{
		base.TakeDamage(amount);
		GD.Print($"[Number {EnemyValue}] took {amount} damage. Remaining: {CurrentHealth}");
	}

	protected override void Die()
	{
		GD.Print($"[Number {EnemyValue}] has died.");
		base.Die();
	}
}
