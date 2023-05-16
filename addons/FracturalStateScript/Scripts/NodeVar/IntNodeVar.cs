using Fractural.Commons;
using Godot;

namespace Fractural.StateScript
{
    [RegisteredType(nameof(IntNodeVar))]
    [CSharpScript]
    public class IntNodeVar : ValueNodeVar<int> { }
}