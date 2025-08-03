using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gmtk2025;
using Environment = Godot.Environment;
using Timer = Godot.Timer;

public partial class PlayerControl : CharacterBody2D
{
	//units per second
	[Export] public int Speed;

	[Export] public Key UpKey;
	[Export] public Key DownKey;
	[Export] public Key LeftKey;
	[Export] public Key RightKey;

	[Export] public uint PlayerHealth;

	[Export] public CollisionShape2D MyShape;
	[Export] public int EnemyLayerMask;
	[Export] public float flashTimer = 0.1f;
	
	public bool isSimulated = false;
	public int simIndex = -1;
	
	[Signal]
	public delegate void HurtPlayerEventHandler();

	Area2D hurtbox;
	Timer timer;
	bool canFlash;
	Sprite2D sprite2D;
	
	public bool[] KeyStates = new bool[4];
	
	[Export]
	public float FireRate;
	//bullets per second

	private float timeSinceLastShot = 0f;
	
	private Node root;
	private PackedScene bulletScene;
	
	private readonly Dictionary<Key, int> KeyMap = new Dictionary<Key, int>
	{
		{ Key.W, 0 },
		{ Key.A, 1 },
		{ Key.S, 2 },
		{ Key.D, 3 },
		{ Key.None, 4 },
	};
	
	public async Task WaitSecondsAsync(float seconds)
	{
		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = seconds;
		timer.OneShot = true;
		timer.Start();
		await ToSignal(timer, "timeout");
		timer.QueueFree();
	}

	public async Task ReplayKeyEventsAsync(List<KeyEvent> events)
	{
		// Make a local copy to avoid issues if the original list changes
		var eventsCopy = events.ToList();

		if (eventsCopy.Count == 0)
			return;

		GD.Print($"Replaying {eventsCopy.Count} events...");

		for (int i = 0; i < eventsCopy.Count; i++)
		{
			var current = eventsCopy[i];
			float waitTime = i == 0 ? current.Timestamp : (current.Timestamp - eventsCopy[i - 1].Timestamp);
			GD.Print("wait time " + waitTime);
			waitTime = Mathf.Max(0.001f, waitTime);

			GD.Print($"[{current.Timestamp:0.00} ms] {current.Type}: {current.KeyCode}, Pos: {current.RelativePosition}");

			// Await a timer before applying next event
			await WaitSecondsAsync(waitTime);

			// Apply the key state or mouse click here
			// Example (you can adjust as needed):
			if ((current.Type == KeyEvent.EventType.KeyDown || current.Type == KeyEvent.EventType.KeyUp) && 
				KeyMap.TryGetValue(current.KeyCode, out int index))
			{
				KeyStates[index] = current.Type == KeyEvent.EventType.KeyDown;
			}
			else if (current.Type == KeyEvent.EventType.MouseClick)
			{
				FireBullet(current.RelativePosition);
			}
		}
	}

	public override void _Ready()
	{
		root = GetTree().Root.GetChildren()[0];
		bulletScene = (PackedScene)ResourceLoader.Load("res://bullet.tscn");
		
		LevelLoader levelLoader = GetTree().Root.GetNode<LevelLoader>("GameScene/LevelLoader");
		levelLoader.LevelChange += OnLevelChange;

		hurtbox = GetNode<Area2D>("HurtBox");
		hurtbox.BodyEntered += OnBodyEntered;

		timer = GetNode<Timer>("Timer");
		timer.Timeout += Timout;

		sprite2D = GetNode<Sprite2D>("Sprite2D");

		HurtPlayer += HandleHurtPlayer;

		if (isSimulated)
		{
			
			var list = (GetTree().Root.GetNode("GameScene/LevelLoader") as LevelLoader).getIndividualEvents()[simIndex];
			bool isMainThread = Thread.CurrentThread.ManagedThreadId == 1;
			GD.Print("runing " + isMainThread);
			_ = ReplayKeyEventsAsync(list);

		}
		else
		{
			CollisionMask += 128;
		}

	}

