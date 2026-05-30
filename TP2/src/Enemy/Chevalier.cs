using System;
using Godot;
using Utils;

public partial class Chevalier : RangedEnemy
{
    [ExportGroup("Attack")]
    [Export]
    public float MaxWait;

    [Export]
    public float MinWait;

    [Export]
    public Timer Timer;

    [Export]
    public Shield Shield;
    private float NextShot;

    private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

    public override void _Ready()
    {
        Timer.EnsureValid();
        Shield.EnsureValid();
        base._Ready();
        if (Shield != null)
            Shield.AddShieldCollisionException(this);

        Timer.Timeout += Attack;
        RestartTimer();
    }

    public void RandomNextShot()
    {
        Random rand = new Random();

        NextShot = (float)(MinWait + rand.NextDouble() * (MaxWait - MinWait));
    }

    public void RestartTimer()
    {
        Shield.EnsureValid();
        Sprite.EnsureValid();
        Shield.SetActive(true);
        Sprite.Play("idle");
        RandomNextShot();
        Timer.WaitTime = NextShot;
        Timer.Start();
    }

    public override void Take_Damage(Node2D body)
    {
        if (Shield?.IsActive == true)
        {
            return;
        }

        base.Take_Damage(body);
    }

    public async void Attack()
    {
        if (!this.IsValid())
            return;
        Shield.EnsureValid();
        Shield.SetActive(false);

        Sprite.EnsureValid();
        Sprite.Play("attack");

        await ToSignal(GetTree().CreateTimer(0.5), "timeout");

        Shoot();

        await ToSignal(GetTree().CreateTimer(0.5), "timeout");

        RestartTimer();
    }
}
