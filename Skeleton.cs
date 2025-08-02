using Godot;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Resolvers;

public partial class Skeleton : Enemy
{
    PackedScene arrow;
    float speed = 20;

    public override void _Ready()
    {
        base._Ready();
        arrow = (PackedScene)ResourceLoader.Load("res://arrow.tscn");
    }

    public override void Attack()
    {
        base.Attack();
        var arrowInstance = arrow.Instantiate() as RigidBody2D;
        arrowInstance.Position = GlobalPosition;
        float rotation = (float)Math.Atan2(player.GlobalPosition.Y - GlobalPosition.Y, player.GlobalPosition.X - GlobalPosition.X);
        arrowInstance.Rotation = rotation;
        arrowInstance.ApplyImpulse(new Vector2(Speed, 0).Rotated(rotation));
        GetTree().Root.AddChild(arrowInstance);
    }
}
