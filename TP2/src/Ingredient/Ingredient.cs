using Godot;
using System;

public abstract partial class Ingredient : AnimatableBody2D
{
	[Export]
	public bool State = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public virtual void ChangeState()
	{
		State = !State;
	}
}
