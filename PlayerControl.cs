using Godot;
using System;
using Environment = Godot.Environment;

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

	[Signal]
	public delegate void HurtPlayerEventHandler();

	Area2D hurtbox;
	Area2D bulletLeave;
	Timer timer;

	public override void _Ready()
	{
		LevelLoader levelLoader = GetTree().Root.GetNode<LevelLoader>("GameScene/LevelLoader");
		levelLoader.LevelChange += OnLevelChange;

		hurtbox = GetNode<Area2D>("HurtBox");
		hurtbox.BodyEntered += OnBodyEntered;

		bulletLeave = GetNode<Area2D>("BulletLeave");
		bulletLeave.BodyExited += OnBodyExited;

		timer = GetNode<Timer>("Timer");
		timer.Timeout += Timout;

		HurtPlayer += HandleHurtPlayer;
	}

	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta);
	}

	public void PlayerMovement(double delta)
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

		// if (IsInsideTargetShape())
		// {
		// 	EmitSignal(SignalName.HurtPlayer);
		// }

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
		//GD.Print("player hurt " + PlayerHealth);
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
		if (!((BulletMover)body.GetParent()).CanHitPlayer)
		{
			return;
		}
		PlayerHealth--;
		GD.Print("player hurt " + PlayerHealth);
		if (PlayerHealth <= 0)
			PlayerDie();
		
		CallDeferred("DisableHurtbox");
	}

	public void OnBodyExited(Node2D body)
	{
		((BulletMover)body.GetParent()).CanHitPlayer = true;
	}

	public void DisableHurtbox()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Disabled;
		timer.Start();
	}

	public void Timout()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Always;
	}
}
