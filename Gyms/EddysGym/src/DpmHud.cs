using Godot;

public partial class DpmHud : Node, IXpObserver
{
    [Export]
    public Node2D Player;

    [Export]
    private ProgressBar XpBar;

    private DpmCharacterController _controller;
    private DpmExperience _experience;

    public override void _Ready()
    {
        _controller = Player?.GetNodeOrNull<DpmCharacterController>("DpmCharacterController");
        _experience = _controller?.Experience;

        if (_experience == null)
        {
            return;
        }

        _experience.SetObserver(this);

        XpBar.Value = _experience.Xp;
        XpBar.MaxValue = _experience.MaxXp;
    }

    public void OnXpChanged(float currentXp, float maxXp)
    {
        XpBar.Value = currentXp;
        XpBar.MaxValue = maxXp;
    }
}
