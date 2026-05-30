using Godot;
using Utils;

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
        Sprite.EnsureValid();

        OriginalPos = this.Position;
        ChangeAnimation();
    }

    public override void _Process(double delta) { }

    public override void ChangeState()
    {
        if (!this.IsValid())
            return;

        base.ChangeState();
        ChangeAnimation();
    }

    public void ChangeAnimation()
    {
        if (!this.IsValid())
            return;

        Vector2 width = new Vector2((Sprite.GetRect().Size * this.Scale).X, 0);

        tween?.Kill();
        tween = CreateTween();

        if (State)
        {
            tween.TweenProperty(this, "position", OriginalPos, duration);
        }
        else
        {
            Vector2 target = GoesLeft ? OriginalPos - width : OriginalPos + width;
            tween.TweenProperty(this, "position", target, duration);
        }
    }
}
