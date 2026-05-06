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
    private CollisionShape2D _standingHitbox;

    [Export]
    private CollisionShape2D _crouchingHitbox;

    [Export]
    public DpmHealth Health;

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

    [ExportGroup("Shoot")]
    [Export]
    public float ShootDuration = 0.6f;

    [ExportGroup("Physics")]
    [Export]
    public float FallSpeedCap = 1000.0f;

    private float _moveAxis;
    private float _verticalAxis;
    private bool _jumpJustPressed;
    private bool _jumpHeld;
    private bool _dashJustPressed;
    private bool _crouchHeld;
    private bool _isCrouching;

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

    private Area2D _currentLadder;

    private bool _shootJustPressed;
    private bool _shooting;
    private float _shootTimeLeft;
    private float _gravityForce;

    private bool _isDead;

    public override void _Ready()
    {
        _body.EnsureValid();
        _sprite.EnsureValid();
        _standingHitbox.EnsureValid();
        _crouchingHitbox.EnsureValid();

        EnsureInputActions();
        _gravityForce = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

        PlaySpawnTween();
        _sprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_body.IsValid() || !_sprite.IsValid())
            return;

        float fDelta = (float)delta;
        if (Health != null)
            _isDead = Health.IsDead;

        if (_isDead)
        {
            Vector2 v = _body.Velocity;
            v.X = Mathf.MoveToward(v.X, 0.0f, MoveAcceleration * fDelta);
            v.Y = Mathf.Min(v.Y + _gravityForce * fDelta, FallSpeedCap);
            _body.Velocity = v;
            _body.MoveAndSlide();
            UpdateAnimation();
            return;
        }

        ReadInputs();

        UpdateCrouch();
        UpdateCoyote(fDelta);
        UpdateLadder();
        UpdateDash(fDelta);
        UpdateShoot(fDelta);

        _body.Velocity = ComputeVelocity(fDelta);
        _body.MoveAndSlide();

        UpdateFacing();
        UpdateAnimation();
    }

    private void PlaySpawnTween()
    {
        Vector2 currentScale = _sprite.Scale;
        Vector2 finalScale = new(-Mathf.Abs(currentScale.X), currentScale.Y);
        _sprite.Scale = finalScale * 0.5f;
        _sprite.Modulate = new Color(1, 1, 1, 0);

        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_sprite, "modulate:a", 1.0f, 2.0f);
        tween.TweenProperty(_sprite, "scale", finalScale, 2.0f);
    }

    private void UpdateCrouch()
    {
        if (!_standingHitbox.IsValid() || !_crouchingHitbox.IsValid())
            return;

        // Le joueur s'accroupit s'il touche le sol et maintient la touche
        if (_crouchHeld && _body.IsOnFloor() && !_isCrouching && !_onLadder)
        {
            _isCrouching = true;
            _standingHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            _crouchingHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        }
        // Le joueur se lève s'il lâche la touche ou s'il n'est plus au sol
        else if ((!_crouchHeld || !_body.IsOnFloor()) && _isCrouching)
        {
            _isCrouching = false;
            _standingHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            _crouchingHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
    }

    private void UpdateShoot(float delta)
    {
        if (_shooting)
        {
            _shootTimeLeft -= delta;
            if (_shootTimeLeft <= 0.0f)
                _shooting = false;
        }

        if (_shootJustPressed && !_shooting)
        {
            _shooting = true;
            _shootTimeLeft = ShootDuration;
            if (_sprite.Animation == "shoot")
                _sprite.Play("shoot");
        }
    }

    private static void EnsureInputActions()
    {
        AddIfMissing("move_left", Key.A);
        AddIfMissing("move_right", Key.D);
        AddIfMissing("move_up", Key.W);
        AddIfMissing("move_down", Key.S);
        AddIfMissing("jump", Key.W);
        AddIfMissing("dash", Key.Space);
        AddIfMissingMouse("shoot", MouseButton.Left);
    }

    private static void AddIfMissing(string action, Key key)
    {
        if (InputMap.HasAction(action))
            return;
        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private static void AddIfMissingMouse(string action, MouseButton button)
    {
        if (InputMap.HasAction(action))
            return;
        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventMouseButton { ButtonIndex = button });
    }

    private void ReadInputs()
    {
        _moveAxis = Input.GetAxis("move_left", "move_right");
        _verticalAxis = Input.GetAxis("move_up", "move_down");
        _crouchHeld = Input.IsActionPressed("move_down");
        _jumpJustPressed = Input.IsActionJustPressed("jump");
        _jumpHeld = Input.IsActionPressed("jump");
        _dashJustPressed = Input.IsActionJustPressed("dash");
        _shootJustPressed = Input.IsActionJustPressed("shoot");
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
            if (_isCrouching)
                _crouchHeld = false;
            _body.GlobalPosition = new Vector2(
                _currentLadder.GlobalPosition.X,
                _body.GlobalPosition.Y
            );
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

    public void OnLadderEntered(Node body, Area2D ladder)
    {
        if (body == _body)
        {
            _inLadderZone = true;
            _currentLadder = ladder;
        }
    }

    public void OnLadderExited(Node body, Area2D ladder)
    {
        if (body == _body && _currentLadder == ladder)
        {
            _inLadderZone = false;
            _onLadder = false;
            _currentLadder = null;
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
        float currentMoveSpeed = _isCrouching ? MoveSpeed * 0.5f : MoveSpeed;

        float velX = Mathf.MoveToward(
            _body.Velocity.X,
            _moveAxis * currentMoveSpeed,
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

        if (Mathf.Abs(_moveAxis) > 0.1f)
            _facingDir = _moveAxis > 0.0f ? 1.0f : -1.0f;

        Vector2 scale = _sprite.Scale;
        scale.X = Mathf.Abs(scale.X) * -_facingDir;
        _sprite.Scale = scale;
    }

    private void UpdateSpriteOffset(string anim)
    {
        float offsetX = 0.0f;

        if (anim == "shoot")
            offsetX = _facingDir < 0.0f ? -58.0f : -58.0f;

        _sprite.Offset = new Vector2(offsetX, 0.0f);
    }

    private void UpdateAnimation()
    {
        string anim = DetermineAnim();
        UpdateSpriteOffset(anim);

        if (_sprite.Animation != anim)
            _sprite.Play(anim);

        if (_onLadder)
        {
            bool moving = Mathf.Abs(_verticalAxis) > 0.1f;
            if (moving)
                _sprite.Play();
            else
                _sprite.Pause();
        }
    }

    private string DetermineAnim()
    {
        if (_isDead)
            return "die";
        if (_shooting)
            return "shoot";
        if (_dashing)
            return "dash";
        if (_onLadder)
            return "climb";

        if (_isCrouching)
            return Mathf.Abs(_body.Velocity.X) > 5.0f ? "crouch_walk" : "crouch";

        if (_body.IsOnFloor())
            return Mathf.Abs(_body.Velocity.X) > 5.0f ? "walk" : "idle";

        return _body.Velocity.Y < 0.0f ? "jump" : "fall";
    }
}
