using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Parser;
using Lexer;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private Lexer.Lexer lexer = new Lexer.Lexer(FunLexingRules.rules);
        private Parser.Parser parser = new Parser.Parser();

        [TestMethod]
        public void TestConstantParser()
        {
            string inp = @"";
            var lexout = lexer.Lex(inp);

            var x = parser.Parse(lexout);
            Console.WriteLine("klaar)");
        }
    }
}
