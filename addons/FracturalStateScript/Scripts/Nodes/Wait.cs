using Godot;
using Godot.Collections;
using GodotRollbackNetcode;

namespace Fractural.StateScript
{
    public class Wait : State, INetworkProcess
    {
        [Export]
        public int DurationTicks { get; set; } = 5;
        [Export]
        public int CurrentTicks { get; private set; } = 0;
        [Export]
        public bool IsRunning { get; private set; }

        public override void Play()
        {
            IsRunning = true;
            CurrentTicks = DurationTicks;
        }

        public void _NetworkProcess(Dictionary input)
        {
            if (IsRunning)
            {
                if (CurrentTicks > DurationTicks)
                    CurrentTicks--;
                else
                {
                    InvokeExited();
                    IsRunning = false;
                }
            }
        }
    }
}