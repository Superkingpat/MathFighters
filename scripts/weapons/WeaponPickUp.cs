using Godot;
using System;

public partial class WeaponPickUp : Area2D {
	[Export] public PackedScene WeaponScene;
	private Weapon weapon;

	public override void _Ready() {
		weapon = GetWeapon();
		BodyEntered += OnBodyEntered;
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.Texture = weapon.GetPickupSprite().Texture;
		sprite.ApplyScale(weapon.GetPickupSprite().Scale);
	}

	private void OnBodyEntered(Node body) {
		if (body is Player player) {
			player.TryPickupWeapon(this);
		}
	}

	public Weapon GetWeapon() {
		if (WeaponScene != null) {
			return (Weapon)WeaponScene.Instantiate();
		}

		GD.PrintErr("No WeaponScene assigned!");
		return null;
	}
}
