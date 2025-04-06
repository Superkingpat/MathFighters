using Godot;
using System;

public abstract partial class Enemy : Node2D
{
    [Export] public string EnemyName { get; private set; } = "Enemy";
    [Export] public int Damage { get; private set; } = 10;
    [Export] public float AttackRange { get; private set; } = 50.0f;
    [Export] public float AttackCooldown { get; private set; } = 1.0f;
    [Export] public float DetectionRange { get; private set; } = 200.0f;
    [Export] public float AggroRange { get; private set; } = 300.0f;
    [Export] public float FleeRange { get; private set; } = 100.0f;
    [Export] public float Speed { get; private set; } = 200.0f;
    [Export] public float MaxHealth { get; private set; } = 100.0f;
    [Export] public float Armor { get; private set; } = 50.0f;

	public float CurrentHealth { get; protected set; }

	protected Player player;

	protected AnimatedSprite2D animatedSprite;

	protected bool isAttacking = false;
	protected bool isFleeing = false;
	protected bool isAggroed = false;
	protected bool isDead = false;

	protected override void _Ready()
	{
		// TODO: AnimatedSprite2D
		player = GetTree().Root.GetNode<Player>("Path/To/Player");
		CurrentHealth = MaxHealth;
	}

	  public override void _PhysicsProcess(float delta)
    {
        if (player != null && IsInstanceValid(player))
        {
            MoveEnemy(delta);
        }
    }

	  protected virtual void MoveEnemy(float delta)
    {
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
        Position += direction * Speed * delta;
    }

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        QueueFree();
    }
}
