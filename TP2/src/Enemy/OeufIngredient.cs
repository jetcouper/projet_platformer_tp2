using Godot;
using Utils;

public partial class OeufIngredient : Ingredient
{
    [Export]
    public Oeuf Oeuf;

    public override void _Ready()
    {
        Oeuf.EnsureValid();
        State = Oeuf.IngredientState;
    }

    public override void ChangeState()
    {
        if (!this.IsValid())
            return;

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
            return;

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
