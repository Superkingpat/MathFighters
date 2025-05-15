using Godot;
using System;

public partial class Calculator : Weapon {
	private float timeSinceLastAttacked = 0f;
	[Export] public float AttackCooldown = 0.5f;
	private double currChargeTime = 0f;
	[Export] public double MaxChargeTime = 5f;
	private double chargeSpeedAmp = 0f;
	public override void _Ready() {
		base._Ready();
		AttackAnimation = "attack_calc";
	}
	public override void _Process(double delta) {

		if (Input.IsActionPressed("attack") && (timeSinceLastAttacked >= AttackCooldown)) {
			currChargeTime += delta * chargeSpeedAmp;
			GD.Print("Charging!!!");
		} else {
			timeSinceLastAttacked += (float)delta;
		}

		if ((AttackScene != null) && (Input.IsActionJustReleased("attack") || (currChargeTime >= MaxChargeTime))) {
			timeSinceLastAttacked = 0f;
			CalculatorAttack attack = (CalculatorAttack)AttackScene.Instantiate();
			GetTree().CurrentScene.AddChild(attack);
			attack.Init(Vector2.Zero, Player.Instance.GlobalPosition, WeaponLevel, currChargeTime);
			currChargeTime = 0;
		}
	}
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}
	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		if (timeSinceLastAttacked < AttackCooldown / attackSpeedAmp) return;
		timeSinceLastAttacked = 0f;
		chargeSpeedAmp = attackSpeedAmp;
	}
}
