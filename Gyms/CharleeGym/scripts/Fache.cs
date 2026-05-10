using System;
using Godot;

public partial class Fache : Enemy
{
	[Export]
	public float JumpChance = 0.05f;

	[Export]
	public float JumpVelocity = -150.0f;

	[ExportGroup("Attack")]
	[Export]
	public float MaxWait = 10.0f;

	[Export]
	public float MinWait = 1.0f;

	[Export]
	public Timer Timer;

	[Export]
	public CharacterBody2D Character;

	[Export]
	public PackedScene ProjectileScene;
	private float NextShot;
	private int _Direction_Faced;

	private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

	public override void _Ready()
	{
		Timer.Timeout += DecideNextAction;
		RestartTimer();
	}

	public override void _PhysicsProcess(double delta)
	{
		float directionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
		int dir = directionX < 0 ? 1 : -1;

		if (dir != _Direction_Faced)
		{
			Sprite.FlipH = dir < 0;
			_Direction_Faced = dir;
		}

		Vector2 velocity = Velocity;

		// gravity
		velocity.Y += _Gravity_Force * (float)delta;

		Velocity = velocity;

		MoveAndSlide();
	}

	public void RandomNextShot()
	{
		Random rand = new Random();

		NextShot = (float)(MinWait + rand.NextDouble() * (MaxWait - MinWait));
	}

	public void RestartTimer()
	{
		Sprite.Play("idle");
		RandomNextShot();

		Timer.WaitTime = NextShot;
		Timer.Start();
	}

	public void DecideNextAction()
	{
		if (GD.Randf() < JumpChance)
		{
			Jump();
			return;
		}

		Attack();
	}

	public async void Attack()
	{
		Sprite.Play("attack");

		await ToSignal(GetTree().CreateTimer(0.5), "timeout");

		Shoot();

		await ToSignal(GetTree().CreateTimer(0.5), "timeout");

		RestartTimer();
	}

	public void Shoot()
	{
		ProjectileArc projectile = ProjectileScene.Instantiate<ProjectileArc>();
		GetParent().AddChild(projectile);
		projectile.Launcher = this;
		projectile.GlobalPosition = GlobalPosition + new Vector2(-_Direction_Faced * 20, -10);
		projectile.LinearVelocity = new Vector2(-_Direction_Faced * 200, -300);
	}

	public async void Jump()
	{
		Velocity = new Vector2(Velocity.X, JumpVelocity);

		Sprite.Play("jump");

		await ToSignal(Sprite, AnimatedSprite2D.SignalName.AnimationFinished);

		RestartTimer();
	}
}
