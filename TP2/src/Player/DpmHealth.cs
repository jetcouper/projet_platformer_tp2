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
    private IHealthObserver _healthObserver;

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
        _body.EnsureValid();
        _sprite.EnsureValid();
        _healthBar.EnsureValid();
        _invincibilityTimer.EnsureValid();
        _explosionParticlesScene.EnsureValid();

        _lives = MaxLives;
        _healthPercent = MaxHealthPercent;

        _invincibilityTimer.WaitTime = InvincibilityDuration;
        _invincibilityTimer.Timeout += OnInvincibilityTimeout;

        UpdateBar();
    }

    public void KillInstantly()
    {
        if (!this.IsValid())
            return;
        if (IsDead)
            return;

        _isInvincible = false;
        _lives = 0;
        _healthPercent = 0.0f;

        CreateExplosion(_body.GlobalPosition);
        _healthObserver?.OnLivesChanged(_lives);

        UpdateBar();
        GD.Print("Mort!");
    }

    public void TakeDamage(int amount)
    {
        if (!this.IsValid())
            return;
        if (IsDead || amount <= 0 || _isInvincible)
            return;

        _healthPercent -= DamagePercent * amount;
        _healthObserver?.OnDamageTaken();

        if (_healthPercent <= 0.0f)
        {
            _lives--;

            CreateExplosion(_body.GlobalPosition);
            _healthObserver?.OnLivesChanged(_lives);

            _healthPercent = _lives > 0 ? MaxHealthPercent : 0.0f;
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
        tween.TweenProperty(_sprite, "modulate:a", 0.3f, 0.1f);
        tween.TweenProperty(_sprite, "modulate:a", 1.0f, 0.1f);

        _invincibilityTimer.Start();
    }

    private void OnInvincibilityTimeout()
    {
        if (!this.IsValid())
            return;

        _isInvincible = false;
        _sprite.Modulate = new Color(1, 1, 1, 1);
    }

    private void UpdateBar()
    {
        if (!this.IsValid())
            return;

        _healthBar.MaxValue = MaxHealthPercent;
        _healthBar.Value = _healthPercent;
    }

    public void Heal(float amount)
    {
        if (!this.IsValid())
            return;
        if (IsDead || amount <= 0)
            return;

        _healthPercent = Mathf.Min(_healthPercent + amount, MaxHealthPercent);
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
        GetTree().CreateTimer(duration).Timeout += () =>
        {
            if (particles.IsValid())
                particles.QueueFree();
        };
    }

    private void OnEnemyContact(Node2D body)
    {
        if (!this.IsValid())
            return;
        if (body != _body && body is CharacterBody2D)
        {
            if (_controller?.IsDashing == true)
                return;
            TakeDamage(1);
        }
    }

    public void SetHealthObserver(IHealthObserver observer)
    {
        _healthObserver = observer;
        _healthObserver?.OnLivesChanged(_lives);
    }
}
