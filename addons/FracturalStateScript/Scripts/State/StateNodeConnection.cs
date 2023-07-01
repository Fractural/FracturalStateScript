using Fractural.Utils;
using Godot;
using System.Collections;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public class StateNodeConnection
    {
        public IAction ToState { get; set; }
        public string FromEvent { get; set; }
        public string ToMethod { get; set; }

        public GDC.Dictionary ToGDDict()
        {
            return new GDC.Dictionary()
            {
                { nameof(ToState), (ToState as Node).Name },
                { nameof(FromEvent), FromEvent },
                { nameof(ToMethod), ToMethod },
            };
        }

        public bool FromGDDict(GDC.Dictionary dictionary, Node stateGraphNode)
        {
            ToState = stateGraphNode.GetNodeOrNull<IAction>(dictionary.Get<string>(nameof(ToState)));
            if (ToState == null) return false;
            FromEvent = dictionary.Get<string>(nameof(FromEvent));
            ToMethod = dictionary.Get<string>(nameof(ToMethod));
            return true;
        }

        public override string ToString()
        {
            return $"[{(ToState as Node)?.Name}]: {FromEvent} -> {ToMethod}";
        }
    }
}