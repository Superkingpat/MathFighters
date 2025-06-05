using Godot;
using System;

public partial class CalculatorAttack : Attack {
    [Export] public float MaxExpandScale = 5.0f;
    [Export] public float ExpandedTime = 1.0f;
    [Export] public float DamageInterval = 0.3f;
    [Export] public int baseDamage = 10;
    
    [Export] public float ExpandAnimationTime = 0.2f;
    [Export] public float ShrinkAnimationTime = 0.15f;
    [Export] public Texture2D expandedSprite;
    [Export] public Sprite2D sprite;
    
    private float timer = 0f;
    private float damageTickTimer = 0f;
    private Vector2 originalScale;
    private CollisionShape2D collisionShape;
    private Shape2D originalCollisionShape;
    private float expandScale = 1.0f;
    private Tween scaleTween = null;
    private bool isShrinking = false;

    public override void _Ready() {
        base._Ready();
        originalScale = Scale;
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        if (sprite == null) {
            sprite = GetNode<Sprite2D>("Sprite2D");
        }

        if (collisionShape != null) {
            originalCollisionShape = collisionShape.Shape;
        } else {
            GD.PrintErr("CollisionShape2D not found!");
        }
    }

    public void Init(Vector2 targetPosition, Vector2 startPosition, int weaponLevel, double currChargeTime) {
        base.Init(targetPosition, startPosition, weaponLevel);
        
        expandScale = Mathf.Lerp(1.0f, MaxExpandScale, (float)(currChargeTime / 0.5f));
        
        switch (weaponLevel) {
            case 1:
                MaxExpandScale = 5f;
                ExpandedTime = 0.5f;
                break;
            case 2:
                MaxExpandScale = 10f;
                ExpandedTime = 1f;
                break;
            case 3:
                MaxExpandScale = 15f;
                ExpandedTime = 1.5f;
                break;
        }
        
        StartExpandAnimation();
        PlayAttackSound();
    }

    private void StartExpandAnimation() {
        Vector2 targetScale = originalScale * expandScale;
        
        if (expandedSprite != null && sprite != null) {
            sprite.Texture = expandedSprite;
        }
        
        if (collisionShape != null && originalCollisionShape is CircleShape2D circleShape) {
            CircleShape2D newShape = new CircleShape2D();
            newShape.Radius = circleShape.Radius * expandScale;
            collisionShape.Shape = newShape;
        }
        
        scaleTween = CreateTween();
        scaleTween.TweenProperty(this, "scale", targetScale, ExpandAnimationTime)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    public override void _PhysicsProcess(double delta) {
        timer += (float)delta;
        
        if (timer >= ExpandAnimationTime && !isShrinking) {
            damageTickTimer += (float)delta;
            if (damageTickTimer >= DamageInterval) {
                damageTickTimer = 0f;
                ApplyAreaDamage();
            }
        }
        
        if (timer >= ExpandedTime && !isShrinking) {
            StartShrinkAnimation();
        }
    }
    
    private void StartShrinkAnimation() {
        isShrinking = true;
        scaleTween?.Kill();
        scaleTween = CreateTween();
        scaleTween.TweenProperty(this, "scale", Vector2.Zero, ShrinkAnimationTime)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In)
            .Connect("finished", new Callable(this, nameof(OnShrinkComplete)));
    }
    
    private void OnShrinkComplete() {
        QueueFree();
    }
    
    private void ApplyAreaDamage() {
        if (!(collisionShape?.Shape is CircleShape2D circle)) return;

        float radiusSquared = circle.Radius * circle.Radius;
        Vector2 attackPos = GlobalPosition;
        int hits = 0;

        foreach (Enemy enemy in GetTree().GetNodesInGroup("enemies")) {
            if (attackPos.DistanceSquaredTo(enemy.GlobalPosition) <= radiusSquared) {
                enemy.TakeDamage(baseDamage * weaponLevel);
                hits++;
            }
        }

        GD.Print($"Calculator hit {hits} enemies this tick");
    }

    protected override void PlayAttackSound() {
        AudioManager.Instance?.PlayShootSound("pen");
    }
}