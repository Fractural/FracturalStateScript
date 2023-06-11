using Fractural.NodeVars;
using Godot;
using GodotRollbackNetcode;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public interface IStateGraph : IState
    {

    }

    public interface IState : IAction
    {
        event System.Action Exited;
        void Stop();
        void StatePreProcess();
        void StateProcess();
        void StatePostProcess();
    }

    public interface IAction : INodeVarContainer, INetworkSerializable
    {
        void Play();
    }
}