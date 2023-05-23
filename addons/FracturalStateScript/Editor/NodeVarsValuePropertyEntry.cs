using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public class NodeVarsValuePropertyEntry : HBoxContainer
    {
        private class ValueTypeData
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public Texture Icon { get; set; }
            public int Index { get; set; }
            public bool UseIconOnly { get; set; }
        }

        /// <summary>
        /// NameChanged(string oldName, Entry entry)
        /// </summary>
        public event Action<string, NodeVarsValuePropertyEntry> NameChanged;
        /// <summary>
        /// DataChanged(string name, NodePath newValue)
        /// </summary>
        public event Action<string, GDC.Dictionary> DataChanged;
        /// <summary>
        /// Deleted(string name)
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
                    NameProperty.Disabled = !NameEditable || value;
                    _deleteButton.Disabled = value;
                }
            }
        }
        public bool IsFixed { get; private set; }
        public bool NameEditable { get; private set; }

        public StringValueProperty NameProperty { get; set; }
        public NodeVarData Data { get; set; }

        private NodePathValueProperty _nodePathProperty;
        private OptionButton _valueTypeButton;
        private OptionButton _operationButton;
        private Button _deleteButton;
        private ValueTypeData[] _valueTypes;

        public NodeVarsValuePropertyEntry() { }
        public NodeVarsValuePropertyEntry(Node sceneRoot, Node relativeToNode, bool isFixed)
        {
            IsFixed = isFixed;

            var control = new Control();
            control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            control.SizeFlagsStretchRatio = 0.25f;
            control.RectSize = new Vector2(24, 0);
            AddChild(control);

            NameProperty = new StringValueProperty();
            NameProperty.ValueChanged += OnNameChanged;
            NameProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            NameProperty.Disabled = IsFixed;
            GD.Print("name property disabled: ", NameProperty.Disabled, " fixed? ", isFixed);

            _nodePathProperty = new NodePathValueProperty(sceneRoot, (node) =>
            {
                var nodeType = node.GetType();
                // NOTE: We do not have a way to 100% ensure the type of the NodeVar is of the type required.
                //       This is because we use nodes like InheritedNodeVar, which have no type requirements
                //       at all, and instead end up throwing a runtime error if you attempt to fetch and use the
                //       NodeVar as a different type.
                if ((Data.Operation == NodeVarOperation.Get || Data.Operation == NodeVarOperation.GetSet) && node is IGetNodeVar)
                {
                    // We have stricter type requirements if you are using ValueNodeVars
                    if (nodeType.IsSubclassOfGeneric(typeof(ValueNodeVar<>)))
                        return nodeType.IsSubclassOfGeneric(typeof(ValueNodeVar<>), Data.ValueType);
                    return true;
                }
                else if ((Data.Operation == NodeVarOperation.Set || Data.Operation == NodeVarOperation.GetSet) && node is ISetNodeVar)
                {
                    // We have stricter type requirements if you are using ValueNodeVars
                    if (nodeType.IsSubclassOfGeneric(typeof(ValueNodeVar<>)))
                        return nodeType.IsSubclassOfGeneric(typeof(ValueNodeVar<>), Data.ValueType);
                    return true;
                }
                return false;
            });
            _nodePathProperty.ValueChanged += OnNodePathChanged;
            _nodePathProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _nodePathProperty.RelativeToNode = relativeToNode;

            _valueTypeButton = new OptionButton();
            _valueTypeButton.SizeFlagsHorizontal = (int)SizeFlags.Fill;
            _valueTypeButton.ClipText = true;
            _valueTypeButton.Connect("item_selected", this, nameof(OnValueSelected));

            // TODO NOW: Move this to _Ready so icons are initialized.
            _valueTypes = new[] {
                new ValueTypeData() {
                    Name = "int",
                    Type = typeof(int),
                    Icon = GetIcon("int", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "float",
                    Type = typeof(float),
                    Icon = GetIcon("float", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "bool",
                    Type = typeof(bool),
                    Icon = GetIcon("bool", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "Vector2",
                    Type = typeof(Vector2),
                    Icon = GetIcon("Vector2", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "Vector3",
                    Type = typeof(Vector3),
                    Icon = GetIcon("Vector3", "EditorIcons"),
                    UseIconOnly = true
                }
            };
            foreach (var type in _valueTypes)
            {
                int currIndex = _valueTypeButton.GetItemCount();
                type.Index = currIndex;
                _valueTypeButton.AddIconItem(type.Icon, type.Name);
            }

            _operationButton = new OptionButton();
            _operationButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _operationButton.SizeFlagsStretchRatio = 0.6f;
            _operationButton.ClipText = true;
            _operationButton.Connect("item_selected", this, nameof(OnOperationSelected));

            var operationTypes = new[] { "Get/Set", "Get", "Set", "Private" };
            foreach (var type in operationTypes)
                _operationButton.AddItem(type);

            AddChild(NameProperty);
            AddChild(_valueTypeButton);
            AddChild(_operationButton);
            AddChild(_nodePathProperty);

            _deleteButton = new Button();
            _deleteButton.Connect("pressed", this, nameof(OnDeletePressed));
            AddChild(_deleteButton);
            _deleteButton.Visible = !IsFixed;
        }

        public override void _Ready()
        {
            base._Ready();
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
            _deleteButton.Icon = GetIcon("Remove", "EditorIcons");
        }

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete)
            {
                NameProperty.ValueChanged -= OnNameChanged;
                _nodePathProperty.ValueChanged -= OnNodePathChanged;
            }
        }

        private void SetValueTypeValue(Type type)
        {
            var valueTypeData = _valueTypes.First(x => x.Type == type);
            _valueTypeButton.Select(valueTypeData.Index);
            if (valueTypeData.UseIconOnly)
                _valueTypeButton.Text = "";
        }

        private void SetOperationsValue(NodeVarOperation operations)
        {
            if (operations == NodeVarOperation.GetSet)
                _operationButton.Select(0);
            else if (operations == NodeVarOperation.Get)
                _operationButton.Select(1);
            else if (operations == NodeVarOperation.Set)
                _operationButton.Select(2);
            else if (operations == NodeVarOperation.Private)
                _operationButton.Select(3);
        }

        public void SetData(NodeVarData value)
        {
            Data = value;
            SetValueTypeValue(value.ValueType);
            SetOperationsValue(value.Operation);
            NameProperty.SetValue(value.Name);
            _nodePathProperty.SetValue(value.Path);
        }

        private void InvokeDataChanged() => DataChanged?.Invoke(Data.Name, Data.ToGDDict());

        private void OnNameChanged(string newName)
        {
            var oldName = Data.Name;
            Data.Name = newName;
            NameChanged?.Invoke(oldName, this);
        }

        private void OnNodePathChanged(NodePath newValue)
        {
            Data.Path = newValue;
            InvokeDataChanged();
        }

        private void OnValueSelected(int index)
        {
            var item = _valueTypeButton.GetItemText(index);
            switch (item)
            {
                case "int":
                    Data.ValueType = typeof(int);
                    break;
                case "float":
                    Data.ValueType = typeof(float);
                    break;
                case "bool":
                    Data.ValueType = typeof(bool);
                    break;
                case "Vector2":
                    Data.ValueType = typeof(Vector2);
                    break;
                case "Vector3":
                    Data.ValueType = typeof(Vector3);
                    break;
                default:
                    Data.ValueType = ReflectionUtils.FindTypeFullName(item);
                    break;
            }
            SetValueTypeValue(Data.ValueType);

            InvokeDataChanged();
        }

        private void OnOperationSelected(int index)
        {
            var operation = _operationButton.GetItemText(index);
            switch (operation)
            {
                case "Get":
                    Data.Operation = NodeVarOperation.Get;
                    break;
                case "Set":
                    Data.Operation = NodeVarOperation.Set;
                    break;
                case "Get/Set":
                    Data.Operation = NodeVarOperation.GetSet;
                    break;
                case "Private":
                    Data.Operation = NodeVarOperation.Private;
                    break;
            }
            SetOperationsValue(Data.Operation);

            InvokeDataChanged();
        }

        private void OnDeletePressed() => Deleted?.Invoke(Data.Name);
    }
}
