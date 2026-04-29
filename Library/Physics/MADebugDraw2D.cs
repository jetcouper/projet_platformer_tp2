//ATTENTION, faites que ce script se charge automatiquement et vous pourrez utiliser ses fonctions.
//Dans Paramètres du projet->Généraux, ajoutez le script afin qu'il se charge automatiquement.
//Exemple d'utilisation
//MADebugDraw2D.Instance.DrawCircleWorld(new Vector2(100,100), 48, Colors.Red, 10.0);
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class MADebugDraw2D : Node2D
{
    private class Shape
    {
        public string Type;
        public Array Args;
        public Color Color;
        public double ExpireTime;
    }

    public static MADebugDraw2D Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    private List<Shape> _shapes = new();

    public MADebugDraw2D()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        double now = Time.GetTicksMsec() / 1000.0;

        var filtered = new List<Shape>();

        foreach (Shape s in _shapes)
        {
            if (s.ExpireTime < 0.0 || s.ExpireTime > now)
                filtered.Add(s);
        }

        _shapes = filtered;
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (Shape s in _shapes)
        {
            switch (s.Type)
            {
                case "line":
                    DrawLine((Vector2)s.Args[0], (Vector2)s.Args[1], s.Color, 2.0f);
                    break;

                case "point":
                    DrawCircle((Vector2)s.Args[0], 3.0f, s.Color);
                    break;

                case "circle":
                    DrawCircle((Vector2)s.Args[0], (float)s.Args[1], s.Color);
                    break;

                case "box":
                    Vector2 pos = (Vector2)s.Args[0];
                    Vector2 size = (Vector2)s.Args[1];
                    DrawRect(new Rect2(pos - size * 0.5f, size), s.Color, false, 2.0f);
                    break;
            }
        }
    }

    // -------------------------------
    // Helper functions
    // -------------------------------

    public void DrawLineWorld(Vector2 a, Vector2 b, Color? color = null, double duration = 0.0)
    {
        AddShape("line", new Array { a, b }, color ?? new Color(1, 1, 1), duration);
    }

    public void DrawPointWorld(Vector2 p, Color? color = null, double duration = 0.0)
    {
        AddShape("point", new Array { p }, color ?? new Color(1, 0, 0), duration);
    }

    public void DrawCircleWorld(
        Vector2 center,
        float radius,
        Color? color = null,
        double duration = 0.0
    )
    {
        AddShape(
            "circle",
            new Array { center, radius },
            color ?? new Color(0, 1, 0, 0.5f),
            duration
        );
    }

    public void DrawBoxWorld(
        Vector2 center,
        Vector2 size,
        Color? color = null,
        double duration = 0.0
    )
    {
        AddShape("box", new Array { center, size }, color ?? new Color(0, 0.6f, 1), duration);
    }

    // -------------------------------
    // Internal helper
    // -------------------------------

    private void AddShape(string type, Array args, Color color, double duration)
    {
        Shape s = new Shape();
        s.Type = type;
        s.Args = args;
        s.Color = color;
        s.ExpireTime = duration <= 0.0 ? -1.0 : (Time.GetTicksMsec() / 1000.0) + duration;

        _shapes.Add(s);
        QueueRedraw();
    }
}
