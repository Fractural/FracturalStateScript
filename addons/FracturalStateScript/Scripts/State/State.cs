using Fractural.NodeVars;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public abstract class State : ActionState, IState
    {
        private bool _isRunning;
        [NodeVar(NodeVarOperation.Get)]
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                SetNodeVar(nameof(IsRunning), value);
            }
        }

        public event System.Action Exited;
        public override void Play()
        {
            IsRunning = true;
            _Play();
        }
        public virtual void Stop()
        {
            _Stop();
            IsRunning = false;
            InvokeExited();
        }
        protected virtual void _Play() { }
        protected virtual void _Stop() { }
        protected void InvokeExited() => Exited?.Invoke();
        public virtual void StatePreProcess() { }
        public virtual void StateProcess() { }
        public virtual void StatePostProcess() { }
    }
}