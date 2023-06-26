using Fractural.NodeVars;

namespace Fractural.StateScript
{
    public abstract class State3D : ActionState3D, IState
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

        [Output]
        public event System.Action Begin;
        [Output]
        public event System.Action Aborted;

        public override void Play()
        {
            IsRunning = true;
            _Play();
            InvokeBegin();
        }

        [Input]
        public virtual void Stop()
        {
            _Stop();
            bool aborted = IsRunning;
            IsRunning = false;
            if (aborted)
                InvokeAborted();
            else
                InvokeExited();
        }

        protected virtual void _Stop() { }
        protected void InvokeBegin() => Begin?.Invoke();
        protected void InvokeAborted() => Aborted?.Invoke();
        public virtual void StatePreProcess() { }
        public virtual void StateProcess() { }
        public virtual void StatePostProcess() { }
    }
}