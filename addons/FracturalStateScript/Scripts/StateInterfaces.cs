using Fractural.NodeVars;
using Godot;
using GodotRollbackNetcode;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public interface IRawStateGraph
    {
        GDC.Dictionary StateNodePositions { get; set; }
        GDC.Dictionary RawConnections { get; set; }
    }

    public interface IStateGraph : IState
    {
        IAction[] States { get; }
        Entry[] EntryStates { get; }
        HashSet<Exit> ExitStates { get; }
        IDictionary<IAction, StateNodeConnection[]> StateToConnectionsDict { get; }
    }

    public interface IState : IAction
    {
        event System.Action Begin;
        event System.Action Aborted;
        void Stop();
        void StatePreProcess();
        void StateProcess();
        void StatePostProcess();
    }

    public interface IAction : INodeVarContainer, INetworkSerializable
    {
        event System.Action CommentChanged;
        event System.Action InfoChanged;
        event System.Action Exited;
        void Play();
        string Comment { get; set; }
        string Info { get; }
    }
}