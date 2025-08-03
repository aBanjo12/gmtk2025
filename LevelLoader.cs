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
	[Export] public int SpawnBound = 64;
	
	PackedScene player = GD.Load<PackedScene>("res://player.tscn");
	PackedScene enemy = GD.Load<PackedScene>("res://enemy.tscn");

	Node2D[] levelArr;
	public List<List<KeyEvent>>[] levelKeyEvents;
	Timer timer;
	Area2D exit;
	bool canChangeLevel = true;
	private int currentLevel = 0;
	private int loopCount = 1;
	bool LevelLoaded = false;
	
	List<int> Seeds = new List<int>();
	public int[] enemiesPerRoom = { 2, 2, 2 };

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
		Seeds.Add(new Random().Next());
			
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

		Node2D p = (Node2D)GetTree().Root.GetNode("GameScene/Player");
		SpawnEnemies(p, 0);
		CallDeferred(nameof(LevelReadyDeferred));

		
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
		foreach (var node in GetTree().GetNodesInGroup("Bullet"))
		{
			node.GetParent().RemoveChild(node);
		}
		
		levelKeyEvents[currentLevel].Add(inputRecorder.StopRecordingGetEvents());
		levelArr[currentLevel].ProcessMode = ProcessModeEnum.Disabled;
		levelArr[currentLevel].Visible = false;
		levelArr[currentLevel].GetNode<TileMapLayer>("Walls").SetEnabled(false);
		exit.BodyEntered -= OnBodyEntered;

		currentLevel++;
		Seeds.Add(new Random().Next());
		
		if (currentLevel == levelArrayNodePath.Length)
		{
			currentLevel = 0;
			loopCount++;
		}

		LevelLoaded = false;
		SpawnEnemies((Node2D)GetTree().Root.GetNode("GameScene/Player"), 0);

		levelArr[currentLevel].ProcessMode = ProcessModeEnum.Always;
		levelArr[currentLevel].Visible = true;
		levelArr[currentLevel].GetNode<TileMapLayer>("Walls").SetEnabled(true);
		int i = 0;
		levelKeyEvents[currentLevel].ForEach(x =>
		{
			Node2D playerNode = (Node2D)player.Instantiate();
			(playerNode as PlayerControl).isSimulated = true;
			GD.Print("sim i " + i);
			(playerNode as PlayerControl).simIndex = i;
			levelArr[currentLevel].AddChild(playerNode);
			i++;
			SpawnEnemies(playerNode, i);
		});

		ConnectEnemiesDead(levelArr[currentLevel]);
		ConnectLevelExit(levelArr[currentLevel]);
		ShowExitBlock(levelArr[currentLevel]);
		
		CallDeferred(nameof(LevelReadyDeferred));
		
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
			if (body is PlayerControl _player)
			{
				if (_player.isSimulated)
				{
					_player.GetParent().RemoveChild(_player);
					return;
				}
			}
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

	public void SpawnEnemies(Node2D follow, int i)
	{
		Random rand = new Random(Seeds[currentLevel + (i) * 3]);
		GD.Print("spawning with " + (currentLevel + (i * 3)));
		for (int j = 0; j < enemiesPerRoom[currentLevel]; j++)
		{
			Node2D e = (Node2D)enemy.Instantiate();
			Vector2 pos;
			pos.X = rand.Next(0, SpawnBound*2) - SpawnBound;
			pos.Y = rand.Next(0, SpawnBound*2) - SpawnBound;
			pos.X += pos.X > 0 ? 64 : -64;
			pos.Y += pos.Y > 0 ? 64 : -64;
			if (currentLevel == 1)
			{
				while (Math.Abs(pos.X) > 64 && Math.Abs(pos.Y) > 64)
				{
					pos.X = rand.Next(0, SpawnBound*2) - SpawnBound;
					pos.Y = rand.Next(0, SpawnBound*2) - SpawnBound;
				}
			}
			e.Position = pos;
			(e as Enemy).follow = follow;
			CallDeferred(nameof(DeferredAddChild), e);
			//dGD.Print(e.Name + " " + e.GetParent().Name);
		}
		
	}
	
	private void DeferredAddChild(Node2D child)
	{
		GetTree().Root.GetNode("GameScene").AddChild(child);
	}

	private void LevelReadyDeferred()
	{
		if (!LevelLoaded)
		{
			(levelArr[currentLevel] as Level).LevelReady();
			LevelLoaded = true;
		}
	}
}
