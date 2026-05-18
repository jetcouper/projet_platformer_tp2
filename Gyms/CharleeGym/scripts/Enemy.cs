using Godot;

public partial class Enemy : CharacterBody2D
{
    [ExportGroup("External")]
    [Export]
    public AnimatedSprite2D Sprite;

    [Export]
    private int Vies;

    [Export]
    private PackedScene EnemyDamageParticleScene;

    [Export]
    public CharacterBody2D Character;

    public int Direction_Faced;

    public float Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

    protected void UpdateFacing()
    {
        float directionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
        int directionTemp = directionX < 0 ? 1 : -1;

        if (directionTemp != Direction_Faced)
        {
            Sprite.FlipH = directionTemp < 0;
            Direction_Faced = directionTemp;
        }
    }

    protected void ApplyGravity(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.Y += Gravity_Force * (float)delta;
        Velocity = velocity;
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateFacing();
        ApplyGravity(delta);

        Vector2 velocity = Velocity;
        velocity.X = 0;
        Velocity = velocity;

        MoveAndSlide();
    }

    public virtual void Take_Damage(Node2D body)
    {
        Vies -= 1;
        SpawnDamageParticles(body.GlobalPosition);
        Flash_Red();

        if (Vies <= 0)
        {
            Be_Destroyed();
        }
    }

    public void OnAreaBodyEntered(Node2D body)
    {
        if (body == this)
            return;

        if (body is TP2.Src.Projectile)
        {
            Take_Damage(body);
            return;
        }

        Node health = body.GetNodeOrNull("DpmHealth");
        if (health == null)
            return;

        Node controller = body.GetNodeOrNull("DpmCharacterController");
        if (controller != null && controller.Get("IsDashing").AsBool())
            return;

        health.Call("TakeDamage", 1);
    }

    private void SpawnDamageParticles(Vector2 position)
    {
        if (EnemyDamageParticleScene == null)
            return;

        GpuParticles2D particles = EnemyDamageParticleScene.Instantiate<GpuParticles2D>();

        GetParent()?.AddChild(particles);

        particles.GlobalPosition = position;

        particles.Restart();
        particles.Emitting = true;

        double duration = particles.Lifetime / Mathf.Max(particles.SpeedScale, 0.001f) + 0.1f;

        GetTree().CreateTimer(duration).Timeout += particles.QueueFree;
    }

    public void Flash_Red()
    {
        if (Sprite.Material is not ShaderMaterial)
            return;

        var mat = (ShaderMaterial)Sprite.Material;

        var tween = CreateTween();

        tween.TweenMethod(
            Callable.From<float>(value => mat.SetShaderParameter("flash_strength", value)),
            0.0f,
            1.0f,
            0.05f
        );
        tween.TweenMethod(
            Callable.From<float>(value => mat.SetShaderParameter("flash_strength", value)),
            1.0f,
            0.0f,
            0.2f
        );
    }

    public void Be_Destroyed()
    {
        if (Sprite.Material is not ShaderMaterial)
        {
            QueueFree();
            return;
        }

        var mat = (ShaderMaterial)Sprite.Material;

        var tween = CreateTween();

        tween.TweenMethod(
            Callable.From<float>(value => mat.SetShaderParameter("death_strength", value)),
            0.0f,
            1.0f,
            0.1f
        );

        tween.TweenInterval(0.3f);

        tween.Finished += QueueFree;
    }
}
