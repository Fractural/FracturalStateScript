using Godot;
using Fractural.Plugin;
using System;
using System.Reflection;
using System.Collections.Generic;

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
            return @object is IAction;
        }

        public override bool ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (path == nameof(IAction.NodeVars))
            {
                // User can add NodeVars to StateGraphs themselves.
                bool addEnabled = true;
                List<Tuple<string, Type>> fixedNodeVars = new List<Tuple<string, Type>>();
                if (@object is IState)
                {
                    addEnabled = false;
                    // Use NodeVar attributes to determine what NodeVars are exposed
                    foreach (var property in @object.GetType().GetProperties(BindingFlags.Public))
                        if (property.IsDefined(typeof(NodeVarAttribute), true))
                        {
                            fixedNodeVars.Add(Tuple.Create(property.Name, property.PropertyType));
                        }
                }
                // TODO NOW: Refactor Tuple<string, Type> into dedicated class. Also include the NodeVarType in the tuple?
                //           ie. is the NodeVar a Getter, a Setter, or Both?
                AddPropertyEditor(path, new ValueEditorProperty(new NodeVarsValueProperty(_plugin.GetEditorInterface().GetEditedSceneRoot(), @object as Node, fixedNodeVars.ToArray(), addEnabled)));
                return true;
            }
            return false;
        }
    }
}
