using Godot;
using Godot.Collections;
using GodotRollbackNetcode;

namespace Fractural.StateScript
{
    public class Wait : Node, IStateScriptNode, INetworkProcess
    {
        public string NodeType => "Wait";
        public event System.Action Exited;

        [Export]
        public int DurationTicks { get; set; } = 5;
        [Export]
        public int CurrentTicks { get; private set; } = 0;
        [Export]
        public bool IsRunning { get; private set; }

        public void OnEnter()
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
                    Exited?.Invoke();
                    IsRunning = false;
                }
            }
        }
    }
}