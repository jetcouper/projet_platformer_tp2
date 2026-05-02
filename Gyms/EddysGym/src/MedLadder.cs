using Godot;
using Utils;

public partial class MedLadder : Node
{
    [ExportGroup("References")]
    [Export]
    public Node2D Player;

    [Export]
    public Godot.Collections.Array<Area2D> Ladders; //Tableau d'échelles

    private DpmCharacterController _playerController;

    public override void _Ready()
    {
        Player.EnsureValid();

        if (!Player.IsValid())
            return;

        _playerController =
            Player.GetNodeOrNull("CharacterBody2D/DpmCharacterController")
            as DpmCharacterController;

        _playerController.EnsureValid();

        if (!_playerController.IsValid())
            return;

        // On parcourt toutes les échelles du niveau
        foreach (Area2D ladder in Ladders)
        {
            if (ladder.IsValid())
            {
                ladder.BodyEntered += (body) => _playerController.OnLadderEntered(body, ladder);
                ladder.BodyExited += (body) => _playerController.OnLadderExited(body, ladder);
            }
        }
    }
}
