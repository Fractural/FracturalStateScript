using Fractural.Utils;
using Godot;
using System;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public class NodeVarData
    {
        public Type ValueType { get; set; }
        public NodeVarOperation Operation { get; set; }
        public string Name { get; set; }
        public NodePath Path { get; set; }

        public GDC.Dictionary ToGDDict()
        {
            return new GDC.Dictionary()
            {
                { nameof(ValueType), ValueType.FullName },
                { nameof(Operation), (int)Operation },
                { nameof(Path), Path },
            };
        }

        public static NodeVarData FromGDDict(GDC.Dictionary dict, string name)
        {
            return new NodeVarData()
            {
                ValueType = ReflectionUtils.FindTypeFullName(dict.Get<string>(nameof(ValueType))),
                Operation = (NodeVarOperation)dict.Get<int>(nameof(Operation)),
                Path = dict.Get<NodePath>(nameof(Path)),
                Name = name
            };
        }
    }
}
