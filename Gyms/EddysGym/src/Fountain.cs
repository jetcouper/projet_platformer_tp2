using Godot;

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
        if (_healArea == null)
            return;

        _healArea.BodyEntered += OnBodyEntered;
        _healArea.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerHealth != null && IsInstanceValid(_playerHealth))
        {
            _playerHealth.Heal(HealPerSecond * (float)delta);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        _playerHealth = body.GetNodeOrNull<DpmHealth>("DpmHealth");
        _playerController = body.GetNodeOrNull<DpmCharacterController>("DpmCharacterController");

        if (_playerController != null)
            _playerController.IsHealing = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (_playerController != null && IsInstanceValid(_playerController))
            _playerController.IsHealing = false;

        _playerHealth = null;
        _playerController = null;
    }
}
