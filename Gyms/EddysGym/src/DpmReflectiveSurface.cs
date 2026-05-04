using Godot;

public partial class DpmReflectiveSurface : StaticBody2D
{
    [Export]
    public bool IsActive = true;

    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private CollisionShape2D _collision;

    public override void _Ready()
    {
        ApplyState();
    }

    public void Toggle()
    {
        IsActive = !IsActive;
        ApplyState();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        ApplyState();
    }

    private void ApplyState()
    {
        // Désactivée : la balle passe à travers
        _collision?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !IsActive);

        // Animation actif inactif
        _sprite?.Play(IsActive ? "active" : "inactive");
    }
}
