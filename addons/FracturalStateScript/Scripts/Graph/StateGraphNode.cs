using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class StateGraphNode : StateScriptGraphNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#ba5414");
            Title = "State";
            base._Ready();
        }
    }
}