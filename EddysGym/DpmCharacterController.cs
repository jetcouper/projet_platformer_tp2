using Godot;
using Utils;

public partial class DpmCharacterController : Node2D
{
    [ExportGroup("Reference")]
    [Export]
    private CharacterBody2D _body;

    [Export]
    private AnimatedSprite2D _sprite;

    [ExportGroup("Movement")]
    [Export]
    public float MoveSpeed = 400.0f;

    [Export]
    public float MoveAcceleration = 4000.0f;

    private float _moveAxis = 0.0f;
    private float _facingDir = 1.0f;
    private float _gravityForce;

    public override void _Ready()
    {
        _body.EnsureValid();
        _sprite.EnsureValid();

        EnsureInputActions();
        _gravityForce = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
    }

    public override void _Process(double delta)
    {
        _moveAxis = Input.GetAxis("move_left", "move_right");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_body.IsValid() || !_sprite.IsValid())
            return;

        float fDelta = (float)delta;

        float velX = Mathf.MoveToward(
            _body.Velocity.X,
            _moveAxis * MoveSpeed,
            MoveAcceleration * fDelta
        );

        float velY = _body.IsOnFloor() ? 0.0f : _body.Velocity.Y + _gravityForce * fDelta;

        _body.Velocity = new Vector2(velX, velY);
        _body.MoveAndSlide();

        UpdateFacing();
        UpdateAnimation();
    }

    private void EnsureInputActions()
    {
        AddActionIfMissing("move_left", Key.A);
        AddActionIfMissing("move_right", Key.D);
    }

    private static void AddActionIfMissing(string action, Key key)
    {
        if (InputMap.HasAction(action))
            return;

        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(_moveAxis) <= 0.1f)
            return;

        _facingDir = _moveAxis > 0.0f ? 1.0f : -1.0f;
        _sprite.FlipH = _facingDir > 0.0f;
    }

    private void UpdateAnimation()
    {
        string anim = Mathf.Abs(_moveAxis) > 0.1f ? "walk" : "idle";

        if (
            _sprite.SpriteFrames != null
            && _sprite.SpriteFrames.HasAnimation(anim)
            && _sprite.Animation != anim
        )
        {
            _sprite.Play(anim);
        }
    }
}
