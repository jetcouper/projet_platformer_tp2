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

    private int _Direction_Faced;

    private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

    public override void _Ready()
    {
        Sprite.Play("inactive");
        ApplyIngredientState();
    }

    public override void _PhysicsProcess(double delta)
    {
        float _DirectionX = Mathf.Sign(Character.GlobalPosition.X - GlobalPosition.X);
        int _Direction_Temp = _DirectionX < 0 ? 1 : -1;
        if (_Direction_Temp != _Direction_Faced)
        {
            Sprite.FlipH = _Direction_Temp < 0;
            _Direction_Faced = _Direction_Temp;
        }

        Velocity = new Vector2(0, Velocity.Y + _Gravity_Force);

        MoveAndSlide();
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
