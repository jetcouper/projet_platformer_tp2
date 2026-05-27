namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmHud : Node, IXpObserver, IHealthObserver
{
    [Export]
    public Node2D Player;

    [Export]
    private ProgressBar XpBar;

    private Godot.Collections.Array<AnimatedSprite2D> _hearts = [];
    private DpmCharacterController _controller;
    private DpmExperience _experience;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_text_indent"))
            _experience?.AddXp(10); // Touche cheat xp: Tab
    }

    public override void _Ready()
    {
        if (!Player.IsValid() || !XpBar.IsValid())
            return;

        var canvas = GetNodeOrNull<CanvasLayer>("CanvasLayer");
        if (canvas.IsValid())
        {
            foreach (var name in new[] { "heart1", "heart2", "heart3" })
            {
                var heart = canvas.GetNodeOrNull<AnimatedSprite2D>(name);
                if (heart.IsValid())
                    _hearts.Add(heart);
            }
        }

        _controller = Player.GetNodeOrNull<DpmCharacterController>("DpmCharacterController");
        _experience = _controller?.Experience;

        DpmHealth health = Player.GetNodeOrNull<DpmHealth>("DpmHealth");
        if (health.IsValid())
            health.SetHealthObserver(this);

        if (_experience != null)
        {
            _experience.SetObserver(this);
            XpBar.EnsureValid().Value = _experience.Xp;
            XpBar.EnsureValid().MaxValue = _experience.MaxXp;
        }
    }

    public void Init(DpmExperience experience)
    {
        _experience = experience;

        if (_experience != null)
        {
            _experience.SetObserver(this);
            XpBar.EnsureValid().Value = _experience.Xp;
            XpBar.EnsureValid().MaxValue = _experience.MaxXp;
        }
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

    public void OnDamageTaken()
    {
        for (int i = _hearts.Count - 1; i >= 0; i--)
        {
            if (_hearts[i].IsValid() && _hearts[i].Visible)
            {
                _hearts[i].Play("hit");
                int capturedI = i;
                GetTree().CreateTimer(0.4f).Timeout += () =>
                {
                    if (_hearts[capturedI].IsValid() && _hearts[capturedI].Visible)
                        _hearts[capturedI].Play("idle");
                };
                break;
            }
        }
    }

    public void OnLivesChanged(int currentLives)
    {
        for (int i = 0; i < _hearts.Count; i++)
        {
            if (_hearts[i].IsValid())
            {
                bool shouldBeVisible = i < currentLives;
                if (!shouldBeVisible && _hearts[i].Visible)
                {
                    int capturedI = i;
                    GetTree().CreateTimer(0.4f).Timeout += () =>
                    {
                        if (_hearts[capturedI].IsValid())
                            _hearts[capturedI].Visible = false;
                    };
                }
                else if (shouldBeVisible)
                {
                    _hearts[i].Visible = true;
                    _hearts[i].Play("idle");
                }
            }
        }

        if (currentLives <= 0)
            GetTree().CreateTimer(0.8f).Timeout += ShowGameOver;
    }

    private void ShowGameOver()
    {
        GetTree().Paused = true;

        CanvasLayer canvas = new() { ProcessMode = ProcessModeEnum.Always };

        ColorRect bg = new();
        bg.Color = new Color(0, 0, 0, 0.7f);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        canvas.AddChild(bg);

        VBoxContainer vbox = new();
        vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
        vbox.GrowHorizontal = Control.GrowDirection.Both;
        vbox.GrowVertical = Control.GrowDirection.Both;
        vbox.AddThemeConstantOverride("separation", 20);
        canvas.AddChild(vbox);

        Label label = new() { Text = "GAME OVER" };
        label.AddThemeColorOverride("font_color", new Color(1, 0.2f, 0.2f));
        label.AddThemeFontSizeOverride("font_size", 72);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(label);

        Button button = new() { Text = "Recommencer" };
        button.AddThemeFontSizeOverride("font_size", 28);
        button.Pressed += () =>
        {
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        };
        vbox.AddChild(button);

        GetTree().CurrentScene.AddChild(canvas);
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

    public void SetHealthObserver(DpmHealth health)
    {
        if (health.IsValid())
            health.SetHealthObserver(this);
    }
}
