using Fractural.Commons;
using Godot;

namespace Fractural.StateScript
{
    [RegisteredType(nameof(FloatNodeVar))]
    [CSharpScript]
    public class FloatNodeVar : ValueNodeVar<float> { }
}