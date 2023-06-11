using Fractural.NodeVars;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    // TODO: Test StateGraph2D
    [Tool]
    public class StateGraph2D : State2D
    {
        [Export]
        private GDC.Dictionary _connections;


        private IAction[] _states;
        public HashSet<IState> CurrentContinuousStates { get; private set; }

        public Entry[] EntryStates { get; private set; }
        public HashSet<Exit> ExitStates { get; private set; }
        public IDictionary<IAction, StateNodeConnection[]> StateToConnectionsDict { get; private set; }

        public override void _Ready()
        {
#if TOOLS
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
#endif
            _states = GetChildren().Cast<Node>().Where(x => x is IAction).Cast<IAction>().ToArray();
            EntryStates = _states.Where(x => x is Entry).Cast<Entry>().ToArray();
            ExitStates = new HashSet<Exit>(_states.Where(x => x is Exit).Cast<Exit>());
            CurrentContinuousStates = new HashSet<IState>();
            StateToConnectionsDict = new Dictionary<IAction, StateNodeConnection[]>();
            foreach (NodePath statePath in _connections.Keys)
            {
                var from = GetNode<IAction>(statePath);
                var connectionsGDCArray = _connections.Get<GDC.Array>(statePath);
                var connectionsArray = new StateNodeConnection[connectionsGDCArray.Count];
                int i = 0;
                foreach (GDC.Dictionary connectionDict in connectionsGDCArray)
                {
                    var connection = new StateNodeConnection();
                    connection.FromGDDict(connectionDict, from as Node);
                    connectionsArray[i] = connection;
                    i++;
                }
                StateToConnectionsDict[from] = connectionsArray;
            }

            foreach (var state in _states)
            {
                if (state is IState continuousState)
                    continuousState.Exited += () => ExitState(continuousState);
            }

            foreach (var pair in StateToConnectionsDict)
            {
                var state = pair.Key;
                var connections = pair.Value;

                // TODO LATER: Maybe add caching for GetEvent and GetMethod here,
                //             since this is ran everytime a StateGraph is created.
                //             For games that create and delete nodes often (ie.
                //             bullet hells), this needs to be performant.
                foreach (var connection in connections)
                {
                    var fromEvent = state.GetType().GetEvent(connection.FromEvent);
                    if (fromEvent == null)
                    {
                        GD.PrintErr($"{nameof(StateGraph2D)} [{Filename}]: Could not find fromEvent for connection {connection}");
                        continue;
                    }
                    var toState = connection.ToState;
                    if (toState == null)
                    {
                        GD.PrintErr($"{nameof(StateGraph2D)} [{Filename}]: Could not find toState for connection {connection}");
                        continue;
                    }
                    var toMethod = state.GetType().GetMethod(connection.ToMethod);
                    if (toMethod == null)
                    {
                        GD.PrintErr($"{nameof(StateGraph2D)} [{Filename}]: Could not find toMethod for connection {connection}");
                        continue;
                    }
                    fromEvent.AddEventHandler(toState, (Action)(() => TransitionToState(toState, toMethod)));
                }
            }
        }

        protected override void _Play()
        {
            CurrentContinuousStates.Clear();

            foreach (var state in EntryStates)
                state.Play();
        }

        protected override void _Stop()
        {
            // Reset all variables.
            foreach (var nodeVar in _nodeVars)
            {
                if (nodeVar is DynamicNodeVarData dynamicNodeVar)
                    dynamicNodeVar.Reset();
            }
        }

        private void ExitState(IState exited)
        {
            GD.Print($"{nameof(StateGraph2D)} [{Filename}]: State exited: \"{(exited as Node)?.Name}\"");
            CurrentContinuousStates.Remove(exited);
        }

        private void TransitionToState(IAction to, MethodInfo toMethod)
        {
            GD.Print($"{nameof(StateGraph2D)} [{Filename}]: Transitioned to ", (to as Node).Name);
            if (to is IState continuousState)
                CurrentContinuousStates.Add(continuousState);
            toMethod.Invoke(to, null);
        }

        public override void StatePreProcess()
        {
            foreach (var state in CurrentContinuousStates)
                state.StatePreProcess();
        }

        public override void StateProcess()
        {
            foreach (var state in CurrentContinuousStates)
                state.StateProcess();
        }

        public override void StatePostProcess()
        {
            foreach (var state in CurrentContinuousStates)
                state.StatePostProcess();
        }
    }
}