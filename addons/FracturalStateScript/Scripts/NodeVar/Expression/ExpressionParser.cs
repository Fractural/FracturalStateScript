using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fractural.StateScript
{
    /// <summary>
    /// Simple expression parser that does arithmetic, boolean, and equality operations. 
    /// Uses int, float, bool, and string types.
    /// </summary>
    public class ExpressionParser
    {
        #region AST Nodes
        public abstract class Expression
        {
            public abstract object Evaluate();
        }

        public class Variable : Expression
        {
            public delegate object FetchVariableDelegate(string name);
            public FetchVariableDelegate FetchVariable { get; set; }
            public string Name { get; set; }
            public override object Evaluate()
            {
                if (FetchVariable == null)
                {
                    GD.PushError($"{nameof(Variable)}: Expected FetchVariable to be assigned before evaluating the variable in an expression AST!");
                    return null;
                }
                return FetchVariable(Name);
            }
        }

        public class FunctionCall : Expression
        {
            public delegate object CallFunctionDelegate(string name, object[] args);
            public CallFunctionDelegate CallFunction { get; set; }
            public string Name { get; set; }
            public Expression[] Args { get; set; }
            public override object Evaluate()
            {
                if (CallFunction == null)
                {
                    GD.PushError($"{nameof(FunctionCall)}: Expected CallFunction to be assigned before running a function call in an expression AST!");
                    return null;
                }
                var evalutedArgs = Args.Select(x => x.Evaluate()).ToArray();
                return CallFunction(Name, evalutedArgs);
            }
        }

        public class Literal : Expression
        {
            public object Value { get; set; }
            public override object Evaluate() => Value;
        }

        public abstract class PreUnaryOperator : Expression
        {
            public Expression Operand { get; set; }

            public override object Evaluate()
            {
                var operand = Operand.Evaluate();
                var result = Evaluate(operand);
                if (result == null)
                    GD.PushError($"{GetType().Name}: Could not evaluate with operand of {operand.GetType().Name}.");
                return result;
            }
            protected abstract object Evaluate(object operand);
        }

        public abstract class BinaryOperator : Expression
        {
            public Expression LeftOperand { get; set; }
            public Expression RightOperand { get; set; }

            public override object Evaluate()
            {
                var leftOperand = LeftOperand.Evaluate();
                var rightOperand = RightOperand.Evaluate();
                var result = Evaluate(leftOperand, rightOperand);
                if (result == null)
                    GD.PushError($"{GetType().Name}: Could not evaluate with operands of {leftOperand.GetType().Name} and {rightOperand.GetType().Name}.");
                return result;
            }
            protected abstract object Evaluate(object leftOperand, object rightOperand);
        }

        public class NegativeOperator : PreUnaryOperator
        {
            protected override object Evaluate(object operand)
            {
                if (operand is float)
                    return -(float)operand;
                if (operand is int)
                    return -(int)operand;
                return null;
            }
        }

        public class AddOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is float && rightOperand is float)
                    return (float)leftOperand + (float)rightOperand;
                if (leftOperand is float && rightOperand is int)
                    return (float)leftOperand + (int)rightOperand;

                if (leftOperand is int && rightOperand is int)
                    return (int)leftOperand + (int)rightOperand;
                if (leftOperand is int && rightOperand is float)
                    return (int)leftOperand + (float)rightOperand;

                if (leftOperand is string && rightOperand is string)
                    return (string)leftOperand + (string)rightOperand;

                return null;
            }
        }

        public class SubtractOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is float && rightOperand is float)
                    return (float)leftOperand - (float)rightOperand;
                if (leftOperand is float && rightOperand is int)
                    return (float)leftOperand - (int)rightOperand;

                if (leftOperand is int && rightOperand is int)
                    return (int)leftOperand - (int)rightOperand;
                if (leftOperand is int && rightOperand is float)
                    return (int)leftOperand - (float)rightOperand;

                return null;
            }
        }

        public class DivideOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is float && rightOperand is float)
                    return (float)leftOperand / (float)rightOperand;
                if (leftOperand is float && rightOperand is int)
                    return (float)leftOperand / (int)rightOperand;

                if (leftOperand is int && rightOperand is int)
                    return (int)leftOperand / (int)rightOperand;
                if (leftOperand is int && rightOperand is float)
                    return (int)leftOperand / (float)rightOperand;

                return null;
            }
        }

        public class MultiplyOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is float && rightOperand is float)
                    return (float)leftOperand * (float)rightOperand;
                if (leftOperand is float && rightOperand is int)
                    return (float)leftOperand * (int)rightOperand;

                if (leftOperand is int && rightOperand is int)
                    return (int)leftOperand * (int)rightOperand;
                if (leftOperand is int && rightOperand is float)
                    return (int)leftOperand * (float)rightOperand;

                return null;
            }
        }

        public class NegationOperator : PreUnaryOperator
        {
            protected override object Evaluate(object operand)
            {
                if (operand is bool)
                    return !(bool)operand;
                return null;
            }
        }

        public class OrOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is bool && rightOperand is bool)
                    return (bool)leftOperand || (bool)rightOperand;
                return null;
            }
        }

        public class AndOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is bool && rightOperand is bool)
                    return (bool)leftOperand && (bool)rightOperand;
                return null;
            }
        }

        public class EqualsOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                return leftOperand == rightOperand;
            }
        }

        public class GreaterThanOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is float && rightOperand is float)
                    return (float)leftOperand > (float)rightOperand;
                if (leftOperand is float && rightOperand is int)
                    return (float)leftOperand > (int)rightOperand;

                if (leftOperand is int && rightOperand is int)
                    return (int)leftOperand > (int)rightOperand;
                if (leftOperand is int && rightOperand is float)
                    return (int)leftOperand > (float)rightOperand;

                return null;
            }
        }

        public class LessThanOperator : BinaryOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand is float && rightOperand is float)
                    return (float)leftOperand < (float)rightOperand;
                if (leftOperand is float && rightOperand is int)
                    return (float)leftOperand < (int)rightOperand;

                if (leftOperand is int && rightOperand is int)
                    return (int)leftOperand < (int)rightOperand;
                if (leftOperand is int && rightOperand is float)
                    return (int)leftOperand < (float)rightOperand;

                return null;
            }
        }

        public class GreaterThanEqualsOperator : GreaterThanOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand == rightOperand)
                    return true;
                return base.Evaluate(leftOperand, rightOperand);
            }
        }

        public class LessThanEqualsOperator : LessThanOperator
        {
            protected override object Evaluate(object leftOperand, object rightOperand)
            {
                if (leftOperand == rightOperand)
                    return true;
                return base.Evaluate(leftOperand, rightOperand);
            }
        }
        #endregion

        private int _index;
        private IList<ExpressionLexer.Token> _tokens;
        private Variable.FetchVariableDelegate _fetchVariableFunc;
        private FunctionCall.CallFunctionDelegate _callFunctionFunc;
        private char _eofCharacter = default;

        public int SaveState()
        {
            return _index;
        }

        public void RestoreState(int index)
        {
            _index = index;
        }

        public bool IsEOF()
        {
            return _index >= _tokens.Count;
        }

        public ExpressionLexer.Token NextToken()
        {
            if (_index >= _tokens.Count)
                return default;
            return _tokens[_index++];
        }

        public ExpressionLexer.Token PeekToken(int offset = 0)
        {
            if ((_index + offset) >= _tokens.Count)
                return default;
            return _tokens[_index + offset];
        }

        public string ExpectPuncOrKeyword()
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType != ExpressionLexer.TokenType.Keyword &&
                nextToken.TokenType != ExpressionLexer.TokenType.Punctuation)
                return null;
            NextToken();
            return (string)nextToken.Value;
        }

        public string ExpectKeyword()
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType != ExpressionLexer.TokenType.Keyword)
                return null;
            NextToken();
            return (string)nextToken.Value;
        }

        public bool ExpectKeyword(string keyword)
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType != ExpressionLexer.TokenType.Keyword ||
                !nextToken.Value.Equals(keyword))
                return false;
            NextToken();
            return true;
        }

        public string ExpectPunctuation()
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType != ExpressionLexer.TokenType.Punctuation)
                return null;
            NextToken();
            return (string)nextToken.Value;
        }

        public bool ExpectPunctuation(string punctuation)
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType != ExpressionLexer.TokenType.Punctuation ||
                !nextToken.Value.Equals(punctuation))
                return false;
            NextToken();
            return true;
        }

        public string ExpectIdentifier()
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType != ExpressionLexer.TokenType.Identifier) return null;
            NextToken();
            return (string)nextToken.Value;
        }

        public Variable ExpectVariable()
        {
            var identifier = ExpectIdentifier();
            if (identifier == null) return null;
            NextToken();
            return new Variable() { Name = identifier, FetchVariable = _fetchVariableFunc };
        }

        public Literal ExpectLiteral()
        {
            var nextToken = PeekToken();
            if (nextToken.TokenType == ExpressionLexer.TokenType.Number ||
                nextToken.TokenType == ExpressionLexer.TokenType.String)
            {
                NextToken();
                return new Literal() { Value = nextToken.Value };
            }
            if (nextToken.TokenType == ExpressionLexer.TokenType.Keyword)
            {
                if (nextToken.Value.Equals("true"))
                    return new Literal() { Value = true };
                if (nextToken.Value.Equals("false"))
                    return new Literal() { Value = false };
            }
            return null;
        }

        public BinaryOperator ExpectBinaryOperator()
        {
            var state = SaveState();
            var leftOperand = ExpectExpression();
            if (leftOperand == null) return null;
            var operatorPuncOrKeyword = ExpectPuncOrKeyword();
            if (operatorPuncOrKeyword == null)
            {
                RestoreState(state);
                return null;
            }
            var rightOperand = ExpectExpression();
            if (rightOperand == null)
            {
                RestoreState(state);
                return null;
            }
            switch (operatorPuncOrKeyword)
            {
                case ">": return new GreaterThanOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
                case "<": return new LessThanOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
                case ">=": return new GreaterThanEqualsOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
                case "<=": return new LessThanEqualsOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
                case "==": return new EqualsOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
                case "and":
                case "&&": return new AndOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
                case "or":
                case "||": return new OrOperator() { LeftOperand = leftOperand, RightOperand = rightOperand };
            }
            return null;
        }

        public PreUnaryOperator ExpectPreUnaryOperator()
        {
            var state = SaveState();
            var operatorPunc = ExpectPunctuation();
            if (operatorPunc == null) return null;
            var operand = ExpectExpression();
            if (operand == null)
            {
                RestoreState(state);
                return null;
            }
            switch (operatorPunc)
            {
                case "!": return new NegationOperator() { Operand = operand };
                case "-": return new NegativeOperator() { Operand = operand };
            }
            return null;
        }

        public Expression[] ExpectCommaSeparated(int minCount = 0)
        {
            var state = SaveState();
            List<Expression> expressions = new List<Expression>();
            while (true)
            {
                var expression = ExpectExpression();
                if (expression == null)
                {
                    RestoreState(state);
                    return null;
                }
                expressions.Add(expression);
                if (!ExpectPunctuation(","))
                    break;
            }
            if (expressions.Count < minCount)
                return null;
            return expressions.ToArray();
        }

        public Expression[] ExpectTuple(int minArgs = 0)
        {
            var state = SaveState();
            if (!ExpectPunctuation("(")) return null;
            var expressions = ExpectCommaSeparated(minArgs);
            if (expressions == null || !ExpectPunctuation(")"))
            {
                RestoreState(state);
                return null;
            }
            return expressions;
        }

        public FunctionCall ExpectFunctionCall()
        {
            var state = SaveState();
            var identifier = ExpectIdentifier();
            if (identifier == null) return null;
            var args = ExpectTuple();
            if (args == null)
            {
                RestoreState(state);
                return null;
            }
            return new FunctionCall() { Args = args, Name = identifier, CallFunction = _callFunctionFunc };
        }

        public Expression ExpectParenthesizedExpression()
        {
            var state = SaveState();
            if (!ExpectPunctuation("(")) return null;
            var expression = ExpectExpression();
            if (expression == null || !ExpectPunctuation(")"))
            {
                RestoreState(state);
                return null;
            }
            return expression;
        }

        public Expression ExpectExpression()
        {
            Expression expression = ExpectLiteral();
            if (expression != null) return expression;
            expression = ExpectVariable();
            if (expression != null) return expression;
            expression = ExpectParenthesizedExpression();
            if (expression != null) return expression;
            expression = ExpectBinaryOperator();
            if (expression != null) return expression;
            expression = ExpectPreUnaryOperator();
            if (expression != null) return expression;
            return null;
        }

        public Expression Parse(IList<ExpressionLexer.Token> tokens, Variable.FetchVariableDelegate fetchVariableFunc, FunctionCall.CallFunctionDelegate callFunctionFunc)
        {
            _index = 0;
            _tokens = tokens;
            _fetchVariableFunc = fetchVariableFunc;
            _callFunctionFunc = callFunctionFunc;

            return ExpectExpression();
        }
    }
}