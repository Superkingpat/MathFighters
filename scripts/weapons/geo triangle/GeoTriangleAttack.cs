using Godot;
using System;

public partial class GeoTriangleAttack : Attack
{
	[Export] private float TravelTime = 0.5f;
	[Export] private float ReturnTime = 0.3f;
	[Export] private float RotationSpeed = 10f;
	[Export] private float HoverPulseAmount = 0.2f;
	[Export] private float HoverPulseSpeed = 3f;
	[Export] private Color ReadyColor = new Color(1, 0.5f, 0.5f);
	[Export] private AudioStreamPlayer2D deploySound;
	[Export] public int baseDamage = 10;
	
	private Sprite2D sprite;
	private Vector2 originalScale;
	private Color originalColor;
	private Tween movementTween;
	private bool isReturning = false;
	private bool isDeployed = false;
	private GeoTriangle parentWeapon;
	private Vector2 targetPosition;
	private CollisionShape2D collisionShape;

	public override void _Ready()
	{
		base._Ready();
		sprite = GetNode<Sprite2D>("Sprite2D");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		originalScale = Scale;
		originalColor = sprite.Modulate;

		if (deploySound != null)
		{
			deploySound.PitchScale = (float)GD.RandRange(0.95f, 1.05f);
			deploySound.Play();
		}
	}

	public void Init(GeoTriangle weapon, Vector2 targetPos, Vector2 startPosition, int WeaponLevel)
	{
		parentWeapon = weapon;
		weaponLevel = WeaponLevel;
		Position = startPosition;
		targetPosition = targetPos;
		direction = (targetPos - startPosition).Normalized();
		Rotation = direction.Angle();

		switch (WeaponLevel)
		{
			case 1:
				TravelTime = 0.6f;
				break;
			case 2:
				TravelTime = 0.4f;
				break;
			case 3:
				TravelTime = 0.3f;
				break;
		}

		MoveToTarget();
	}

	private void MoveToTarget()
	{
		movementTween = CreateTween();
		movementTween.TweenProperty(this, "position", targetPosition, TravelTime)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back);
		
		movementTween.TweenCallback(Callable.From(() => {
			isDeployed = true;
			Speed = 0f;
		}));
	}

	public override void _PhysicsProcess(double delta)
	{
		sprite.Rotation += RotationSpeed * (float)delta;

		if (isDeployed && !isReturning)
		{
			float pulse = Mathf.Sin(Time.GetTicksMsec() * 0.001f * HoverPulseSpeed) * HoverPulseAmount;
			Scale = originalScale * (1 + pulse);
			sprite.Modulate = ReadyColor;
			
			ApplyDamageToEnemiesInRange();
		}

		if (isDeployed && !isReturning && Input.IsActionJustPressed("right_attack"))
		{
			ReturnToPlayer();
		}

		if (!isReturning)
		{
			var collision = MoveAndCollide(Velocity * (float)delta);
			if (collision != null && collision.GetCollider() is Enemy enemy)
			{
				enemy.TakeDamage(baseDamage * weaponLevel);
			}
		}
		else
		{
			base._PhysicsProcess(delta);
		}
	}

	private void ApplyDamageToEnemiesInRange()
	{
		if (!(collisionShape?.Shape is CircleShape2D circle)) return;

		float radiusSquared = circle.Radius * circle.Radius;
		Vector2 attackPos = GlobalPosition;

		foreach (Enemy enemy in GetTree().GetNodesInGroup("enemies"))
		{
			if (attackPos.DistanceSquaredTo(enemy.GlobalPosition) <= radiusSquared)
			{
				enemy.TakeDamage(baseDamage * weaponLevel);
			}
		}
	}

	private void ReturnToPlayer()
	{
		if (isReturning || !isDeployed) return;
		
		isReturning = true;
		movementTween?.Kill();

		direction = (Player.Instance.GlobalPosition - GlobalPosition).Normalized();
		Speed = (Player.Instance.GlobalPosition - GlobalPosition).Length() / ReturnTime;
		
		movementTween = CreateTween();
		movementTween.TweenProperty(this, "position", Player.Instance.GlobalPosition, ReturnTime)
			.SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Quad);
		
		movementTween.TweenCallback(Callable.From(OnReturnComplete));
	}

	private void OnReturnComplete()
	{
		parentWeapon?.NotifyTriangleDestroyed();
		QueueFree();
	}

	protected override void OnScreenExited()
	{
		if (!isReturning)
		{
			parentWeapon?.NotifyTriangleDestroyed();
			QueueFree();
		}
	}
}
