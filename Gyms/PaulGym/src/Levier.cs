using System;
using System.Diagnostics;
using Godot;

public partial class Levier : Node2D
{
    [Export]
    public Godot.Collections.Array<Ingredient> Ingredients { get; set; }

    [Export]
    CharacterBody2D player;

    public bool State = false;
    AnimatedSprite2D sprite;

    bool CanBeActivated = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("LeverSprite");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public override void _Input(InputEvent @event)
    {
        if (CanBeActivated && @event.IsActionPressed("interact"))
        {
            if (State)
                sprite.SetFrameAndProgress(1, 0);
            else
                sprite.SetFrameAndProgress(3, 0);

            State = !State;

            foreach (var ing in Ingredients)
            {
                ing.ChangeState();
            }
        }
    }

    public void _on_body_entered(Node2D areaContact)
    {
        if (areaContact == player)
        {
            if (!CanBeActivated)
            {
                CanBeActivated = true;
                int currFrame = sprite.Frame;
                sprite.SetFrameAndProgress(currFrame + 1, 0);
            }
        }
    }

    public void _on_body_exited(Node2D areaContact)
    {
        if (areaContact == player)
        {
            if (CanBeActivated)
            {
                CanBeActivated = false;
                int currFrame = sprite.Frame;
                sprite.SetFrameAndProgress(currFrame - 1, 0);
            }
        }
    }
}
