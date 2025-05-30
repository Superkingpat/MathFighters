using Godot;
using System.Collections.Generic;

public partial class Compass : Weapon
{
	[Export] public float Radius = 100f;
	[Export] public int CircleCount = 1;
	[Export] public float RotationSpeed = 180f; // Degrees per second
	[Export] public int MaxAbsorbDamage = 30;
	
	private List<CompassCircle> activeCircles = new List<CompassCircle>();
	public List<CompassCircle> GetActiveCircles()
	{
		return new List<CompassCircle>(activeCircles);
	}
	private float currentRotation = 0f;

	public override void _Ready()
	{
		base._Ready();
		AttackAnimation = "compass_spin";
		SpawnCircles();
	}

	public override void _Process(double delta)
	{
		currentRotation += Mathf.DegToRad(RotationSpeed * (float)delta);
		UpdateCirclePositions();
	}

	private void SpawnCircles()
	{
		// Clear existing circles
		foreach (var circle in activeCircles)
		{
			circle.QueueFree();
		}
		activeCircles.Clear();

		// Create new circles based on upgrade level
		for (int i = 0; i < CircleCount; i++)
		{
			CompassCircle circle = (CompassCircle)AttackScene.Instantiate();
			GetTree().CurrentScene.AddChild(circle);
			circle.Init(this, Radius * (1 + i * 0.2f), MaxAbsorbDamage / CircleCount, RotationSpeed);
			activeCircles.Add(circle);
		}
	}

	private void UpdateCirclePositions()
	{
		foreach (var circle in activeCircles)
		{
			circle.UpdatePosition(Player.Instance.GlobalPosition, currentRotation);
		}
	}

	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp)
	{
		// Passive weapon - no active shooting
	}

	public override Sprite2D GetPickupSprite()
	{
		return GetNode<Sprite2D>("Sprite2D");
	}

	public void ApplyUpgrades()
	{
		switch (WeaponLevel)
		{
			case 1:
				CircleCount = 1;
				Radius = 100f;
				MaxAbsorbDamage = 30;
				break;
			case 2:
				CircleCount = 2;
				Radius = 120f;
				MaxAbsorbDamage = 50;
				break;
			case 3:
				CircleCount = 3;
				Radius = 150f;
				MaxAbsorbDamage = 80;
				break;
		}
		SpawnCircles();
	}
}
