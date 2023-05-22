using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;

namespace Fractural.StateScript
{
    public class NodeVarsValuePropertyKeyValueEntry : HBoxContainer
    {

        /// <summary>
        /// KeyChanged(string key, Entry entry)
        /// </summary>
        public event Action<string, NodeVarsValuePropertyKeyValueEntry> KeyChanged;
        /// <summary>
        /// ValueChanged(string key, NodePath newValue)
        /// </summary>
        public event Action<string, NodePath> ValueChanged;
        /// <summary>
        /// Deleted(string key)
        /// </summary>
        public event Action<string> Deleted;

        private bool _disabled;
        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                if (IsInsideTree())
                {
                    KeyProperty.Disabled = !NameEditable || value;
                    _deleteButton.Disabled = value;
                }
            }
        }
        public bool Deletable { get; private set; }
        public bool NameEditable { get; private set; }

        public string CurrentKey { get; set; }
        public StringValueProperty KeyProperty { get; set; }
        public NodePathValueProperty ValueProperty { get; set; }

        private Button _deleteButton;
        private TextureRect _typeIconRect;
        private NodeVarData _data;

        public NodeVarsValuePropertyKeyValueEntry() { }
        public NodeVarsValuePropertyKeyValueEntry(NodeVarData data, Node rootNode, bool deletable, bool nameEditable)
        {
            _data = data;

            Deletable = deletable;
            NameEditable = nameEditable;

            var control = new Control();
            control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            control.SizeFlagsStretchRatio = 0.25f;
            control.RectSize = new Vector2(24, 0);
            AddChild(control);

            KeyProperty = new StringValueProperty();
            KeyProperty.ValueChanged += OnKeyChanged;
            KeyProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            KeyProperty.Disabled = !nameEditable;

            ValueProperty = new NodePathValueProperty(rootNode, (node) =>
            {
                // NOTE: We do not have a way to 100% ensure the type of the NodeVar is of the type required.
                //       This is because we use nodes like InheritedNodeVar, which have no type requirements
                //       at all, and instead end up throwing a runtime error if you attempt to fetch and use the
                //       NodeVar as a different type.
                if ((data.Operations & NodeVarOperations.Getter) != 0 && node is IGetNodeVar)
                {
                    // We have stricter type requirements if you are using ValueNodeVars
                    if (node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>)))
                        return node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>), data.ValueType);
                    return true;
                }
                else if ((data.Operations & NodeVarOperations.Setter) != 0 && node is ISetNodeVar)
                {
                    // We have stricter type requirements if you are using ValueNodeVars
                    if (node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>)))
                        return node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>), data.ValueType);
                    return true;
                }
                return false;
            });
            ValueProperty.ValueChanged += OnValueChanged;
            ValueProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            AddChild(KeyProperty);

            _typeIconRect = new TextureRect();
            AddChild(_typeIconRect);

            var label = new Label();
            label.Text = ":  ";
            label.ClipText = true;
            AddChild(label);

            AddChild(ValueProperty);

            _deleteButton = new Button();
            _deleteButton.Connect("pressed", this, nameof(OnDeletePressed));
            AddChild(_deleteButton);
            _deleteButton.Visible = deletable;
        }

        public override void _Ready()
        {
            base._Ready();
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
            _deleteButton.Icon = GetIcon("Remove", "EditorIcons");

            // TODO: Add type based icon fetching
            _typeIconRect.Texture = GetIcon("int", "EditorIcons");
        }

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete)
            {
                KeyProperty.ValueChanged -= OnKeyChanged;
                ValueProperty.ValueChanged -= OnValueChanged;
            }
        }

        public void SetKeyValue(string key, NodePath value)
        {
            KeyProperty.SetValue(key);
            ValueProperty.SetValue(value);
            CurrentKey = key;
        }

        private void OnKeyChanged(string newKey)
        {
            var oldKey = CurrentKey;
            CurrentKey = newKey;
            KeyChanged?.Invoke(oldKey, this);
        }

        private void OnValueChanged(NodePath newValue) => ValueChanged?.Invoke(CurrentKey, newValue);
        private void OnDeletePressed() => Deleted?.Invoke(CurrentKey);
    }
}
