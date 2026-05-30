using Godot;
using Utils;

public partial class Levier : Node2D
{
    [Export]
    public Godot.Collections.Array<Ingredient> Ingredients { get; set; }

    [Export]
    CharacterBody2D player;

    public bool State = false;
    AnimatedSprite2D sprite;

    bool CanBeActivated = false;

    public override void _Ready()
    {
        player.EnsureValid();

        sprite = GetNode<AnimatedSprite2D>("LeverSprite");
        sprite.EnsureValid();
    }

    public override void _Process(double delta) { }

    public override void _Input(InputEvent @event)
    {
        if (!this.IsValid())
            return;
        if (!CanBeActivated)
            return;
        if (!@event.IsActionPressed("interact"))
            return;

        sprite.SetFrameAndProgress(State ? 1 : 3, 0);
        State = !State;

        foreach (var ing in Ingredients)
        {
            if (ing.IsValid())
                ing.ChangeState();
        }
    }

    public void _on_body_entered(Node2D areaContact)
    {
        if (!this.IsValid())
            return;
        if (areaContact != player)
            return;
        if (CanBeActivated)
            return;

        CanBeActivated = true;
        sprite.SetFrameAndProgress(sprite.Frame + 1, 0);
    }

    public void _on_body_exited(Node2D areaContact)
    {
        if (!this.IsValid())
            return;
        if (areaContact != player)
            return;
        if (!CanBeActivated)
            return;

        CanBeActivated = false;
        sprite.SetFrameAndProgress(sprite.Frame - 1, 0);
    }
}
