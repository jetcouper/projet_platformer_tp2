namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmCharacterAnimator : Node2D
{
    [ExportGroup("Reference")]
    [Export]
    private DpmCharacterController _controller;

    [Export]
    private AnimatedSprite2D _sprite;

    [ExportGroup("Spawn Tween")]
    [Export]
    public float SpawnTweenDuration = 2.0f;

    [ExportGroup("Sprite Offset")]
    [Export]
    public float ShootOffsetX = -58.0f;

    public override void _Ready()
    {
        _controller.EnsureValid();
        _sprite.EnsureValid();

        if (!_controller.IsValid() || !_sprite.IsValid())
            return;

        PlaySpawnTween();
        _sprite.Play("idle");
    }

    public override void _Process(double delta)
    {
        if (!_controller.IsValid() || !_sprite.IsValid())
            return;

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
        tween.TweenProperty(_sprite, "modulate:a", 1.0f, SpawnTweenDuration);
        tween.TweenProperty(_sprite, "scale", finalScale, SpawnTweenDuration);
    }

    private void UpdateFacing()
    {
        if (_controller.IsDashing)
            return;

        Vector2 scale = _sprite.Scale;
        scale.X = Mathf.Abs(scale.X) * -_controller.FacingDir;
        _sprite.Scale = scale;
    }

    private void UpdateSpriteOffset(string anim)
    {
        float offsetX = anim == "shoot" ? ShootOffsetX : 0.0f;
        _sprite.Offset = new Vector2(offsetX, 0.0f);
    }

    private void UpdateAnimation()
    {
        string anim = DetermineAnim();
        UpdateSpriteOffset(anim);

        if (_sprite.Animation != anim)
            _sprite.Play(anim);

        if (_controller.IsOnLadder)
        {
            bool moving = Mathf.Abs(_controller.VerticalAxis) > 0.1f;
            if (moving)
                _sprite.Play();
            else
                _sprite.Pause();
        }
    }

    private string DetermineAnim()
    {
        if (_controller.IsDead)
            return "die";
        if (_controller.IsHit)
            return "normal_hit";
        if (_controller.IsShooting)
            return "shoot";
        if (_controller.IsDashing)
            return "dash";
        if (_controller.IsOnLadder)
            return "climb";

        CharacterBody2D body = _controller.Body;

        if (
            _controller.IsHealing
            && body.IsOnFloor()
            && Mathf.Abs(body.Velocity.X) < 5.0f
            && !_controller.IsCrouching
        )
            return "heal";

        if (_controller.IsCrouching)
            return Mathf.Abs(body.Velocity.X) > 5.0f ? "crouch_walk" : "crouch";

        if (body.IsOnFloor())
            return Mathf.Abs(body.Velocity.X) > 5.0f ? "walk" : "idle";

        return body.Velocity.Y < 0.0f ? "jump" : "fall";
    }
}
