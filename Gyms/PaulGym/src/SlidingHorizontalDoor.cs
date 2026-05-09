using System;
using Godot;

public partial class SlidingHorizontalDoor : Ingredient
{
	[Export]
	double duration = 3.0;
	[Export]
	bool GoesLeft = true;

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
		Vector2 width = new Vector2((Sprite.GetRect().Size * this.Scale).X,0);
		if (tween != null)
        	tween.Kill(); 
		tween = CreateTween();
		if (State)
		{
   			tween.TweenProperty(this, "position", OriginalPos , duration );
		}
		else
		{
			
			if (GoesLeft)
   				tween.TweenProperty(this, "position", OriginalPos - width , duration);
			else
				tween.TweenProperty(this, "position", OriginalPos + width , duration);
		}
		
		
	}
}
