using System;
using System.Threading.Tasks.Dataflow;
using Godot;

public partial class Goomba : Enemy
{
	[ExportGroup("Internal")]
	[Export]
	private float Speed;

	[Export]
	private float Scale_Anim;

	private int _Direction_Faced = 1;

	private bool Is_Active = false;

	public override void _PhysicsProcess(double delta)
	{
		if (!Is_Active)
			return;

		UpdateFacing();
		ApplyGravity(delta);

		float directionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
		Vector2 velocity = Velocity;
		velocity.X = directionX * Speed;
		Velocity = velocity;

		MoveAndSlide();
	}

	public void _Begin_walk()
	{
		Is_Active = true;
		Sprite.Play("inactive");
	}
}
