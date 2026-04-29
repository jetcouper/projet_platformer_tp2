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

    [ExportGroup("Jump")]
    [Export]
    public float CoyoteJumpBoost = 1.0f;

    [Export]
    public float JumpInitialVelocity = -200.0f;

    [Export]
    public float JumpMaxVelocity = -600.0f;

    [Export]
    public float JumpSustainForce = -20.0f;

    [Export]
    public float JumpSustainMaxTime = 0.50f;

    [Export]
    public float CoyoteDuration = 0.15f;

    [ExportGroup("Dash")]
    [Export]
    public float DashSpeed = 900.0f;

    [Export]
    public float DashDuration = 0.30f;

    [Export]
    public float DashCooldown = 0.35f;

    [ExportGroup("Physics")]
    [Export]
    public float FallSpeedCap = 1000.0f;

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

    private bool _dashRequested = false;
    private bool _dashing = false;
    private float _dashTimeLeft = 0.0f;
    private float _dashCooldownLeft = 0.0f;
    private float _dashDir = 1.0f;
    private bool _airDashUsed = false;

    public override void _Ready()
    {
        _body.EnsureValid();
        _sprite.EnsureValid();

        EnsureInputActions();
        _gravityForce = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

        _sprite.Play("idle");
        _sprite.FlipH = true; // flip car sprite dessiné vers la gauche
    }

    private void UpdateFacing()
    {
        if (_dashing)
            return;

        if (Mathf.Abs(_moveAxis) <= 0.1f)
            return;

        _facingDir = _moveAxis > 0.0f ? 1.0f : -1.0f;
        _sprite.FlipH = _facingDir > 0.0f; // flip = regarde à droite
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_body.IsValid() || !_sprite.IsValid())
            return;

        float fDelta = (float)delta;

        ReadInputs();
        UpdateCoyoteState(fDelta);
        UpdateDashState(fDelta);

        Vector2 velocity = _dashing ? ComputeDashVelocity() : ComputeNormalVelocity(fDelta);

        _body.Velocity = velocity;
        _body.MoveAndSlide();

        UpdateFacing();
        UpdateAnimation();
    }

    private void EnsureInputActions()
    {
        AddActionIfMissing("move_left", Key.A);
        AddActionIfMissing("move_right", Key.D);
        AddActionIfMissing("jump", Key.W);
        AddActionIfMissing("dash", Key.Space);
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
        _dashRequested = Input.IsActionJustPressed("dash");
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
            _airDashUsed = false;
        }
        else
        {
            _coyoteTimeLeft = Mathf.Max(_coyoteTimeLeft - delta, 0.0f);
        }

        _coyoteActive = _coyoteTimeLeft > 0.0f && !_coyoteAlreadyUsed;
        _groundedLastFrame = grounded;
    }

    private void UpdateDashState(float delta)
    {
        _dashCooldownLeft = Mathf.Max(_dashCooldownLeft - delta, 0.0f);

        if (_dashing)
        {
            _dashTimeLeft -= delta;
            if (_dashTimeLeft <= 0.0f)
            {
                _dashing = false;
                _dashCooldownLeft = DashCooldown;
            }
            return;
        }

        if (_dashRequested && CanDash())
            StartDash();
    }

    private bool CanDash()
    {
        if (_dashCooldownLeft > 0.0f)
            return false;

        if (!_body.IsOnFloor() && _airDashUsed)
            return false;

        return true;
    }

    private void StartDash()
    {
        _dashing = true;
        _dashTimeLeft = DashDuration;
        _dashDir = _facingDir;
        _jumpOngoing = false;

        if (!_body.IsOnFloor())
            _airDashUsed = true;
    }

    private Vector2 ComputeDashVelocity()
    {
        return new Vector2(_dashDir * DashSpeed, 0.0f);
    }

    private Vector2 ComputeNormalVelocity(float delta)
    {
        float velX = Mathf.MoveToward(
            _body.Velocity.X,
            _moveAxis * MoveSpeed,
            MoveAcceleration * delta
        );

        float velY = ComputeVerticalVelocity(delta);
        velY = ClampFall(velY);

        return new Vector2(velX, velY);
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

    private void UpdateAnimation()
    {
        string anim = DetermineAnim();

        if (_sprite.Animation == anim)
            return;

        _sprite.Stop();
        _sprite.Animation = anim;
        _sprite.Frame = 0;
        _sprite.FrameProgress = 0.0f;
        _sprite.Play();
    }

    private string DetermineAnim()
    {
        if (_dashing)
            return "dash";

        if (_body.IsOnFloor())
        {
            _coyoteJumpTriggered = false;

            if (Mathf.Abs(_body.Velocity.X) > 5.0f)
                return "walk";

            return "idle";
        }

        if (_body.Velocity.Y < 0.0f)
        {
            if (_coyoteJumpTriggered)
                return "coyote";

            return "jump";
        }

        return "fall";
    }
}
