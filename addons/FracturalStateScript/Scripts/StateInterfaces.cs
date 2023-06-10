using Fractural.NodeVars;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public interface IStateGraph : IState
    {

    }

    public interface IState : IAction
    {
        event System.Action Exited;
        void Reset();
    }

    public interface IAction : INodeVarContainer
    {
        void Play();
    }
}