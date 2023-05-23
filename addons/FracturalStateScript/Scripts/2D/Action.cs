using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public abstract class Action : Node, IAction
    {
        /// <summary>
        /// Mapping of Names to NodePaths that lead to NodeVars.
        /// </summary>
        [Export]
        public GDC.Dictionary NodeVars { get; set; }
        private IDictionary<string, ISetNodeVar> _runtimeSetNodeVars;
        private IDictionary<string, IGetNodeVar> _runtimeGetNodeVars;
        public void SetNodeVar(string name, object value)
        {
            if (_runtimeSetNodeVars.TryGetValue(name, out ISetNodeVar setNodeVar))
                setNodeVar.Value = value;
        }
        public T GetNodeVar<T>(string name) => (T)GetNodeVar(name);
        public object GetNodeVar(string name)
        {
            if (_runtimeGetNodeVars.TryGetValue(name, out IGetNodeVar getNodeVar))
                return getNodeVar.Value;
            return null;
        }
        public abstract void Play();
        public override void _Ready()
        {
            _runtimeSetNodeVars = new Dictionary<string, ISetNodeVar>();
            _runtimeGetNodeVars = new Dictionary<string, IGetNodeVar>();

            foreach (string key in NodeVars.Keys)
            {
                var nodeVarData = NodeVarData.FromGDDict(NodeVars.Get<GDC.Dictionary>(key), key);
                var node = GetNode(nodeVarData.Path);
                switch (nodeVarData.Operation)
                {
                    case NodeVarOperation.Get:
                        if (node is IGetNodeVar)
                            _runtimeGetNodeVars.Add(key, (IGetNodeVar)node);
                        else
                            GD.PushError($"{nameof(Action)}: NodeVars expected IGetNodeVar for {nodeVarData.Path}.");
                        break;
                    case NodeVarOperation.Set:
                        if (node is ISetNodeVar)
                            _runtimeSetNodeVars.Add(key, (ISetNodeVar)node);
                        else
                            GD.PushError($"{nameof(Action)}: NodeVars expected ISetNodeVar for {nodeVarData.Path}.");
                        break;
                    case NodeVarOperation.GetSet:
                        if (node is IGetNodeVar)
                            _runtimeGetNodeVars.Add(key, (IGetNodeVar)node);
                        else if (node is ISetNodeVar)
                            _runtimeSetNodeVars.Add(key, (ISetNodeVar)node);
                        else
                            GD.PushError($"{nameof(Action)}: NodeVars expected IGetNodeVar or ISetNodeVar for {nodeVarData.Path}.");
                        break;
                }
            }
        }
    }
}