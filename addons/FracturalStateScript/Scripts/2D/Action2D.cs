using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    /// <summary>
    /// Attribute to mark a property as a NodeVar that's settable from the inspector. Used within State nodes.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Class, AllowMultiple = false)]
    public class NodeVarAttribute : System.Attribute
    {
        public NodeVarAttribute() { }
    }

    [Tool]
    public abstract class Action2D : Node2D, IAction
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
    }
}