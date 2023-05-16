using Fractural.DependencyInjection;
using Godot;
using Godot.Collections;

namespace Fractural.StateScript
{
    [Tool]
    public abstract class ValueNodeVar<T> : Dependency, IValueNodeVar
    {
        [Export]
        public T InitialValueTyped { get; set; }
        [Export]
        public T ValueTyped { get; private set; }

        public object Value
        {
            get => ValueTyped;
            set => ValueTyped = (T)value;
        }

        public override void _Ready()
        {
            Reset();
        }

        public virtual void Reset()
        {
            InitialValueTyped = ValueTyped;
        }

        public override Array _GetPropertyList()
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