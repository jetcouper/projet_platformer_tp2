namespace TP2.Src;

using Godot;
using Utils;

public partial class DpmExperience : Node2D
{
    private IXpObserver _observer;

    [Export]
    public float MaxXp = 100.0f;

    [Export]
    public DcmBulletSpawnr BulletSpawnr;

    private float _currentXp = 0.0f;

    [Export]
    public float Xp
    {
        get => _currentXp;
        set
        {
            _currentXp = Mathf.Clamp(value, 0, MaxXp);
            NotifyObserver();
        }
    }

    public bool IsFull => _currentXp >= MaxXp;

    public override void _Ready()
    {
        BulletSpawnr.EnsureValid();
        NotifyObserver();
    }

    public void AddXp(float amount)
    {
        if (!this.IsValid())
            return;
        if (amount <= 0 || IsFull)
            return;

        Xp += amount;

        if (IsFull)
        {
            GD.Print("XP maximum!");
            if (BulletSpawnr.IsValid())
                BulletSpawnr.UpgradeFireRate();
        }
    }

    public void ResetXp()
    {
        if (!this.IsValid())
            return;
        Xp = 0.0f;
    }

    public void SetObserver(IXpObserver observer)
    {
        _observer = observer;
    }

    public void ClearObserver()
    {
        _observer = null;
    }

    private void NotifyObserver()
    {
        _observer?.OnXpChanged(_currentXp, MaxXp);
    }
}
