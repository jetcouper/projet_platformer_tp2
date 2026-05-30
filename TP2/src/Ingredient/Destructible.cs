using Godot;
using TP2.Src;
using Utils;

public partial class Destructible : Node2D
{
    [ExportGroup("External")]
    [Export]
    public AnimatedSprite2D Sprite;

    [Export]
    public PackedScene ParticleScene;

    [Export]
    public PackedScene DamageParticleScene;

    [Export]
    protected int Vies = 3;

    public override void _Ready()
    {
        ParticleScene.EnsureValid();
        DamageParticleScene.EnsureValid();
    }

    public void Take_Damage(Node2D body)
    {
        if (!this.IsValid())
            return;
        if (body is not Projectile)
            return;

        Vies -= 1;
        SpawnParticles(DamageParticleScene);

        if (Vies <= 0)
            Be_Destroyed();
    }

    public void _on_body_entered(Node2D body)
    {
        if (!this.IsValid())
            return;
        if (body is not Projectile)
            return;

        Vies -= 1;
        SpawnParticles(DamageParticleScene);

        if (Vies <= 0)
            Be_Destroyed();
    }

    protected void SpawnParticles(PackedScene scene)
    {
        if (scene == null)
            return;

        GpuParticles2D particles = scene.Instantiate<GpuParticles2D>();
        particles.GlobalPosition = this.GlobalPosition;
        GetParent().AddChild(particles);
        particles.Emitting = true;
        particles.Finished += () =>
        {
            if (particles.IsValid())
                particles.QueueFree();
        };
    }

    public void Be_Destroyed()
    {
        if (!this.IsValid())
            return;

        SpawnParticles(ParticleScene);
        QueueFree();
    }
}
