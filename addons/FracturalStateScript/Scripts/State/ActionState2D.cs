using Fractural.NodeVars;
using Fractural.Utils;
using Godot;
using GodotRollbackNetcode;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class ActionState2D : ParentPropagatedNodeVarContainer2D, IAction, INetworkSerializable
    {
        public override HintString.DictNodeVarsMode Mode { get => HintString.DictNodeVarsMode.Attributes; set { } }

        [Output("Out")]
        public event System.Action Exited;

        [Input("In")]
        public virtual void Play()
        {
            _Play();
            InvokeExited();
        }

        protected virtual void _Play() { }

        protected void InvokeExited() => Exited?.Invoke();

        public virtual GDC.Dictionary _SaveState()
        {
            var dict = new GDC.Dictionary();

            var nodeVarsDict = new GDC.Dictionary();
            foreach (var nodeVar in NodeVars.Values)
                if (nodeVar.Strategy is ISerializableNodeVarStrategy serializableNodeVarStrategy)
                    nodeVarsDict[nodeVar.Name] = serializableNodeVarStrategy.Save();
            return dict;
        }

        // TODO: Refactor RollbackNetcodePlugin to use pure C# serialization if performance is an issue.
        public virtual void _LoadState(GDC.Dictionary state)
        {
            var nodeVarsDict = state.Get<GDC.Dictionary>(nameof(NodeVars));
            foreach (string key in nodeVarsDict.Keys)
            {
                var serializableNodeVarStrategy = NodeVars[key].Strategy as ISerializableNodeVarStrategy;
                serializableNodeVarStrategy?.Load(nodeVarsDict[key]);
            }
        }
    }
}