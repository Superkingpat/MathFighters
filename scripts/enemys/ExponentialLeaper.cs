using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ExponentialLeaper : Enemy
{
    [Export] public float baseSpeed = 10.0f;
    [Export] public float baseDamage = 5.0f;
    [Export] public float baseCooldown = 2.0f;

    private int speedIncreaseCount = 0;
    private int AttackIncreaseCount = 0;
    private float scale = 4;

    float minCooldown = 0.5f;

    public void Initialize(){
        EnemyName = "Exponential Leaper";
        Speed = baseSpeed;
        Damage = baseDamage;
    }

    public override void _Ready()
    {
        base._Ready();

        var spawnTimer = new Timer();
        spawnTimer.WaitTime = 5.0f;
        spawnTimer.OneShot = false;
        spawnTimer.Autostart = true;
        spawnTimer.Timeout += OnTimerTimeout;
        AddChild(spawnTimer);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);  
    }

     private void OnTimerTimeout()
    {
        speedIncreaseCount++;
        IncreaseSpeed(speedIncreaseCount);
    }

    private void IncreaseSpeed(int count)
    {
        Speed = baseSpeed * (float)Math.Pow(2, count);;
        GD.Print($"Run #{count} — Speed is now: {Speed}");
    }

    private void IncreaseDamage(int count)
    {
        Damage = baseDamage + (float)Math.Sqrt(AttackIncreaseCount) * scale;
        GD.Print($"Attack #{count} — Damage is now: {Damage}");
    }

    private void IncreaseAttackSpeed(int count)
    {
        AttackCooldown = Math.Max(minCooldown, baseCooldown / (1 + (float)Math.Sqrt(count)));
        GD.Print($"Attack #{count} — Attack Cooldown is now: {AttackCooldown}");
    }

    protected override void Attack(){
        float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		
		if (distanceToPlayer <= AggroRange)
		{
			isAggroed = true;
			
			if (distanceToPlayer <= AttackRange && !isAttacking)
			{
				isAttacking = true;
				GD.Print("Enemy attacking player");
				
				GetTree().CreateTimer(AttackCooldown).Timeout += () =>
				{
					isAttacking = false;
				};

                AttackIncreaseCount++;
                IncreaseDamage(AttackIncreaseCount);
                IncreaseAttackSpeed(AttackIncreaseCount);
			}
		}
		else
		{
			isAggroed = false;
		}
    }
}