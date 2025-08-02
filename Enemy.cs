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

	public PlayerControl player;
	
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

		animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animation.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (attacking)
		{
			// Attack();
		}
		else if (!dead)
		{
			EnemyMovement(delta);
		}

	}

	public virtual void EnemyMovement(double delta)
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

	public virtual void OnBodyEntered(Node2D body)
	{
		GD.Print("A");
		Health--;
		if (Health <= 0)
			CallDeferred("Die");

		CallDeferred("DisableHurtbox");
	}

	public virtual void OnPlayerInRange()
	{
		if (canAttack)
		{
			canAttack = false;
			atkTimer.Start();
			attacking = true;
			Attack();
		}
	}

	public virtual void OnAnimationFinished()
	{
		attacking = false;
	}

	public virtual void DisableHurtbox()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Disabled;
		timer.Start();
	}

	public virtual void Timout()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Always;
	}

	public virtual void OnAttackTimerTimout()
    {
		canAttack = true;
	}

	public virtual void Die()
	{
		// ProcessMode = ProcessModeEnum.Disabled;
		// Visible = false;
		animation.Play("Dying");
		dead = true;
	}

	public virtual void Attack()
	{
		animation.Play("Attacking");
	}
}
