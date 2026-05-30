using Godot;
using Utils;

public partial class Goomba : Enemy
{
    [ExportGroup("Internal")]
    [Export]
    private float Speed;

    [Export]
    private float Scale_Anim;

    private int _Direction_Faced = 1;

    private bool Is_Active = false;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Is_Active)
            return;
        if (!this.IsValid())
            return;

        UpdateFacing();
        ApplyGravity(delta);

        float directionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
        Vector2 velocity = Velocity;
        velocity.X = directionX * Speed;
        Velocity = velocity;

        MoveAndSlide();
    }

    public void _Begin_walk()
    {
        if (!this.IsValid())
            return;

        Sprite.EnsureValid();
        Is_Active = true;
        Sprite.Play("inactive");
    }
}
