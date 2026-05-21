using System;
using Godot;
using TP2.Src;

public partial class Void : Area2D
{
    [Export]
    CharacterBody2D player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void _on_body_entered(Node2D areaContact)
    {
        if (areaContact == player)
        {
            DpmHealth health = areaContact.GetNodeOrNull<DpmHealth>("DpmHealth");
            health?.TakeDamage((int)99999999);
        }
    }
}
