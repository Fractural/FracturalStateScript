using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public static class StateScriptUtils
    {
        public static IDictionary<IAction, StateNodeConnection[]> ConnectionArrayDictFromGDDict(Node source, GDC.Dictionary rawConnectionDict)
        {
            var stateToConnectionsDict = new Dictionary<IAction, StateNodeConnection[]>();
            if (rawConnectionDict == null)
                return stateToConnectionsDict;
            foreach (NodePath statePath in rawConnectionDict.Keys)
            {
                var from = source.GetNode<IAction>(statePath);
                var connectionsGDCArray = rawConnectionDict.Get<GDC.Array>(statePath);
                var connectionsArray = new StateNodeConnection[connectionsGDCArray.Count];
                int i = 0;
                foreach (GDC.Dictionary connectionDict in connectionsGDCArray)
                {
                    var connection = new StateNodeConnection();
                    connection.FromGDDict(connectionDict, from as Node);
                    connectionsArray[i] = connection;
                    i++;
                }
                stateToConnectionsDict[from] = connectionsArray;
            }
            return stateToConnectionsDict;
        }

        public static GDC.Dictionary ConnectionListDictToGDDict(Node source, IDictionary<IAction, IList<StateNodeConnection>> connectionListDict)
        {
            var stateToConnectionsDict = new GDC.Dictionary();
            foreach (IAction state in connectionListDict.Keys)
            {
                if (!(state is Node fromStateNode)) continue;
                var path = source.GetPathTo(fromStateNode);
                var connectionsArray = new GDC.Array();
                foreach (var connection in connectionListDict[state])
                    connectionsArray.Add(connection.ToGDDict(fromStateNode));
                stateToConnectionsDict[path] = connectionsArray;
            }
            return stateToConnectionsDict;
        }

        public static IDictionary<IAction, IList<StateNodeConnection>> ConnectionListDictFromGDDict(Node source, GDC.Dictionary rawConnectionDict)
        {
            var stateToConnectionsDict = new Dictionary<IAction, IList<StateNodeConnection>>();
            if (rawConnectionDict == null)
                return stateToConnectionsDict;
            foreach (NodePath statePath in rawConnectionDict.Keys)
            {
                var from = source.GetNode<IAction>(statePath);
                var connectionsGDCArray = rawConnectionDict.Get<GDC.Array>(statePath);
                var connectionsList = new List<StateNodeConnection>(connectionsGDCArray.Count);
                foreach (GDC.Dictionary connectionDict in connectionsGDCArray)
                {
                    var connection = new StateNodeConnection();
                    connection.FromGDDict(connectionDict, from as Node);
                    connectionsList.Add(connection);
                }
                stateToConnectionsDict[from] = connectionsList;
            }
            return stateToConnectionsDict;
        }
    }
}