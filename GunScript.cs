using Godot;
using System;

public partial class GunScript : Node2D
{
	[Export]
	public float FireRate;
	//bullets per second

	private float timeSinceLastShot = 0f;
	
	[Export] public CharacterBody2D FollowTarget;

	private Node root;
	private PackedScene bulletScene;

	private Vector2 direction = Vector2.Zero;

	public override void _Ready()
	{
		root = GetTree().Root.GetChildren()[0];
		bulletScene = (PackedScene)ResourceLoader.Load("res://bullet.tscn");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Vector2 mousePosition = GetGlobalMousePosition();
		direction = mousePosition - GlobalPosition;
		float angle = direction.Angle();
		Rotation = angle + (float)Math.PI/2;
		Position = new Vector2(FollowTarget.Transform.Origin.X, FollowTarget.Transform.Origin.Y);

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			float fireInterval = 1.0f / FireRate;
			if (timeSinceLastShot >= fireInterval)
			{
				GD.Print("firing");

				FireBullet();
				timeSinceLastShot = 0f;
			}
		}
		timeSinceLastShot += (float)delta;
	}

	public void FireBullet()
	{
		Node bullet = bulletScene.Instantiate();
		BulletMover mover = bullet as BulletMover;
		mover.Position = GlobalPosition;
		mover.Angle = direction.Normalized();
		GetTree().Root.AddChild(bullet);
	}
}
