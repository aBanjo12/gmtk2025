using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node2D
{
    [Signal] public delegate void EnemiesDeadEventHandler();
    List<Node2D> enemies;
    bool sendSignal = true;
    public override void _Ready()
    {
        enemies = new List<Node2D>();
        Godot.Collections.Array<Node> arr = GetChildren();
        foreach (Node i in arr)
        {
            if (i.IsInGroup("Enemy"))
            {
                GD.Print("A");
                enemies.Add((Node2D)i);
            }
        }
        GD.Print("E: " + enemies.Count());
    }

    public override void _Process(double delta)
    {
        if (sendSignal && AllEnemiesDead())
        {
            EmitSignal(SignalName.EnemiesDead);
            sendSignal = false;
        }
    }

    public bool AllEnemiesDead()
    {
        foreach (Node2D i in enemies)
        {
            if (!i.GetNode<Enemy>("CharacterBody2D").dead) return false;
        }
        return true;
    }
}
