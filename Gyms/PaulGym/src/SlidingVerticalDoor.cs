using System;
using Godot;

public partial class SlidingVerticalDoor : Ingredient
{
	[Export]
	double duration = 3.0;

	Sprite2D Sprite;
	Vector2 OriginalPos;

	Tween tween;
	public override void _Ready()
	{
		Sprite = GetNode<Sprite2D>("Sprite2D");
		OriginalPos = this.Position;
		ChangeAnimation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void ChangeState()
	{
		base.ChangeState();
		ChangeAnimation();
		
	}
	public void ChangeAnimation()
	{
		if (tween != null)
        	tween.Kill(); 
		tween = CreateTween();
		if (State)
		{
   			tween.TweenProperty(this, "position", OriginalPos , duration * Math.Abs(Position.Y / OriginalPos.Y));
		}
		else
		{
			Vector2 height = new Vector2(0,Sprite.GetRect().Size.Y);
   			tween.TweenProperty(this, "position", OriginalPos - height , duration);
		}
		
		
	}
}
