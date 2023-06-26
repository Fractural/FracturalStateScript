using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class ExitGraphNode : StateScriptGraphNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#ba1414");
            Title = "Exit";
            base._Ready();
        }

        public override void UpdateState(IAction newState)
        {
            base.UpdateState(newState);
            RemoveSlotRight(0); // Remove the output slot for Exit node
        }
    }
}