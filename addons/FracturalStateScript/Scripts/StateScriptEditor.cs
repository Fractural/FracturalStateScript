using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class StateScriptEditor : Control
    {
        public override void _Ready()
        {
            GD.Print("Instanced!");
        }

        public void Unload()
        {

        }
    }
}
