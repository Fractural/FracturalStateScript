using Fractural.NodeVars;
using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class Action3D : NodeVarContainer3D, IAction
    {
        public override HintString.DictNodeVarsMode Mode { get => HintString.DictNodeVarsMode.Attributes; set { } }

        public abstract void Play();
    }
}