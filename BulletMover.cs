using Godot;
using System;

public partial class BulletMover : Node2D
{
	[Export] public Vector2 Angle;
	[Export] public float Speed;
	
	[Export] public RayCast2D North;
	[Export] public RayCast2D South;
	[Export] public RayCast2D East;
	[Export] public RayCast2D West;

	[Export] public int BounceLimit = 3;

	public bool CanHitPlayer = false;

	private int yCooldown = 0;
	private int xCooldown = 0;

	public override void _Ready()
	{
		var hurtBox = GetTree().Root.GetNode<Node2D>("GameScene/Player").GetNode<Area2D>("HurtBox");
		hurtBox.BodyExited += OnBodyExit;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += Angle * Speed * (float)delta;
		if (North.IsColliding() && Angle.Y < 0)
		{
			Angle.Y = -Angle.Y;
			BounceLimit--;
			if (BounceLimit == 0)
				QueueFree();
		}

		if ((South.IsColliding()) && Angle.Y > 0)
		{
			Angle.Y = -Angle.Y;
			BounceLimit--;
			if (BounceLimit == 0)
				QueueFree();
		}

		if (East.IsColliding() && Angle.X > 0)
		{
			Angle.X = -Angle.X;
			BounceLimit--;
			if (BounceLimit == 0)
				QueueFree();
		}

		if (West.IsColliding() && Angle.X < 0)
		{
			Angle.X = -Angle.X;
			BounceLimit--;
			if (BounceLimit == 0)
				QueueFree();
		}
	}

	public void OnBodyExit(Node2D body)
	{
		CanHitPlayer = true;
	}
}
