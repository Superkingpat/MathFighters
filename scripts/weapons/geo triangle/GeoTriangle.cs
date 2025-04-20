using Godot;
using System;

public partial class GeoTriangle : Weapon {
	[Export] public int maxTriangleCount = 1;
	public override void _Ready() {
		base._Ready();
		AttackAnimation = "attack_geo";
	}
	
	public override Sprite2D GetPickupSprite() {
		return GetNode<Sprite2D>("Sprite2D");
	}

	
}
