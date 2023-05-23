using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    // TODO NOW: Finish integerating NodeVarData
    public enum NodeVarOperation
    {
        Get,
        Set,
        GetSet,
        Private
    }

    [Tool]
    public class NodeVarsValueProperty : ValueProperty<GDC.Dictionary>
    {
        private Button _editButton;
        private Control _container;
        private Button _addElementButton;
        private VBoxContainer _keyValueEntriesVBox;
        private Node _sceneRoot;
        private Node _relativeToNode;
        private NodeVarData[] _fixedNodeVars;

        private string EditButtonText => $"NodeVars [{Value.Count}]";
        private bool IsFixed => _fixedNodeVars != null;

        public NodeVarsValueProperty() { }
        public NodeVarsValueProperty(Node sceneRoot, Node relativeToNode, NodeVarData[] fixedNodeVars = null) : base()
        {
            _sceneRoot = sceneRoot;
            _relativeToNode = relativeToNode;
            _fixedNodeVars = fixedNodeVars;

            _editButton = new Button();
            _editButton.ToggleMode = true;
            _editButton.ClipText = true;
            _editButton.Connect("toggled", this, nameof(OnEditToggled));
            AddChild(_editButton);

            if (!IsFixed)
            {
                _addElementButton = new Button();
                _addElementButton.Text = "Add NodeVar";
                _addElementButton.Connect("pressed", this, nameof(OnAddElementPressed));
                _addElementButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                _addElementButton.RectMinSize = new Vector2(24 * 4, 0);
            }

            _keyValueEntriesVBox = new VBoxContainer();

            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            if (_fixedNodeVars == null)
                vbox.AddChild(_addElementButton);
            vbox.AddChild(_keyValueEntriesVBox);

            _container = vbox;
            AddChild(_container);
        }

        public override void _Ready()
        {
            base._Ready();
            if (NodeUtils.IsInEditorSceneTab(this))
                return;

            if (_addElementButton != null)
                _addElementButton.Icon = GetIcon("Add", "EditorIcons");
            GetViewport().Connect("gui_focus_changed", this, nameof(OnFocusChanged));
        }

        private int _setBottomEditorFrameTimer = 1;
        public override void _Process(float delta)
        {
            if (_setBottomEditorFrameTimer > 0)
                _setBottomEditorFrameTimer--;
            else
            {
                SetBottomEditor(_container);
                SetProcess(false);
            }
        }

        private Control _currentFocused;
        private void OnFocusChanged(Control control) => _currentFocused = control;

        public override void UpdateProperty()
        {
            // Force the entries in Value dict to match the entires in _fixedNodeVars.
            if (IsFixed)
            {
                bool changed = false;
                if (Value == null)
                    Value = new GDC.Dictionary();
                // Popupulate Value with any _fixedNodeVars that it is missing
                foreach (var entry in _fixedNodeVars)
                {
                    if (Value.Contains(entry.Name))
                        continue;
                    // Value dict does not contain an entry in _fixedNodeVars, so we add it to Value dict
                    changed = true;
                    Value[entry.Name] = entry.ToGDDict();
                }
                foreach (string key in Value.Keys)
                {
                    if (_fixedNodeVars.Any(x => x.Name == key))
                        continue;
                    // _fixedNodeVars doesn't contain an entry in Value dict, so we remove it from Value dict
                    changed = true;
                    Value.Remove(key);
                }

                if (changed)
                {
                    InvokeValueChanged(Value);  // InvokeValueChanged should call UpdateProperty again.
                    return;
                }
            }

            GD.Print("Updating prop");
            _container.Visible = this.GetMeta<bool>("visible", true);   // Default to being visible if the meta tag doesn't exist.
            _editButton.Pressed = _container.Visible;
            _editButton.Text = EditButtonText;

            GD.Print("Updating prop 2");
            int index = 0;
            int childCount = _keyValueEntriesVBox.GetChildCount();

            GD.Print("Updating prop 3");
            var currFocusedEntry = _currentFocused?.GetAncestor<NodeVarsValuePropertyEntry>();
            if (currFocusedEntry != null)
            {
                int keyIndex = 0;
                foreach (var key in Value.Keys)
                {
                    if (key != null && key.Equals(currFocusedEntry.Data.Name))
                        break;
                    keyIndex++;
                }
                if (keyIndex == Value.Keys.Count)
                {
                    // Set current focused entry back to null. We couldn't
                    // find the entry in the new dictionary, meaning this entry
                    // must have been deleted, therefore we don't care about it
                    // anymore.
                    currFocusedEntry = null;
                }
                else
                {
                    var targetEntry = _keyValueEntriesVBox.GetChild<NodeVarsValuePropertyEntry>(keyIndex);
                    _keyValueEntriesVBox.SwapChildren(targetEntry, currFocusedEntry);
                }
            }

            GD.Print("Updating prop 4, with Value: ", Value);
            foreach (string key in Value.Keys)
            {
                GD.Print("\tkey: ", key);
                NodeVarsValuePropertyEntry entry;
                if (index >= childCount)
                    entry = CreateDefaultEntry();
                else
                    entry = _keyValueEntriesVBox.GetChild<NodeVarsValuePropertyEntry>(index);
                GD.Print("\tentry: ", entry);

                if (currFocusedEntry == null || entry != currFocusedEntry)
                    entry.SetData(NodeVarData.FromGDDict(Value.Get<GDC.Dictionary>(key), key));
                index++;
            }

            GD.Print("Updating prop 5");
            // Free extra entries
            if (index < childCount)
            {
                for (int i = childCount - 1; i >= index; i--)
                {
                    var entry = _keyValueEntriesVBox.GetChild<NodeVarsValuePropertyEntry>(i);
                    entry.NameChanged -= OnEntryNameChanged;
                    entry.DataChanged -= OnEntryDataChanged;
                    entry.QueueFree();
                }
            }

            GD.Print("Updating prop 6");
            if (!IsInstanceValid(currFocusedEntry))
                currFocusedEntry = null;

            GD.Print("Updating prop 7");
            var nextKey = DefaultValueUtils.GetDefault(Value.Keys.Cast<string>());
        }

        private new ValueProperty CreateValueProperty(Type type)
        {
            var property = ValueProperty.CreateValueProperty(type);
            if (type == typeof(NodePath) && property is NodePathValueProperty valueProperty)
            {
                valueProperty.SelectRootNode = _sceneRoot;
                valueProperty.RelativeToNode = _relativeToNode;
            }
            return property;
        }

        private NodeVarsValuePropertyEntry CreateDefaultEntry()
        {
            GD.Print("Create default entry");
            var entry = new NodeVarsValuePropertyEntry(_sceneRoot, _relativeToNode, IsFixed);
            entry.NameChanged += OnEntryNameChanged;
            entry.DataChanged += OnEntryDataChanged;
            entry.Deleted += OnEntryDeleted;
            // Add entry if we ran out of existing ones
            _keyValueEntriesVBox.AddChild(entry);
            GD.Print("Create default entry finished");

            return entry;
        }

        private void OnEntryNameChanged(string oldKey, NodeVarsValuePropertyEntry entry)
        {
            var newKey = entry.Data.Name;
            if (Value.Contains(newKey))
            {
                // Revert CurrentKey back
                entry.Data.Name = oldKey;
                // Reject change since the newKey already exists
                entry.NameProperty.SetValue(oldKey);
                return;
            }
            var currValue = Value[oldKey];
            Value.Remove(oldKey);
            Value[newKey] = currValue;
            InvokeValueChanged(Value);
        }

        private void OnEntryDataChanged(object key, object newValue)
        {
            Value[key] = newValue;
            InvokeValueChanged(Value);
        }

        private void OnEntryDeleted(object key)
        {
            Value.Remove(key);
            InvokeValueChanged(Value);
        }

        private void OnAddElementPressed()
        {
            // The adding is done in UpdateProperty
            // Note the edited a field in Value doesn't invoke ValueChanged, so we must do it manually
            //
            // Use default types for the newly added element
            var nextKey = DefaultValueUtils.GetDefault(Value.Keys.Cast<string>());
            Value[nextKey] = DefaultValueUtils.GetDefault<NodePath>();
            InvokeValueChanged(Value);
        }

        private void OnEditToggled(bool toggled)
        {
            SetMeta("visible", toggled);
            _container.Visible = toggled;
        }
    }
}
