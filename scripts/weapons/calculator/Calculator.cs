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
    private Sprite2D sprite;
    private Vector2 originalScale;
    private Color originalColor;
    [Export] private AudioStreamPlayer2D chargingSoundPlayer;
    [Export] private AudioStreamPlayer2D dischargeSoundPlayer;
    private bool isPlayingChargingSound = false;
    public override void _Ready()
    {
        base._Ready();
        AttackAnimation = "attack_calc";
        originalPosition = Position;
        sprite = GetNode<Sprite2D>("Sprite2D");
        originalScale = sprite.Scale;
        originalColor = sprite.Modulate;
    }
    
    public override void _Process(double delta) {
        if (actuallyUsed && Input.IsActionPressed("attack") && (timeSinceLastAttacked >= AttackCooldown)) {
            Position = new Vector2(-5, -80);
            currChargeTime += delta * chargeSpeedAmp * chargeSpeed;

            if (!isPlayingChargingSound)
            {
                chargingSoundPlayer.Play();
                isPlayingChargingSound = true;
            }
            
            float chargeRatio = (float)(currChargeTime / MaxChargeTime);
            chargingSoundPlayer.PitchScale = 1.0f + chargeRatio * 0.5f;
            
            sprite.Scale = originalScale * (1 + chargeRatio * 2f);
            sprite.Modulate = new Color(
                originalColor.R * (1 - chargeRatio * 0.5f),
                originalColor.G, 
                originalColor.B * (1 + chargeRatio * 0.5f),
                originalColor.A
            );
            
            GD.Print("Charging!!!");
        } else {
            Position = originalPosition;
            timeSinceLastAttacked += (float)delta;
            
            if (isPlayingChargingSound)
            {
                chargingSoundPlayer.Stop();
                isPlayingChargingSound = false;
            }
            
            if (sprite.Scale != originalScale || sprite.Modulate != originalColor)
            {
                sprite.Scale = originalScale;
                sprite.Modulate = originalColor;
            }
        }

        if ((AttackScene != null) && ((actuallyUsed && Input.IsActionJustReleased("attack")) || (currChargeTime >= MaxChargeTime)))
        {
            dischargeSoundPlayer.Play();
            
            if (isPlayingChargingSound)
            {
                chargingSoundPlayer.Stop();
                isPlayingChargingSound = false;
            }

            timeSinceLastAttacked = 0f;
            CalculatorAttack attack = (CalculatorAttack)AttackScene.Instantiate();
            GetTree().CurrentScene.AddChild(attack);
            attack.Init(Vector2.Zero, Player.Instance.GlobalPosition, WeaponLevel, currChargeTime);
            currChargeTime = 0;
            actuallyUsed = false;
            Position = originalPosition;
            
            sprite.Scale = originalScale;
            sprite.Modulate = originalColor;
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