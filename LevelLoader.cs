using Godot;
using System;
using System.Linq;

public partial class LevelLoader : Node2D
{
	[Signal] public delegate void LevelChangeEventHandler();
	[Export] NodePath[] levelArrayNodePath;

	Node2D[] levelArr;
	Node2D activeLevel;
	Timer timer;
	Area2D exit;
	bool canChangeLevel = true;
	private int currentLevel = 0;

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
		activeLevel = levelArr[currentLevel];
		ConnectEnemiesDead(activeLevel);
		ConnectLevelExit(activeLevel);
		ShowExitBlock(activeLevel);

		timer = GetNode<Timer>("Timer");
		timer.Timeout += OnTimerTimout;
	}

	public void GoToNextLevel(Node2D nextLevel)
	{
		activeLevel.ProcessMode = ProcessModeEnum.Disabled;
		activeLevel.Visible = false;
		activeLevel.GetNode<TileMapLayer>("Walls").SetEnabled(false);
		exit.BodyEntered -= OnBodyEntered;

		activeLevel = nextLevel;
		activeLevel.ProcessMode = ProcessModeEnum.Always;
		activeLevel.Visible = true;
		activeLevel.GetNode<TileMapLayer>("Walls").SetEnabled(true);

		ConnectEnemiesDead(activeLevel);
		ConnectLevelExit(activeLevel);
		ShowExitBlock(activeLevel);
	}

	public void ConnectLevelExit(Node2D level)
	{
		exit = level.GetNode<Area2D>("Exit");
		exit.BodyEntered += OnBodyEntered;
	}

	public void ConnectEnemiesDead(Node2D level)
	{
		((Level)level).EnemiesDead += OnLevelEnemiesDead;
	}

	public void ShowExitBlock(Node2D level)
	{
		var block = level.GetNode<StaticBody2D>("OuterWalls").GetNode<CollisionShape2D>("ExitBlock");
		block.Disabled = false;
		block.Visible = true;
	}

	public void HideExitBlock(Node2D level)
	{
		var block = level.GetNode<StaticBody2D>("OuterWalls").GetNode<CollisionShape2D>("ExitBlock");
		block.Disabled = true;
		block.Visible = false;
	}

	public void OnBodyEntered(Node2D body)
	{
		if (canChangeLevel)
		{
			currentLevel++;
			if (currentLevel == levelArrayNodePath.Length)
				currentLevel = 0;
			GD.Print("Level Change");
			EmitSignal(SignalName.LevelChange);

			CallDeferred("GoToNextLevel", levelArr[currentLevel]);
			canChangeLevel = false;
			timer.Start();
		}
	}

	public void OnLevelEnemiesDead()
	{
		HideExitBlock(activeLevel);
		GD.Print("Enemies dead");
	}

	public void OnTimerTimout()
	{
		canChangeLevel = true;
	}
}
