namespace TP2.Src;

using Godot;

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
    {
        // Désactivée : la balle passe à travers
        _collision?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !State);

        // Animation actif inactif
        _sprite?.Play(State ? "active" : "inactive");
    }
}
