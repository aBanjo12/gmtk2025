using Godot;
using System;

public partial class AttackRange : Area2D
{
    [Signal] public delegate void PlayerInRangeEventHandler();

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (GetOverlappingBodies().Count != 0)
        {
            EmitSignal(SignalName.PlayerInRange);
        }
    }
}
