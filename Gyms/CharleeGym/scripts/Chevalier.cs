using System;
using System.Diagnostics;
using Godot;

public partial class Chevalier : Enemy
{
	[ExportGroup("Attack")]
	[Export]
	public float MaxWait;

	[Export]
	public float MinWait;

	[Export]
	public Timer Timer;

	[Export]
	public CharacterBody2D Character;

	[Export]
	public Shield Shield;

	[Export]
	public PackedScene ProjectileScene;
	private float NextShot;
	private int _Direction_Faced;

	private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

	public override void _Ready()
	{
		if (Shield != null)
		{
			AddCollisionExceptionWith(Shield);
			Shield.AddCollisionExceptionWith(this);
		}

		Timer.Timeout += Attack;
		RestartTimer();
	}

	public override void _PhysicsProcess(double delta)
	{
		float _DirectionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
		int _Direction_Temp = _DirectionX < 0 ? 1 : -1;
		if (_Direction_Temp != _Direction_Faced)
		{
			Sprite.FlipH = _Direction_Temp < 0;
			_Direction_Faced = _Direction_Temp;
		}

		Velocity = new Vector2(0, Velocity.Y + _Gravity_Force);

		MoveAndSlide();
		GlobalPosition = GlobalPosition.Round();
	}

	public void RandomNextShot()
	{
		Random rand = new Random();

		NextShot = (float)(MinWait + rand.NextDouble() * (MaxWait - MinWait));
	}

	public void RestartTimer()
	{
		Shield.SetActive(true);
		Sprite.Play("idle");
		RandomNextShot();
		Timer.WaitTime = NextShot;
		Timer.Start();
	}

	public new void Take_Damage(Node2D body)
	{
		if (Shield?.IsActive == true)
		{
			return;
		}

		base.Take_Damage(body);
	}

	public async void Attack()
	{
		Shield.SetActive(false);

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
}
