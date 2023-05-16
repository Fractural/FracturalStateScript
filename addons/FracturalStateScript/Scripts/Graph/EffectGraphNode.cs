using Godot;

namespace Fractural.StateScript
{
    [Tool]
    public class EffectGraphNode : StateScriptGraphNode
    {
        public override void _Ready()
        {
            BorderColor = new Color("#1457ba");
            Title = "Effect";
            AddSlotLeft("In");
            AddSlotRight("Out");
            base._Ready();
        }
    }

    public class Effect
    {

    }
}