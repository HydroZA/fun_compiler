using System;
using System.IO;
using Lexer;
using CodeGen;
using System.Text.RegularExpressions;
using LLVMSharp;
using System.Text;

namespace fun_compiler
{
    class Program
    {
        private const string HELP_MSG = @"usage: FunCompiler.exe [args] <file>";

        static void Main(string[] args)
        {
            bool interpret = false;
            string input = "", outpath = "", filename = "";
            Lexer.Lexer lexer = new Lexer.Lexer(FunLexingRules.rules);
            Parser.Parser parser = new Parser.Parser();

            if (args.Length == 0)
            {
                Console.Error.WriteLine(HELP_MSG);
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
                                Console.Error.WriteLine(HELP_MSG);
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

            string outputFile = $"{outpath}/{filename}.ll";

            Console.WriteLine("Lexing...");
            var lexout = lexer.Lex(input);

            Console.WriteLine("Parsing...");
            var parseout = parser.Parse(lexout);

            Console.WriteLine("Generating Code...");
            var ir = new LLVMCodeGen(filename).GenerateCode(parseout);

            //LLVM.DumpModule(ir);
            LLVM.PrintModuleToFile(ir, outputFile, out _);


            Console.WriteLine("Done!");
        }
    }
}
