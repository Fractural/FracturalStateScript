﻿using Fractural.DependencyInjection;
using Godot;
using System;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public class NameToNodeVar : Resource
    {
        [Export]
        public string
    }
    [Tool]
    public class ExpressionNodeVar : Dependency, IGetNodeVar
    {
        [Export]
        public string Expression { get; set; }
        /// <summary>
        /// Mapping of Names to NodeVars
        /// </summary>
        public GDC.Dictionary NodeVars { get; set; }

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(NodeVars),
                type: Variant.Type.Dictionary,
                hint: PropertyHint.None,
                hintString: HintString., // TODO: Finish this after better Dictionary support is implemented in FracturalCommons
                usage: PropertyUsageFlags.Default
            );
            return builder.Build();
        }
    }
    [Tool]
    public abstract class ValueNodeVar<T> : Dependency, IValueNodeVar
    {
        public T InitialValue { get; private set; }
        [Export]
        public T ValueTyped { get; private set; }

        public object Value
        {
            get => ValueTyped;
            set => ValueTyped = (T)value;
        }

        public override void _Ready()
        {
            InitialValue = ValueTyped;
            Reset();
        }

        public virtual void Reset()
        {
            ValueTyped = InitialValue;
        }

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(DependencyPath),
                type: Variant.Type.NodePath,
                hint: PropertyHint.None,
                hintString: HintString.DependencyPath,
                usage: PropertyUsageFlags.Default
            );
            return builder.Build();
        }
    }
}