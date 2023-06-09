using Fractural.DependencyInjection;
using Fractural.NodeVars;
using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public abstract class Action : Node, IAction, INodeVarContainer
    {
        // Native C# Dictionary is around x9 faster than Godot Dictionary
        private IDictionary<string, NodeVarData> _dictNodeVars;
        public IDictionary<string, NodeVarData> DictNodeVars { get; private set; }

        private GDC.Dictionary _nodeVars;
        private HintString.DictNodeVarsMode _mode = HintString.DictNodeVarsMode.LocalAttributes;
        [Export]
        public HintString.DictNodeVarsMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                PropertyListChangedNotify();
            }
        }

        public GDC.Dictionary NodeVars { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private PackedSceneDefaultValuesRegistry _packedSceneDefaultValuesRegistry;

        public void Construct(DIContainer container)
        {
            _packedSceneDefaultValuesRegistry = container.Resolve<PackedSceneDefaultValuesRegistry>();
        }

        public override void _Ready()
        {
#if TOOLS
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
#endif
            DictNodeVars = new Dictionary<string, NodeVarData>();
            foreach (string key in _nodeVars.Keys)
                AddNodeVar(NodeVarData.FromGDDict(_nodeVars.Get<GDC.Dictionary>(key), key));

            if (Filename != "")
            {
                var defaultNodeVars = _packedSceneDefaultValuesRegistry.GetDefaultValue<GDC.Dictionary>(Filename, nameof(_nodeVars));
                foreach (string key in defaultNodeVars.Keys)
                    if (!DictNodeVars.ContainsKey(key))
                        AddNodeVar(NodeVarData.FromGDDict(defaultNodeVars.Get<GDC.Dictionary>(key), key));
            }

            var defaultAttributes = NodeVarUtils.GetNodeVarsFromAttributes(GetType());
            foreach (var nodeVar in defaultAttributes)
                if (!DictNodeVars.ContainsKey(nodeVar.Name))
                    AddNodeVar(nodeVar);
        }

        /// <summary>
        /// Adds a new NodeVar to the container. This is used at runtime.
        /// </summary>
        /// <param name="nodeVar"></param>
        public void AddNodeVar(NodeVarData nodeVar)
        {
            DictNodeVars.Add(nodeVar.Name, nodeVar);

            if (nodeVar.IsPointer)
                nodeVar.Container = GetNode<INodeVarContainer>(nodeVar.ContainerPath);
            else
                nodeVar.Reset();
        }

        /// <summary>
        /// Gets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetDictNodeVar<T>(string key, bool force = false) => (T)GetDictNodeVar(key, force);

        /// <summary>
        /// Gets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetDictNodeVar(string key, bool force = false)
        {
            var data = DictNodeVars[key];
            if (!force && data.Operation != NodeVarOperation.Get && data.Operation != NodeVarOperation.GetSet)
                throw new Exception($"NodeVar: Attempted to get a non-getttable NodeVar \"{data.Name}\".");
            return data.Value;
        }

        /// <summary>
        /// Sets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetDictNodeVar(string key, object value, bool force = false)
        {
            var data = DictNodeVars[key];
            if (!force && data.Operation != NodeVarOperation.Set && data.Operation != NodeVarOperation.GetSet)
                throw new Exception($"NodeVar: Attempted to set a non-setttable NodeVar \"{data.Name}\".");
            data.Value = value;
        }

        /// <summary>
        /// Gets a list of all DictNodeVars for this <see cref="INodeVarContainer"/>
        /// </summary>
        /// <returns></returns>
        public NodeVarData[] GetNodeVarsList()
        {
            int index = 0;
            NodeVarData[] result = new NodeVarData[_nodeVars.Count];
            foreach (string key in _nodeVars.Keys)
            {
                result[index] = NodeVarData.FromGDDict(_nodeVars.Get<GDC.Dictionary>(key), key);
                index++;
            }
            return result;
        }

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddDictNodeVarsProp(
                name: nameof(_nodeVars),
                mode: Mode
            );
            return builder.Build();
        }

        public abstract void Play();
    }
}