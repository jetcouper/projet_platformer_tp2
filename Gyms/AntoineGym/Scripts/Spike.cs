using Godot;
using TP2.Src;

public partial class Spike : Area2D
{
    [Export]
    CharacterBody2D player;

    public void _on_body_entered(Node2D body)
    {
        if (body == player)
        {
            DpmHealth health = body.GetNodeOrNull<DpmHealth>("DpmHealth");
            health?.TakeDamage(1);
        }
    }
}
