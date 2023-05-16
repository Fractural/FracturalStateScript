using Godot;

namespace Fractural.StateScript
{
    public class StateNodeConnection : Resource
    {
        [Export]
        public StateNodeData From { get; set; }
        [Export]
        public StateNodeData To { get; set; }
        [Export]
        public string FromSignal { get; set; }
        [Export]
        public string ToMethod { get; set; }
    }
}