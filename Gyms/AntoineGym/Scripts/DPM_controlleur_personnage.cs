using System;
using Godot;
using Utils;

public partial class DPM_controlleur_personnage : CharacterBody2D
{
    [Export]
    private AnimatedSprite2D _sprite;
    private float speed = 200f;
    public float JumpVelocity = -400.0f;
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private bool etaitSurSol = false;
    private bool peutSauter = false;
    private bool EstSurSol = false;
    private bool isPressingJumping = false;
    private Timer jumpTimer = new Timer();
    private Timer rushTimer = new Timer();
    private Timer dashTimer = new Timer();
    private bool isRushing = false;
    private bool isDashing = false;
    private Vector2 velocity = Vector2.Zero;
    private bool isFacingRight = true;
    private float lastVelocityX = 0f;
    private float directionDash = 1f;

    public override void _Ready()
    {
        base._Ready();
        _sprite.EnsureValid();
        GetTree().DebugCollisionsHint = true;
        AddChild(jumpTimer);
        AddChild(rushTimer);
        AddChild(dashTimer);
        jumpTimer.OneShot = true;
        rushTimer.OneShot = true;
        dashTimer.OneShot = true;
    }

    public override void _Process(double delta)
    {
        EstSurSol = IsOnFloor();

        if (isPressingJumping && (EstSurSol || (peutSauter && !jumpTimer.IsStopped())))
        {
            peutSauter = false;
            jumpTimer.Stop();
        }
        // Coyote time : si on vient de quitter le sol sans sauter
        if (etaitSurSol && !EstSurSol && velocity.Y >= 0)
        {
            jumpTimer.WaitTime = 0.5f;
            jumpTimer.Start();
            peutSauter = true;
        }

        // Atterrissage : annuler le saut de coyote
        if (!etaitSurSol && EstSurSol)
        {
            peutSauter = false;
            jumpTimer.Stop();
        }

        isPressingJumping = Input.IsActionJustPressed("ui_accept");

        if (Input.IsActionJustPressed("dash") && !isDashing)
        {
            isDashing = true;
            dashTimer.WaitTime = 0.2f;
            dashTimer.Start();
        }
        else if (dashTimer.IsStopped() && isDashing)
        {
            isDashing = false;
        }

        if (Input.IsActionJustPressed("rush") && !isRushing)
        {
            isRushing = true;
            rushTimer.WaitTime = 2.0f;
            rushTimer.Start();
        }
        else if (rushTimer.IsStopped() && isRushing)
        {
            isRushing = false;
        }

        etaitSurSol = EstSurSol;
    }

    public override void _PhysicsProcess(double delta)
    {
        velocity = Velocity;
        MADebugDraw2D.Instance.DrawLineWorld(
            GlobalPosition,
            GlobalPosition + velocity * 0.5f,
            Colors.Green,
            0.05f
        );
        // Saut
        if (isPressingJumping && (EstSurSol || (peutSauter && !jumpTimer.IsStopped())))
        {
            velocity.Y = JumpVelocity;
        }

        // Gravité (suspendue pendant le dash)
        if (!EstSurSol && !isDashing)
            velocity.Y += gravity * (float)delta;

        float inputX = Input.GetAxis("ui_left", "ui_right");
        if (inputX != 0)
            directionDash = Mathf.Sign(inputX);

        // Dash (priorité sur le rush)
        if (isDashing)
        {
            velocity.Y = 0;
            float dashDir = inputX != 0 ? inputX : directionDash;
            velocity.X = dashDir * speed * 3;
        }
        // Rush
        else if (isRushing)
            velocity.X = inputX * speed * 2;
        else
            velocity.X = inputX * speed;

        Velocity = velocity;
        MoveAndSlide();
        UpdateAnimation(inputX);
    }

    private void UpdateAnimation(float inputX)
    {
        if (inputX != 0)
        {
            isFacingRight = inputX < 0;
            _sprite.FlipH = !isFacingRight;
        }

        string anim = inputX != 0 ? "walk" : "idle";

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
