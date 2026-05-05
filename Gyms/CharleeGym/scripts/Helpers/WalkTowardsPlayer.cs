using Godot;

public partial class WalkTowardsPlayer : Node2D
{
    [ExportGroup("Walk")]
    [Export]
    public bool Is_Active = false;

    [Export]
    public CharacterBody2D OwnerNode;

    private int _Direction_Faced = 1;
    private float _Gravity_Force = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

    public void Walk_Towards_Player(CharacterBody2D Character, AnimatedSprite2D Sprite, float Speed)
    {
        if (Is_Active)
        {
            float _DirectionX = Mathf.Sign(Character.GlobalPosition.X - OwnerNode.GlobalPosition.X);
            int _Direction_Temp = _DirectionX < 0 ? 1 : -1;
            if (_Direction_Temp != _Direction_Faced)
            {
                Sprite.FlipH = _Direction_Temp < 0;
                _Direction_Faced = _Direction_Temp;
            }

            OwnerNode.Velocity = new Vector2(
                _DirectionX * Speed,
                OwnerNode.Velocity.Y + _Gravity_Force
            );
            OwnerNode.MoveAndSlide();
            OwnerNode.GlobalPosition = OwnerNode.GlobalPosition.Round();
        }
    }
}
