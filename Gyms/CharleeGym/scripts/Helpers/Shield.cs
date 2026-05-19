using Godot;

public partial class Shield : StaticBody2D, IReflectiveSurface
{
    [Export]
    public float VisualFadeDuration = 0.2f;

    [Export]
    public float VisualInactiveScaleMultiplier = 0.9f;

    [Export]
    public bool IsActive { get; private set; } = true;

    [Export]
    private CollisionShape2D ProjectileCollision;

    [Export]
    public StaticBody2D PlatformBody;

    [Export]
    private CollisionShape2D PlatformCollision;

    [Export]
    public Sprite2D ShieldSprite;

    private Tween _visualTween;
    private Color _shieldBaseModulate = Colors.White;
    private Vector2 _shieldBaseScale = Vector2.One;

    public override void _Ready()
    {
        if (ShieldSprite != null)
        {
            _shieldBaseModulate = ShieldSprite.Modulate;
            _shieldBaseScale = ShieldSprite.Scale;
        }

        ApplyState(true);
    }

    public void Toggle()
    {
        IsActive = !IsActive;
        ApplyState();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        ApplyState();
    }

    public void AddShieldCollisionException(PhysicsBody2D body)
    {
        if (body == null)
            return;

        AddCollisionExceptionWith(body);
        body.AddCollisionExceptionWith(this);

        if (PlatformBody == null)
            return;

        PlatformBody.AddCollisionExceptionWith(body);
        body.AddCollisionExceptionWith(PlatformBody);
    }

    private void ApplyState()
    {
        ApplyState(false);
    }

    private void ApplyState(bool immediate)
    {
        ProjectileCollision?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !IsActive);
        PlatformCollision?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !IsActive);

        ApplyVisualState(immediate);
    }

    private void ApplyVisualState(bool immediate)
    {
        _visualTween?.Kill();

        float targetShieldAlpha = IsActive ? _shieldBaseModulate.A : 0.0f;
        float targetScaleMultiplier = IsActive ? 1.0f : VisualInactiveScaleMultiplier;

        if (ShieldSprite != null)
            ShieldSprite.Visible = true;

        if (immediate || Mathf.IsZeroApprox(VisualFadeDuration))
        {
            SetShieldAlpha(targetShieldAlpha);
            SetShieldScale(targetScaleMultiplier);
            FinalizeVisualState();
            return;
        }

        _visualTween = CreateTween();
        _visualTween.SetParallel(true);

        if (ShieldSprite != null)
            _visualTween.TweenMethod(
                Callable.From<float>(SetShieldAlpha),
                ShieldSprite.Modulate.A,
                targetShieldAlpha,
                VisualFadeDuration
            );

        if (ShieldSprite != null)
            _visualTween.TweenMethod(
                Callable.From<float>(SetShieldScale),
                GetCurrentScaleMultiplier(ShieldSprite.Scale, _shieldBaseScale),
                targetScaleMultiplier,
                VisualFadeDuration
            );

        _visualTween.Finished += FinalizeVisualState;
    }

    private void SetShieldAlpha(float alpha)
    {
        if (ShieldSprite == null)
            return;

        Color modulate = _shieldBaseModulate;
        modulate.A = alpha;
        ShieldSprite.Modulate = modulate;
    }

    private void SetShieldScale(float scaleMultiplier)
    {
        if (ShieldSprite == null)
            return;

        ShieldSprite.Scale = _shieldBaseScale * scaleMultiplier;
    }

    private static float GetCurrentScaleMultiplier(Vector2 currentScale, Vector2 baseScale)
    {
        if (!Mathf.IsZeroApprox(baseScale.X))
            return currentScale.X / baseScale.X;

        if (!Mathf.IsZeroApprox(baseScale.Y))
            return currentScale.Y / baseScale.Y;

        return 1.0f;
    }

    private void FinalizeVisualState()
    {
        if (ShieldSprite != null)
            ShieldSprite.Visible = IsActive;

        _visualTween = null;
    }
}
