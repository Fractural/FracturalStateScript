using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class EntryNode : StateScriptNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#4e9c2d");
            Title = "Entry";
            var styleBox = GetStylebox("breakpoint");
            AddSlotRight("Out");
            base._Ready();
        }
    }
}