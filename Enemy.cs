using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed;
	[Export] public int Health = 10;
	public bool dead = false;

	Area2D hurtbox;
	Area2D	 atkRange;
	Timer timer;
	Timer atkTimer;
	AnimatedSprite2D animation;
	bool attacking = false;
	bool canAttack = true;

	PlayerControl player;


	public override void _Ready()
	{
		player = (PlayerControl)GetTree().Root.GetNode("GameScene/Player");

		hurtbox = GetNode<Area2D>("HurtBox");
		hurtbox.BodyEntered += OnBodyEntered;

		atkRange = GetNode<Area2D>("AttackRange");
		((AttackRange)atkRange).PlayerInRange += OnPlayerInRange;

		timer = GetNode<Timer>("Timer");
		timer.Timeout += Timout;

		atkTimer = GetNode<Timer>("AttackTimer");
		atkTimer.Timeout += OnAttackTimerTimout;
		GD.Print(atkTimer);

		animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animation.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (attacking)
		{
			Attack();
		}
		else if (!dead)
		{
			EnemyMovement(delta);
		}

	}

	public void EnemyMovement(double delta)
	{
		Vector2 playerPos = player.Position;
		Vector2 posDelta = playerPos - GlobalPosition;
		Velocity = posDelta.Normalized() * Speed;
		if (Velocity.X > 0)
		{
			Scale = new Vector2(1, 1);
			RotationDegrees = 0;
		}
		else if (Velocity.X < 0)
		{
			Scale = new Vector2(1, -1);
			RotationDegrees = 180;
		}
		MoveAndSlide();

		animation.Play("Walking");
	}

	public void OnBodyEntered(Node2D body)
	{
		Health--;
		//GD.Print("player hurt " + PlayerHealth);
		if (Health <= 0)
			CallDeferred("Die");

		CallDeferred("DisableHurtbox");
	}

	public void OnPlayerInRange()
	{
		if (canAttack)
		{
			canAttack = false;
			atkTimer.Start();
			attacking = true;
		}
	}

	public void OnAnimationFinished()
	{
		attacking = false;
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

	public void OnAttackTimerTimout()
	{
		canAttack = true;
	}

	public void Die()
	{
		// ProcessMode = ProcessModeEnum.Disabled;
		// Visible = false;
		animation.Play("Dying");
		dead = true;
	}

	public void Attack()
	{
		animation.Play("Attacking");
	}
}
