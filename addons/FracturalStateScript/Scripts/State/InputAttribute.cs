using System;

namespace Fractural.StateScript
{
    /// <summary>
    /// Attribute to mark a method as an input for a state
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        public string Name { get; set; }
        public InputAttribute(string name = null)
        {
            Name = name;
        }
    }
}