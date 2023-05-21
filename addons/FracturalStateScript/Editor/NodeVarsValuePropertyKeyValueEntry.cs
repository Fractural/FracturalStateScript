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
                    _nameValueProperty.Disabled = !NameEditable || value;
                    _deleteButton.Disabled = value;
                }
            }
        }
        public bool Deletable { get; private set; }
        public bool NameEditable { get; private set; }

        public string CurrentKey { get; set; }
        private StringValueProperty _nameValueProperty;
        private NodePathValueProperty _nodePathValueProperty;

        private Button _deleteButton;
        private TextureRect _typeIconRect;

        public NodeVarsValuePropertyKeyValueEntry() { }
        public NodeVarsValuePropertyKeyValueEntry(string name, Type type, NodeVarType nodeVarType, Node rootNode, bool deletable, bool nameEditable)
        {
            Deletable = deletable;
            NameEditable = nameEditable;

            var control = new Control();
            control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            control.SizeFlagsStretchRatio = 0.25f;
            control.RectSize = new Vector2(24, 0);
            AddChild(control);

            _nameValueProperty = new StringValueProperty();
            _nameValueProperty.ValueChanged += OnKeyChanged;
            _nameValueProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _nameValueProperty.Disabled = !nameEditable;

            _nodePathValueProperty = new NodePathValueProperty(rootNode, (node) =>
            {
                // NOTE: We do not have a way to 100% ensure the type of the NodeVar is of the type required.
                //       This is because we use nodes like InheritedNodeVar, which have no type requirements
                //       at all, and instead end up throwing a runtime error if you attempt to fetch and use the
                //       NodeVar as a different type.
                if ((nodeVarType & NodeVarType.Getter) != 0 && node is IGetNodeVar)
                {
                    // We have stricter type requirements if you are using ValueNodeVars
                    if (node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>)))
                        return node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>), type);
                    return true;
                }
                else if ((nodeVarType & NodeVarType.Setter) != 0 && node is ISetNodeVar)
                {
                    // We have stricter type requirements if you are using ValueNodeVars
                    if (node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>)))
                        return node.GetType().IsSubclassOfGeneric(typeof(ValueNodeVar<>), type);
                    return true;
                }
                return false;
            });
            _nodePathValueProperty.ValueChanged += OnValueChanged;
            _nodePathValueProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            AddChild(_nameValueProperty);

            _typeIconRect = new TextureRect();
            AddChild(_typeIconRect);

            var label = new Label();
            label.Text = ":  ";
            label.ClipText = true;
            AddChild(label);

            AddChild(_nodePathValueProperty);

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
                _nameValueProperty.ValueChanged -= OnKeyChanged;
                _nodePathValueProperty.ValueChanged -= OnValueChanged;
            }
        }

        public void SetKeyValue(string key, NodePath value)
        {
            _nameValueProperty.SetValue(key);
            _nodePathValueProperty.SetValue(value);
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
