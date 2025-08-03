using Godot;
using System;
using gmtk2025;

public partial class GunScript : Node2D
{
	[Export]
	public float FireRate;
	//bullets per second

	private float timeSinceLastShot = 0f;
	
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

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			float fireInterval = 1.0f / FireRate;
			if (timeSinceLastShot >= fireInterval)
			{
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
		GetTree().Root.GetNode("GameScene").AddChild(bullet);
		(GetTree().Root.GetNode<Node>("GlobalEvents") as GlobalEvents).EmitFireEvent(direction.Normalized());
	}
}
