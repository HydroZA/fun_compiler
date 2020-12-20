using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*
 * Adapted from Jack Vanlightly's blog post:
 * https://jack-vanlightly.com/blog/2016/2/3/creating-a-simple-tokenizer-lexer-in-c
 */


namespace Lexer
{
    public static class FunLexingRules
    {
        public static readonly List<TokenDefinition> rules = new List<TokenDefinition>
        {
            new TokenDefinition(TokenType.COMMENT, @"^\/\/([0-9]| |[._><=;,:\\]|[a-zA-Z])*"),
            new TokenDefinition(TokenType.KEYWORD, @"^def|^val|^if|^then|^else|^write|^true|^false"),
          //  new TokenDefinition(TokenType.CALL, @"\w+(?=\(.*\))"), // positive lookahead
            new TokenDefinition(TokenType.TYPE, @"^Double|^Int|^Void"),
            new TokenDefinition(TokenType.STRING, @"^""([@._ ><=;,:\\[a-zA-Z]|\s|[0 - 9])*"""),
            new TokenDefinition(TokenType.GLOBAL_IDENTIFIER, @"^[A-Z][a-zA-Z0-9_]*"),
            new TokenDefinition(TokenType.LOCAL_IDENTIFIER, @"^[a-z][a-zA-Z0-9_]*"),
            new TokenDefinition(TokenType.BOOL_OP, @"^==|^!=|^<=|^>=|^!|^>|^<"),
            new TokenDefinition(TokenType.ASSIGN_OP, @"^:|^="),
            new TokenDefinition(TokenType.ARITH_OP, @"^-|^\+|^\*|^\/|^%"),
            new TokenDefinition(TokenType.FLOAT_NUMBER, @"^-?\d+\.\d*"),
            new TokenDefinition(TokenType.NUMBER, @"^0|^-?[1-9][0-9]*"),
            new TokenDefinition(TokenType.SEMICOLON, @"^;"),
            new TokenDefinition(TokenType.LPAREN, @"^{|^\("),
            new TokenDefinition(TokenType.RPAREN, @"^}|^\)"),
            new TokenDefinition(TokenType.ARG_SEPERATOR, @"^,"),
            new TokenDefinition(TokenType.WHITESPACE, @"^\s")
        };
    }
}
