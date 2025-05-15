using Godot;
using System;
using System.Collections.Generic;

public partial class ZeroGuardian : Enemy
{
	[Export] public float protectionRadius = 100.0f;
	[Export] public float duration = 5.0f;

	private List<Enemy> protectedEnemies = new List<Enemy>();
	private Timer durationTimer;
	private Area2D protectionArea;

	public override void _Ready()
	{
		base._Ready();

		EnemyName = "Zero Guardian";
		MaxHealth = float.MaxValue; 
		CurrentHealth = MaxHealth;
		Damage = 0.0f; 
		Speed = 0.0f; 

		protectionArea = new Area2D();
		protectionArea.Name = "ProtectionArea";
		AddChild(protectionArea);

		var protectionShape = new CollisionShape2D();
		var circleShape = new CircleShape2D();
		circleShape.Radius = protectionRadius;
		protectionShape.Shape = circleShape;
		protectionArea.AddChild(protectionShape);

		protectionArea.BodyEntered += OnBodyEntered;
		protectionArea.BodyExited += OnBodyExited;

		CreateShieldVisual();

		durationTimer = new Timer();
		durationTimer.WaitTime = duration;
		durationTimer.OneShot = true;
		durationTimer.Autostart = true;
		durationTimer.Timeout += OnDurationTimerTimeout;
		AddChild(durationTimer);

		AddInitialProtection();
	}

	private void CreateShieldVisual()
	{
		var shield = new Node2D();
		shield.Name = "ShieldVisual";

		var circle = new Polygon2D();
		var points = new Vector2[32];

		for (int i = 0; i < 32; i++)
		{
			float angle = i * Mathf.Pi * 2 / 32;
			points[i] = new Vector2(
				Mathf.Cos(angle) * protectionRadius,
				Mathf.Sin(angle) * protectionRadius
			);
		}

		circle.Polygon = points;
		circle.Color = new Color(0, 0.5f, 1, 0.2f);
		shield.AddChild(circle);

		var outline = new Line2D();
		outline.Width = 2.0f;
		outline.DefaultColor = new Color(0, 0.7f, 1, 0.7f);

		for (int i = 0; i <= 32; i++)
		{
			float angle = i * Mathf.Pi * 2 / 32;
			outline.AddPoint(new Vector2(
				Mathf.Cos(angle) * protectionRadius,
				Mathf.Sin(angle) * protectionRadius
			));
		}

		shield.AddChild(outline);
		AddChild(shield);
	}

	private void AddInitialProtection()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");
		foreach (var node in enemies)
		{
			if (node is Enemy enemy && enemy != this)
			{
				float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (distance <= protectionRadius)
				{
					protectedEnemies.Add(enemy);
					GD.Print($"Enemy {enemy.EnemyName} is initially protected by Zero Guardian");
				}
			}
		}
	}

	protected override void MoveEnemy(float delta)
	{
		// Zero Guardian doesn't move
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Enemy enemy && enemy != this)
		{
			protectedEnemies.Add(enemy);
			GD.Print($"Enemy {enemy.EnemyName} is now protected by Zero Guardian");
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Enemy enemy)
		{
			protectedEnemies.Remove(enemy);
			GD.Print($"Enemy {enemy.EnemyName} is no longer protected by Zero Guardian");
		}
	}

	public void InterceptDamage(Enemy targetEnemy, float amount)
	{
		GD.Print($"Zero Guardian absorbed {amount} damage meant for {targetEnemy.EnemyName}");
	}

	public override void TakeDamage(float amount)
	{
		GD.Print("Zero Guardian absorbed damage");
	}

	private void OnDurationTimerTimeout()
	{
		DissolveAndRemove();
	}

	private void DissolveAndRemove()
	{
		protectedEnemies.Clear();

		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 0.5f);
		tween.Finished += QueueFree;
	}

	protected override void DropItems()
	{
		// Zero Guardian doesn't drop items
	}

	public static bool IsEnemyProtected(Enemy enemy)
	{
		var guardians = enemy.GetTree().GetNodesInGroup("enemies");

		foreach (var node in guardians)
		{
			if (node is ZeroGuardian guardian && guardian != enemy)
			{
				float distance = guardian.GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (distance <= guardian.protectionRadius)
				{
					return true;
				}
			}
		}

		return false;
	}
}
