using System;
using System.Threading.Tasks.Dataflow;
using Godot;

public partial class SansCervelle : CharacterBody2D
{
	[ExportGroup("External")]
	[Export]
	public Node2D Character;

	[ExportGroup("Internal")]
	[Export]
	private bool Is_Active = false;

	[Export]
	private float Speed;

	[Export]
	private AnimatedSprite2D Sprite;

	[Export]
	private float Scale_Anim;

	private int _Direction_Faced = 1;

	private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

	public override void _Ready() { }

	public override void _PhysicsProcess(double delta)
	{
		if (Is_Active)
		{
			float _DirectionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
			int _Direction_Temp = _DirectionX < 0 ? 1 : -1;
			if (_Direction_Temp != _Direction_Faced)
			{
				Scale = new Vector2(_Direction_Temp * Scale_Anim, Scale_Anim);
				_Direction_Faced = _Direction_Temp;
			}

			Velocity = new Vector2(_DirectionX * Speed, Velocity.Y + _Gravity_Force);
			MoveAndSlide();
		}
	}

	public void begin_walk()
	{
		Is_Active = true;
		Sprite.Play("walk");
	}
}
