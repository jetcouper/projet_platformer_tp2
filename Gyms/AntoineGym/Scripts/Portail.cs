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

    [Export]
    public bool AfficherCredits = false;

    public string[] Auteurs =
    [
        "Antoine Dextraze",
        "Charlotte Dupras",
        "Paul-Édouard Fortin",
        "Eddy Huart",
    ];

    public override void _Ready()
    {
        BodyEntered += _on_body_entered;
        _boutonRejouer.Pressed += OnRejouerPressed;
    }

    private void OnRejouerPressed()
    {
        if (AfficherCredits && Auteurs.Length > 0)
        {
            _overlayVictoire.Visible = false;
            ShowCredits();
        }
        else
        {
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
            _overlayVictoire.Visible = false;
        }
    }

    private async void _on_body_entered(Node2D body)
    {
        if (_transitioning || body is not CharacterBody2D)
            return;

        _transitioning = true;
        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

        if (!string.IsNullOrEmpty(NextScene) && !AfficherCredits)
            GetTree().ChangeSceneToFile(NextScene);
        else
        {
            _overlayVictoire.Visible = true;
            GetTree().Paused = true;
        }
    }

    private void ShowCredits()
    {
        GetTree().Paused = true;

        CanvasLayer canvas = new() { ProcessMode = ProcessModeEnum.Always };

        ColorRect bg = new();
        bg.Color = new Color(0, 0, 0, 0.85f);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        canvas.AddChild(bg);

        VBoxContainer vbox = new();
        vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
        vbox.GrowHorizontal = Control.GrowDirection.Both;
        vbox.GrowVertical = Control.GrowDirection.Both;
        vbox.AddThemeConstantOverride("separation", 16);
        canvas.AddChild(vbox);

        Label titre = new() { Text = "Merci d'avoir joué !" };
        titre.AddThemeColorOverride("font_color", new Color(1, 0.85f, 0));
        titre.AddThemeFontSizeOverride("font_size", 56);
        titre.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(titre);

        Label sousTitre = new() { Text = "\nCrédits" };
        sousTitre.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        sousTitre.AddThemeFontSizeOverride("font_size", 32);
        sousTitre.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(sousTitre);

        foreach (string auteur in Auteurs)
        {
            Label auteurLabel = new() { Text = auteur };
            auteurLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 1f));
            auteurLabel.AddThemeFontSizeOverride("font_size", 24);
            auteurLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(auteurLabel);
        }

        Button bouton = new() { Text = "Retour au niveau 1" };
        bouton.AddThemeFontSizeOverride("font_size", 24);
        bouton.Pressed += () =>
        {
            GetTree().Paused = false;
            if (!string.IsNullOrEmpty(NextScene))
                GetTree().ChangeSceneToFile(NextScene);
            else
                GetTree().ReloadCurrentScene();
        };
        vbox.AddChild(bouton);

        GetTree().CurrentScene.AddChild(canvas);
    }
}
