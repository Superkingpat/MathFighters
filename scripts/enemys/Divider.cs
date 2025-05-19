using Godot;
using System;

public partial class Divider : Enemy
{
	[Export] public float baseSpeed = 40.0f;
	[Export] public float PreferredDistance = 500f;
	[Export] public float RetreatThreshold = 200f;

	private Area2D retreatArea;
	private bool isRetreating = false;

	private PackedScene DividerProjectileScene= ResourceLoader.Load<PackedScene>("res://scenes/Enemys/DividerProjectile.tscn");

	public override void _Ready()
	{
		base._Ready();
		EnemyName = "Divider";
		Speed = baseSpeed;
		MaxHealth = 80f;
		AttackCooldown = 3f;

		CurrentHealth = MaxHealth;

		retreatArea = GetNode<Area2D>("retreat_area");
		if(retreatArea == null){
			GD.PrintErr($"[Enemy {EnemyName}]: retreatArea was not initialized properly");
		}

		retreatArea.BodyEntered += (Node2D body) => { 
			if(player == body)
				isRetreating = true; 
		};
		retreatArea.BodyExited += (Node2D body) => { 
			if(player == body)
				isRetreating = false; 
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player != null && IsInstanceValid(player))
		{
			if(!isRetreating){
				Speed = baseSpeed;
				if(isAggroed) isMoving = false;
				else isMoving = true;
			}
			else {
				isMoving = true;
				Speed = 200f;
			}
			if(isMoving)
				MoveEnemy((float)delta);
		}
	}

	protected override void MoveEnemy(float delta)
	{
		if(!isRetreating){
	   		if (isDead || player == null)
				return;

			if (isAttacking)
				return;

			Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();

			Position += direction * Speed * delta;
		}
		else{
			if (isDead || player == null)
				return;

			if (isAttacking)
				return;

			Vector2 direction = -(player.GlobalPosition - GlobalPosition).Normalized();

			Position += direction * Speed * delta;
		}
	}

	protected override void Attack()
	{
		if (DividerProjectileScene == null || player == null)
			return;

		Vector2 baseDirection = (player.GlobalPosition - GlobalPosition).Normalized();
		float baseAngle = baseDirection.Angle();

		// float spread = (float)GD.RandRange(-SpreadAngleDegrees / 2, SpreadAngleDegrees / 2);
		DividerProjectile projectile = (DividerProjectile)DividerProjectileScene.Instantiate();

		projectile.GlobalPosition = GlobalPosition;

		float angle = baseAngle + Mathf.DegToRad(projectile.Speed);
		Vector2 spreadDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		GetTree().CurrentScene.AddChild(projectile);
		projectile.Init(Player.Instance.GlobalPosition + spreadDirection * 8, GlobalPosition, 0);

		GD.Print($"[Enemy {EnemyName}]: Shot projectile at player");
	}
}
