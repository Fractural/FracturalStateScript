using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public class NodeVarsValueProperty : ValueProperty<GDC.Dictionary>
    {
        private Button _editButton;
        private Control _container;
        private Button _addElementButton;
        private VBoxContainer _keyValueEntriesVBox;
        private IDictionary<string, Type> _nodeVarsDict = new Dictionary<string, Type>();;
        private Node _sceneRoot;

        private string EditButtonText => $"NodeVars [{Value.Count}]";

        public NodeVarsValueProperty() { }
        public NodeVarsValueProperty(Node sceneRoot, Tuple<string, Type>[] fixedNodeVars, bool addEnabled) : base()
        {
            _sceneRoot = sceneRoot;

            foreach (var nodeVar in fixedNodeVars)
                _nodeVarsDict.Add(nodeVar.Item1, nodeVar.Item2);

            _editButton = new Button();
            _editButton.ToggleMode = true;
            _editButton.ClipText = true;
            _editButton.Connect("toggled", this, nameof(OnEditToggled));
            AddChild(_editButton);

            if (addEnabled)
            {
                _addElementButton = new Button();
                _addElementButton.Text = "Add Key Value Pair";
                _addElementButton.Connect("pressed", this, nameof(OnAddElementPressed));
                _addElementButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                _addElementButton.RectMinSize = new Vector2(24 * 4, 0);
            }

            _keyValueEntriesVBox = new VBoxContainer();

            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            if (addEnabled)
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
            _container.Visible = this.GetMeta<bool>("visible", false);
            _editButton.Pressed = _container.Visible;

            _editButton.Text = EditButtonText;

            int index = 0;
            int childCount = _keyValueEntriesVBox.GetChildCount();

            var currFocusedEntry = _currentFocused?.GetAncestor<DictionaryValuePropertyKeyValueEntry>();
            if (currFocusedEntry != null)
            {
                int keyIndex = 0;
                foreach (var key in Value.Keys)
                {
                    if (key != null && key.Equals(currFocusedEntry.CurrentKey))
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
                    var targetEntry = _keyValueEntriesVBox.GetChild<DictionaryValuePropertyKeyValueEntry>(keyIndex);
                    _keyValueEntriesVBox.SwapChildren(targetEntry, currFocusedEntry);
                }
            }

            foreach (var key in Value.Keys)
            {
                DictionaryValuePropertyKeyValueEntry entry;
                if (index >= childCount)
                    entry = CreateDefaultEntry();
                else
                    entry = _keyValueEntriesVBox.GetChild<DictionaryValuePropertyKeyValueEntry>(index);

                if (currFocusedEntry == null || entry != currFocusedEntry)
                    entry.SetKeyValue(key, Value[key]);
                index++;
            }

            // Free extra entries
            if (index < childCount)
            {
                for (int i = childCount - 1; i >= index; i--)
                {
                    var entry = _keyValueEntriesVBox.GetChild<DictionaryValuePropertyKeyValueEntry>(i);
                    entry.KeyChanged -= OnDictKeyChanged;
                    entry.ValueChanged -= OnDictValueChanged;
                    entry.QueueFree();
                }
            }

            if (!IsInstanceValid(currFocusedEntry))
                currFocusedEntry = null;

            var nextKey = DefaultValueUtils.GetDefault(Value.Keys.Cast<string>());
        }

        private new ValueProperty CreateValueProperty(Type type)
        {
            var property = ValueProperty.CreateValueProperty(type);
            if (type == typeof(NodePath) && property is NodePathValueProperty valueProperty)
                valueProperty.SelectRootNode = _sceneRoot;
            return property;
        }

        private NodeVarsValuePropertyKeyValueEntry CreateDefaultEntry()
        {
            // TODO NOW: FInish NodeVarsValueProperty
            var entry = new NodeVarsValuePropertyKeyValueEntry(CreateValueProperty(_keyType), CreateValueProperty(_valueType));
            entry.KeyChanged += OnDictKeyChanged;
            entry.ValueChanged += OnDictValueChanged;
            entry.Deleted += OnDictKeyDeleted;
            // Add entry if we ran out of existing ones
            _keyValueEntriesVBox.AddChild(entry);

            return entry;
        }

        private void OnDictKeyChanged(object oldKey, DictionaryValuePropertyKeyValueEntry entry)
        {
            var newKey = entry.CurrentKey;
            if (Value.Contains(newKey))
            {
                // Revert CurrentKey back
                entry.CurrentKey = oldKey;
                // Reject change since the newKey already exists
                entry.KeyProperty.SetValue(oldKey);
                return;
            }
            var currValue = Value[oldKey];
            Value.Remove(oldKey);
            Value[newKey] = currValue;
            InvokeValueChanged(Value);
        }

        private void OnDictValueChanged(object key, object newValue)
        {
            Value[key] = newValue;
            InvokeValueChanged(Value);
        }

        private void OnDictKeyDeleted(object key)
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
            var nextKey = DefaultValueUtils.GetDefault(_keyType, Value.Keys);
            Value[nextKey] = DefaultValueUtils.GetDefault(_valueType);
            InvokeValueChanged(Value);
        }

        private void OnEditToggled(bool toggled)
        {
            SetMeta("visible", toggled);
            _container.Visible = toggled;
        }
    }
}
