using Godot;

public partial class Projectile : RigidBody2D
{
    [Export]
    public float Speed = 200.0f;

    [Export]
    public float Damage = 1.0f;

    [Export]
    public float Lifetime = 3.0f;

    private float _direction = 1.0f;
    private float _timeLeft;
    private Node2D _player;

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
        GravityScale = 0.0f;
        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += OnBodyEntered;

        if (LinearVelocity.LengthSquared() < 1.0f)
            LinearVelocity = new Vector2(_direction * Speed, 0.0f);
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeLeft -= (float)delta;
        if (_timeLeft <= 0.0f)
        {
            QueueFree();
            return;
        }

        Vector2 v = LinearVelocity;
        if (v.LengthSquared() > 0.01f)
            LinearVelocity = v.Normalized() * Speed;
    }

    private void OnBodyEntered(Node body)
    {
        if (body == _player)
            return;

        QueueFree();
    }
}
