using Godot;
using System;

public partial class GeoTriangleAttack : Attack {
	private GeoTriangle GeoTriangleInstance { get; set; }
	[Export] private float MaxTravelTime = 1f;
	[Export] private float TriangleRotation = 0.1f;
	private float CurrTravelTime = 0f;
	private bool hasTurned = false;
	private Sprite2D sprite;

	public override void _Ready() {
		base._Ready();
		sprite = GetNode<Sprite2D>("Sprite2D");
	}
	protected override void OnScreenExited() {
		GD.Print("Triangle was deleted!");
		GeoTriangleInstance.NotifyTriangleDestroyed();
		QueueFree();
	}
	public void Init(GeoTriangle weapon, Vector2 targetPosition, Vector2 startPosition, int WeaponLevel) {
		weaponLevel = WeaponLevel;
		Position = startPosition;
		base.direction = (targetPosition - startPosition).Normalized();
		Rotation = base.direction.Angle();
		GeoTriangleInstance = weapon;
	}

	public override void _PhysicsProcess(double delta) {
		CurrTravelTime += (float)delta;

		if (CurrTravelTime >= MaxTravelTime) {
			hasTurned = true;
		}

		if (!hasTurned) {
			base._PhysicsProcess(delta);
		} else {
			direction = (Player.Instance.GlobalPosition - GlobalPosition).Normalized();
			base._PhysicsProcess(delta);

			if(Math.Abs((Player.Instance.GlobalPosition - GlobalPosition).Length()) < 1f) {
				GD.Print("Triangle was deleted!");
				GeoTriangleInstance.NotifyTriangleDestroyed();
				QueueFree();
			}
		}

		sprite.Rotate(TriangleRotation);
	}

		protected override void PlayAttackSound()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayShootSound("geotriangle");
		}

	}
}
