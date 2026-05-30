using Godot;

public partial class GhostPlatform : AnimatableBody2D
{
    [Export]
    private float distance = 200f;

    [Export]
    private float visibleDuration = 4f;

    [Export]
    private float invisibleDuration = 2f;

    [Export]
    private float ghostDelay = 0.2f;

    [Export]
    private float startDelay = 0f;

    private Vector2 startPosition;

    public override void _Ready()
    {
        startPosition = Position;

        Tween moveTween = CreateTween();
        moveTween.SetProcessMode(Tween.TweenProcessMode.Physics);
        moveTween.SetLoops();
        moveTween.SetTrans(Tween.TransitionType.Sine);
        moveTween.SetEase(Tween.EaseType.InOut);
        moveTween.TweenInterval(startDelay);
        moveTween.TweenProperty(
            this,
            "position",
            startPosition + Vector2.Right * distance,
            visibleDuration
        );
        moveTween.TweenProperty(this, "position", startPosition, visibleDuration);

        Tween fadeTween = CreateTween();
        fadeTween.SetLoops();
        fadeTween.TweenInterval(startDelay);
        fadeTween.TweenInterval(visibleDuration * 2f);
        fadeTween.TweenProperty(this, "modulate:a", 0f, invisibleDuration);
        fadeTween.TweenInterval(ghostDelay);
        fadeTween.TweenCallback(
            Callable.From(() =>
            {
                CollisionLayer = 0;
                CollisionMask = 0;
            })
        );
        fadeTween.TweenProperty(this, "modulate:a", 1f, invisibleDuration);
        fadeTween.TweenCallback(
            Callable.From(() =>
            {
                CollisionLayer = 1;
                CollisionMask = 1;
            })
        );
    }
}
