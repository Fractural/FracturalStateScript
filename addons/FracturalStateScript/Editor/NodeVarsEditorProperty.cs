using Godot;
using System;
using System.Collections;

namespace Fractural.StateScript
{
    [Flags]
    public enum NodeVarType
    {
        Getter,
        Setter
    }

    public class NodeVarsEditorProperty : EditorProperty
    {
        // TODO: Use NodeVarsValueProperty
    }
}
