using Fractural.Commons;
using Godot;

namespace Fractural.StateScript
{
    [RegisteredType(nameof(StringNodeVar))]
    [CSharpScript]
    public class StringNodeVar : ValueNodeVar<string> { }
}