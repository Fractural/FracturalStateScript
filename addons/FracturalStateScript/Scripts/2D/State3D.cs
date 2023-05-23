namespace Fractural.StateScript
{
    public abstract class State3D : Action3D, IState
    {
        public event System.Action Exited;
        public virtual void Reset() { }
        protected void InvokeExited() => Exited?.Invoke();
    }
}