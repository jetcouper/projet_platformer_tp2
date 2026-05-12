using System;
using Godot;

public partial class DcmWindSystem : Node2D
{
    private float _SpeedMove = 1.0f;

    [Export]
    public float SpeedMove
    {
        get => _SpeedMove;
        set
        {
            //Style early return pour éviter les if imbriqués
            if (Mathf.IsEqualApprox(_SpeedMove, value))
            {
                return;
            }
            //Value = valeur passée au setter
            _SpeedMove = value;
            SetSpeedMove(_SpeedMove);
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Tween tw = CreateTween();
        tw.TweenProperty(this, "SpeedMove", _SpeedMove + 0.5f, 2.0f);
        tw.TweenProperty(this, "SpeedMove", _SpeedMove - 0.5f, 2.0f);
        tw.SetLoops();
    }

    public void SetSpeedMove(float InSpeedMove)
    {
        //GetChildren() retourne les enfants directes(Pas les petits-enfants)
        foreach (Node2D child in GetChildren())
        {
            if (child is Oscillation osc)
            {
                osc.SetSpeedMove(InSpeedMove);
            }
            else if (child is OscillationOiseau osc2)
            {
                osc2.SetSpeedMove(InSpeedMove);
            }
            else if (child is OscillationBat osc3)
            {
                osc3.SetSpeedMove(InSpeedMove);
            }
        }
    }
}
