using Godot;
using System;

public partial class Weapon : Node2D {
	[Export] public PackedScene AttackScene;
	[Export] public string AttackAnimation = "attack";
	[Export] public int WeaponLevel = 1;

	public override void _Process(double delta) { }

	public virtual Sprite2D GetPickupSprite() { return null; }

	public virtual void TryShoot(Vector2 targetPosition) { }
}
