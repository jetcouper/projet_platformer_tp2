using Godot;
using Utils;

public partial class Oeuf : Enemy
{
    [Export]
    public Shield Shield;

    [Export]
    public bool IngredientState = true;

    private int _Direction_Faced;

    public override void _Ready()
    {
        Shield.EnsureValid();
        Sprite.EnsureValid();
        base._Ready();
        Shield?.AddShieldCollisionException(this);

        Sprite.Play("inactive");
        ApplyIngredientState();
    }

    public void SetIngredientState(bool state)
    {
        if (!this.IsValid())
            return;
        IngredientState = state;
        ApplyIngredientState();
    }

    public override void Take_Damage(Node2D body)
    {
        if (Shield?.IsActive == true)
        {
            return;
        }

        base.Take_Damage(body);
    }

    private void ApplyIngredientState()
    {
        Shield?.SetActive(IngredientState);
    }
}
