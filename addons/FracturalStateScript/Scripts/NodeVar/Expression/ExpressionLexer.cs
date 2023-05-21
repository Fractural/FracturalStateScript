using Godot;
using System.Collections.Generic;

namespace Fractural.StateScript
{
    /// <summary>
    /// Simple expression lexer that converts text to tokens 
    /// that are parsable by the <seealso cref="ExpressionParser"/>.
    /// </summary>
    public class ExpressionLexer
    {
        public enum TokenType
        {
            String,
            Number,
            Identifier,
            Keyword,
            Punctuation,
        }

        public class Token
        {
            public object Value { get; set; }
            public TokenType TokenType { get; set; }
        }

        private int _index;
        private string _text;
        private List<Token> _tokens;
        private char _eofCharacter = default;
        private string[] _keywords;
        private string[] _punctuation;
        private string[] DefaultKeywords => new string[] {
            "true",
            "false",
            "and",
            "or"
        };
        private string[] DefaultPunctuation => new string[] {
            "!",
            "+",
            "-",
            "/",
            "*",
            "(",
            ")",
            "==",
            ">=",
            "<=",
            ">",
            "<",
            "&&",
            "||",
            ","
        };

        public bool IsEOF()
        {
            return _index >= _text.Length;
        }

        public char NextChar()
        {
            if (_index >= _text.Length)
                return default;
            return _text[_index++];
        }

        public char PeekChar(int offset = 0)
        {
            if ((_index + offset) >= _text.Length)
                return default;
            return _text[_index + offset];
        }

        public bool IsValidIdentifierStart(char c)
        {
            int lowerCharIdx = (int)char.ToLower(c);
            return 'a' <= lowerCharIdx && 'z' >= lowerCharIdx || c == '_';
        }

        public bool IsValidIdentifierChar(char c)
        {
            int lowerCharIdx = (int)char.ToLower(c);
            return ('a' <= lowerCharIdx && 'z' >= lowerCharIdx) || c == '_' || char.IsDigit(c);
        }

        public void ConsumeWhitespace()
        {
            while (char.IsWhiteSpace(PeekChar()))
                NextChar();
        }

        public bool ExpectString(string keyword)
        {
            if (_index + keyword.Length >= _text.Length)
                return false;
            return _text.Substring(_index, keyword.Length).Equals(keyword);
        }

        public Token ExpectIdentifier()
        {
            var name = "";
            if (!IsValidIdentifierStart(PeekChar())) return null;
            name += NextChar();
            while (IsValidIdentifierChar(PeekChar()))
                name += NextChar();
            return new Token() { Value = name, TokenType = TokenType.Identifier };
        }

        public Token ExpectPunctuation()
        {
            foreach (string punctuation in _keywords)
                if (ExpectString(punctuation))
                    return new Token() { TokenType = TokenType.Keyword, Value = punctuation };
            return null;
        }

        public Token ExpectKeyword()
        {
            foreach (string keyword in _keywords)
                if (ExpectString(keyword))
                    return new Token() { TokenType = TokenType.Keyword, Value = keyword };
            return null;
        }

        public Token ExpectNumberLiteral()
        {
            string numberLiteralText = "";
            int index = 0;
            bool foundDecimal = false;
            while (char.IsDigit(PeekChar(index)) || PeekChar(index) == '.')
            {
                var nextChar = NextChar();
                if (nextChar == '.')
                {
                    // We can't have two decimals!
                    if (foundDecimal)
                        return null;
                    foundDecimal = true;
                }
                numberLiteralText += nextChar;
            }
            if (foundDecimal)
            {
                if (float.TryParse(numberLiteralText, out float result))
                    return new Token() { Value = result, TokenType = TokenType.Number };
            }
            else
            {
                if (int.TryParse(numberLiteralText, out int result))
                    return new Token() { Value = result, TokenType = TokenType.Number };
            }
            return null;
        }

        public Token ExpectStringLiteral()
        {
            if (PeekChar() != '"') return null;
            NextChar();
            string resultString = "";
            while (PeekChar() != '"')
            {
                resultString += NextChar();
                if (IsEOF())
                    // Unterminated string
                    return null;
            }
            NextChar();
            return new Token() { Value = resultString, TokenType = TokenType.String };
        }

        public Token ExpectToken()
        {
            var token = ExpectKeyword();
            if (token != null) return token;
            token = ExpectPunctuation();
            if (token != null) return token;
            token = ExpectIdentifier();
            if (token != null) return token;
            token = ExpectNumberLiteral();
            if (token != null) return token;
            token = ExpectStringLiteral();
            return token;
        }

        public string PeekString(int amount)
        {
            int substringLength = amount;
            if (_text.Length - _index > substringLength)
                substringLength = _text.Length - _index;
            return _text.Substring(_index, substringLength);
        }

        public IList<Token> Tokenize(string text, string[] keywords = null, string[] punctuation = null)
        {
            _text = text;
            _tokens = new List<Token>();

            _keywords = keywords ?? DefaultKeywords;
            _punctuation = punctuation ?? DefaultPunctuation;

            while (!IsEOF())
            {
                var token = ExpectToken();
                if (token == null)
                {
                    GD.PushError($"{nameof(ExpressionLexer)}: Unknown token \"{PeekString(10)}\".");
                    return null;
                }
                ConsumeWhitespace();
            }
            return _tokens;
        }
    }
}