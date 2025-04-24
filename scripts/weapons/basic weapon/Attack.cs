using Godot;
using System;

//This is the Bullet class
public partial class Attack : CharacterBody2D {
	//Speed is an external function that we can change in the Godot UI
	[Export] public float Speed = 500.0f;
	protected Vector2 direction;

	//_Ready is called when the root node (Bullet) entered the scene
	//VisibleOnScreenNotifier2D has multiple functionalities that tell us if an object is on the screen
	//All actions(functions) in notifier.ScreenExited are executed when the object has exited the screen
	public override void _Ready() {
		VisibleOnScreenNotifier2D notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += OnScreenExited;
	}

	//Its an initialization function. I think it's obvious what it does
	public virtual void Init(Vector2 targetPosition, Vector2 startPosition) {
		Position = startPosition;
		direction = (targetPosition - startPosition).Normalized();
		Rotation = direction.Angle();
	}

	//Moves the Bullet in the direction of direction at the speed of speed. Delta time is taken care of automaticly by Godot
	public override void _PhysicsProcess(double delta) {
		Velocity = direction * Speed;
		MoveAndSlide();
	}

	//Is called when the Bullet instance exits the screen.
	protected virtual void OnScreenExited() {
		GD.Print("Bullet was deleted!");
		//Deletes the Bullet instance at the end of the current frame
		QueueFree();
	}

	   protected virtual void PlayAttackSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShootSound("default");
        }
    }
}
