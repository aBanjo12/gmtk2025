using Godot;
using System;

public partial class enemy : CharacterBody2D
{
	[Export] public float Speed;
	PlayerControl player;
	
	
	public override void _Ready()
	{
		player = (PlayerControl)GetParent().GetParent().GetNode("Player").GetNode("CharacterBody2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		EnemyMovement(delta);
	}

	public void EnemyMovement(double delta)
	{
		Vector2 playerPos = ((CharacterBody2D)player).Position;
		Vector2 posDelta = playerPos - Position;
		Velocity = posDelta.Normalized() * Speed;
		MoveAndSlide();
	}
	
	

}
