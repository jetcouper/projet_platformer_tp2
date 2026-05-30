using Godot;
using Utils;

public partial class RangedEnemy : Enemy
{
    [Export]
    protected PackedScene ProjectileScene;

    [Export]
    protected float ProjectileSpeed = 200.0f;

    [Export]
    protected float ProjectileArcY = -300.0f;

    [Export]
    protected Vector2 ProjectileOffset = new(20, -10);

    public override void _Ready()
    {
        ProjectileScene.EnsureValid();
        base._Ready();
    }

    protected void Shoot()
    {
        if (!this.IsValid())
            return;
        if (ProjectileScene == null)
            return;

        ProjectileArc projectile = ProjectileScene.Instantiate<ProjectileArc>();
        GetParent().AddChild(projectile);
        projectile.Launcher = this;
        projectile.GlobalPosition =
            GlobalPosition + new Vector2(-Direction_Faced * ProjectileOffset.X, ProjectileOffset.Y);
        projectile.LinearVelocity = new Vector2(-Direction_Faced * ProjectileSpeed, ProjectileArcY);
    }
}
