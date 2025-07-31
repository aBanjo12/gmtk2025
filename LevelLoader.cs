using Godot;
using System;
using System.Linq;

public partial class LevelLoader : Node2D
{
	[Signal] public delegate void LevelChangeEventHandler();
	[Export] NodePath[] levelArrayNodePath;

	Node2D[] levelArr;
	Node2D activeLevel;
	public override void _Ready()
	{
		levelArr = new Node2D[levelArrayNodePath.Length];
		for (int i = 0; i < levelArr.Count(); i++)
		{
			levelArr[i] = GetNode<Node2D>(levelArrayNodePath[i]);
			if (i != 0)
			{
				levelArr[i].ProcessMode = ProcessModeEnum.Disabled;
				levelArr[i].Visible = false;
			}
		}
		activeLevel = levelArr[0];
		ReadyLevelExit(activeLevel);
		
	}
	public void OnBodyEntered(Node2D body)
	{
		GD.Print("Level Change");
		GoToNextLevel(levelArr[1]);
	}

	public void GoToNextLevel(Node2D nextLevel)
	{
		activeLevel.ProcessMode = ProcessModeEnum.Disabled;
		activeLevel.Visible = false;
		activeLevel.GetNode<Area2D>("Exit").BodyEntered -= OnBodyEntered;

		activeLevel = nextLevel;
		activeLevel.ProcessMode = ProcessModeEnum.Always;
		activeLevel.Visible = true;
		ReadyLevelExit(activeLevel);
		EmitSignal(SignalName.LevelChange);
	}

	public void ReadyLevelExit(Node2D level)
	{
		Area2D exit = level.GetNode<Area2D>("Exit");
		exit.BodyEntered += OnBodyEntered;
	}
}
