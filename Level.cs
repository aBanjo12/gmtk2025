using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node2D
{
	[Signal] public delegate void EnemiesDeadEventHandler();
	List<Enemy> enemies;
	bool sendSignal = true;
	public List<List<KeyEvent>> RecordedEvents = new List<List<KeyEvent>>();
	
	public void LevelReady()
	{
		enemies = new List<Enemy>();
		Godot.Collections.Array<Node> arr = GetChildren();
		foreach (Node i in arr)
		{
			if (i.IsInGroup("Enemy"))
			{
				enemies.Add((Enemy)i);
			}
		}
		
		GD.Print("level init");
		
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
		foreach (Enemy i in enemies)
		{
			if (!i.dead) return false;
		}
		return true;
	}
}
