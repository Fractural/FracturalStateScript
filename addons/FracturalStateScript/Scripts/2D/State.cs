namespace Fractural.StateScript
{
    public abstract class State : Action, IState
    {
        public event System.Action Exited;
        public virtual void Reset() { }
        protected void InvokeExited() => Exited?.Invoke();
    }
}