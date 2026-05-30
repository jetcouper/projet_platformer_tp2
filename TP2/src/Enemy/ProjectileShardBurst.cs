using Godot;
using Utils;

public partial class ProjectileShardBurst : GpuParticles2D
{
    public override void _Ready()
    {
        Restart();
        Emitting = true;

        double duration = Lifetime / Mathf.Max(SpeedScale, 0.001f) + 0.1f;
        GetTree().CreateTimer(duration).Timeout += () =>
        {
            if (this.IsValid())
                QueueFree();
        };
    }
}
