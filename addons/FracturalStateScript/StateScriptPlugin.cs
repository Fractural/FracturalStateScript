using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
using Godot;
using System;

namespace Fractural.StateScript
{
    [Tool]
    public class StateScriptPlugin : ExtendedPlugin
    {
        public override string PluginName => "Fractural State Script";
        public StateScriptEditor stateScriptEditor;
        public EditorSelection editorSelection;

        private IStateGraph focusedStateGraph = null;
        public IStateGraph FocusedStateGraph
        {
            get => focusedStateGraph;
            set
            {
                if (focusedStateGraph != value)
                {
                    focusedStateGraph = value;
                    OnFocusedStateGraphChanged(value);
                }
            }
        }

        protected override void Load()
        {
            AssetsRegistry = new EditorAssetsRegistry(this);

            stateScriptEditor = new StateScriptEditor(this, AssetsRegistry);
            ShowStateScriptEditor();

            editorSelection = GetEditorInterface().GetSelection();
            editorSelection.Connect("selection_changed", this, nameof(OnEditorSelectionSelectionChanged));
        }

        protected override void Unload()
        {
            AssetsRegistry = null;
            HideStateScriptEditor();
            stateScriptEditor.QueueFree();

            editorSelection = GetEditorInterface().GetSelection();
            editorSelection.Disconnect("selection_changed", this, nameof(OnEditorSelectionSelectionChanged));
        }

        public void ShowStateScriptEditor()
        {
            if (FocusedStateGraph != null && stateScriptEditor != null)
            {
                if (!stateScriptEditor.IsInsideTree())
                    AddControlToBottomPanel(stateScriptEditor, "State Script");
                MakeBottomPanelItemVisible(stateScriptEditor);
            }
        }

        public void HideStateScriptEditor()
        {
            if (stateScriptEditor.IsInsideTree())
            {
                stateScriptEditor.Unload();
                RemoveControlFromBottomPanel(stateScriptEditor);
            }
        }

        // TODO: Finish StateScript graph editor
        private void OnEditorSelectionSelectionChanged()
        {
            var selectedNodes = editorSelection.GetSelectedNodes();
            if (selectedNodes.Count == 1)
            {
                var selectedNode = selectedNodes[0];
                if (selectedNode is IStateGraph stateGraph)
                {
                    FocusedStateGraph = stateGraph;
                    return;
                }
            }
            FocusedStateGraph = null;
        }

        private void OnFocusedStateGraphChanged(IStateGraph newStateGraph)
        {
            if (newStateGraph == null)
                HideStateScriptEditor();
            else
            {
                ShowStateScriptEditor();
                stateScriptEditor.Load(newStateGraph);
            }
        }
    }
}
