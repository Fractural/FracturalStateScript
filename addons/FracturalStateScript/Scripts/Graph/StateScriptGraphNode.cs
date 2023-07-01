using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fractural.StateScript
{
    [Tool]
    public class StateScriptGraphNode : CustomizedGraphNode
    {
        private IAction _state;
        public IAction State
        {
            get => _state;
            set
            {
                UpdateState(value);
            }
        }

        public IDictionary<string, string> InputAliasLookupDict { get; private set; }
        public IDictionary<string, string> OutputAliasLookupDict { get; private set; }

        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationPredelete)
            {
                if (State != null)
                {
                    State.CommentChanged -= OnCommentChanged;
                    State.InfoChanged -= OnInfoChanged;
                }
            }
        }

        public string GetInputPortMethod(int index)
        {
            return InputAliasLookupDict[LeftSlotNames[index]];
        }

        public int GetInputPortFromMethod(string methodName)
        {
            for (int i = 0; i < LeftSlotNames.Count; i++)
                if (InputAliasLookupDict[LeftSlotNames[i]] == methodName)
                    return i;
            return -1;
        }

        public string GetOutputPortEvent(int index)
        {
            return OutputAliasLookupDict[RightSlotNames[index]];
        }

        public int GetOutputPortFromEvent(string eventName)
        {
            for (int i = 0; i < RightSlotNames.Count; i++)
                if (OutputAliasLookupDict[RightSlotNames[i]] == eventName)
                    return i;
            return -1;
        }

        public virtual void UpdateState(IAction newState)
        {
            _state = newState;
            _state.InfoChanged += OnInfoChanged;
            _state.CommentChanged += OnCommentChanged;
            ClearAllSlots();
            InputAliasLookupDict = new Dictionary<string, string>();
            OutputAliasLookupDict = new Dictionary<string, string>();
            foreach (var method in newState.GetType().GetMethods())
            {
                var inputAttribute = method.GetCustomAttributes(typeof(InputAttribute), true).FirstOrDefault() as InputAttribute;
                if (inputAttribute != null)
                {
                    var inputName = inputAttribute.Name != null ? inputAttribute.Name : method.Name;
                    if (InputAliasLookupDict.ContainsKey(inputName)) continue;
                    InputAliasLookupDict[inputName] = method.Name;
                    AddSlotLeft(inputName);
                }
            }
            foreach (var @event in newState.GetType().GetEvents())
            {
                var outputAttribute = @event.GetCustomAttributes(typeof(OutputAttribute), true).FirstOrDefault() as OutputAttribute;
                if (outputAttribute != null)
                {
                    var outputName = outputAttribute.Name != null ? outputAttribute.Name : @event.Name;
                    if (OutputAliasLookupDict.ContainsKey(outputName)) continue;
                    OutputAliasLookupDict[outputName] = @event.Name;
                    AddSlotRight(outputName);
                }
            }
            OnInfoChanged();
            OnCommentChanged();
        }

        private void OnInfoChanged()
        {
            InfoText = State.Info;
        }

        private void OnCommentChanged()
        {
            GD.Print("Comment changed to ", State.Info);
            CommentText = State.Comment;
        }
    }
}