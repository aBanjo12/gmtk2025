using Godot;
using System;

public partial class BulletMover : Node2D
{
	[Export] public Vector2 Angle;
	[Export] public float Speed;

	public override void _PhysicsProcess(double delta)
	{
		Position += Angle * Speed * (float)delta;
	}
}
