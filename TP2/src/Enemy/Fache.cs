using System;
using Godot;
using Utils;

public partial class Fache : RangedEnemy
{
    [Export]
    public float JumpChance = 0.3f;

    [Export]
    public float JumpVelocity = -250.0f;

    [ExportGroup("Attack")]
    [Export]
    public float MaxWait = 10.0f;

    [Export]
    public float MinWait = 1.0f;

    [Export]
    public Timer Timer;

    private float NextShot;

    private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

    public override void _Ready()
    {
        Timer.EnsureValid();
        base._Ready();
        Timer.Timeout += DecideNextAction;
        RestartTimer();
    }

    public override void _ExitTree()
    {
        if (Timer != null)
            Timer.Timeout -= DecideNextAction;
    }

    public void RandomNextShot()
    {
        Random rand = new Random();

        NextShot = (float)(MinWait + rand.NextDouble() * (MaxWait - MinWait));
    }

    public void RestartTimer()
    {
        if (!this.IsValid())
            return;

        Sprite.EnsureValid();
        Sprite.Play("idle");
        RandomNextShot();

        Timer.WaitTime = NextShot;
        Timer.Start();
    }

    public void DecideNextAction()
    {
        if (!this.IsValid())
            return;
        if (GD.Randf() < JumpChance)
        {
            Jump();
            return;
        }

        Attack();
    }

    public async void Attack()
    {
        if (!this.IsValid())
            return;

        Sprite.EnsureValid();
        Sprite.Play("attack");

        SceneTree tree = GetTree();
        if (tree == null)
            return;

        await ToSignal(tree.CreateTimer(0.5), "timeout");

        if (!this.IsValid() || GetTree() == null)
            return;

        Shoot();

        tree = GetTree();
        if (tree == null)
            return;

        await ToSignal(tree.CreateTimer(0.5), "timeout");

        if (!this.IsValid() || GetTree() == null)
            return;

        RestartTimer();
    }

    public async void Jump()
    {
        if (!this.IsValid() || GetTree() == null)
            return;

        if (!IsOnFloor())
            return;

        Vector2 velocity = Velocity;
        velocity.Y = JumpVelocity;
        Velocity = velocity;

        Sprite.EnsureValid();
        Sprite.Play("jump");

        while (!IsOnFloor() || Velocity.Y < 0)
        {
            SceneTree tree = GetTree();
            if (tree == null || !this.IsValid())
                return;

            await ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
        }

        if (!this.IsValid() || GetTree() == null)
            return;

        RestartTimer();
    }
}
