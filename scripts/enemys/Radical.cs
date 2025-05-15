using Godot;
using System;

public partial class Radical : Enemy
{
	private ProgressBar healthBar;
	
	[Export] public float BaseSpeed = 100f;
	[Export] public float MaxChargeSpeed = 400f;
	[Export] public float ChargeCooldownTime = 8f;
	[Export] public float ChargeDuration = 1.0f;
	
	private bool isCharging = false;
	private float chargeTimer = 0f;
	private float currSpeed;
	private Timer chargeCooldown;

	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null)
		{
			player = GetTree().Root.GetNode<Player>("World_1/Player");
		}

		CurrentHealth = MaxHealth;
		chargeCooldown = GetNode<Timer>("ChargeCooldown");
		chargeCooldown.WaitTime = ChargeCooldownTime;
		healthBar = GetNode<ProgressBar>("HealthBar");
		chargeCooldown.Start();

		UpdateSpeed();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player == null || !IsInstanceValid(player)) return;

		Vector2 dir = (player.GlobalPosition - GlobalPosition).Normalized();

		if (isCharging)
		{
			chargeTimer -= (float)delta;
			GlobalPosition += dir * MaxChargeSpeed * (float)delta;

			if (chargeTimer <= 0)
				isCharging = false;
		}
		else
		{
			UpdateSpeed();
			GlobalPosition += dir * currSpeed * (float)delta;
		}

		if (GlobalPosition.DistanceTo(player.GlobalPosition) < 20f)
		{
			float damage = CalculateDamage();
			player.TakeDamage(damage);
		}
	}

	private void UpdateSpeed()
	{
		float hpRatio = Mathf.Clamp(CurrentHealth / MaxHealth, 0f, 1f);
		if (hpRatio > 0.75f)
			currSpeed = BaseSpeed * 0.5f;
		else if (hpRatio > 0.25f)
			currSpeed = BaseSpeed;
		else
			currSpeed = BaseSpeed * 1.50f;
	}

	private float CalculateDamage()
	{
		float ratio = 1f - (CurrentHealth / MaxHealth);
		return Mathf.Lerp(10f, 40f, Mathf.Sqrt(ratio));
	}

	private void TryToCharge()
	{
		if (CurrentHealth / MaxHealth < 0.25f && !isCharging && !chargeCooldown.IsStopped())
		{
			isCharging = true;
			chargeTimer = ChargeDuration;
			chargeCooldown.Start();
			GD.Print("Radical is sprinting!");
		}
	}

	public override void TakeDamage(int val)
	{
		base.TakeDamage(val);
		TryToCharge();
		if (healthBar != null)
		{
			healthBar.Value = CurrentHealth;
		}
	}

	protected override void MoveEnemy(float delta)
	{
	}
}
