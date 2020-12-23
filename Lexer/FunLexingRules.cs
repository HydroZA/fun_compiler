/*
keywords
	val
	def
	if
	then
	else
	write
type
	Double
	Int
	Void
identifiers
	global	->	[A-Z][a-zA-Z0-9_]* 
	local	->	[a-z][a-zA-Z0-9_]*
arith_operators
	+
	-
	/
	*
	%
bool_operators
	<=
	>=
	<
	>
Comments
    //
*/

using System;
using System.Linq;

namespace Lexer
{
    public static class SulzmannFunLexingRules
    {
        private static readonly Rexp rDIGIT = new RANGE("0123456789".ToHashSet());

        private static readonly Rexp rKEYWORD = new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT("if", "then"), "else"), "write"), "true"), "false"), "val"), "def");

        private static readonly Rexp rTYPE = new ALT(new ALT("Double", "Int"), "Void");

        private static readonly Rexp rASSIGN_OP = new ALT(new CHAR('='), new CHAR(':'));

        private static readonly Rexp rBOOL_OP = new ALT(new ALT(new ALT(new ALT(new ALT(new ALT("==", "!="), "<"), ">"), "<="), ">="), "!");

        private static readonly Rexp rARITH_OP = new ALT(new ALT(new ALT(new ALT("-", "+"), "*"), "/"), "%");

        private static readonly Rexp rARG_SEPERATOR = new CHAR(',');

        private static readonly Rexp rLETTER = new RANGE("ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz".ToHashSet());

        private static readonly Rexp rCAP_LETTER = new RANGE("ABCDEFGHIJKLMNOPQRSTUVXYZ".ToHashSet());

        private static readonly Rexp rSYMBOL = new ALT(rLETTER, new RANGE("._><=;,:\\".ToHashSet()));

        private static readonly Rexp rLPAREN = new ALT("{", "(");

        private static readonly Rexp rRPAREN = new ALT("}", ")");

        private static readonly Rexp rSEMICOLON = ";";

        private static readonly Rexp rWHITESPACE = new ALT(new ALT(new PLUS(new CHAR(' ')), Environment.NewLine), new CHAR('\t'));

        private static readonly Rexp rGLOBAL_IDENTIFIER = new SEQ(rCAP_LETTER, new STAR(new ALT(new ALT(new CHAR('_'), rLETTER), rDIGIT)));

        private static readonly Rexp rLOCAL_IDENTIFIER = new SEQ(rLETTER, new STAR(new ALT(new ALT(new CHAR('_'), rLETTER), rDIGIT)));

        private static readonly Rexp rNUMBER = new ALT(new CHAR('0'), new SEQ(new OPTIONAL(new CHAR('-')), new SEQ(new RANGE("123456789".ToHashSet()), new STAR(rDIGIT))));

        private static readonly Rexp rFLOAT_NUMBER = new SEQ(new SEQ(rNUMBER, new CHAR('.')), new STAR(rDIGIT));

        private static readonly Rexp rCOMMENT = new SEQ("//", new STAR(new ALT(new ALT(new ALT(rDIGIT, " "), rSYMBOL), rLETTER)));

        private static readonly Rexp rSTRING = new SEQ(new SEQ(new CHAR('\"'), new STAR(new ALT(new ALT(rSYMBOL, rWHITESPACE), rDIGIT))), "\"");

        public static readonly Rexp rules =
            new STAR(
                new ALT(
                    new ALT(
                    new ALT(
                        new ALT(
                            new ALT(
                                new ALT(
                                    new ALT(
                                        new ALT(
                                            new ALT(
                                                new ALT(
                                                    new ALT(
                                                        new ALT(
                                                            new ALT(
                                                                new ALT(
                                                                    new ALT(
                                                                        new RECD(TokenType.FLOAT_NUMBER, rFLOAT_NUMBER),
                                                                new RECD(TokenType.ASSIGN_OP, rASSIGN_OP)),
                                                            new RECD(TokenType.KEYWORD, rKEYWORD)),
                                                        new RECD(TokenType.TYPE, rTYPE)),
                                                    new RECD(TokenType.GLOBAL_IDENTIFIER, rGLOBAL_IDENTIFIER)),
                                                new RECD(TokenType.LOCAL_IDENTIFIER, rLOCAL_IDENTIFIER)),
                                            new RECD(TokenType.BOOL_OP, rBOOL_OP)),
                                        new RECD(TokenType.ARITH_OP, rARITH_OP)),
                                    new RECD(TokenType.NUMBER, rNUMBER)),
                                new RECD(TokenType.SEMICOLON, rSEMICOLON)),
                            new RECD(TokenType.STRING, rSTRING)),
                        new RECD(TokenType.LPAREN, rLPAREN)),
                    new RECD(TokenType.RPAREN, rRPAREN)),
                new RECD(TokenType.ARG_SEPERATOR, rARG_SEPERATOR)),
            new RECD(TokenType.COMMENT, rCOMMENT)),
        new RECD(TokenType.WHITESPACE, rWHITESPACE))
        );

    }
}
