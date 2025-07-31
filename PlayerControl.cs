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

	public override void _Ready()
	{
		LevelLoader levelLoader = GetTree().Root.GetNode<LevelLoader>("GameScene/LevelLoader");
		levelLoader.LevelChange += OnLevelChange;

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

		if (IsInsideTargetShape())
		{
			EmitSignal(SignalName.HurtPlayer);
		}

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
		GD.Print("player hurt " + PlayerHealth);
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

}
