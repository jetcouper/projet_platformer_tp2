namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmCharacterAnimator : Node2D
{
    [ExportGroup("Internal")]
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

    [ExportGroup("Label Dash")]
    [Export]
    private Label _dashLabel;

    private bool _wasOnFloor = true;

    public override void _Ready()
    {
        _controller.EnsureValid();
        _sprite.EnsureValid();

        if (!_controller.IsValid() || !_sprite.IsValid())
            return;

        PlaySpawnTween();
        _sprite.Play("idle");
        PlayDashReadyFlash();
    }

    public override void _Process(double delta)
    {
        if (!_controller.IsValid() || !_sprite.IsValid())
            return;

        UpdateFacing();
        UpdateAnimation();

        if (_controller.DashJustBecameAvailable)
            PlayDashReadyFlash();

        bool isOnFloor = _controller.Body.IsOnFloor();
        if (isOnFloor && !_wasOnFloor)
            PlayLandingEffect();
        _wasOnFloor = isOnFloor;
    }

    public void SetController(DpmCharacterController controller)
    {
        _controller = controller;
    }

    private void PlayLandingEffect()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(_sprite, "modulate", new Color(2.0f, 1.0f, 1.0f, 1.0f), 0.04f);
        tween
            .TweenProperty(_sprite, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void PlayDashReadyFlash()
    {
        Tween tween = CreateTween();
        tween
            .TweenProperty(_sprite, "modulate", new Color(0.3f, 0.7f, 1.0f, 1.0f), 0.15f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_sprite, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.15f);
        tween
            .TweenProperty(_sprite, "modulate", new Color(0.3f, 0.7f, 1.0f, 1.0f), 0.15f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_sprite, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.15f);
        tween
            .TweenProperty(_sprite, "modulate", new Color(0.3f, 0.7f, 1.0f, 1.0f), 0.15f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        tween
            .TweenProperty(_sprite, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.4f)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        if (_dashLabel.IsValid())
        {
            Vector2 startPos = _dashLabel.Position;
            Tween labelTween = CreateTween();
            labelTween.Parallel().TweenProperty(_dashLabel, "modulate:a", 1.0f, 0.2f);
            labelTween
                .Parallel()
                .TweenProperty(_dashLabel, "position:y", startPos.Y - 20f, 0.3f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
            labelTween.TweenInterval(1.0f);

            labelTween.TweenProperty(_dashLabel, "modulate:a", 0.0f, 0.3f);
            labelTween.TweenProperty(_dashLabel, "position:y", startPos.Y, 0.0f);
        }
    }

    private void PlaySpawnTween()
    {
        _controller.IsSpawning = true;

        Vector2 currentScale = _sprite.Scale;
        Vector2 finalScale = new(-Mathf.Abs(currentScale.X), currentScale.Y);
        _sprite.Scale = finalScale * 0.5f;
        _sprite.Modulate = new Color(1, 1, 1, 0);

        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_sprite, "modulate:a", 1.0f, SpawnTweenDuration);
        tween.TweenProperty(_sprite, "scale", finalScale, SpawnTweenDuration);

        tween.Finished += () => _controller.IsSpawning = false;
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
            return Mathf.Abs(_controller.MoveAxis) > 0.1f ? "walk" : "idle";

        return body.Velocity.Y < 0.0f ? "jump" : "fall";
    }
}
