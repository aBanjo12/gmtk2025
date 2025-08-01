using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed;
	[Export] public int Health = 10;
	public bool dead = false;

	Area2D hurtbox;
	Timer timer;


	PlayerControl player;


	public override void _Ready()
	{
		player = (PlayerControl)GetTree().Root.GetNode("GameScene/Player").GetNode("CharacterBody2D");

		hurtbox = GetNode<Area2D>("HurtBox");
		hurtbox.BodyEntered += OnBodyEntered;

		timer = GetNode<Timer>("Timer");
		timer.Timeout += Timout;
	}

	public override void _PhysicsProcess(double delta)
	{
		EnemyMovement(delta);
	}

	public void EnemyMovement(double delta)
	{
		Vector2 playerPos = player.Position;
		Vector2 posDelta = playerPos - GlobalPosition;
		Velocity = posDelta.Normalized() * Speed;
		MoveAndSlide();
	}

	public void OnBodyEntered(Node2D body)
	{
		Health--;
		//GD.Print("player hurt " + PlayerHealth);
		if (Health <= 0)
			CallDeferred("Die");

		CallDeferred("DisableHurtbox");
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

	public void Die()
	{
		ProcessMode = ProcessModeEnum.Disabled;
		Visible = false;
		dead = true;
	}
}
