namespace TP2.Src;

using Godot;

public partial class DcmBulletSpawnr : Node2D
{
    [ExportGroup("External")]
    [Export]
    public PackedScene WeaponScene;

    [Export]
    public Node2D Player;

    [Export]
    public AnimatedSprite2D Sprite;

    [ExportGroup("Settings")]
    [Export]
    public float FireRate = 0.50f;

    [Export]
    public float SpawnOffsetX = 40.0f;

    [Export]
    public string FireAction = "shoot";

    private Projectile activeWeapon;
    private float cooldown;

    public override void _Ready()
    {
        if (!InputMap.HasAction(FireAction))
        {
            InputMap.AddAction(FireAction);
            InputMap.ActionAddEvent(
                FireAction,
                new InputEventMouseButton { ButtonIndex = MouseButton.Left }
            );
        }

        Player ??= GetParent<Node2D>();
    }

    public override void _Process(double delta)
    {
        cooldown = Mathf.Max(cooldown - (float)delta, 0.0f);

        if (Input.IsActionPressed(FireAction) && cooldown <= 0.0f)
            SpawnBullet();
    }

    private void SpawnBullet()
    {
        if (WeaponScene == null || Player == null)
            return;

        float dir = GetFacingDirection();

        Node2D bodyRef = Player.GetNodeOrNull<Node2D>("CharacterBody2D") ?? Player;
        Vector2 spawnPos = bodyRef.GlobalPosition + new Vector2(dir * SpawnOffsetX, 0.0f);

        Projectile bullet = WeaponScene.Instantiate<Projectile>();
        bullet.GlobalPosition = spawnPos;
        GetTree().CurrentScene.AddChild(bullet);

        bullet.Launch(dir, bodyRef);
        activeWeapon = bullet;

        cooldown = FireRate;
    }

    private float GetFacingDirection()
    {
        return Sprite.Scale.X < 0.0f ? 1.0f : -1.0f;
    }
}
