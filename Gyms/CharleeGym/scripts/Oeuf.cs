using System.Diagnostics.CodeAnalysis;
using Godot;

public partial class Oeuf : Enemy
{
	[Export]
	public Shield Shield;

	[Export]
	public bool IngredientState = true;

	[Export]
	public CharacterBody2D Character;

	public override void _Ready()
	{
		Sprite.Play("inactive");
		ApplyIngredientState();
	}

	public void SetIngredientState(bool state)
	{
		IngredientState = state;
		ApplyIngredientState();
	}

	public new void Take_Damage(Node2D body)
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
