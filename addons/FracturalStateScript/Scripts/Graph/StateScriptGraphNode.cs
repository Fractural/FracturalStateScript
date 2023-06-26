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
                    AddSlotLeft(outputName);
                }
            }
        }
    }
}