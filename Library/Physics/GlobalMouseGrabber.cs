using Godot;

public partial class GlobalMouseGrabber : Node2D
{
    private RigidBody2D _pickedBody = null;
    private Vector2 _lastMousePos = Vector2.Zero;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed)
            {
                TryPickBody();
            }
            else
            {
                _pickedBody = null;
            }
        }
    }

    private void TryPickBody()
    {
        var space = GetWorld2D().DirectSpaceState;
        var mousePos = GetGlobalMousePosition();

        var query = new PhysicsPointQueryParameters2D
        {
            Position = mousePos,
            CollideWithBodies = true,
            CollideWithAreas = false,
        };

        var result = space.IntersectPoint(query);

        foreach (var hit in result)
        {
            var collider = hit["collider"].AsGodotObject();
            if (collider is RigidBody2D body)
            {
                _pickedBody = body;
                _lastMousePos = mousePos;
                break;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_pickedBody == null)
            return;

        Vector2 mousePos = GetGlobalMousePosition();

        if (_pickedBody.Freeze)
        {
            _pickedBody.GlobalPosition = mousePos;
        }
        else
        {
            _pickedBody.LinearVelocity = (mousePos - _lastMousePos) / (float)delta;
            _lastMousePos = mousePos;
        }
    }
}
