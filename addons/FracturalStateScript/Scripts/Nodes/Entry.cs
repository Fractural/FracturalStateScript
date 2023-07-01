using Godot;

namespace Fractural.StateScript
{
    [CSharpScript]
    [Tool]
    public class Entry : ActionState
    {
        private string _entryName = "";
        [Export]
        public string EntryName
        {
            get => _entryName;
            set
            {
                _entryName = value;
                InvokeInfoChanged();
            }
        }
        public override string Info => EntryName;
    }
}