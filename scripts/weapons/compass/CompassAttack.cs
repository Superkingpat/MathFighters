using Godot;
using System;

public partial class CompassCircle : Node2D
{
	[Export] public float DamagePerHit = 10f;
	[Export] public float KnockbackForce = 80f;
	
	private Line2D circleVisual;
	private Area2D hitArea;
	private float radius;
	private float currentAbsorbDamage;
	private float maxAbsorbDamage;
	private Compass parentWeapon;

	public void Init(Compass parent, float circleRadius, int absorbAmount, float rotationSpeed)
	{
		parentWeapon = parent;
		radius = circleRadius;
		maxAbsorbDamage = absorbAmount;
		currentAbsorbDamage = maxAbsorbDamage;

		circleVisual = GetNode<Line2D>("Line2D");
		hitArea = GetNode<Area2D>("Area2D");

		// Generate perfect circle
		int points = 36;
		circleVisual.Points = new Vector2[points];
		for (int i = 0; i < points; i++)
		{
			float angle = 2 * Mathf.Pi * i / points;
			circleVisual.Points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
	}

	public void UpdatePosition(Vector2 centerPosition, float rotationAngle)
	{
		GlobalPosition = centerPosition;
		Rotation = rotationAngle;
	}

	private void OnAreaEntered(Area2D area)
	{
		if (currentAbsorbDamage <= 0) return;

		if (area.GetParent() is Enemy enemy)
		{
			// Damage enemy
			enemy.TakeDamage((int)DamagePerHit);
			
			// Apply knockback
			Vector2 knockbackDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
		
			// Try CharacterBody2D approach first, fallback to position
			if (enemy is CharacterBody2D enemyBody)
			{
			enemyBody.Velocity = knockbackDir * KnockbackForce;
			}
			else
			{
				enemy.GlobalPosition += knockbackDir * Mathf.Min(KnockbackForce * 0.1f, 10f);
			}

			// Absorb damage
			currentAbsorbDamage -= enemy.Damage;
			UpdateShieldVisual();

			if (currentAbsorbDamage <= 0)
			{
				circleVisual.Visible = false;
				hitArea.Monitoring = false;
				
				// Start recharge timer if this is the last active circle
				if (IsLastActiveCircle())
				{
					GetTree().CreateTimer(5f).Timeout += RechargeAllCircles;
				}
			}
		}
	}

	private void UpdateShieldVisual()
	{
		float integrity = currentAbsorbDamage / maxAbsorbDamage;
		circleVisual.Modulate = new Color(1, 1, 1, integrity * 0.8f + 0.2f);
	}

	private bool IsLastActiveCircle()
	{
		foreach (var circle in parentWeapon.GetActiveCircles())
		{
			if (circle != this && circle.currentAbsorbDamage > 0)
				return false;
		}
		return true;
	}

	private void RechargeAllCircles()
	{
		foreach (var circle in parentWeapon.GetActiveCircles())
		{
			circle.currentAbsorbDamage = circle.maxAbsorbDamage;
			circle.circleVisual.Visible = true;
			circle.hitArea.Monitoring = true;
			circle.UpdateShieldVisual();
		}
	}
}
