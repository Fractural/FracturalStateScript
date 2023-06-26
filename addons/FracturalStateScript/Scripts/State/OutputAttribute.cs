using System;

namespace Fractural.StateScript
{
    /// <summary>
    /// Attribute to mark an event as an output for a state
    /// </summary>
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        public string Name { get; set; }
        public OutputAttribute(string name = null)
        {
            Name = name;
        }
    }
}