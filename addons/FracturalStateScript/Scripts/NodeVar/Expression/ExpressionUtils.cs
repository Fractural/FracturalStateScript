using Godot;

namespace Fractural.StateScript
{
    public static class ExpressionUtils
    {
        public static ExpressionLexer Lexer { get; } = new ExpressionLexer();
        public static ExpressionParser Parser { get; } = new ExpressionParser();
        public static ExpressionParser.Expression ParseFromText(string text, ExpressionParser.Variable.FetchVariableDelegate fetchVariableFunc = null, ExpressionParser.FunctionCall.CallFunctionDelegate callFunctionFunc = null)
        {
            var tokens = Lexer.Tokenize(text);
            if (tokens == null)
            {
                GD.PushError($"{nameof(ParseFromText)}: Could not tokenize the text.");
                return null;
            }
            var ast = Parser.Parse(tokens, fetchVariableFunc, callFunctionFunc);
            if (ast == null)
            {
                GD.PushError($"{nameof(ParseFromText)}: Could not parse the tokens.");
                return null;
            }
            return ast;
        }

        public static object EvaluateFromText(string text, ExpressionParser.Variable.FetchVariableDelegate fetchVariableFunc = null, ExpressionParser.FunctionCall.CallFunctionDelegate callFunctionFunc = null)
        {
            var ast = ParseFromText(text, fetchVariableFunc, callFunctionFunc);
            if (ast == null) return null;
            return ast.Evaluate();
        }

    }
}