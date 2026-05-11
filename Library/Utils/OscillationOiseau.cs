using System;
using Godot;

public partial class OscillationOiseau : Node2D
{
	[Export]
	private AnimatedSprite2D node;

	[Export]
	private Vector2 limitRange = new(-700, 700);

	[Export]
	private Vector2 durationRange = new(10.0f, 40.0f);

	private Tween _Tween;

	public override void _Ready()
	{
		if (!IsInstanceValid(node))
		{
			return;
		}

		float randomDuration = (float)GD.RandRange(durationRange.X, durationRange.Y);
		float initialX = node.Position.X;

		_Tween = CreateTween();
		//Tweener supportent le chaînage (appels subséquants de méthodes)
		node.Play();
		_Tween.TweenCallback(Callable.From(() => node.Play("moveLeft")));
		_Tween.TweenCallback(Callable.From(() => node.FlipH = true));
		_Tween
			.TweenProperty(node, "position:x", initialX + limitRange.X, randomDuration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Linear);

		_Tween.TweenCallback(Callable.From(() => node.Play("moveRight")));
		_Tween.TweenCallback(Callable.From(() => node.FlipH = false));
		_Tween
			.TweenProperty(node, "position:x", initialX + limitRange.Y, randomDuration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Linear);

		_Tween.SetLoops();
	}

	public void SetSpeedMove(float InSpeedScale)
	{
		_Tween?.SetSpeedScale(InSpeedScale);
	}
}
