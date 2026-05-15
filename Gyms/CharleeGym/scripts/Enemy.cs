using Godot;

public partial class Enemy : CharacterBody2D
{
    [ExportGroup("External")]
    [Export]
    public AnimatedSprite2D Sprite;

    [Export]
    private int Vies;

    private int _Direction_Faced;

    public void Take_Damage(Node2D body)
    {
        if (body is Projectile)
        {
            Vies -= 1;
            Flash_Red();

            if (Vies <= 0)
            {
                Be_Destroyed();
            }
        }
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
