namespace Fractural.StateScript
{
    public abstract class State2D : Action2D, IState
    {
        public event System.Action Exited;
        public virtual void Reset() { }
        protected void InvokeExited() => Exited?.Invoke();
    }
}