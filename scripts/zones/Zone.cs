using Godot;
using System;

public partial class Zone : Area2D
{
	[Export] public float ActivationDelay = 2.0f;
	[Export] public float EffectValue = 20.0f;
	
	public ZoneType Type;
	private Timer timer;
	private bool isActivated = false;
	
	public override void _Ready()
	{
		GD.Randomize();
		int enumCount = Enum.GetValues(typeof(ZoneType)).Length;
		int randomIndex = (int)(GD.Randi() % enumCount);
		Type = (ZoneType)(randomIndex + 1);

		GD.Print("Naključen zone tip: " + Type);

		timer = GetNode<Timer>("Timer");
		timer.WaitTime = ActivationDelay;
		timer.OneShot = true;
		timer.Timeout += OnTimerTimeout;

		BodyEntered += _on_body_entered;
	}

	
	private void OnTimerTimeout()
	{
		if(Type == ZoneType.heal){
			var player = GetNode<Player>("../Player");
			GD.Print("Heal zone activated...");
		}
		else if(Type == ZoneType.damageBoost){
			var player = GetNode<Player>("../Player");
			GD.Print("damage boost zone activated...");
		}
		else if(Type == ZoneType.damageEnemy){
			var player = GetNode<Player>("../Player");
			GD.Print("enemy damage zone activated...");
		}
		QueueFree(); //odstrani cono
	}
	
	private void _on_body_entered(Node2D body)
	{
		GD.Print("==>Body entered");
		if(!isActivated && body is Player)
		{
			isActivated = true;
			timer.Start();
			GD.Print("Timer started");
		}
	}
}

public enum ZoneType
{
	heal = 1,
	damageBoost = 2,
	damageEnemy = 3
}
