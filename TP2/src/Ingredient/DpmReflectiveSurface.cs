using Godot;
using Utils;

public partial class DpmReflectiveSurface : Ingredient
{
    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private CollisionShape2D _collision;

    public override void _Ready()
    {
        _sprite.EnsureValid();
        _collision.EnsureValid();

        ApplyState();
    }

    public override void ChangeState()
    {
        if (!this.IsValid())
            return;

        base.ChangeState();
        ApplyState();
    }

    public void SetActive(bool NewState)
    {
        if (!this.IsValid())
            return;

        State = NewState;
        ApplyState();
    }

    private void ApplyState()
    {
        _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, !State);
        SetCollisionLayerValue(2, State);
        _sprite.Play(State ? "active" : "inactive");
    }
}
