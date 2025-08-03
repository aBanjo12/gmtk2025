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
	[Export] public float flashTimer = 0.1f;

	[Signal]
	public delegate void HurtPlayerEventHandler();

	Area2D hurtbox;
	Timer timer;
	bool canFlash;
	Sprite2D sprite2D;

	public override void _Ready()
	{
		LevelLoader levelLoader = GetTree().Root.GetNode<LevelLoader>("GameScene/LevelLoader");
		levelLoader.LevelChange += OnLevelChange;

		hurtbox = GetNode<Area2D>("HurtBox");
		hurtbox.BodyEntered += OnBodyEntered;

		timer = GetNode<Timer>("Timer");
		timer.Timeout += Timout;

		sprite2D = GetNode<Sprite2D>("Sprite2D");

		HurtPlayer += HandleHurtPlayer;

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
}
