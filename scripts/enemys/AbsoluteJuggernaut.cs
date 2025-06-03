using Godot;
using System;

public partial class AbsoluteJuggernaut : Enemy
{
	private ProgressBar healthBar;

	[Export] public float contactDamage = 40f;

	private Vector2 targetPosition;
	private float moveTimer = 0f;
	private const float moveInterval = 3.0f;

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
	 	CurrentHealth = MaxHealth;
		healthBar = GetNode<ProgressBar>("HealthBar");
		if (healthBar != null)
		{
			healthBar.MaxValue = MaxHealth;
			healthBar.Value = CurrentHealth;
		}
		else
		{
			GD.PrintErr("HealthBar not found under 'HealthBar'!");
		}
		targetPosition = player?.GlobalPosition ?? GlobalPosition;
		var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		anim.Play();
	}

	protected override void MoveEnemy(float delta)
	{
		moveTimer += delta;

		if (!isCharging && !isDecelerating && moveTimer >= moveInterval)
			StartCharge();

		if (isCharging || isDecelerating)
			UpdateChargeMovement(delta);
	}

	private void StartCharge()
	{
		moveTimer = 0f;
		if (player == null) return;

		targetPosition = player.GlobalPosition;
		chargeDirection = (targetPosition - GlobalPosition).Normalized();
		chargeSpeed = 0f;
		isCharging = true;
		isDecelerating = false;
	}

	private void UpdateChargeMovement(float delta)
	{
		if (isCharging)
		{
			chargeSpeed = Mathf.Min(chargeSpeed + acceleration * delta, maxChargeSpeed);
			GlobalPosition += chargeDirection * chargeSpeed * delta + externalForce;

			if (GlobalPosition.DistanceTo(targetPosition) < 10f)
			{
				isCharging = false;
				isDecelerating = true;
			}
		}
		else if (isDecelerating)
		{
			chargeSpeed = Mathf.Max(chargeSpeed - deceleration * delta, 0f);
			GlobalPosition += chargeDirection * chargeSpeed * delta + externalForce;

			if (chargeSpeed <= 5f)
			{
				isDecelerating = false;
				chargeSpeed = 0f;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) < 15f)
		{
			player.TakeDamage(contactDamage);
		}
	}

	public override void TakeDamage(float val)
	{
		float oldHP = CurrentHealth;
		base.TakeDamage(val);

		if (healthBar != null)
			healthBar.Value = CurrentHealth;

		Tint(new Color(1f, 0.3f, 0.3f), 0.2f);
	}
}
