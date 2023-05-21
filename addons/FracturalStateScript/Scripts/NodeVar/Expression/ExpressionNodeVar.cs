using Fractural.DependencyInjection;
using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    /// <summary>
    /// Gettable NodeVar that get's its value from an expression. 
    /// This expression can include other NodeVars and basic
    /// operations like equality, logic, etc.
    /// 
    /// This expression is compiled on ready and then evaluated
    /// everytime Value.Get is called.
    /// 
    /// The expression can be recompiled by 
    /// calling <seealso cref="RecompileExpression(string, IDictionary{string, IGetNodeVar})"/>.
    /// </summary>
    [Tool]
    public class ExpressionNodeVar : Dependency, IGetNodeVar
    {
        [Export]
        public string Expression { get; private set; }
        /// <summary>
        /// Mapping of Names to NodeVars
        /// </summary>
        public GDC.Dictionary NodeVars { get; private set; }
        public ExpressionParser.Expression AST { get; private set; }
        public object Value => AST?.Evaluate();

        private IDictionary<string, IGetNodeVar> _runtimeNodeVars = new Dictionary<string, IGetNodeVar>();

        public override void _Ready()
        {
            foreach (string key in NodeVars.Keys)
            {
                var getNodeVar = NodeVars.Get<IGetNodeVar>(key);
                if (getNodeVar != null)
                    _runtimeNodeVars.Add(key, getNodeVar);
            }

            RecompileExpression();
        }

        public void RecompileExpression(string newExpression = null, IDictionary<string, IGetNodeVar> newRuntimeNodeVars = null)
        {
            if (newExpression != null)
                Expression = newExpression;
            if (newRuntimeNodeVars != null)
                _runtimeNodeVars = newRuntimeNodeVars;
            AST = ExpressionUtils.ParseFromText(Expression, GetVariable);
        }

        public object GetVariable(string name) => _runtimeNodeVars[name].Value;

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(NodeVars),
                type: Variant.Type.Dictionary,
                hint: PropertyHint.None,
                hintString: HintString.GetTypedDictionary<string, NodePath>()
            );
            return builder.Build();
        }
    }
}