using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Fractural.StateScript
{
    // TODO: Finish StateGraph2D
    // TODO: Test NodeVar
    // TODO: Test Expression lexing, parsing + evaluation
    // TODO: Make example StateScript
    //public class StateGraph2D : State2D
    //{
    //    [Export]
    //    public bool IsRunning { get; private set; } = false;
    //    [Export]
    //    public List<StateNodeConnection> Connections { get; set; }

    //    private NodePath[] _states;

    //    public override void _Ready()
    //    {
    //        _nodeVars = NodeVarPaths.Select(x => GetNode(x)).ToArray();
    //        _states =
    //    }

    //    public override void Play()
    //    {
    //        if (IsRunning)
    //            StopGraph();
    //    }

    //    public void StopGraph()
    //    {
    //        // Reset all variables.
    //        foreach (var nodeVar in _nodeVars)
    //        {
    //            if (nodeVar is IResetNodeVar resetNodeVar)
    //                resetNodeVar.Reset();
    //        }
    //    }
    //}
}