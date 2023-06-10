using Fractural.NodeVars;
using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class Action2D : NodeVarContainer2D, IAction
    {
        public override HintString.DictNodeVarsMode Mode { get => HintString.DictNodeVarsMode.Attributes; set { } }

        public abstract void Play();
    }
}