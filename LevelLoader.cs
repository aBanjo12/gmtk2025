using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using gmtk2025;

public partial class LevelLoader : Node2D
{
	[Signal] public delegate void LevelChangeEventHandler();
	[Export] NodePath[] levelArrayNodePath;
	[Export] public InputRecorder inputRecorder;
	
	PackedScene player = GD.Load<PackedScene>("res://player.tscn");

	Node2D[] levelArr;
	public List<List<KeyEvent>>[] levelKeyEvents;
	Timer timer;
	Area2D exit;
	bool canChangeLevel = true;
	private int currentLevel = 0;
	private int loopCount = 0;

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.J))
		{
			CallDeferred("GoToNextLevel", levelArr[currentLevel]);
		}
	}

	public List<List<KeyEvent>> getIndividualEvents()
	{
		return levelKeyEvents[currentLevel];
	}

	public override void _Ready()
	{
		levelArr = new Node2D[levelArrayNodePath.Length];
		levelKeyEvents = new List<List<KeyEvent>>[levelArrayNodePath.Length];
		for (int i = 0; i < levelArrayNodePath.Length; i++)
		{
			levelKeyEvents[i] = new List<List<KeyEvent>>();
		}
		for (int i = 0; i < levelArr.Length; i++)
		{
			levelArr[i] = GetNode<Node2D>(levelArrayNodePath[i]);
			if (i != 0)
			{
				levelArr[i].ProcessMode = ProcessModeEnum.Disabled;
				levelArr[i].Visible = false;
			}
		}
		(levelArr[currentLevel] as Level).LevelReady();
		ConnectEnemiesDead(levelArr[currentLevel]);
		ConnectLevelExit(levelArr[currentLevel]);
		ShowExitBlock(levelArr[currentLevel]);

		timer = GetNode<Timer>("Timer");
		timer.Timeout += OnTimerTimout;
		
		inputRecorder = GetTree().Root.GetNode("GameScene").GetNode("Recorder") as InputRecorder;
		inputRecorder.StartRecording();
	}

	public void GoToNextLevel(Node2D nextLevel)
	{
		levelKeyEvents[currentLevel].Add(inputRecorder.StopRecordingGetEvents());
		levelArr[currentLevel].ProcessMode = ProcessModeEnum.Disabled;
		levelArr[currentLevel].Visible = false;
		levelArr[currentLevel].GetNode<TileMapLayer>("Walls").SetEnabled(false);
		exit.BodyEntered -= OnBodyEntered;

		currentLevel++;
		
		if (currentLevel == levelArrayNodePath.Length)
		{
			currentLevel = 0;
			loopCount++;
		}

		levelArr[currentLevel].ProcessMode = ProcessModeEnum.Always;
		levelArr[currentLevel].Visible = true;
		(levelArr[currentLevel] as Level).LevelReady();
		levelArr[currentLevel].GetNode<TileMapLayer>("Walls").SetEnabled(true);
		int i = 0;
		levelKeyEvents[currentLevel].ForEach(x =>
		{
			Node playerNode = player.Instantiate();
			(playerNode as PlayerControl).isSimulated = true;
			GD.Print("sim i " + i);
			(playerNode as PlayerControl).simIndex = i;
			levelArr[currentLevel].AddChild(playerNode);
			i++;
		});

		ConnectEnemiesDead(levelArr[currentLevel]);
		ConnectLevelExit(levelArr[currentLevel]);
		ShowExitBlock(levelArr[currentLevel]);
		
		inputRecorder.StartRecording();
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
			GD.Print("Level Change");
			EmitSignal(SignalName.LevelChange);

			CallDeferred("GoToNextLevel", levelArr[currentLevel]);
			canChangeLevel = false;
			timer.Start();
		}
	}

	public void OnLevelEnemiesDead()
	{
		HideExitBlock(levelArr[currentLevel]);
		GD.Print("Enemies dead");
	}

	public void OnTimerTimout()
	{
		canChangeLevel = true;
	}
}
