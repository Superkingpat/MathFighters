using Godot;
using System;

public partial class Zone : Area2D
{
	[Export] public float ActivationDelay = 4.2f;
	[Export] public float EffectValue = 20.0f;
	
	public ZoneType Type;
	private Timer timer;
	private bool isActivated = false;
	private Color zoneColor;
	private float currRadius = 0f;
	private float maxRadius;
	private float growSpeed = 50f;
	
	private Sprite2D sprite;
	private CollisionShape2D collShape;
	
	public override void _Ready()
	{
		GD.Randomize();
		int enumCount = Enum.GetValues(typeof(ZoneType)).Length;
		int randomIndex = (int)(GD.Randi() % enumCount);
		Type = (ZoneType)(randomIndex + 1);
		
		sprite = GetNode<Sprite2D>("Sprite2D");
		collShape = GetNode<CollisionShape2D>("CollisionShape2D");

		GD.Print("Naključen zone tip: " + Type);

		timer = GetNode<Timer>("Timer");
		timer.WaitTime = ActivationDelay;
		timer.OneShot = true;
		timer.Timeout += OnTimerTimeout;

		BodyEntered += _on_body_entered;
	}
	
	private void OnTimerTimeout()
	{
		collShape.Disabled = false;
		ActivateEffect();
		QueueFree();
	}
	
	private void ActivateEffect()
	{
	var overlappingBodies = GetOverlappingBodies();
	foreach (var body in overlappingBodies)
	{
		if (body is Player player)
		{
			if(Type == ZoneType.heal)
			{
				GD.Print("Healing player by ", EffectValue);
			}
			else if(Type == ZoneType.damageBoost)
			{
				GD.Print("Giving damage boost");
			}
			else if(Type == ZoneType.damageEnemy)
			{
				GD.Print("Damaging enemies");
			}
		}
	}
}
	
	private void _on_body_entered(Node2D body)
	{
		maxRadius = ((CircleShape2D)collShape.Shape).Radius * 5;
		currRadius = 0f;
		GD.Print("==>Body entered");
		if(!isActivated && body is Player){
			isActivated = true;
			sprite.Visible = false;
			QueueRedraw();
			timer.Start();
			GD.Print("Timer started");
			
			switch (Type)
			{
			case ZoneType.heal:
				zoneColor = new Color(0, 1, 0, 0.5f); // Zelena
				break;
			case ZoneType.damageBoost:
				zoneColor = new Color(1, 1, 0, 0.5f); // Rumena
				break;
			case ZoneType.damageEnemy:
				zoneColor = new Color(1, 0, 0, 0.5f); // Rdeča
				break;
			}
		}
	}
	
	public override void _Process(double delta)
	{
		if(isActivated && currRadius < maxRadius){
			currRadius += (float)delta * growSpeed;
			if(currRadius > maxRadius){
				currRadius = maxRadius;
				ActivateEffect();
			}
			QueueRedraw();
		}
	}
	
	public override void _Draw()
	{
		if(isActivated){
			DrawCircle(Vector2.Zero, currRadius, zoneColor);
			DrawArc(Vector2.Zero, maxRadius, 0, Mathf.Tau, 64, new Color(1, 1, 1, 0.8f), 2.0f);
		}
	}
}

public enum ZoneType
{
	heal = 1,
	damageBoost = 2,
	damageEnemy = 3
}
