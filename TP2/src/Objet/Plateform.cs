using Godot;

public partial class Plateform : AnimatableBody2D
{
    [Export]
    private float distance = 200f;

    [Export]
    private float duree = 2f;

    private Vector2 positionDepart;

    public override void _Ready()
    {
        positionDepart = Position;

        Tween tween = CreateTween();

        tween.SetLoops();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);

        tween.TweenProperty(this, "position", positionDepart + Vector2.Down * distance, duree);

        tween.TweenProperty(this, "position", positionDepart, duree);
    }
}
