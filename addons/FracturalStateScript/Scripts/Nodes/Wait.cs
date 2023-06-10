using Fractural.NodeVars;
using Godot;
using Godot.Collections;
using GodotRollbackNetcode;

namespace Fractural.StateScript
{
    [Tool]
    public class Wait : State, INetworkProcess
    {
        [NodeVar]
        public int DurationTicks { get => GetDictNodeVar<int>(nameof(DurationTicks)); }

        private int _currentTicks;
        [NodeVar(NodeVarOperation.Get)]
        public int CurrentTicks
        {
            get => _currentTicks;
            private set
            {
                _currentTicks = value;
                SetDictNodeVar(nameof(CurrentTicks), value);
            }
        }

        private bool _isRunning;
        [NodeVar(NodeVarOperation.Get)]
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                SetDictNodeVar(nameof(IsRunning), value);
            }
        }

        public override void Play()
        {
            IsRunning = true;
            CurrentTicks = DurationTicks;
        }

        public void _NetworkProcess(Dictionary input)
        {
            if (IsRunning)
            {
                if (_currentTicks > DurationTicks)
                    _currentTicks--;
                else
                {
                    InvokeExited();
                    IsRunning = false;
                }
            }
        }
    }
}