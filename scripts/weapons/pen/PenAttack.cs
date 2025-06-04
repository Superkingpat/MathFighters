using Godot;
using System;

public partial class PenAttack : Attack
{
    [Export] public float ExpandScale = 5.0f;
    [Export] public float TravelTime = 1.0f;
    [Export] public float ExpandedTime = 1.0f;
    [Export] public float DamageInterval = 0.3f;
    [Export] public int baseDamage = 10;
    [Export] public AudioStreamPlayer2D splatSoundPlayer;

    private float timer = 0f;
    private float damageTickTimer = 0f;
    private Vector2 originalScale;
    private bool hasExpanded = false;
    private bool hasStopped = false;
    private bool hasHitEnemy = false;
    private CollisionShape2D collisionShape;
    private Shape2D originalCollisionShape;
    [Export] public float ExpandAnimationTime = 0.2f;
    [Export] public float ShrinkAnimationTime = 0.15f;
    [Export] public Texture2D normalSprite;
    [Export] public Texture2D expandedSprite;
    [Export] public Sprite2D sprite;

    private Tween scaleTween = null;
    private Vector2 targetScale;
    private bool isShrinking = false;

    public override void _Ready()
    {
        base._Ready();
        originalScale = Scale;
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        if (sprite == null)
        {
            sprite = GetNode<Sprite2D>("Sprite2D");
        }

        if (collisionShape != null)
        {
            originalCollisionShape = collisionShape.Shape;
        }
    }

    public override void Init(Vector2 targetPosition, Vector2 startPosition, int WeaponLevel)
    {
        base.Init(targetPosition, startPosition, WeaponLevel);
        TravelTime = (float)GD.Randf() * 1.0f + 0.3f;
        ExpandedTime = (float)GD.Randf() * 0.8f + 0.3f;

        scaleTween = CreateTween();

        switch (WeaponLevel)
        {
            case 1:
                ExpandScale = 5f;
                ExpandedTime = 0.5f;
                break;
            case 2:
                ExpandScale = 10f;
                ExpandedTime = 1f;
                break;
            case 3:
                ExpandScale = 15f;
                ExpandedTime = 1.5f;
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        timer += (float)delta;

        if (!hasStopped && timer >= TravelTime)
        {
            hasStopped = true;
            Speed = 0f;
            timer = 0f;
            ExpandAndStop();
        }

        if (hasExpanded)
        {
            damageTickTimer += (float)delta;
            if (damageTickTimer >= DamageInterval)
            {
                damageTickTimer = 0f;
                ApplyAreaDamage();
            }

            if (timer >= ExpandedTime && !isShrinking)
            {
                StartShrinkAnimation();
            }
        }

        if (!hasStopped)
        {
            base._PhysicsProcess(delta);
            Velocity = direction * Speed;
            var collision = MoveAndCollide(Velocity * (float)delta);
            if (collision != null && collision.GetCollider() is Enemy enemy)
            {
                enemy.TakeDamage(baseDamage * weaponLevel);
                ExpandAndStop();
            }
        }
    }

    private void ExpandAndStop()
    {
        hasStopped = true;
        Speed = 0f;
        timer = 0f;
        targetScale = originalScale * ExpandScale;

        if (splatSoundPlayer != null)
        {
            splatSoundPlayer.PitchScale = (float)GD.RandRange(0.9f, 1.1f);
            splatSoundPlayer.Play();
        }

        if (expandedSprite != null && sprite != null)
        {
            Vector2 scale;
            scale.X = 3f;
            scale.Y = 3f;
            sprite.Texture = expandedSprite;
            sprite.ApplyScale(scale);
        }

        if (collisionShape != null && originalCollisionShape != null)
        {
            if (originalCollisionShape is CircleShape2D circleShape)
            {
                var newShape = new CircleShape2D();
                newShape.Radius = circleShape.Radius * ExpandScale;
                collisionShape.Shape = newShape;
            }
        }

        scaleTween.Kill();
        scaleTween = CreateTween();
        scaleTween.TweenProperty(this, "scale", targetScale, ExpandAnimationTime)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        hasExpanded = true;
    }

    private void StartShrinkAnimation()
    {
        isShrinking = true;
        scaleTween.Kill();
        scaleTween = CreateTween();
        scaleTween.TweenProperty(this, "scale", Vector2.Zero, ShrinkAnimationTime)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In)
            .Connect("finished", new Callable(this, nameof(OnShrinkComplete)));
    }

    private void OnShrinkComplete()
    {
        QueueFree();
        GD.Print("Pen bullet was deleted after shrink animation!");
    }

    private void ApplyAreaDamage()
    {
        if (!(collisionShape?.Shape is CircleShape2D circle)) return;

        float radiusSquared = circle.Radius * circle.Radius;
        Vector2 attackPos = GlobalPosition;
        int hits = 0;

        foreach (Enemy enemy in GetTree().GetNodesInGroup("enemies"))
        {
            if (attackPos.DistanceSquaredTo(enemy.GlobalPosition) <= radiusSquared)
            {
                enemy.TakeDamage(baseDamage * weaponLevel);
                hits++;
            }
        }

        GD.Print($"Hit {hits} enemies this tick");
    }
}