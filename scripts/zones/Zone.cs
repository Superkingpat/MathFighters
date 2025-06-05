using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Zone : Area2D
{
	[Export] public float ActivationDelay = 4.2f;

	private Player player;
	public ZoneType Type;

	private Timer timer;
	private bool isActivated = false;
	private bool effectApplied = false;
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
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null)
		{
			GD.PrintErr("Player not found in group 'player'.");
			QueueFree();
			return;
		}

		GD.Randomize();
		int enumCount = Enum.GetValues(typeof(ZoneType)).Length;
		Type = (ZoneType)GD.RandRange(1, enumCount);

		sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		triggerShape = GetNodeOrNull<CollisionShape2D>("triggerShape");
		effectShape = GetNodeOrNull<CollisionShape2D>("effectShape");
		if (sprite == null || triggerShape == null || effectShape == null)
		{
			GD.PrintErr("Missing essential nodes in Zone.");
			QueueFree();
			return;
		}

		effectCircle = effectShape.Shape as CircleShape2D;
		effectCircle.Radius = maxRadius;
		effectShape.Disabled = true;

		SetIconForZoneType();
		zoneColor = GetZoneColor(Type);

		timer = GetNodeOrNull<Timer>("Timer");
		if (timer == null)
		{
			GD.PrintErr("Timer node missing.");
			QueueFree();
			return;
		}
		timer.WaitTime = ActivationDelay;
		timer.OneShot = true;
		timer.Timeout += OnTimerTimeout;

		BodyEntered += _on_body_entered;
	}

	private void OnTimerTimeout()
	{
		triggerShape.Disabled = false;
		if (!effectApplied)
		{
			ActivateEffect();
			effectApplied = true;
		}
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
			if (texture != null) sprite.Texture = texture;
			else GD.PrintErr("Texture load failed: " + path);
		}
		else GD.PrintErr("No icon for ZoneType: " + Type);
	}

	private void ActivateEffect()
	{
		var overlappingBodies = GetOverlappingBodies();
		foreach (var body in overlappingBodies)
		{
			if (body is Player p)
			{
				switch (Type)
				{
					case ZoneType.heal:
						HealPlayer(50);
						break;
					case ZoneType.damageBoost:
						_ = DamageUpPlayer(5, 20);
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
						_ = SlowEnemy(3f);
						break;
					case ZoneType.stunEnemy:
						_ = StunEnemy(1f);
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

		else if (triggerShape.Disabled || isActivated || !(body is Player)) return;
		else
		{
			isActivated = true;
			sprite.Visible = false;
			triggerShape.CallDeferred("set_disabled", true);
			effectShape.CallDeferred("set_disabled", false);
			QueueRedraw();
			timer.Start();
			GD.Print("Timer started");
		}
	}

	public override void _Process(double delta)
	{
		if (isActivated && currRadius < maxRadius)
		{
			currRadius += (float)delta * growSpeed;
			if (currRadius >= maxRadius && !effectApplied)
			{
				currRadius = maxRadius;
				ActivateEffect();
				effectApplied = true;
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

	private void HealPlayer(int amount) => player?.Heal(amount);
	private async Task DamageUpPlayer(int duration, int boost) => await ToSignal(GetTree().CreateTimer(duration), "timeout");
	private void DamageEnemies(int amount)
	{
		foreach (var body in GetOverlappingBodies())
			if (body is Enemy enemy) enemy.TakeDamage(amount);
	}
	private async Task SlowEnemy(float duration)
	{
		foreach (var body in GetOverlappingBodies())
			if (body is Enemy enemy)
			{
				enemy.Slow(duration);
				enemy.Tint(new Color(0.2f, 0.2f, 0.5f), duration);
			} 
	}
	private async Task StunEnemy(float duration)
	{
		foreach (var body in GetOverlappingBodies())
			if (body is Enemy enemy)
			{
				 enemy.Stun(duration);
				enemy.Tint(new Color(1, 1, 0.3f), duration);
			}
	}

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

	private Color GetZoneColor(ZoneType type) => type switch
	{
		ZoneType.heal => new Color(0, 1, 0, 0.5f),
		ZoneType.damageBoost => new Color(1, 0.5f, 0, 0.7f),
		ZoneType.damageEnemy => new Color(1, 0, 0, 0.5f),
		ZoneType.slowEnemy => new Color(0, 0, 1, 0.7f),
		ZoneType.stunEnemy => new Color(1, 1, 0, 0.7f),
		ZoneType.blackHole => new Color(0, 0, 0, 0.7f),
		_ => new Color(1, 1, 1, 0.5f)
	};
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
