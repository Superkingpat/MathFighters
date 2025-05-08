using Godot;
using System;

public partial class Zone : Area2D
{
	[Export] public float ActivationDelay = 4.2f;
	
	private Player player;
	
	public ZoneType Type;
	private Timer timer;
	private bool isActivated = false;
	private Color zoneColor;
	private float currRadius = 0f;
	private float maxRadius = 215f;
	private float growSpeed = 50f;
	
	private Sprite2D sprite;
	private CollisionShape2D triggerShape;
	private CollisionShape2D effectShape;
	private CircleShape2D effectCircle;
	
	public override void _Ready()
	{
		player = GetTree().GetNodesInGroup("player")[0] as Player;
		
		GD.Randomize();
		int enumCount = Enum.GetValues(typeof(ZoneType)).Length;
		int randomIndex = (int)(GD.Randi() % enumCount);
		Type = (ZoneType)(randomIndex + 1);
		
		sprite = GetNode<Sprite2D>("Sprite2D");
		triggerShape = GetNode<CollisionShape2D>("triggerShape");
		effectShape = GetNode<CollisionShape2D>("effectShape");
		effectShape.Disabled = true;
		effectCircle = (CircleShape2D)effectShape.Shape;
		effectCircle.Radius = maxRadius;

		GD.Print("Naključen zone tip: " + Type);

		timer = GetNode<Timer>("Timer");
		timer.WaitTime = ActivationDelay;
		timer.OneShot = true;
		timer.Timeout += OnTimerTimeout;

		BodyEntered += _on_body_entered;
	}
	
	private void OnTimerTimeout()
	{
		triggerShape.Disabled = false;
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
			switch (Type)
			{
			case ZoneType.heal:
				HealPlayer(50);
				break;
			case ZoneType.damageBoost:
				DamageUpPlayer(5, 20);
				break;
			case ZoneType.damageEnemy:
				DamageEnemies(50);
				break;
			case ZoneType.slowEnemy:
				SlowEnemy(3);
				break;
			case ZoneType.stunEnemy:
				StunEnemy(2);
				break;
			case ZoneType.blackHole:
				BlackHole(100f);
				break;
			}
		}
	}
}
	
	private void _on_body_entered(Node2D body)
	{
		if(triggerShape.Disabled){
			return;
		}
		currRadius = 0f;
		GD.Print("==>Body entered");
		if(!isActivated && body is Player){
			isActivated = true;
			sprite.Visible = false;
			triggerShape.CallDeferred("set_disabled", true);
			effectShape.CallDeferred("set_disabled", false);
			QueueRedraw();
			timer.Start();
			GD.Print("Timer started");
			
			switch (Type)
			{
			case ZoneType.heal:
				zoneColor = new Color(0, 1, 0, 0.5f); // Zelena
				break;
			case ZoneType.damageBoost:
				zoneColor = new Color(1, 0.5f, 0, 0.7f); // Orange
				break;
			case ZoneType.damageEnemy:
				zoneColor = new Color(1, 0, 0, 0.5f); // Rdeča
				break;
			case ZoneType.slowEnemy:
				zoneColor = new Color(0, 0, 1, 0.7f); // Modra
				break;
			case ZoneType.stunEnemy:
				zoneColor = new Color(1, 1, 0, 0.7f); // Rumena
				break;
			case ZoneType.blackHole:
				zoneColor = new Color(0, 0, 0, 0.7f); // crna
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
	
	//Zone effect functions
	private void HealPlayer(int val){
		if(player == null) return;
		GD.Print("Heal pride ko bo dobil player health");
		//dodaj ko player dobi health
	}
	
	private async void DamageUpPlayer(int dur, int val){
		if(player == null) return;
		GD.Print("DmgUp pride ko bo dobil player dmg num");
		await ToSignal(GetTree().CreateTimer(dur), "timeout");
		//dodaj ko player dobi dmg
	}
	
	private void DamageEnemies(int val){
		var overlappingBodies = GetOverlappingBodies();
		foreach(var body in overlappingBodies)
		{
			if(body is Enemy enemy){
				GD.Print($"Damaging enemy {enemy.EnemyName} for 50 hp");
				enemy.TakeDamage(val);
			}
		}
	}
	
	private async void SlowEnemy(int dur){
		var overlappingBodies = GetOverlappingBodies();
		foreach(var body in overlappingBodies)
		{
			if(body is Enemy enemy){
				GD.Print($"slowing enemy {enemy.EnemyName} for 50%");
				enemy.Speed *= 0.5f;
				await ToSignal(GetTree().CreateTimer(dur), "timeout");
				enemy.Speed *= 2.0f;
			}
		}
	}
	
	private async void StunEnemy(int dur){
		var overlappingBodies = GetOverlappingBodies();
		foreach(var body in overlappingBodies)
		{
			if(body is Enemy enemy){
				GD.Print($"slowing enemy {enemy.EnemyName} for 50%");
				enemy.Speed = 0.0f;
				await ToSignal(GetTree().CreateTimer(dur), "timeout");
				enemy.Speed = 200.0f;
			}
		}
	}
	
	private void BlackHole(float pullStrength){
		var overlappingBodies = GetOverlappingBodies();
		foreach (var body in overlappingBodies)
		{
			if (body is Enemy enemy && enemy.IsInsideTree() && !enemy.IsQueuedForDeletion())
			{
				Vector2 toCenter = GlobalPosition - enemy.GlobalPosition;
				Vector2 pull = toCenter.Normalized() * pullStrength * (float)GetProcessDeltaTime();
				enemy.GlobalPosition += pull;
			}
		}
	}
}

public enum ZoneType
{
	heal = 1,
	damageBoost = 2,
	damageEnemy = 3,
	slowEnemy = 4,
	stunEnemy = 5,
	blackHole = 6
}
