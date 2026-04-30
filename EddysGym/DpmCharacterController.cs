using Godot;
using Utils;

public partial class DpmCharacterController : Node2D
{
    [ExportGroup("Reference")]
    [Export]
    private CharacterBody2D _body;

    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private Area2D _ladder;

    [ExportGroup("Movement")]
    [Export]
    public float MoveSpeed = 400.0f;

    [Export]
    public float MoveAcceleration = 4000.0f;

    [ExportGroup("Jump")]
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

    [Export]
    public float CoyoteJumpBoost = 1.0f;

    [ExportGroup("Dash")]
    [Export]
    public float DashSpeed = 900.0f;

    [Export]
    public float DashDuration = 0.50f;

    [Export]
    public float DashCooldown = 0.35f;

    [ExportGroup("Climb")]
    [Export]
    public float ClimbSpeed = 250.0f;

    [ExportGroup("Physics")]
    [Export]
    public float FallSpeedCap = 1000.0f;

    private float _moveAxis;
    private float _verticalAxis;
    private bool _jumpJustPressed;
    private bool _jumpHeld;
    private bool _dashJustPressed;

    private float _facingDir = 1.0f;

    private float _coyoteTimeLeft;
    private float _jumpHoldTime;
    private bool _jumpOngoing;

    private bool _dashing;
    private float _dashTimeLeft;
    private float _dashCooldownLeft;
    private float _dashDir = 1.0f;
    private bool _airDashUsed;

    private bool _inLadderZone;
    private bool _onLadder;

    private float _gravityForce;

    public override void _Ready()
    {
        _body.EnsureValid();
        _sprite.EnsureValid();

        EnsureInputActions();
        _gravityForce = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

        _sprite.Play("idle");

        if (_ladder != null)
        {
            _ladder.BodyEntered += body =>
            {
                if (body == _body)
                    _inLadderZone = true;
            };
            _ladder.BodyExited += body =>
            {
                if (body == _body)
                {
                    _inLadderZone = false;
                    _onLadder = false;
                }
            };
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_body.IsValid() || !_sprite.IsValid())
            return;

        float fDelta = (float)delta;

        ReadInputs();
        UpdateCoyote(fDelta);
        UpdateLadder();
        UpdateDash(fDelta);

        _body.Velocity = ComputeVelocity(fDelta);
        _body.MoveAndSlide();

        UpdateFacing();
        UpdateAnimation();
    }

    private static void EnsureInputActions()
    {
        AddIfMissing("move_left", Key.A);
        AddIfMissing("move_right", Key.D);
        AddIfMissing("move_up", Key.W);
        AddIfMissing("move_down", Key.S);
        AddIfMissing("jump", Key.W);
        AddIfMissing("dash", Key.Space);
    }

    private static void AddIfMissing(string action, Key key)
    {
        if (InputMap.HasAction(action))
            return;
        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private void ReadInputs()
    {
        _moveAxis = Input.GetAxis("move_left", "move_right");
        _verticalAxis = Input.GetAxis("move_up", "move_down");
        _jumpJustPressed = Input.IsActionJustPressed("jump");
        _jumpHeld = Input.IsActionPressed("jump");
        _dashJustPressed = Input.IsActionJustPressed("dash");
    }

    private void UpdateCoyote(float delta)
    {
        if (_body.IsOnFloor() && _body.Velocity.Y >= -1.0f)
        {
            _coyoteTimeLeft = CoyoteDuration;
            _airDashUsed = false;
        }
        else
        {
            _coyoteTimeLeft = Mathf.Max(_coyoteTimeLeft - delta, 0.0f);
        }
    }

    private void UpdateLadder()
    {
        if (!_inLadderZone)
        {
            _onLadder = false;
            return;
        }

        if (_onLadder && _body.IsOnFloor() && _verticalAxis >= 0.0f)
        {
            _onLadder = false;
            return;
        }

        bool upPressed = Input.IsActionPressed("move_up");
        bool downPressed = Input.IsActionPressed("move_down");
        bool wantsToClimb = upPressed || (downPressed && !_body.IsOnFloor());

        if (!_onLadder && wantsToClimb)
        {
            _onLadder = true;
            _jumpOngoing = false;
            _body.GlobalPosition = new Vector2(_ladder.GlobalPosition.X, _body.GlobalPosition.Y);
            _body.Velocity = Vector2.Zero;
        }

        if (_onLadder && _jumpJustPressed && !upPressed)
        {
            _onLadder = false;
            _body.Velocity = new Vector2(_body.Velocity.X, JumpInitialVelocity);
            _jumpOngoing = true;
            _jumpHoldTime = 0.0f;
        }
    }

    private void UpdateDash(float delta)
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

        if (_dashJustPressed && CanDash())
            StartDash();
    }

    private bool CanDash()
    {
        if (_dashCooldownLeft > 0.0f)
            return false;
        if (_onLadder)
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

    private Vector2 ComputeVelocity(float delta)
    {
        if (_dashing)
            return new Vector2(_dashDir * DashSpeed, 0.0f);

        if (_onLadder)
            return new Vector2(0.0f, _verticalAxis * ClimbSpeed);

        float velX = Mathf.MoveToward(
            _body.Velocity.X,
            _moveAxis * MoveSpeed,
            MoveAcceleration * delta
        );
        float velY = Mathf.Min(ComputeVerticalVelocity(delta), FallSpeedCap);
        return new Vector2(velX, velY);
    }

    private float ComputeVerticalVelocity(float delta)
    {
        float velY = _body.Velocity.Y + _gravityForce * delta;

        bool canJump = _body.IsOnFloor() || _coyoteTimeLeft > 0.0f;
        if (_jumpJustPressed && canJump)
        {
            bool isCoyote = !_body.IsOnFloor();
            velY = JumpInitialVelocity * (isCoyote ? CoyoteJumpBoost : 1.0f);
            _jumpOngoing = true;
            _jumpHoldTime = 0.0f;
            _coyoteTimeLeft = 0.0f;
        }

        if (_jumpOngoing && _jumpHeld)
        {
            _jumpHoldTime += delta;
            if (_jumpHoldTime < JumpSustainMaxTime)
                velY = Mathf.Max(velY + JumpSustainForce, JumpMaxVelocity);
            else
                _jumpOngoing = false;
        }

        if (!_jumpHeld)
            _jumpOngoing = false;

        return velY;
    }

    private void UpdateFacing()
    {
        if (_dashing)
            return;
        if (Mathf.Abs(_moveAxis) <= 0.1f)
            return;

        _facingDir = _moveAxis > 0.0f ? 1.0f : -1.0f;
        _sprite.FlipH = _facingDir > 0.0f;
    }

    private void UpdateAnimation()
    {
        string anim = DetermineAnim();

        if (_onLadder)
        {
            bool moving = Mathf.Abs(_verticalAxis) > 0.1f;
            if (moving)
                _sprite.Play();
            else
                _sprite.Pause();
        }

        if (_sprite.Animation == anim)
            return;

        _sprite.Animation = anim;
        _sprite.Frame = 0;
        _sprite.FrameProgress = 0.0f;
        _sprite.Play();
    }

    private string DetermineAnim()
    {
        if (_dashing)
            return "dash";
        if (_onLadder)
            return "climb";

        if (_body.IsOnFloor())
            return Mathf.Abs(_body.Velocity.X) > 5.0f ? "walk" : "idle";

        return _body.Velocity.Y < 0.0f ? "jump" : "fall";
    }
}
