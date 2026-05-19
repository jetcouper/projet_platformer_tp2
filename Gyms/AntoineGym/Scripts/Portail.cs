using Godot;

public partial class Portail : Area2D
{
    [Export]
    public string NextScene = "";
    private bool _transitioning;

    [Export]
    private CanvasLayer _overlayVictoire;

    [Export]
    private Button _boutonRejouer;

    public override void _Ready()
    {
        BodyEntered += _on_body_entered;
        _boutonRejouer.Pressed += OnRejouerPressed;
    }

    private void OnRejouerPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
        _overlayVictoire.Visible = false;
    }

    private async void _on_body_entered(Node2D body)
    {
        if (_transitioning || body is not CharacterBody2D)
            return;

        _transitioning = true;
        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

        if (!string.IsNullOrEmpty(NextScene))
            GetTree().ChangeSceneToFile(NextScene);
        else
        {
            _overlayVictoire.Visible = true;
            GetTree().Paused = true;
        }
    }
}
