using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class EntryGraphNode : StateScriptGraphNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#4e9c2d");
            Title = "Entry";
            AddSlotRight("Out");
            base._Ready();
        }
    }
}