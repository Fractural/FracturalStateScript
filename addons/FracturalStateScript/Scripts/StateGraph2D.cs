using Godot;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public interface IState
    {
        event System.Action Exited;
        void Init();
        void Play();
    }

    public abstract class State2D : Node2D, IState
    {
        public event System.Action Exited;
        public abstract void Init();
        public abstract void Play();
    }

    public class StateGraph2D : State2D
    {
        [Export]
        public bool IsRunning { get; private set; } = false;
        [Export]
        public List<StateNodeConnection> Connections { get; set; }

        private NodePath[] _states;

        public override void _Ready()
        {
            _nodeVars = NodeVarPaths.Select(x => GetNode(x)).ToArray();
            _states =
        }

        public override void Play()
        {
            if (IsRunning)
                StopGraph();
        }

        public void StopGraph()
        {
            // Reset all variables.
            foreach (var nodeVar in _nodeVars)
            {
                if (nodeVar is IResetNodeVar resetNodeVar)
                    resetNodeVar.Reset();
            }
        }
    }
}