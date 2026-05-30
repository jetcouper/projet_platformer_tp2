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

    public override void _ExitTree()
    {
        RemoveFromLevers(GetTree()?.CurrentScene);
    }

    private void RemoveFromLevers(Node node)
    {
        if (node == null)
        {
            return;
        }

        if (node is Levier levier && levier.Ingredients != null)
        {
            levier.Ingredients.Remove(this);
        }

        foreach (Node child in node.GetChildren())
        {
            RemoveFromLevers(child);
        }
    }
}
