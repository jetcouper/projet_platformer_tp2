using Godot;
using Utils;

public partial class DpmCharacterController : Node2D
{
    [ExportGroup("Reference")]
    [Export]
    private CharacterBody2D _body;

    [Export]
    private CollisionShape2D _standingHitbox;

    [Export]
    private CollisionShape2D _crouchingHitbox;

    [Export]
    public DpmHealth Health;

    [Export]
    public DpmExperience Experience;

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

    [ExportGroup("Hit")]
    [Export]
    public float HitDuration = 0.75f;

    [ExportGroup("FootStep")]
    [Export]
    public PackedScene FootStepScene { get; set; }

    // Etats expose
    public float MoveAxis { get; private set; }
    public float VerticalAxis { get; private set; }
    public float FacingDir { get; private set; } = 1.0f;
    public bool IsCrouching { get; private set; }
    public bool IsDashing => _dashing;
    public bool IsShooting => _shooting;
    public bool IsOnLadder => _onLadder;
    public bool IsDead { get; private set; }
    public bool IsHealing { get; set; }

    public bool IsHit { get; private set; }
    private float _hitTimeLeft;

    public CharacterBody2D Body => _body;

    // Etats interne
    private bool _jumpJustPressed;
    private bool _jumpHeld;
    private bool _dashJustPressed;
    private bool _crouchHeld;

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

    private float _footStepTimer = 0f;
    private const float FootStepInterval = 0.25f;
    private Vector2 _footStepOffset = new(20f, 25f);

    public override void _Ready()
    {
        _body.EnsureValid();
        _standingHitbox.EnsureValid();
        _crouchingHitbox.EnsureValid();

        EnsureInputActions();
        _gravityForce = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_body.IsValid())
            return;

        float fDelta = (float)delta;
        if (Health != null)
            IsDead = Health.IsDead;

        if (IsDead)
        {
            Vector2 v = _body.Velocity;
            v.X = Mathf.MoveToward(v.X, 0.0f, MoveAcceleration * fDelta);
            v.Y = Mathf.Min(v.Y + _gravityForce * fDelta, FallSpeedCap);
            _body.Velocity = v;
            _body.MoveAndSlide();
            return;
        }

        ReadInputs();

        UpdateCrouch();
        UpdateCoyote(fDelta);
        UpdateLadder();
        UpdateDash(fDelta);
        UpdateShoot(fDelta);

        _body.Velocity = ComputeVelocity(fDelta);
        HandleFootSteps((float)delta);
        _body.MoveAndSlide();

        UpdateHit(fDelta);
        UpdateFacing();
    }

    private void UpdateCrouch()
    {
        if (!_standingHitbox.IsValid() || !_crouchingHitbox.IsValid())
            return;
        // Le joueur s'accroupit s'il touche le sol et maintient la touche
        if (_crouchHeld && _body.IsOnFloor() && !IsCrouching && !_onLadder)
        {
            IsCrouching = true;
            _standingHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            _crouchingHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        }
        // Le joueur se lève s'il lâche la touche ou s'il n'est plus au sol
        else if ((!_crouchHeld || !_body.IsOnFloor()) && IsCrouching)
        {
            IsCrouching = false;
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
        MoveAxis = Input.GetAxis("move_left", "move_right");
        VerticalAxis = Input.GetAxis("move_up", "move_down");
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

        if (_onLadder && _body.IsOnFloor() && VerticalAxis >= 0.0f)
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
            if (IsCrouching)
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
        _dashDir = FacingDir;
        _jumpOngoing = false;
        if (!_body.IsOnFloor())
            _airDashUsed = true;
    }

    private Vector2 ComputeVelocity(float delta)
    {
        if (_dashing)
            return new Vector2(_dashDir * DashSpeed, 0.0f);

        if (_onLadder)
            return new Vector2(0.0f, VerticalAxis * ClimbSpeed);

        float currentMoveSpeed = IsCrouching ? MoveSpeed * 0.5f : MoveSpeed;

        float velX = Mathf.MoveToward(
            _body.Velocity.X,
            MoveAxis * currentMoveSpeed,
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

        if (Mathf.Abs(MoveAxis) > 0.1f)
            FacingDir = MoveAxis > 0.0f ? 1.0f : -1.0f;
    }

    private void UpdateHit(float delta)
    {
        if (_hitTimeLeft > 0.0f)
        {
            _hitTimeLeft -= delta;
            if (_hitTimeLeft <= 0.0f)
                IsHit = false;
        }
    }

    public void TriggerHit()
    {
        IsHit = true;
        _hitTimeLeft = HitDuration;
    }

    private void HandleFootSteps(float delta)
    {
        // Seulement si au sol et en mouvement horizontal
        if (!_body.IsOnFloor())
            return;
        if (Mathf.Abs(_body.Velocity.X) < 10f)
            return;

        _footStepTimer -= delta;
        if (_footStepTimer > 0f)
            return;

        _footStepTimer = FootStepInterval;
        SpawnFootStep();
    }

    private void SpawnFootStep()
    {
        if (FootStepScene == null)
            return;

        var footStep = FootStepScene.Instantiate<FootStep>();
        Vector2 spawnPos = _body.GlobalPosition + _footStepOffset;
        _body.GetParent().AddChild(footStep);
        footStep.GlobalPosition = spawnPos;
        footStep.Emitting = true;
    }
}
