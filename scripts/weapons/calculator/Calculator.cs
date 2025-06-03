using Godot;
using System;

public partial class Calculator : Weapon {
    private float timeSinceLastAttacked = 0f;
    [Export] public float AttackCooldown = 0.5f;
    private double currChargeTime = 0f;
    [Export] public double MaxChargeTime = 1f;
    private double chargeSpeedAmp = 0f;
    private double chargeSpeed = 1f;
    private bool actuallyUsed = false;
    private Vector2 originalPosition;
    
    public override void _Ready()
    {
        base._Ready();
        AttackAnimation = "attack_calc";
        originalPosition = Position;
    }
    
    public override void _Process(double delta) {
        if (actuallyUsed && Input.IsActionPressed("attack") && (timeSinceLastAttacked >= AttackCooldown)) {
            Position = new Vector2(-5, -30);
            currChargeTime += delta * chargeSpeedAmp * chargeSpeed;
            GD.Print("Charging!!!");
        } else {
            Position = originalPosition;
            timeSinceLastAttacked += (float)delta;
        }

        if ((AttackScene != null) && ((actuallyUsed && Input.IsActionJustReleased("attack")) || (currChargeTime >= MaxChargeTime)))
        {
            timeSinceLastAttacked = 0f;
            CalculatorAttack attack = (CalculatorAttack)AttackScene.Instantiate();
            GetTree().CurrentScene.AddChild(attack);
            attack.Init(Vector2.Zero, Player.Instance.GlobalPosition, WeaponLevel, currChargeTime);
            currChargeTime = 0;
            actuallyUsed = false;
            Position = originalPosition;
        }
    }
    
    public override Sprite2D GetPickupSprite() {
        return GetNode<Sprite2D>("Sprite2D");
    }
    
    public override void TryShoot(Vector2 targetPosition, float attackSpeedAmp)
    {
        switch (WeaponLevel)
        {
            case 1:
                chargeSpeed = 1f;
                AttackCooldown = 1f;
                MaxChargeTime = 1f;
                break;
            case 2:
                chargeSpeed = 1.4f;
                AttackCooldown = 0.7f;
                MaxChargeTime = 2.5f;
                break;
            case 3:
                chargeSpeed = 2f;
                AttackCooldown = 0.4f;
                MaxChargeTime = 5f;
                break;
        }

        if (timeSinceLastAttacked < AttackCooldown / attackSpeedAmp) return;
        timeSinceLastAttacked = 0f;
        chargeSpeedAmp = attackSpeedAmp;
        actuallyUsed = true;
    }
}