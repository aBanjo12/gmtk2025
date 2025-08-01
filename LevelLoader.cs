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
    bool canChangeLevel = true;

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

        timer = GetNode<Timer>("Timer");
        timer.Timeout += OnTimerTimout;
    }
    public void OnBodyEntered(Node2D body)
    {
        if (canChangeLevel)
        {
            GD.Print("Level Change");
            EmitSignal(SignalName.LevelChange);

            CallDeferred("GoToNextLevel", levelArr[1]);
            canChangeLevel = false;
            timer.Start();
        }
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
    }

    public void ReadyLevelExit(Node2D level)
    {
        Area2D exit = level.GetNode<Area2D>("Exit");
        exit.BodyEntered += OnBodyEntered;
    }

    public void OnTimerTimout() {
        canChangeLevel = true;
    }
}
