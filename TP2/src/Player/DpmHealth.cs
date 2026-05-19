namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmHealth : Node2D
{
    [Export]
    private CharacterBody2D _body;

    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private ProgressBar _healthBar;

    [Export]
    private PackedScene _explosionParticlesScene;

    [Export]
    public int MaxLives = 3;

    [Export]
    public float MaxHealthPercent = 100.0f;

    [Export]
    public float DamagePercent = 10.0f;

    [Export]
    public float InvincibilityDuration = 1.0f;
    private bool _isInvincible = false;

    [Export]
    private Timer _invincibilityTimer;

    private int _lives;
    private float _healthPercent;
    private DpmCharacterController _controller;

    public bool IsDead => _lives <= 0;
    public bool IsFullHealth => _lives >= MaxLives && _healthPercent >= MaxHealthPercent;

    public void SetDead(bool value)
    {
        _lives = value ? 0 : 1;
    }

    public void SetController(DpmCharacterController controller)
    {
        _controller = controller;
    }

    public override void _Ready()
    {
        if (!_body.IsValid() || !_sprite.IsValid() || !_invincibilityTimer.IsValid())
            return;

        _lives = MaxLives;
        _healthPercent = MaxHealthPercent;

        _invincibilityTimer.WaitTime = InvincibilityDuration;
        _invincibilityTimer.Timeout += OnInvincibilityTimeout;

        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0 || _isInvincible)
            return;

        _healthPercent -= DamagePercent * amount;
        _healthObserver?.OnDamageTaken();

        if (_healthPercent <= 0.0f)
        {
            _lives--;

            CreateExplosion(_body.GlobalPosition);

            _healthObserver?.OnLivesChanged(_lives);

            if (_lives > 0)
                _healthPercent = MaxHealthPercent;
            else
                _healthPercent = 0.0f;
        }

        UpdateBar();

        if (!IsDead)
        {
            _controller?.TriggerHit();
            StartInvincibility();
        }

        if (IsDead)
            GD.Print("Mort!");
    }

    private void StartInvincibility()
    {
        _isInvincible = true;

        Tween tween = CreateTween();
        tween.SetLoops((int)(InvincibilityDuration / 0.2f));
        tween.TweenProperty(_sprite.EnsureValid(), "modulate:a", 0.3f, 0.1f);
        tween.TweenProperty(_sprite.EnsureValid(), "modulate:a", 1.0f, 0.1f);

        _invincibilityTimer.EnsureValid().Start();
    }

    private void OnInvincibilityTimeout()
    {
        _isInvincible = false;
        if (_sprite.IsValid())
            _sprite.Modulate = new Color(1, 1, 1, 1);
    }

    private void UpdateBar()
    {
        if (_healthBar.IsValid())
        {
            _healthBar.MaxValue = MaxHealthPercent;
            _healthBar.Value = _healthPercent;
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0)
            return;

        _healthPercent += amount;

        if (_healthPercent > MaxHealthPercent)
            _healthPercent = MaxHealthPercent;

        UpdateBar();
    }

    private void CreateExplosion(Vector2 position)
    {
        if (!_explosionParticlesScene.IsValid())
            return;

        CpuParticles2D particles = _explosionParticlesScene.Instantiate<CpuParticles2D>();

        GetParent()?.AddChild(particles);

        particles.GlobalPosition = position;

        particles.Restart();
        particles.Emitting = true;

        double duration = particles.Lifetime / Mathf.Max(particles.SpeedScale, 0.001f) + 0.1f;

        GetTree().CreateTimer(duration).Timeout += particles.QueueFree;
    }

    private void OnEnemyContact(Node2D body)
    {
        if (body != _body && body is CharacterBody2D)
        {
            if (_controller?.IsDashing == true)
                return;
            TakeDamage(1);
        }
    }

    private IHealthObserver _healthObserver;

    public void SetHealthObserver(IHealthObserver observer)
    {
        _healthObserver = observer;
        _healthObserver?.OnLivesChanged(_lives);
    }
}
