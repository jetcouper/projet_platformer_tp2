using System;
using Godot;

public partial class Panel : Sprite2D
{
    public override void _Ready()
    {
        RotationDegrees = -6.0f;

        Tween tween = CreateTween();

        tween.SetLoops();

        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);

        tween.TweenProperty(this, "rotation_degrees", 6.0f, 1.0f);
        tween.TweenProperty(this, "rotation_degrees", -6.0f, 1.0f);
    }
}
