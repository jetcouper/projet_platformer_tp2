using Godot;
using TP2.Src;

public partial class Destructible : Node2D
{
    [ExportGroup("External")]
    [Export]
    public AnimatedSprite2D Sprite;

    [Export]
    public PackedScene ParticleScene; // particules à la destruction

    [Export]
    public PackedScene DamageParticleScene; // particules quand prend dommage

    [Export]
    private int Vies = 3;

    public void Take_Damage(Node2D body)
    {
        GD.Print("take_damage reached");
        if (body is Projectile)
        {
            Vies -= 1;
            SpawnParticles(DamageParticleScene);

            if (Vies <= 0)
            {
                Be_Destroyed();
            }
        }
    }

    public void _on_body_entered(Node2D body)
    {
        GD.Print("on body entered reached");
        if (body is Projectile)
        {
            Vies -= 1;
            SpawnParticles(DamageParticleScene);

            if (Vies <= 0)
            {
                Be_Destroyed();
            }
        }
    }

    private void SpawnParticles(PackedScene scene)
    {
        if (scene == null)
            return;

        GpuParticles2D particles = scene.Instantiate<GpuParticles2D>();
        particles.GlobalPosition = this.GlobalPosition;
        GetParent().AddChild(particles);
        particles.Emitting = true;
        particles.Finished += particles.QueueFree;
    }

    public void Be_Destroyed()
    {
        SpawnParticles(ParticleScene);
        QueueFree();
    }
}
