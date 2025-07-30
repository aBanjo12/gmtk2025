using Godot;
using System;

public partial class PlayerControl : CharacterBody2D
{
	//units per second
	[Export] public int Speed;

	[Export] public Key UpKey;
	[Export] public Key DownKey;
	[Export] public Key LeftKey;
	[Export] public Key RightKey;

	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta);
	}

	public void PlayerMovement(double delta)
	{
		Vector2 movement = Vector2.Zero;
		if (Input.IsKeyPressed(UpKey))
			movement += new Vector2(0, -1);
		if (Input.IsKeyPressed(DownKey))
			movement += new Vector2(0, 1);
		if (Input.IsKeyPressed(RightKey))
			movement += new Vector2(1, 0);
		if (Input.IsKeyPressed(LeftKey))
			movement += new Vector2(-1, 0);
		
		movement = movement.Normalized();
		
		Velocity = movement * Speed;
		MoveAndSlide();

	}
	
	

}
