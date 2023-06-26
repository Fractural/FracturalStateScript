using Godot;
using System;

namespace Fractural.StateScript
{
    [Tool]
    public class ActionGraphNode : StateScriptGraphNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#ba7214");
            Title = "Action";
            AddSlotLeft("In");
            AddSlotRight("Out");
            base._Ready();
        }
    }
}