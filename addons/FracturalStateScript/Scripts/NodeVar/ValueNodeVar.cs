using Fractural.DependencyInjection;
using Godot;
using System.Collections;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class ValueNodeVar<T> : Dependency, IValueNodeVar
    {
        public T InitialValue { get; private set; }
        [Export]
        public T ValueTyped { get; private set; }

        public object Value
        {
            get => ValueTyped;
            set => ValueTyped = (T)value;
        }

        public override void _Ready()
        {
            InitialValue = ValueTyped;
            Reset();
        }

        public virtual void Reset()
        {
            ValueTyped = InitialValue;
        }

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(DependencyPath),
                type: Variant.Type.NodePath,
                hint: PropertyHint.None,
                hintString: HintString.DependencyPath,
                usage: PropertyUsageFlags.Default
            );
            return builder.Build();
        }
    }
}