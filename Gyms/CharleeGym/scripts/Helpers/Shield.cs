using Godot;

public partial class Shield : StaticBody2D, IReflectiveSurface
{
    [Export]
    public bool IsActive { get; private set; } = true;

    [Export]
    private CollisionShape2D ProjectileCollision;

    [Export]
    public StaticBody2D PlatformBody;

    [Export]
    private CollisionShape2D PlatformCollision;

    [Export]
    public Sprite2D ShieldSprite;

    public override void _Ready()
    {
        ApplyState();
    }

    public void Toggle()
    {
        IsActive = !IsActive;
        ShieldSprite.Visible = !ShieldSprite.Visible;
        ApplyState();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        ShieldSprite.Visible = active;
        ApplyState();
    }

    public void AddShieldCollisionException(PhysicsBody2D body)
    {
        if (body == null)
            return;

        AddCollisionExceptionWith(body);
        body.AddCollisionExceptionWith(this);

        if (PlatformBody == null)
            return;

        PlatformBody.AddCollisionExceptionWith(body);
        body.AddCollisionExceptionWith(PlatformBody);
    }

    private void ApplyState()
    {
        ProjectileCollision?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !IsActive);
        PlatformCollision?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !IsActive);
    }
}
