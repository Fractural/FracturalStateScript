using Godot;

namespace Fractural.StateScript
{
    public class ActionInspectorPlugin : EditorInspectorPlugin
    {
        public override bool CanHandle(Godot.Object @object)
        {
            return @object is IAction;
        }

        public override bool ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (path == nameof(IAction.NodeVars))
            {
                AddPropertyEditor(path, new NodeVarsEditorProperty());
                return true;
            }
            return false;
        }
    }
}
