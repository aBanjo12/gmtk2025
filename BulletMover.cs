using Godot;
using System;

public partial class BulletMover : Node2D
{
	[Export] public Vector2 Angle;
	[Export] public float Speed;
	
	[Export] public int WallLayerMask;

	[Export] public RayCast2D North;
	[Export] public RayCast2D South;
	[Export] public RayCast2D East;
	[Export] public RayCast2D West;
	
	private int yCooldown = 0;
	private int xCooldown = 0;

	public override void _PhysicsProcess(double delta)
	{
		Position += Angle * Speed * (float)delta;
		if ((North.IsColliding() || South.IsColliding()) && yCooldown == 0)
		{
			yCooldown = 3;
			Angle.Y = -Angle.Y;
		}
		if ((East.IsColliding() || West.IsColliding()) && xCooldown == 0)
		{
			xCooldown = 3;
			Angle.X = -Angle.X;
		}
		
		if (yCooldown > 0)
			yCooldown--;
		if (xCooldown > 0)
			xCooldown--;
	}
	
	
}
