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
    public int MaxLives = 3;

    [Export]
    public float MaxHealthPercent = 100.0f;

    [Export]
    public float DamagePercent = 10.0f; // pourcentage de dégats retiré

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

    public override void _Ready()
    {
        _controller = GetParent().GetNode<DpmCharacterController>("DpmCharacterController");
        _lives = MaxLives;
        _healthPercent = MaxHealthPercent;
        if (_invincibilityTimer != null)
        {
            _invincibilityTimer.WaitTime = InvincibilityDuration;
            _invincibilityTimer.Timeout += OnInvincibilityTimeout;
        }
        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0 || _isInvincible)
            return;

        // Retire un pourcentage par point de dégât
        _healthPercent -= DamagePercent * amount;

        // Si barre de dégats a 0 ->  perte 1 point de vie
        if (_healthPercent <= 0.0f)
        {
            _lives--;
            GD.Print($"Une vie perdue! Vies restantes: {_lives}");

            if (_lives > 0)
                _healthPercent = MaxHealthPercent; // recharge la barre
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
        {
            GD.Print("Mort!");
        }
    }

    private async void StartInvincibility()
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
        _isInvincible = false;
        _sprite.Modulate = new Color(1, 1, 1, 1);
    }

    private void UpdateBar()
    {
        if (_healthBar != null)
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
}
