using System;
using Lexer;
using CodeGen;
using Parser.AbstractSyntaxTrees;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace fun_compiler
{
    class Program
    {
        

        private static readonly string helpMsg = @"usage: FunCompiler.exe [args] <file>";


        static void Main(string[] args)
        {
            bool interpret = false, assemble = false;
            string input = "", outpath = "", filename = "";
            Lexer.Lexer lexer = new Lexer.Lexer(FunLexingRules.rules);
            Parser.Parser parser = new Parser.Parser();
            CodeGenerator gen = new CodeGenerator();

            if (args.Length == 0)
            {
                Console.Error.WriteLine(helpMsg);
                Environment.Exit(1);
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch (arg)
                {
                    case "-i":
                    case "-interpret":
                        {
                            interpret = true;
                            break;
                        }
                    case "-o":
                        {
                            outpath = args[++i];

                            // -o and -i can not be used together
                            if (interpret)
                            {
                                Console.Error.WriteLine("-i and -o cannot be used together");
                                Console.Error.WriteLine(helpMsg);
                                Environment.Exit(1);
                            }
                            break;
                        }
                    case "-a":
                        {
                            assemble = true;

                            // -r and -i can not be used together
                            if (interpret)
                            {
                                Console.Error.WriteLine("-i and -a cannot be used together");
                                Console.Error.WriteLine(helpMsg);
                                Environment.Exit(1);
                            }
                            break;
                        }
                    default:
                        {
                            try
                            {
                                input = File.ReadAllText(arg);

                                // This regular expression extracts the filename and discards the rest of the path and the file extension
                                filename = Regex.Match(arg, @"^.*[\\|\/](.+?)\.[^\.]+$").Groups[1].ToString();
                            }
                            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
                            {
                                Console.Error.WriteLine("File Not Found: " + arg);
                                Environment.Exit(1);
                            }
                            break;
                        }
                }
            }

            // if no output path given, output the file to the working dir
            if (outpath == "")
                outpath = ".";

            /*    Console.WriteLine("Lexing...");
                var lexout = lexer.Lex(input);

                Console.WriteLine("Parsing...");
                var parseout = parser.Parse(lexout);

                Console.WriteLine("Done!");
            */

            // var a = gen.CPSi(new ArithmeticOperation(OperationType.PLUS, new ArithmeticOperation(OperationType.TIMES, new Var("a"), new Num(3)), new Num(4)));

            var a = gen.CPSi(new If(new BooleanOperation(OperationType.EQUAL, new Var("k"), new Num(23)), new ArithmeticOperation(OperationType.TIMES, new Var("a"), new Num(3)), new ArithmeticOperation(OperationType.TIMES, new Var("b"), new Num(4))));

            Console.WriteLine("yo");
        }
    }
}
