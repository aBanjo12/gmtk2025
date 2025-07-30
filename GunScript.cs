using Godot;
using System;

public partial class GunScript : Node
{
	[Export]
	public double FireRate;
	[Export] public CharacterBody2D FollowTarget;
	[Export] public Sprite2D Sprite;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 mousePosition = Sprite.GetGlobalMousePosition();
		Vector2 direction = mousePosition - Sprite.GlobalPosition;
		float angle = direction.Angle();
		Sprite.Rotation = angle + (float)Math.PI/2;
		Sprite.Position = new Vector2(FollowTarget.Transform.Origin.X, FollowTarget.Transform.Origin.Y);
	}
}
