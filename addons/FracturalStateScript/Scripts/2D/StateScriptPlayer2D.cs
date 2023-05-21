using Godot;
using System.Collections.Generic;
using Fractural.Utils;

namespace Fractural.StateScript
{
    public interface IStateScriptGraph
    {
        Node Node { get; }
        bool IsRunning { get; }
        void PlayGraph();
        void StopGraph();

        List<NodePath> States { get; set; }
        List<StateNodeConnection> Connections { get; set; }
    }
}