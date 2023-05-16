using Fractural.Commons;
using Godot;

namespace Fractural.StateScript
{
    [RegisteredType(nameof(BoolNodeVar))]
    [CSharpScript]
    public class BoolNodeVar : ValueNodeVar<bool> { }
}