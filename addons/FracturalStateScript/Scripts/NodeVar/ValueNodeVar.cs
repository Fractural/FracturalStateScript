using Fractural.DependencyInjection;
using Godot;
using System.Collections;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class ValueNodeVar<T> : Node, IValueNodeVar
    {
        public T InitialValue { get; private set; }
        [Export]
        public T Value { get; private set; }

        object ISetNodeVar.Value { set => Value = (T)value; }
        object IGetNodeVar.Value => Value;

        public override void _Ready()
        {
            InitialValue = Value;
            Reset();
        }

        public virtual void Reset()
        {
            Value = InitialValue;
        }
    }
}