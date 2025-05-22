using Godot;
using System;
using System.Collections.Generic;

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

	private float blackHoleDur = 2f;
	private float blackHoleTimer = 0f;
	private float blackHoleStrength = 400f;
	private float blackHoleRange = 250f;
	private bool isBlackHoleActive = false;

	public override void _Ready()
	{
		player = GetTree().GetNodesInGroup("player")[0] as Player;

		GD.Randomize();
		int enumCount = Enum.GetValues(typeof(ZoneType)).Length;
		int randomIndex = (int)(GD.Randi() % enumCount);
		Type = (ZoneType)(randomIndex + 1);

		sprite = GetNode<Sprite2D>("Sprite2D");
		SetIconForZoneType();
		triggerShape = GetNode<CollisionShape2D>("triggerShape");
		effectShape = GetNode<CollisionShape2D>("effectShape");
		effectShape.Disabled = true;
		effectCircle = (CircleShape2D)effectShape.Shape;
		effectCircle.Radius = maxRadius;

		//GD.Print("Naključen zone tip: " + Type);

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
		if (Type != ZoneType.blackHole)
			QueueFree();
	}

	private void SetIconForZoneType()
	{
		var iconPaths = new Dictionary<ZoneType, string>
		{
			{ ZoneType.heal, "res://assets/zones/zone_heal.tres" },
			{ ZoneType.damageBoost, "res://assets/zones/zone_dmgUp.tres" },
			{ ZoneType.damageEnemy, "res://assets/zones/zone_dmgEnemy.tres" },
			{ ZoneType.slowEnemy, "res://assets/zones/zone_slow.tres" },
			{ ZoneType.stunEnemy, "res://assets/zones/zone_stun.tres" },
			{ ZoneType.blackHole, "res://assets/zones/zone_blackhole.tres" }
		};

		if (iconPaths.TryGetValue(Type, out string path))
		{
			var texture = GD.Load<Texture2D>(path);
			sprite.Texture = texture;
		}
		else
		{
			GD.PrintErr("Ni ikone za tip: " + Type);
		}
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
				}
			}
			if (body is Enemy enemy)
			{
				GD.Print("Enemy detected!");
				switch (Type)
				{
					case ZoneType.damageEnemy:
						DamageEnemies(90);
						break;
					case ZoneType.slowEnemy:
						SlowEnemy(3f);
						break;
					case ZoneType.stunEnemy:
						StunEnemy(1f);
						break;
					case ZoneType.blackHole:
						isBlackHoleActive = true;
						blackHoleTimer = blackHoleDur;
						break;
				}
			}
		}
	}

	private void _on_body_entered(Node2D body)
	{
		if (triggerShape.Disabled) return;
		currRadius = 0f;
		//GD.Print("==>Body entered");
		if(!isActivated && body is Player){
			isActivated = true;
			sprite.Visible = false;
			triggerShape.CallDeferred("set_disabled", true);
			effectShape.CallDeferred("set_disabled", false);
			QueueRedraw();
			timer.Start();
			//GD.Print("Timer started");
			
			switch (Type)
			{
				case ZoneType.heal:
					zoneColor = new Color(0, 1, 0, 0.5f);
					break;
				case ZoneType.damageBoost:
					zoneColor = new Color(1, 0.5f, 0, 0.7f);
					break;
				case ZoneType.damageEnemy:
					zoneColor = new Color(1, 0, 0, 0.5f);
					break;
				case ZoneType.slowEnemy:
					zoneColor = new Color(0, 0, 1, 0.7f);
					break;
				case ZoneType.stunEnemy:
					zoneColor = new Color(1, 1, 0, 0.7f);
					break;
				case ZoneType.blackHole:
					zoneColor = new Color(0, 0, 0, 0.7f);
					break;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (isActivated && currRadius < maxRadius)
		{
			currRadius += (float)delta * growSpeed;
			if (currRadius > maxRadius)
			{
				currRadius = maxRadius;
				ActivateEffect();
			}
			QueueRedraw();
		}

		if (isBlackHoleActive)
		{
			blackHoleTimer -= (float)delta;
			PullEnemiesIntoBlackHole((float)delta);
			if (blackHoleTimer <= 0f)
			{
				isBlackHoleActive = false;
				QueueFree();
			}
		}
	}

	public override void _Draw()
	{
		if (isActivated)
		{
			DrawCircle(Vector2.Zero, currRadius, zoneColor);
			DrawArc(Vector2.Zero, maxRadius, 0, Mathf.Tau, 64, new Color(1, 1, 1, 0.8f), 2.0f);
		}
	}

	private void HealPlayer(int val){
		if (player == null) return;
		else{
			player.Heal(30f);
		}
	
	}
	private async void DamageUpPlayer(int dur, int val) { if (player == null) return; await ToSignal(GetTree().CreateTimer(dur), "timeout"); }
	private void DamageEnemies(int val)
	{
		foreach (var body in GetOverlappingBodies())
			if (body is Enemy enemy) enemy.TakeDamage(val);
	}
	private async void SlowEnemy(float dur) { foreach (var body in GetOverlappingBodies()) if (body is Enemy enemy) enemy.Slow(dur); }
	private async void StunEnemy(float dur) { foreach (var body in GetOverlappingBodies()) if (body is Enemy enemy) enemy.Stun(dur); }

	private void PullEnemiesIntoBlackHole(float delta)
	{
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Enemy enemy && enemy.IsInsideTree() && !enemy.IsQueuedForDeletion())
			{
				float distance = enemy.GlobalPosition.DistanceTo(GlobalPosition);
				if (distance <= blackHoleRange)
				{
					Vector2 toCenter = GlobalPosition - enemy.GlobalPosition;
					Vector2 pull = toCenter.Normalized() * blackHoleStrength * delta;
					enemy.ApplyForce(pull);
				}
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
