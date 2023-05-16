using Fractural.Plugin;
using Godot;
using System;

namespace Fractural.StateScript
{
    [Tool]
    public class Plugin : ExtendedPlugin
    {
        public override string PluginName => "Fractural State Script";
        public StateScriptEditor stateScriptEditor;
        public EditorSelection editorSelection;

        private Godot.Object focusedObject = null;
        /// <summary>
        /// Can be StateMachine/StateMachinePlayer
        /// </summary>
        public Godot.Object FocusedObject
        {
            get => focusedObject;
            set
            {
                if (focusedObject != value)
                {
                    focusedObject = value;
                    OnFocusedObjectChanged(value);
                }
            }
        }

        protected override void Load()
        {
            var stateScriptEditorPrefab = ResourceLoader.Load<PackedScene>("res://addons/FracturalStateScript/StateScriptEditor.tscn");
            stateScriptEditor = stateScriptEditorPrefab.Instance<StateScriptEditor>();
            ShowStateScriptEditor();

            editorSelection = GetEditorInterface().GetSelection();
            editorSelection.Connect("selection_changed", this, nameof(OnEditorSelectionSelectionChanged));

        }

        protected override void Unload()
        {
            stateScriptEditor.QueueFree();
        }

        public void ShowStateScriptEditor()
        {
            if (FocusedObject != null && stateScriptEditor != null)
            {
                if (!stateScriptEditor.IsInsideTree())
                    AddControlToBottomPanel(stateScriptEditor, "State Script");
                MakeBottomPanelItemVisible(stateScriptEditor);
            }
        }

        public void HideStateMachineEditor()
        {
            if (stateScriptEditor.IsInsideTree())
            {
                stateScriptEditor.Unload();
                RemoveControlFromBottomPanel(stateScriptEditor);
            }
        }

        private void OnEditorSelectionSelectionChanged()
        {
            var selectedNodes = editorSelection.GetSelectedNodes();
            if (selectedNodes.Count == 1)
            {
                var selectedNode = selectedNodes[0];
                if (selectedNode is StateScriptPlayer2D stateScriptPlayer)
                {
                    FocusedObject = stateScriptPlayer;
                    return;
                }
            }
            FocusedObject = null;
        }

        private void OnFocusedObjectChanged(Godot.Object newFocusedObj)
        {
            if (newFocusedObj is StateScriptPlayer2D)
            {

            }
        }
    }
}
