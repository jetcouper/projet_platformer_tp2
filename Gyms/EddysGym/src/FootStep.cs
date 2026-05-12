using System;
using Godot;

public partial class FootStep : CpuParticles2D
{
    public override void _Ready()
    {
        base._Ready();

        double duration = Lifetime / SpeedScale + 0.1;
        GetTree().CreateTimer(duration).Timeout += QueueFree;
    }
}
