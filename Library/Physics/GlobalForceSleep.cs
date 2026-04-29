using System.Threading.Tasks;
using Godot;

public partial class GlobalForceSleep : Node
{
    public override void _Ready()
    {
        //Convention pour les no discard
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        //Attendre 2 frames de physique pcq 1 n'est pas assez...
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        SleepAllRigidBodies(GetTree().Root);
    }

    //Appel recursif
    private void SleepAllRigidBodies(Node node)
    {
        if (node is RigidBody2D rigidBody)
        {
            rigidBody.Sleeping = true;
        }

        foreach (Node child in node.GetChildren())
        {
            SleepAllRigidBodies(child);
        }
    }
}
