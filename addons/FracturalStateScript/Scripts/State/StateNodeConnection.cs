using Fractural.Utils;
using Godot;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public class StateNodeConnection
    {
        public IAction ToState { get; set; }
        public string FromEvent { get; set; }
        public string ToMethod { get; set; }

        public GDC.Dictionary ToGDDict(Node relativeToNode)
        {
            return new GDC.Dictionary()
            {
                { nameof(ToState), relativeToNode.GetPathTo(ToState as Node) },
                { nameof(FromEvent), FromEvent },
                { nameof(ToMethod), ToMethod },
            };
        }

        public void FromGDDict(GDC.Dictionary dictionary, Node relativeToNode)
        {
            ToState = relativeToNode.GetNode<IAction>(dictionary.Get<NodePath>(nameof(ToState)));
            FromEvent = dictionary.Get<string>(nameof(FromEvent));
            ToMethod = dictionary.Get<string>(nameof(ToMethod));
        }

        public override string ToString()
        {
            return $"[{(ToState as Node)?.Name}]: {FromEvent} -> {ToMethod}";
        }
    }
}