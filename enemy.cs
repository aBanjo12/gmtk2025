using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed;

	PlayerControl player;


	public override void _Ready()
	{
		player = (PlayerControl)GetTree().Root.GetNode("GameScene/Player").GetNode("CharacterBody2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		EnemyMovement(delta);
	}

	public void EnemyMovement(double delta)
	{
		Vector2 playerPos = player.Position;
		Vector2 posDelta = playerPos - GlobalPosition;
		Velocity = posDelta.Normalized() * Speed;
		MoveAndSlide();
	}

	public void SetPos(Vector2 pos)
	{
		Position = pos;
	}
}
