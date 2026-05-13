using Godot;

public partial class OeufIngredient : Ingredient
{
    [Export]
    public Oeuf Oeuf;

    public override void _Ready()
    {
        if (Oeuf != null)
        {
            State = Oeuf.IngredientState;
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();
        Oeuf?.SetIngredientState(State);
    }
}
