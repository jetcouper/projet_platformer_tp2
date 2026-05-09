using Godot;

public partial class ProjectileArc : RigidBody2D
{
    public Node2D Launcher { get; set; }

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += OnBodyEntered;

        VisibleOnScreenNotifier2D notifier = GetNodeOrNull<VisibleOnScreenNotifier2D>(
            "VisibleOnScreenNotifier2D"
        );
        if (notifier != null)
            notifier.ScreenExited += QueueFree;
    }

    private void OnBodyEntered(Node body)
    {
        if (body == Launcher)
            return;

        DpmHealth health = body.GetNodeOrNull<DpmHealth>("DpmHealth");
        health?.TakeDamage(1);

        QueueFree();
    }
}
