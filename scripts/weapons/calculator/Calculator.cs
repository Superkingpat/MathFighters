using Godot;
using System;

public partial class Calculator : Weapon {
	private float timeSinceLastAttacked = 0f;
	[Export] public float AttackCooldown = 1f;
	private double currChargeTime = 0f;
	[Export] public double MaxChargeTime = 0.5f;
	private double dtForCharging = 0f;
	private bool isCharging = false;
	public override void _Ready() {
		base._Ready();
		AttackAnimation = "attack_calc";
	}
	public override void _Process(double delta) {
		dtForCharging = delta;

		if (!isCharging) {
			timeSinceLastAttacked += (float)delta;
		}

		if ((AttackScene != null) && (!isCharging || currChargeTime >= MaxChargeTime) ) {

			timeSinceLastAttacked = 0f;

			CalculatorAttack attack = (CalculatorAttack)AttackScene.Instantiate();
			GetTree().CurrentScene.AddChild(attack);
			attack.Init(Vector2.Zero, Player.Instance.GlobalPosition, WeaponLevel, currChargeTime);
		}

		isCharging = false;
	}
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}
	public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp) {
		if (timeSinceLastAttacked < AttackCooldown / attackSpeedAmp) return;

		currChargeTime += dtForCharging * attackSpeedAmp;
		isCharging = true;
	}
}
