using Godot;
using System;

public partial class exit : Area2D
{
    public override void _Ready()
    {
        // var Collision2d = GetNode<CollisionShape2D>("CollisionShape");
        // Collision2d.OnBodyEntered();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print("Exit");
    }
}
