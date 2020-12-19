using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lexer
{
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        ARITH_OP,
        COMP_BOOL_OP,
        OPERATOR,
        NUMBER,
        SEMICOLON,
        STRING,
        LPAREN,
        RPAREN,
        COMMENT,
        WHITESPACE
    }
    

    public class Lexer
    {
        List<TokenDefinition> rules;
        public Lexer(List<TokenDefinition> r)
        {
            rules = r;
        }

        public List<(TokenType, string)> Lex (string s)
        {
            var tokens = new List<(TokenType, string)>();
            string remainingText = s;

            while (!string.IsNullOrWhiteSpace(remainingText))
            {
                var match = FindMatch(remainingText);
                if (match.IsMatch)
                {
                    tokens.Add((match.Type, match.Value));
                    remainingText = match.RemainingText;
                }
                else
                {
                    remainingText = remainingText[1..];
                }
            }
            return RemoveWhitespace(tokens);
        }
        private TokenMatch FindMatch(string s)
        {
            foreach (var tokenDefinition in rules)
            {
                var match = tokenDefinition.Match(s);
                if (match.IsMatch)
                    return match;
            }

            return new TokenMatch() { IsMatch = false };
        }

        public List<(TokenType, string)> RemoveWhitespace(List<(TokenType, string)> lst) => lst.Where(x => x.Item1 != TokenType.WHITESPACE && x.Item1 != TokenType.COMMENT).ToList();    }
}
