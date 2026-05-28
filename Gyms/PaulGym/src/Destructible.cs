using Godot;
using TP2.Src;

public partial class Destructible : Node2D
{

    [ExportGroup("External")]
    [Export]
    public AnimatedSprite2D Sprite;

    [Export]
    private int Vies = 3;

    public void Take_Damage(Node2D body)
    {
        GD.Print("take_damage reached");
        if (body is Projectile)
        {
            Vies -= 1;
            

            if (Vies <= 0)
            {
                Be_Destroyed();
            }
        }
    }
    public void _on_body_entered(Node2D body)
    {
        GD.Print("on body entered reached");
        if (body is Projectile)
        {
            Vies -= 1;
            

            if (Vies <= 0)
            {
                Be_Destroyed();
            }
        }
    }


    public void Be_Destroyed()
    {

        //explode into particles
        QueueFree();
        
    }
}
