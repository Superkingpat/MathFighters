using Godot;
using System;
using System.Collections.Generic;

public partial class HPBar : Node
{
	[Export] public Sprite2D HPSprite { get; set; }
	private Player Player;
	private Dictionary<int, Texture2D> _presets = new()
	{
		[100] = GD.Load<Texture2D>("res://assets/player/HPBar/hp100.png"),
		[80] = GD.Load<Texture2D>("res://assets/player/HPBar/hp80.png"),
		[50] = GD.Load<Texture2D>("res://assets/player/HPBar/hp50.png"),
		[20] = GD.Load<Texture2D>("res://assets/player/HPBar/hp20.png"),
		[0] = GD.Load<Texture2D>("res://assets/player/HPBar/hp0.png"),

	};

	public override void _Ready()
	{
		Player = GetTree().GetNodesInGroup("player")[0] as Player;
		//GD.Print("HP BAR STARTED!");
		HPSprite.Position += new Vector2(100, 30);
		HPSprite.Scale = new Vector2(3, 3);


		HPSprite.Texture = _presets[100];
	}

	float currentHpPercent=1;
	float newcur = 1;
	public override void _PhysicsProcess(double delta)
	{
		newcur = Player.PlayerStats.CurrentHealth / Player.PlayerStats.BaseHealth;
		if (newcur != currentHpPercent)
		{
			currentHpPercent = newcur;
			if (currentHpPercent >= 0.90)
				HPSprite.Texture = _presets[100];
			else if (currentHpPercent >= 0.70)
				HPSprite.Texture = _presets[80];
			else if (currentHpPercent >= 0.40)
				HPSprite.Texture = _presets[50];
			else if (currentHpPercent >= 0.20)
				HPSprite.Texture = _presets[20];
			else if (currentHpPercent >= 0.0)
				HPSprite.Texture = _presets[0];

			//GD.Print("MAX HP: " + Player.PlayerStats.BaseHealth + " CUR HP: " + Player.PlayerStats.CurrentHealth);
		} 
			
	}

}
