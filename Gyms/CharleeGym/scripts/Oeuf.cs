using System.Diagnostics.CodeAnalysis;
using Godot;

public partial class Oeuf : Enemy
{
	[Export]
	public Shield Shield;

	[Export]
	public bool IngredientState = true;

	private int _Direction_Faced;

	public override void _Ready()
	{
		if (Shield != null)
		{
			AddCollisionExceptionWith(Shield);
			Shield.AddCollisionExceptionWith(this);
		}

		Sprite.Play("inactive");
		ApplyIngredientState();
	}

	public void SetIngredientState(bool state)
	{
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
