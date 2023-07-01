using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public static class StateScriptUtils
    {
        public struct ConnectionArrayDictFromGDDictResult
        {
            public IDictionary<IAction, StateNodeConnection[]> Dictionary { get; set; }
            public bool HadParseFailures { get; set; }
        }

        public static ConnectionArrayDictFromGDDictResult ConnectionArrayDictFromGDDict(Node stateGraphNode, GDC.Dictionary rawConnectionDict)
        {
            var result = new ConnectionArrayDictFromGDDictResult()
            {
                Dictionary = new Dictionary<IAction, StateNodeConnection[]>(),
                HadParseFailures = false
            };
            if (rawConnectionDict == null)
                return result;
            foreach (string fromStateName in rawConnectionDict.Keys)
            {
                var from = stateGraphNode.GetNodeOrNull<IAction>(fromStateName);
                if (from == null)
                {
                    result.HadParseFailures = true;
                    continue;
                }
                var connectionsGDCArray = rawConnectionDict.Get<GDC.Array>(fromStateName);
                if (connectionsGDCArray.Count == 0)
                {
                    result.HadParseFailures = true;
                    continue;
                }
                var connectionsArray = new StateNodeConnection[connectionsGDCArray.Count];
                int i = 0;
                foreach (GDC.Dictionary connectionDict in connectionsGDCArray)
                {
                    var connection = new StateNodeConnection();
                    if (!connection.FromGDDict(connectionDict, stateGraphNode))
                        result.HadParseFailures = true;
                    else
                        connectionsArray[i] = connection;
                    i++;
                }
                result.Dictionary[from] = connectionsArray;
            }
            return result;
        }

        public static GDC.Dictionary ConnectionListDictToGDDict(IDictionary<IAction, IList<StateNodeConnection>> connectionListDict)
        {
            var stateToConnectionsDict = new GDC.Dictionary();
            foreach (IAction state in connectionListDict.Keys)
            {
                if (!(state is Node fromStateNode)) continue;

                var connectionsArray = new GDC.Array();
                foreach (var connection in connectionListDict[state])
                    connectionsArray.Add(connection.ToGDDict());
                stateToConnectionsDict[fromStateNode.Name] = connectionsArray;
            }
            return stateToConnectionsDict;
        }

        public struct ConnectionListDictFromGDDictResult
        {
            public IDictionary<IAction, IList<StateNodeConnection>> Dictionary { get; set; }
            public bool HadParseFailures { get; set; }
        }

        public static ConnectionListDictFromGDDictResult ConnectionListDictFromGDDict(Node stateGraphNode, GDC.Dictionary rawConnectionDict)
        {
            ConnectionListDictFromGDDictResult result = new ConnectionListDictFromGDDictResult()
            {
                Dictionary = new Dictionary<IAction, IList<StateNodeConnection>>(),
                HadParseFailures = false
            };
            if (rawConnectionDict == null)
                return result;
            foreach (string fromStateName in rawConnectionDict.Keys)
            {
                var from = stateGraphNode.GetNodeOrNull<IAction>(fromStateName);
                if (from == null)
                {
                    result.HadParseFailures = true;
                    continue;
                }
                var connectionsGDCArray = rawConnectionDict.Get<GDC.Array>(fromStateName);
                if (connectionsGDCArray.Count == 0)
                {
                    result.HadParseFailures = true;
                    continue;
                }
                var connectionsList = new List<StateNodeConnection>(connectionsGDCArray.Count);
                foreach (GDC.Dictionary connectionDict in connectionsGDCArray)
                {
                    var connection = new StateNodeConnection();
                    if (!connection.FromGDDict(connectionDict, stateGraphNode))
                        result.HadParseFailures = true;
                    else
                        connectionsList.Add(connection);
                }

                result.Dictionary[from] = connectionsList;
            }
            return result;
        }
    }
}