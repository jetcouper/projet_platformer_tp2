using Godot;

public partial class Projectile : RigidBody2D
{
    [Export]
    public float Speed = 350.0f;

    [Export]
    public float Damage = 1.0f;

    [Export]
    public float Lifetime = 4.0f; // Condition 1 : Temps maximum

    [Export]
    public float MaxDistance = 3000.0f; // Condition 2 : Distance maximale

    [Export]
    public VisibleOnScreenNotifier2D ScreenNotifier;

    private float _direction = 1.0f;
    private Node2D _player;

    private float _timeLeft;
    private Vector2 _startPosition;

    public void Launch(float direction, Node2D player = null)
    {
        _direction = Mathf.Sign(direction);
        if (_direction == 0.0f)
            _direction = 1.0f;
        _player = player;

        LinearVelocity = new Vector2(_direction * Speed, 0.0f);

        Vector2 scale = Scale;
        scale.X = Mathf.Abs(scale.X) * _direction;
        Scale = scale;
    }

    public override void _Ready()
    {
        _timeLeft = Lifetime;
        _startPosition = GlobalPosition;

        GravityScale = 0.0f;
        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += OnBodyEntered;

        // Destruction à la sortie d'écran
        if (ScreenNotifier != null)
        {
            ScreenNotifier.ScreenExited += QueueFree;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Destruction par le temps
        _timeLeft -= (float)delta;

        if (_timeLeft <= 0.0f)
        {
            QueueFree();
            return;
        }

        // Destruction par la distance
        if (GlobalPosition.DistanceTo(_startPosition) >= MaxDistance)
        {
            QueueFree();
            return;
        }

        Vector2 v = LinearVelocity;
        if (v.Length() > 0.01f)
            LinearVelocity = v.Normalized() * Speed;
    }

    private void OnBodyEntered(Node body)
    {
        if (body == _player)
            return;

        if (body is BoiteJaune)
            return;

        QueueFree();
    }
}
