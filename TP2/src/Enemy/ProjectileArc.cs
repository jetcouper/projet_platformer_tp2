using Godot;
using Utils;

public partial class ProjectileArc : RigidBody2D
{
    [Export]
    public PackedScene ShardBurstScene { get; set; }

    public Node2D Launcher { get; set; }

    private bool _isDestroyed;

    public override void _Ready()
    {
        ShardBurstScene.EnsureValid();

        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += OnBodyEntered;

        VisibleOnScreenNotifier2D notifier = GetNodeOrNull<VisibleOnScreenNotifier2D>(
            "VisibleOnScreenNotifier2D"
        );
        if (notifier != null)
            notifier.ScreenExited += () => DestroyProjectile(false);
    }

    private void OnBodyEntered(Node body)
    {
        if (!this.IsValid())
            return;
        if (body == Launcher)
            return;

        Node health = body.GetNodeOrNull("DpmHealth");
        health?.Call("TakeDamage", 1);

        DestroyProjectile(true);
    }

    private void DestroyProjectile(bool spawnBurst)
    {
        if (_isDestroyed)
            return;

        _isDestroyed = true;

        if (spawnBurst && ShardBurstScene != null)
        {
            Node2D burst = ShardBurstScene.Instantiate<Node2D>();
            GetParent()?.AddChild(burst);
            burst.GlobalPosition = GlobalPosition;
        }

        QueueFree();
    }
}
