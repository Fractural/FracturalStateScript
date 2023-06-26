using Fractural.Utils;
using Godot;
using System.Collections.Generic;

namespace Fractural.StateScript
{
    [Tool]
    public class CustomizedGraphNode : GraphNode
    {
        public const int LabelVBoxOffset = 4;
        public Color BackgroundColor = new Color("#2e2a2a");
        public Color BorderColor = new Color("#4e9c2d");
        public Color HeaderColor = Colors.White;

        private List<string> _leftSlotNames = new List<string>();
        public IReadOnlyList<string> LeftSlotNames => _leftSlotNames;
        private List<string> _rightSlotNames = new List<string>();
        public IReadOnlyList<string> RightSlotNames => _rightSlotNames;

        private Label _commentLabel;
        private string _commentText = "";
        [Export(PropertyHint.MultilineText)]
        private string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
                if (_commentLabel != null)
                    _commentLabel.Text = value;
            }
        }

        private Label _infoLabel;
        private string _infoText = "";
        [Export(PropertyHint.MultilineText)]
        private string InfoText
        {
            get => _infoText;
            set
            {
                _infoText = value;
                if (_infoLabel != null)
                    _infoLabel.Text = value;
            }
        }

        private VBoxContainer _labelVBox;

        public override void _Ready()
        {
            ShowClose = true;
            Resizable = false;
            RectMinSize = new Vector2(100, 50);

            InitLabels();
            UpdateStyleboxesAndConstants();
            UpdateSlots();
        }

        public override void _Process(float delta)
        {
            if (_labelVBox.RectSize != _labelVBox.RectMinSize)
                _labelVBox.RectSize = _labelVBox.RectMinSize;
            var parentGraphEdit = GetParent<GraphEdit>();
            if (parentGraphEdit != null)
            {
                float zoom = GetParent<GraphEdit>().Zoom;
                if (_labelVBox.RectScale.x != zoom)
                    _labelVBox.RectScale = Vector2.One * zoom;
            }
            _labelVBox.RectGlobalPosition = RectGlobalPosition - new Vector2(0, _labelVBox.RectSize.y * _labelVBox.RectScale.y + LabelVBoxOffset);
            _commentLabel.Visible = _commentLabel.Text != "";
            _infoLabel.Visible = _infoLabel.Text != "";
        }

        public override void _EnterTree()
        {
            // NOTE: When deleting nodes in the editor, the editor
            //       just removes the deleted node from the scene, but
            //       stil keeps it in memory in-case we need to redo.
            //       Therefore we need to use _EnterTree and _ExitTree
            //       calls to handle moving the _labelVBox.

            // _EnterTree is called before _Ready
            // We're only interested in the node reentering back into the tree
            // after the user redos a delete in the editor.
            if (_labelVBox != null)
            {
                _labelVBox.Reparent(GetParent());
            }
        }

        public override void _ExitTree()
        {
            _labelVBox.Reparent(this);
        }

        public void InitLabels()
        {
            _labelVBox = new VBoxContainer();
            _labelVBox.GrowVertical = GrowDirection.Begin;
            _labelVBox.SizeFlagsVertical = (int)SizeFlags.ShrinkEnd;
            _labelVBox.MouseFilter = MouseFilterEnum.Ignore;

            _commentLabel = new Label();
            _commentLabel.GrowVertical = GrowDirection.Begin;
            _commentLabel.SizeFlagsVertical = (int)SizeFlags.ShrinkEnd;
            _commentLabel.Text = CommentText;
            _commentLabel.MouseFilter = MouseFilterEnum.Ignore;

            _infoLabel = new Label();
            _infoLabel.GrowVertical = GrowDirection.Begin;
            _infoLabel.SizeFlagsVertical = (int)SizeFlags.ShrinkEnd;
            _infoLabel.Text = InfoText;
            _infoLabel.MouseFilter = MouseFilterEnum.Ignore;

            var commentColor = Colors.White;
            var infoColor = BorderColor.Lightened(0.4f);

            _commentLabel.AddColorOverride("font_color", commentColor);
            _infoLabel.AddColorOverride("font_color", infoColor);

            _labelVBox.AddChild(_commentLabel);
            _labelVBox.AddChild(_infoLabel);

            GetParent().CallDeferred("add_child", _labelVBox);
        }

        public void UpdateStyleboxesAndConstants()
        {
            StyleBoxFlat defaultStyleBox = new StyleBoxFlat();
            defaultStyleBox.BgColor = BackgroundColor;

            int portMargin = 8;
            int contentMargin = 8;
            int headerHeight = 24;
            int borderWidth = 2;
            int cornerRadius = 4;
            int separation = 8;

            defaultStyleBox.ContentMarginLeft = borderWidth + contentMargin + portMargin;
            defaultStyleBox.ContentMarginRight = borderWidth + contentMargin + portMargin;
            defaultStyleBox.ContentMarginTop = headerHeight + borderWidth + contentMargin;
            defaultStyleBox.ContentMarginBottom = borderWidth + contentMargin;
            defaultStyleBox.BorderWidthTop = headerHeight;
            defaultStyleBox.BorderWidthLeft = borderWidth;
            defaultStyleBox.BorderWidthRight = borderWidth;
            defaultStyleBox.BorderWidthBottom = borderWidth;
            defaultStyleBox.BorderColor = BorderColor;
            defaultStyleBox.CornerRadiusBottomLeft = cornerRadius;
            defaultStyleBox.CornerRadiusTopLeft = cornerRadius;
            defaultStyleBox.CornerRadiusBottomRight = cornerRadius;
            defaultStyleBox.CornerRadiusTopRight = cornerRadius;
            defaultStyleBox.ShadowColor = new Color(Colors.Black, 0.5f);
            defaultStyleBox.ShadowSize = 8;
            defaultStyleBox.ShadowOffset = new Vector2(0, 4);
            StyleBoxFlat selectedStyleBox = defaultStyleBox.Duplicate() as StyleBoxFlat;
            selectedStyleBox.BorderColor = new Color(BorderColor, 0.5f);
            RemoveStyleboxOverride("frame");
            RemoveStyleboxOverride("selectedframe");
            RemoveColorOverride("title_color");
            RemoveColorOverride("close_color");
            RemoveColorOverride("resizer_color");

            AddStyleboxOverride("frame", defaultStyleBox);
            AddStyleboxOverride("selectedframe", selectedStyleBox);
            AddColorOverride("title_color", HeaderColor);
            AddColorOverride("close_color", HeaderColor);
            AddColorOverride("resizer_color", HeaderColor);
            AddConstantOverride("separation", separation);
            AddConstantOverride("port_offset", 1);
            AddConstantOverride("close_h_offset", contentMargin);
            AddConstantOverride("title_h_offset", -contentMargin);
        }

        public void UpdateSlots()
        {
            int index = 0;
            int maxCount = LeftSlotNames.Count;
            if (RightSlotNames.Count > maxCount)
                maxCount = RightSlotNames.Count;
            int childCount = GetChildCount();
            if (childCount < maxCount)
            {
                int amount = maxCount - childCount;
                for (int i = 0; i < amount; i++)
                    AddSlot();
            }
            foreach (Node child in GetChildren())
            {
                if (!(child is HSplitContainer))
                    return;
                var leftLabel = child.GetChild(0) as Label;
                if (leftLabel == null)
                    return;
                var rightLabel = child.GetChild(1) as Label;
                if (rightLabel == null)
                    return;
                if (leftLabel.Text != GetSlotNameLeft(index))
                    leftLabel.Text = GetSlotNameLeft(index);
                if (rightLabel.Text != GetSlotNameRight(index))
                    rightLabel.Text = GetSlotNameRight(index);
                if (index >= maxCount)
                    child.QueueFree();
                index++;
            }
        }

        public string GetSlotNameLeft(int index)
        {
            if (index >= LeftSlotNames.Count)
                return "";
            return LeftSlotNames[index];
        }

        public string GetSlotNameRight(int index)
        {
            if (index >= RightSlotNames.Count)
                return "";
            return RightSlotNames[index];
        }

        public void SetSlotNameLeft(int index, string text)
        {
            if (index >= LeftSlotNames.Count)
                return;
            _leftSlotNames[index] = text;
            GetChild(index).GetChild<Label>(0).Text = text;
        }

        public void SetSlotNameRight(int index, string text)
        {
            if (index >= RightSlotNames.Count)
                return;
            _rightSlotNames[index] = text;
            GetChild(index).GetChild<Label>(1).Text = text;
        }

        public void AddSlotLeft(string name = "", Color color = default, int type = 0)
        {
            AddSlotLeft(LeftSlotNames.Count, name, color, type);
        }

        public void AddSlotLeft(int index, string name = "", Color color = default, int type = 0)
        {
            if (color.a == 0)
                color = Colors.White;
            _leftSlotNames.Add(name);
            UpdateSlots();
            SetSlotEnabledLeft(index, true);
            SetSlotColorLeft(index, color);
            SetSlotTypeLeft(index, type);
        }

        public void RemoveSlotLeft(int index)
        {
            if (index >= _leftSlotNames.Count)
                return;
            _leftSlotNames.RemoveAt(index);
            UpdateSlots();
        }

        public void AddSlotRight(string name = "", Color color = default, int type = 0)
        {
            AddSlotRight(RightSlotNames.Count, name, color, type);
        }

        public void AddSlotRight(int index, string name = "", Color color = default, int type = 0)
        {
            if (color.a == 0)
                color = Colors.White;
            _rightSlotNames.Add(name);
            UpdateSlots();
            SetSlotEnabledRight(index, true);
            SetSlotColorRight(index, color);
            SetSlotTypeRight(index, type);
        }

        public void RemoveSlotRight(int index)
        {
            if (index >= _rightSlotNames.Count)
                return;
            _rightSlotNames.RemoveAt(index);
            UpdateSlots();
        }

        private Node AddSlot(int index = -1)
        {
            var hsplit = new HSplitContainer();
            var leftLabel = new Label();
            var rightLabel = new Label();
            leftLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            leftLabel.MouseFilter = MouseFilterEnum.Ignore;
            rightLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            rightLabel.Align = Label.AlignEnum.Right;
            rightLabel.MouseFilter = MouseFilterEnum.Ignore;
            hsplit.DraggerVisibility = SplitContainer.DraggerVisibilityEnum.Hidden;
            hsplit.MouseFilter = MouseFilterEnum.Ignore;
            hsplit.AddChild(leftLabel);
            hsplit.AddChild(rightLabel);
            AddChild(hsplit);
            if (index >= 0)
                MoveChild(hsplit, index);
            return hsplit;
        }
    }
}