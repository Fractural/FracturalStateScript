﻿using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class EntryGraphNode : StateScriptGraphNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#4e9c2d");
            Title = "Entry";
            base._Ready();
        }

        public override void UpdateState(IAction newState)
        {
            base.UpdateState(newState);
            RemoveSlotLeft(0); // Remove the input slot for Entry node
        }
    }
}