	public override void _Process(double delta)
	{
		if (canFlash && timer.WaitTime - timer.TimeLeft >= flashTimer)
		{
			FlashTimeout();
			canFlash = false;
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		if (!isSimulated)
		{
			GunPhysicsProcess(delta);
			PlayerMovement();
		}
		else
		{
			SimulatePlayerMovement();
		}
		
	}

	public void PlayerMovement()
	{
		Vector2 movement = Vector2.Zero;
		if (Input.IsKeyPressed(UpKey))
			movement += new Vector2(0, -1);
		if (Input.IsKeyPressed(DownKey))
			movement += new Vector2(0, 1);
		if (Input.IsKeyPressed(RightKey))
			movement += new Vector2(1, 0);
		if (Input.IsKeyPressed(LeftKey))
			movement += new Vector2(-1, 0);

		movement = movement.Normalized();

		Velocity = movement * Speed;
		MoveAndSlide();

	}

	public void SimulatePlayerMovement()
	{
		Vector2 movement = Vector2.Zero;
		if (KeyStates[0])
			movement += new Vector2(0, -1);
		if (KeyStates[1])
			movement += new Vector2(-1, 0);
		if (KeyStates[2])
			movement += new Vector2(0, 1);
		if (KeyStates[3])
			movement += new Vector2(1, 0);

		movement = movement.Normalized();

		Velocity = movement * Speed;
		MoveAndSlide();
	}

	public void OnLevelChange()
	{
		ResetPlayerPos();
	}

	public void ResetPlayerPos()
	{
		Position = new Vector2(0, 96);
	}

	public void HandleHurtPlayer()
	{
		PlayerHealth--;
		if (PlayerHealth <= 0)
			PlayerDie();
	}

	public void PlayerDie()
	{
		GD.Print("player dead");
	}

	private bool IsInsideTargetShape()
	{
		if (MyShape == null || MyShape.Shape == null)
			return false;

		var space = GetWorld2D().DirectSpaceState;

		var shapeQuery = new PhysicsShapeQueryParameters2D
		{
			Shape = MyShape.Shape,
			Transform = MyShape.GetGlobalTransform(),
			CollisionMask = (uint)EnemyLayerMask,
			CollideWithAreas = true,
			CollideWithBodies = true
		};

		var results = space.IntersectShape(shapeQuery, 1); // maxResults = 1 for performance
		return results.Count > 0;
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Bullet") && !((BulletMover)body.GetParent()).CanHitPlayer)
		{
			return;
		}
		PlayerHealth--;
		if (PlayerHealth <= 0)
			PlayerDie();

		CallDeferred("DisableHurtbox");
	}

	public void DisableHurtbox()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Disabled;
		timer.Start();
		canFlash = true;
		sprite2D.Modulate = Color.FromHtml("#ff0000");
	}

	public void Timout()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Always;
	}

	public void FlashTimeout()
	{
		sprite2D.Modulate = Color.FromHtml("#ffffff");
	}
	
	public void GunPhysicsProcess(double delta)
	{
		Vector2 mousePosition = GetGlobalMousePosition();
		Vector2 direction = mousePosition - GlobalPosition;

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			float fireInterval = 1.0f / FireRate;
			if (timeSinceLastShot >= fireInterval)
			{
				FireBullet(direction);
				timeSinceLastShot = 0f;
			}
		}
		timeSinceLastShot += (float)delta;
	}
	
	public void FireBullet(Vector2 direction)
	{
		Node bullet = bulletScene.Instantiate();
		BulletMover mover = bullet as BulletMover;
		mover.Position = GlobalPosition;
		mover.Angle = direction.Normalized();
		GetTree().Root.GetNode("GameScene").AddChild(bullet);
		(GetTree().Root.GetNode<Node>("GlobalEvents") as GlobalEvents).EmitFireEvent(direction.Normalized());
	}
}
