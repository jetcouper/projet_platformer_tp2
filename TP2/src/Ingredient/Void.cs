using Godot;
using TP2.Src;
using Utils;

public partial class Void : Area2D
{
    [Export]
    CharacterBody2D player;

    public override void _Ready()
    {
        player.EnsureValid();
    }

    public override void _Process(double delta) { }

    public void _on_body_entered(Node2D areaContact)
    {
        if (!this.IsValid())
            return;
        if (areaContact != player)
            return;

        DpmHealth health = areaContact.GetNodeOrNull<DpmHealth>("DpmHealth");
        health?.KillInstantly();
    }
}
