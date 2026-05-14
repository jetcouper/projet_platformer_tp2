namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmReflectiveBox : Ingredient
{
    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private CollisionShape2D _collision;

    public override void _Ready()
    {
        ApplyState();
    }

    public override void ChangeState()
    {
        base.ChangeState();
        ApplyState();
    }

    public void SetActive(bool NewState)
    {
        State = NewState;
        ApplyState();
    }

    private void ApplyState()
    { // Désactivée : la balle passe à travers
        if (_collision.IsValid())
            _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, !State);

        // Animation actif inactif
        if (_sprite.IsValid())
            _sprite.Play(State ? "active" : "inactive");
    }
}
