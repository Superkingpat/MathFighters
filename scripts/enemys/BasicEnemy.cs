using Godot;
using System;
/*
public partial class BasicEnemy : Enemy
{
	public override void _Ready()
	{
		base._Ready();

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (dropTable.Count == 0)
		{
			GD.Print("No drops configured for this enemy.");
		}

		GetTree().CreateTimer(5.0f).Timeout += () =>
		{
			TakeDamage((int)MaxHealth); // Deal enough damage to kill it instantly
			GD.Print("Enemy killed after 5 seconds!");
		};
	}

	protected override void MoveEnemy(float delta)
	{
		if (isDead || player == null)
			return;

		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

		if (distanceToPlayer <= AggroRange)
		{
			isAggroed = true;
			Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
			Position += direction * Speed * delta;

			if (distanceToPlayer <= AttackRange && !isAttacking)
			{
				Attack();
			}
		}
		else
		{
			isAggroed = false;
		}

		// UpdateAnimation((player.GlobalPosition - GlobalPosition).Normalized());
	}

	private void Attack()
	{
		isAttacking = true;

		if (GlobalPosition.DistanceTo(player.GlobalPosition) <= AttackRange)
		{
			// You'll need to add a TakeDamage method to your Player class
			// player.TakeDamage(Damage);
		}

		GetTree().CreateTimer(AttackCooldown).Timeout += () => isAttacking = false;
	}

   
}
*/