using Godot;
using System;

public partial class Arrow : RigidBody2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public void OnBodyEntered(Node body)
    {
        QueueFree();
        GD.Print("A");
    }
}
