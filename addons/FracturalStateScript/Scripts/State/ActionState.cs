using Fractural.DependencyInjection;
using Fractural.NodeVars;
using Fractural.Utils;
using Godot;
using GodotRollbackNetcode;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class ActionState : ParentPropagatedNodeVarContainer, IAction, INetworkSerializable
    {
        public override HintString.DictNodeVarsMode Mode { get => HintString.DictNodeVarsMode.Attributes; set { } }

        public abstract void Play();

        // TODO: Refactor RollbackNetcodePlugin to use pure C# serialization if performance is an issue.
        public virtual void _LoadState(GDC.Dictionary state)
        {
            var nodeVarsDict = state.Get<GDC.Dictionary>(nameof(NodeVars));
            foreach (string key in nodeVarsDict.Keys)
            {
                var serializableNodeVar = NodeVars[key] as ISerializableNodeVar;
                serializableNodeVar.Load(nodeVarsDict[key]);
            }
        }

        public virtual GDC.Dictionary _SaveState()
        {
            var dict = new GDC.Dictionary();

            var nodeVarsDict = new GDC.Dictionary();
            foreach (var nodeVar in NodeVars.Values)
                if (nodeVar is ISerializableNodeVar serializableNodeVar)
                    nodeVarsDict[nodeVar.Name] = serializableNodeVar.Save();
            return dict;
        }
    }
}