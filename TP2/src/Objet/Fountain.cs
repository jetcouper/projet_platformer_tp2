namespace TP2.Src;

using Godot;
using Utils;

public partial class Fountain : Node2D
{
    [Export]
    private Area2D _healArea;

    [Export]
    public float HealPerSecond = 20.0f;

    private DpmHealth _playerHealth;
    private DpmCharacterController _playerController;

    public override void _Ready()
    {
        _healArea.EnsureValid();

        _healArea.BodyEntered += OnBodyEntered;
        _healArea.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!this.IsValid())
            return;

        if (_playerHealth.IsValid())
            _playerHealth.Heal(HealPerSecond * (float)delta);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!this.IsValid())
            return;

        _playerHealth = body.GetNodeOrNull<DpmHealth>("DpmHealth");
        _playerController = body.GetNodeOrNull<DpmCharacterController>("DpmCharacterController");

        if (_playerController.IsValid())
            _playerController.IsHealing = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (!this.IsValid())
            return;

        if (_playerController.IsValid())
            _playerController.IsHealing = false;

        _playerHealth = null;
        _playerController = null;
    }
}
