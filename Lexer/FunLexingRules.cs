using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*
 * Adapted from Jack Vanlightly's blog post:
 * https://jack-vanlightly.com/blog/2016/2/3/creating-a-simple-tokenizer-lexer-in-c
 */


namespace Lexer
{
    public static class WhileLexingRules
    {
        public static readonly List<TokenDefinition> rules = new List<TokenDefinition>
        {
            new TokenDefinition(TokenType.KEYWORD, @"^skip|^while|^do|^if|^then|^else|^read|^write|^for|^to|^true|^false|^:=|^upto"),
            new TokenDefinition(TokenType.STRING, @"^""([@._ ><=;,:\\[a-zA-Z]|\s|[0 - 9])*"""),
            new TokenDefinition(TokenType.IDENTIFIER, @"^[a-zA-Z](_|[a-zA-Z]|[0-9])*"),
            new TokenDefinition(TokenType.OPERATOR, @"^==|^!=|^<|^>|^<=|^>=|^!"),
            new TokenDefinition(TokenType.COMP_BOOL_OP, @"^\|\||^&&"),
            new TokenDefinition(TokenType.ARITH_OP, @"^-|^\+|^\*|^\/|^%"),
            new TokenDefinition(TokenType.NUMBER, @"^0|^-?[1-9][0-9]*"),
            new TokenDefinition(TokenType.SEMICOLON, @"^;"),
            new TokenDefinition(TokenType.LPAREN, @"^{|^\("),
            new TokenDefinition(TokenType.RPAREN, @"^}|^\)"),
            new TokenDefinition(TokenType.WHITESPACE, @"^\s"),
            new TokenDefinition(TokenType.COMMENT, @"^\/\/([0-9]| |[._><=;,:\\]|[a-zA-Z])*")
        };
        
    }
}
