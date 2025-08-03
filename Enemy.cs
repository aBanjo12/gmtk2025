using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed;
	[Export] public int Health = 10;
	[Export] public float FlashTimer = 0.1f;
	public bool dead = false;
	public Node2D follow;

	Area2D hurtbox;
	Area2D atkRange;
	Timer timer;
	Timer atkTimer;
	AnimatedSprite2D animation;
	bool attacking = false;
	bool canAttack = true;
	bool canFlash = true;
	public override void _Ready()
	{
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

	public override void _Process(double delta)
	{
		if (canFlash && timer.WaitTime - timer.TimeLeft >= FlashTimer)
		{
			FlashTimeout();
			canFlash = false;
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		if (dead)
		{

		}
		else if (attacking)
		{
			// Attack();
		}
		else if (!attacking)
		{
			EnemyMovement(delta);
		}

	}

	public virtual void EnemyMovement(double delta)
	{
		Vector2 playerPos = follow.Position;
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
		if (attacking) attacking = false;
		if (dead)
		{
			GetParent().RemoveChild(this);
		}
	}

	public virtual void DisableHurtbox()
	{
		hurtbox.ProcessMode = ProcessModeEnum.Disabled;
		timer.Start();
		canFlash = true;
		animation.Modulate = Color.FromHtml("#ff0000");
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
		animation.Play("Dying");
		dead = true;
	}

	public virtual void Attack()
	{
		animation.Play("Attacking");
	}
	
	public void FlashTimeout()
	{
		animation.Modulate = Color.FromHtml("#ffffff");
	}
}
