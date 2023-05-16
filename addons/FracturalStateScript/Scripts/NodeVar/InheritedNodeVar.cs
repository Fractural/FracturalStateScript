using Fractural.Commons;
using Godot;

namespace Fractural.StateScript
{
    [RegisteredType(nameof(InheritedNodeVar))]
    [CSharpScript]
    public class InheritedNodeVar : Node, IGetNodeVar
    {
        [Export]
        public NodePath NodeVarPath;
        public object Value => _sourceGetNodeVar.Value;

        private IGetNodeVar _sourceGetNodeVar;

        public override void _Ready()
        {
            _sourceGetNodeVar = GetNode<IGetNodeVar>(NodeVarPath);
        }
    }
}