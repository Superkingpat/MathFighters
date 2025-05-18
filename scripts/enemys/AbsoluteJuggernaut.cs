using Godot;
using System;

public partial class AbsoluteJuggernaut : Enemy
{
	private ProgressBar healthBar;
	
	[Export] public float contactDamage = 40f;
	
	private Vector2 targetPosition;
	private float moveTimer = 0f;
	private float moveInterval = 3.0f;
	private bool isCharging = false;
	private bool isDecelerating = false;
	
	private float chargeSpeed = 0f;
	[Export] public float maxChargeSpeed = 400f;
	[Export] public float acceleration = 500f;
	[Export] public float deceleration = 700f;
	
	private Vector2 chargeDirection;
	
	public override void _Ready()
	{
		base._Ready();
		targetPosition = player?.GlobalPosition ?? GlobalPosition;
		healthBar = GetNode<ProgressBar>("HealthBar");
		MaxHealth = 500;
		if (healthBar != null)
		{
			healthBar.MaxValue = MaxHealth;
			healthBar.Value = CurrentHealth;
		}
	}
	
	protected override void MoveEnemy(float delta)
	{
		moveTimer += delta;

		if (!isCharging && !isDecelerating && moveTimer >= moveInterval)
		{
			moveTimer = 0f;
			if (player != null && IsInstanceValid(player))
			{
				targetPosition = player.GlobalPosition;
				isCharging = true;
				chargeSpeed = 0f;
				chargeDirection = (targetPosition - GlobalPosition).Normalized();
			}
		}

		if (isCharging)
		{
			chargeSpeed = Mathf.Min(chargeSpeed + acceleration * delta, maxChargeSpeed);
			GlobalPosition += chargeDirection * chargeSpeed * delta;
			GlobalPosition += externalForce;
			
			if(GlobalPosition.DistanceTo(targetPosition) < 10f)
			{
				isCharging = false;
				isDecelerating = true;
			}
		}
		else if(isDecelerating)
		{
			chargeSpeed = Mathf.Max(chargeSpeed - deceleration * delta, 0f);
			GlobalPosition += chargeDirection * chargeSpeed * delta;
			GlobalPosition += externalForce;
			
			if(chargeSpeed <= 5f)
			{
				isDecelerating = false;
				chargeSpeed = 0f;
			}
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		if (healthBar != null)
		{
			healthBar.Value = CurrentHealth;
		}
		
		//charge zadene
		if(player != null && GlobalPosition.DistanceTo(player.GlobalPosition) < 15f)
		{
			player.TakeDamage(contactDamage);
		}
	}
	
	public override void TakeDamage(int val)
	{
		base.TakeDamage(val);
		GD.Print("ABS tejkno dmg");
		if (healthBar != null)
		{
			healthBar.Value = CurrentHealth;
		}
	}
}
