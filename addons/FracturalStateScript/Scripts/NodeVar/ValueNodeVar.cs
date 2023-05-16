using Godot;

namespace Fractural.StateScript
{
    public abstract class ValueNodeVar<T> : Node, IValueNodeVar
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
    }
}