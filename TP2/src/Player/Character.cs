namespace TP2.Src;

using Godot;
using Utils;

public partial class Character : CharacterBody2D
{
    [ExportGroup("Internal")]
    [Export]
    public DpmCharacterController Controller;

    [Export]
    public DpmHealth Health;

    [Export]
    public DpmExperience Experience;

    [Export]
    public DpmCharacterAnimator Animator;

    [ExportGroup("External")]
    [Export]
    public DpmHud Hud;

    public override void _Ready()
    {
        Controller.EnsureValid();
        Health.EnsureValid();
        Experience.EnsureValid();
        Animator.EnsureValid();
        Hud.EnsureValid();

        Health.SetController(Controller);
        Controller.Health = Health;
        Controller.Experience = Experience;
        Animator.SetController(Controller);

        Health.SetHealthObserver(Hud);
        Experience.SetObserver(Hud);
        Hud.Init(Experience);
    }
}
