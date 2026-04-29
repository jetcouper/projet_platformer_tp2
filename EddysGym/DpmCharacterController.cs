using Godot;
using Utils;

public partial class DpmCharacterController : Node2D
{
    [ExportGroup("Reference")]
    [Export] private CharacterBody2D _body;

    [Export] private AnimatedSprite2D _sprite;

    [ExportGroup("Movement")]
    [Export] public float MoveSpeed = 400.0f;

    [Export] public float MoveAcceleration = 4000.0f;

    [ExportGroup("Jump")]
    [Export] public float CoyoteJumpBoost = 1.0f;

    [Export] public float JumpInitialVelocity = -200.0f;

    [Export] public float JumpMaxVelocity = -600.0f;

    [Export] public float JumpSustainForce = -20.0f;

    [Export] public float JumpSustainMaxTime = 0.50f;

    [Export] public float CoyoteDuration = 0.15f;

    [ExportGroup("Physics")]
    [Export] public float FallSpeedCap = 1000.0f;

    private float _moveAxis = 0.0f;
    private float _facingDir = 1.0f;
    private float _coyoteTimeLeft = 0.0f;
    private float _jumpHoldTime = 0.0f;
    private float _gravityForce = 0.0f;

    private bool _jumpRequested = false;
    private bool _jumpReleased = false;
    private bool _jumpHeld = false;
    private bool _jumpOngoing = false;
    private bool _hasJumped = false;

    private bool _groundedLastFrame = false;
    private bool _coyoteActive = false;
    private bool _coyoteAlreadyUsed = false;
    private bool _coyoteJumpTriggered = false;

    public override void _Ready()
    {
        _body.EnsureValid();
        _sprite.EnsureValid();

        EnsureInputActions();
        _gravityForce = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

        if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation("idle"))
            _sprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_body.IsValid() || !_sprite.IsValid())
            return;

        float fDelta = (float)delta;

        ReadInputs();
        UpdateCoyoteState(fDelta);

        float velX = Mathf.MoveToward(
            _body.Velocity.X,
            _moveAxis * MoveSpeed,
            MoveAcceleration * fDelta
        );

        float velY = ComputeVerticalVelocity(fDelta);
        velY = ClampFall(velY);

        _body.Velocity = new Vector2(velX, velY);
        _body.MoveAndSlide();

        UpdateFacing();
        UpdateAnimation();
    }

    private void EnsureInputActions()
    {
        AddActionIfMissing("move_left", Key.A);
        AddActionIfMissing("move_right", Key.D);
        AddActionIfMissing("jump", Key.W);
    }

    private static void AddActionIfMissing(string action, Key key)
    {
        if (InputMap.HasAction(action))
            return;

        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private void ReadInputs()
    {
        _moveAxis = Input.GetAxis("move_left", "move_right");
        _jumpRequested = Input.IsActionJustPressed("jump");
        _jumpReleased = Input.IsActionJustReleased("jump");
        _jumpHeld = Input.IsActionPressed("jump");
    }

    private void UpdateCoyoteState(float delta)
    {
        bool grounded = _body.IsOnFloor() && _body.Velocity.Y >= -1.0f;

        if (grounded)
        {
            _coyoteTimeLeft = CoyoteDuration;
            _coyoteAlreadyUsed = false;
            _hasJumped = false;
            _coyoteJumpTriggered = false;
        }
        else
        {
            _coyoteTimeLeft = Mathf.Max(_coyoteTimeLeft - delta, 0.0f);
        }

        _coyoteActive = _coyoteTimeLeft > 0.0f && !_coyoteAlreadyUsed;
        _groundedLastFrame = grounded;
    }

    private bool CanJump()
    {
        return _body.IsOnFloor() || _coyoteActive;
    }

    private float ComputeVerticalVelocity(float delta)
    {
        float velY = _body.Velocity.Y + _gravityForce * delta;

        if (_jumpRequested && CanJump())
        {
            bool isCoyoteJump = !_body.IsOnFloor() && _coyoteActive;

            velY = JumpInitialVelocity * (isCoyoteJump ? CoyoteJumpBoost : 1.0f);
            _jumpOngoing = true;
            _jumpHoldTime = 0.0f;
            _coyoteTimeLeft = 0.0f;
            _coyoteAlreadyUsed = true;
            _hasJumped = true;
            _coyoteJumpTriggered = isCoyoteJump;
        }

        if (_jumpOngoing && _jumpHeld)
        {
            _jumpHoldTime += delta;

            if (_jumpHoldTime < JumpSustainMaxTime)
                velY = Mathf.Max(velY + JumpSustainForce, JumpMaxVelocity);
            else
                _jumpOngoing = false;
        }

        if (_jumpReleased)
            _jumpOngoing = false;

        return velY;
    }

    private float ClampFall(float velY)
    {
        return Mathf.Min(velY, FallSpeedCap);
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
        string anim = DetermineAnim();

        if (_sprite.SpriteFrames == null || !_sprite.SpriteFrames.HasAnimation(anim))
            return;

        if (_sprite.Animation != anim)
            _sprite.Play(anim);
    }

private string DetermineAnim()
{
    if (_body.IsOnFloor())
    {
        _coyoteJumpTriggered = false;

        if (Mathf.Abs(_body.Velocity.X) > 5.0f)
            return "walk";

        return "idle";
    }

    if (_body.Velocity.Y < 0.0f)
    {
        if (_coyoteJumpTriggered && _sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation("coyote"))
            return "coyote";

        return "jump";
    }

    if (_body.Velocity.Y >= 0.0f)
    {
        if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation("fall"))
            return "fall";

        return "jump";
    }

    return "idle";
}
}