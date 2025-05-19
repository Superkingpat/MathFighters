using Godot;
using System;

public partial class Radical : Enemy
{
	private enum RadicalMode {Ranged, Charge};
	private RadicalMode currMode = RadicalMode.Ranged;
	private ProgressBar healthBar;
	
	[Export] public float BaseSpeed = 100f;
	[Export] public float MaxChargeSpeed = 400f;
	[Export] public float ChargeCooldownTime = 8f;
	[Export] public float ChargeDuration = 1.0f;
	
	private bool isCharging = false;
	private float chargeTimer = 0f;
	private float currSpeed;
	private Timer chargeCooldown;
	
	private Timer waveCooldown;
	private bool canShoot = true;
	[Export] public PackedScene WaveProjectileScene;

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
		
		waveCooldown = GetNode<Timer>("WaveCooldown");
		waveCooldown.WaitTime = 2.0f;
		waveCooldown.OneShot = true;
		waveCooldown.Timeout += () => { canShoot = true; };

		UpdateSpeed();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player == null || !IsInstanceValid(player)) return;

		float hpRatio = CurrentHealth / MaxHealth;
		if (hpRatio <= 0.25f && currMode != RadicalMode.Charge)
		{
			currMode = RadicalMode.Charge;
			GD.Print("Radical switch to charge mode");
		}

		if (isStunned) return;

		Vector2 dir = (player.GlobalPosition - GlobalPosition).Normalized();

		if (currMode == RadicalMode.Ranged)
		{
			UpdateSpeed();
			base.MoveEnemy((float)delta);

			if (canShoot)
			{
				FireWave(dir);
				canShoot = false;
				waveCooldown.Start();
			}
		}
		else if (currMode == RadicalMode.Charge)
		{
			if (isCharging)
			{
				chargeTimer -= (float)delta;
				GlobalPosition += dir * MaxChargeSpeed * (float)delta;
				GlobalPosition += externalForce;

				if (chargeTimer <= 0)
					isCharging = false;
			}
			else
			{
				UpdateSpeed();
				GlobalPosition += dir * currSpeed * (float)delta;
				TryToCharge();
			}
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
			Speed = 50f;
		else if (hpRatio > 0.25f)
			Speed = 200f;
		else
			currSpeed = BaseSpeed * 1.50f;

		if (isSlowed)
			currSpeed *= 0.5f;
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

	public override void TakeDamage(float val)
	{
		base.TakeDamage(val);
		TryToCharge();
		if (healthBar != null)
		{
			healthBar.Value = CurrentHealth;
		}
	}

	private void FireWave(Vector2 dir)
	{
		if (WaveProjectileScene == null) return;

		WaveProjectile wave = WaveProjectileScene.Instantiate<WaveProjectile>();
		GetTree().CurrentScene.AddChild(wave);
		wave.GlobalPosition = GlobalPosition;
		wave.Launch(dir);
		GD.Print("Launching");
	}

}
