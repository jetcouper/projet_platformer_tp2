namespace TP2.Src;

using Godot;
using Utils;

public partial class MedLadder : Node
{
    [ExportGroup("References")]
    [Export]
    public Node2D Player;

    [Export]
    public Godot.Collections.Array<Area2D> Ladders;

    private DpmCharacterController _playerController;

    public override void _Ready()
    {
        Player.EnsureValid();

        _playerController = Player.GetNodeOrNull<DpmCharacterController>("DpmCharacterController");
        _playerController.EnsureValid();

        foreach (Area2D ladder in Ladders)
        {
            ladder.EnsureValid();
            ladder.BodyEntered += (body) => _playerController.OnLadderEntered(body, ladder);
            ladder.BodyExited += (body) => _playerController.OnLadderExited(body, ladder);
        }
    }
}
