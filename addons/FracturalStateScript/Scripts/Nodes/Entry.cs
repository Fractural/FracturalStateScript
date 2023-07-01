using Godot;

namespace Fractural.StateScript
{
    [CSharpScript]
    [Tool]
    public class Entry : ActionState
    {
        [Export]
        public string EntryName { get; set; } = "";
        public override string Info => EntryName;
    }
}