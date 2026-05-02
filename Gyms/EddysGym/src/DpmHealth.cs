using Godot;

public partial class DpmHealth : Node2D
{
    [Export]
    private CharacterBody2D _body;

    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private ProgressBar _healthBar;

    [Export]
    public int MaxHealth = 3;

    private int _health;

    public bool IsDead => _health <= 0;

    public override void _Ready()
    {
        _health = MaxHealth;
        UpdateBar();
    }

    public override void _PhysicsProcess(double delta) { }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        UpdateBar();
        if (IsDead)
            Die();
    }

    private void UpdateBar()
    {
        if (_healthBar != null)
        {
            _healthBar.MaxValue = MaxHealth;
            _healthBar.Value = _health;
        }
    }

    private void Die()
    {
        _sprite.Play("die");
    }
}
