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
                SetNodeVar(nameof(CurrentTicks), value);
            }
        }

        protected override void _Play()
        {
            CurrentTicks = DurationTicks;
        }

        public void _NetworkProcess(Dictionary input)
        {
            if (IsRunning)
            {
                if (_currentTicks > DurationTicks)
                    _currentTicks--;
                else
                    Stop();
            }
        }
    }
}