namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmHud : Node, IXpObserver
{
    [Export]
    public Node2D Player;

    [Export]
    private ProgressBar XpBar;

    private DpmCharacterController _controller;
    private DpmExperience _experience;

    // Touche Tab : cheat fait augmenter l'experience
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_text_indent")) // Tab
            _experience?.AddXp(10);
    }

    public override void _Ready()
    {
        if (!Player.IsValid() || !XpBar.IsValid())
            return;

        _controller = Player.GetNodeOrNull<DpmCharacterController>("DpmCharacterController");
        _experience = _controller?.Experience;

        if (_experience == null)
            return;

        _experience.SetObserver(this);

        XpBar.Value = _experience.Xp;
        XpBar.MaxValue = _experience.MaxXp;
    }

    public void OnXpChanged(float currentXp, float maxXp)
    {
        if (!XpBar.IsValid())
            return;

        XpBar.Value = currentXp;
        XpBar.MaxValue = maxXp;

        Vector2 originalPos = XpBar.Position;

        Tween tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Elastic);
        tween.TweenProperty(XpBar, "scale", new Vector2(1.3f, 1.6f), 0.1f);
        tween.TweenProperty(XpBar, "scale", new Vector2(1.0f, 1.0f), 0.5f);

        Tween shakeTween = CreateTween();
        shakeTween.TweenProperty(XpBar, "position", originalPos + new Vector2(5, 0), 0.05f);
        shakeTween.TweenProperty(XpBar, "position", originalPos + new Vector2(-5, 0), 0.05f);
        shakeTween.TweenProperty(XpBar, "position", originalPos + new Vector2(3, 0), 0.05f);
        shakeTween.TweenProperty(XpBar, "position", originalPos + new Vector2(-3, 0), 0.05f);
        shakeTween.TweenProperty(XpBar, "position", originalPos, 0.05f);

        if (currentXp >= maxXp)
            ShowMaxLabel();
    }

    private void ShowMaxLabel()
    {
        CanvasLayer canvas = new();
        Label label = new() { Text = "XP MAX!" };
        label.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0));
        label.AddThemeFontSizeOverride("font_size", 64);
        label.SetAnchorsPreset(Control.LayoutPreset.Center);
        label.Position = new Vector2(0, -100);

        canvas.AddChild(label);
        GetTree().CurrentScene.AddChild(canvas);

        Tween tween = label.CreateTween();
        tween.TweenProperty(label, "scale", new Vector2(1.5f, 1.5f), 0.3f);
        tween.TweenInterval(2.0f);
        tween.TweenProperty(label, "modulate:a", 0.0f, 0.5f);
        tween.TweenCallback(Callable.From(canvas.QueueFree));
        tween.TweenCallback(Callable.From(() => _experience?.ResetXp()));
    }
}
