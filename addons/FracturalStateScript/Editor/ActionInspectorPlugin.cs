using Godot;
using Fractural.Plugin;
using System;
using System.Reflection;
using System.Collections.Generic;
using Fractural.Utils;

namespace Fractural.StateScript
{
    public class ActionInspectorPlugin : EditorInspectorPlugin
    {
        private EditorPlugin _plugin;

        public ActionInspectorPlugin() { }
        public ActionInspectorPlugin(EditorPlugin plugin)
        {
            _plugin = plugin;
        }

        public override bool CanHandle(Godot.Object @object)
        {
            if (!(@object is Node node)) return false;
            var objectType = node.GetCSharpType();
            return typeof(IAction).IsAssignableFrom(objectType);
        }

        public override bool ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (!(@object is Node node)) return false;
            if (path == nameof(IAction.NodeVars))
            {
                var objectType = node.GetCSharpType();
                // User can add NodeVars to StateGraphs themselves.
                List<NodeVarData> fixedNodeVars = null;
                if (!typeof(IStateGraph).IsAssignableFrom(objectType))
                {
                    // Use NodeVar attributes attached to properties on the State's C# script
                    // to determine what NodeVars are exposed
                    //
                    // User cannot edit the NodeVars of a IState, since it's determined
                    // by the State's script.
                    fixedNodeVars = new List<NodeVarData>();
                    foreach (var property in objectType.GetProperties())
                    {
                        var attribute = property.GetCustomAttribute<NodeVarAttribute>();
                        if (attribute == null)
                            continue;
                        fixedNodeVars.Add(new NodeVarData()
                        {
                            Name = property.Name,
                            ValueType = property.PropertyType,
                            Operation = attribute.Operation,
                            Path = new NodePath()
                        });
                    }
                }
                AddPropertyEditor(path, new ValueEditorProperty(new NodeVarsValueProperty(_plugin.GetEditorInterface().GetEditedSceneRoot(), @object as Node, fixedNodeVars?.ToArray())));
                return true;
            }
            return false;
        }
    }
}
