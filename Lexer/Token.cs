using System;
using System.Text.RegularExpressions;

namespace Lexer
{
    public class TokenMatch
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
        public string RemainingText { get; set; }
        public bool IsMatch { get; set; }
    }

    public class TokenDefinition
    {
        private Regex _regex;
        private readonly TokenType _returnsToken;

        public TokenDefinition(TokenType returnsToken, string regexPattern)
        {
            _regex = new Regex(regexPattern);
            _returnsToken = returnsToken;
        }

        public TokenMatch Match(string inputString)
        {
            var match = _regex.Match(inputString);
            if (match.Success)
            {
                string remainingText = string.Empty;
                if (match.Length != inputString.Length)
                    remainingText = inputString.Substring(match.Length);

                return new TokenMatch()
                {
                    IsMatch = true,
                    RemainingText = remainingText,
                    Value = match.Value,
                    Type = _returnsToken
                };
            }
            else
            {
                return new TokenMatch() { IsMatch = false };
            }

        }
    }
}
