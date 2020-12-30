using System;
using System.Collections.Generic;
using Lexer;
using Parser.AbstractSyntaxTrees;

namespace Parser
{
    public class Parser
    {
        Stack<(TokenType, string)> tokens;

        public Parser()
        {
        }

        public List<Decl> Parse(List<(TokenType, string)> t)
        {
            t.Reverse();
            tokens = new Stack<(TokenType, string)>(t);

            var parseOut = new List<Decl>();
            while (tokens.Count != 0)
            {
                if (tokens.Peek().Item1 == TokenType.SEMICOLON)
                    tokens.Pop();
                parseOut.Add(DeclParser());
            }
                
            return parseOut;
        }

        private Decl DeclParser()
        {
            Decl x;
            try
            {
                x = DefParser();
            }
            catch (NoMatchException)
            {
                try
                {
                    x = ConstantParser();
                }
                catch(NoMatchException)
                {
                    try
                    {
                        x = MainParser();
                    }
                    catch (NoMatchException)
                    {
                        throw new Exception("Unable to Parse");
                    }
                }
            }
            return x;
        }


        private Exp Exp()
        {
            Exp x;
            try
            {
                x = CallParser();
                
            }
            catch (NoMatchException)
            { 
                try
                {
                    x = ArithmeticParser();
                }
                catch (NoMatchException)
                {
                    try
                    {
                        x = IfParser();
                    }
                    catch (NoMatchException)
                    {
                        try
                        {
                            x = NumberParser();
                        }
                        catch (NoMatchException)
                        {
                            try
                            {
                                x = IdentifierParser();
                            }
                            catch (NoMatchException)
                            {
                                throw new Exception("Unable to Parse");
                            }
                        }
                    }
                }
            }
            
            return x;
        }

        private Exp ExpParser()
        {
            Exp e1 = Exp();

            if (tokens.Count == 0)
                return e1;


            // BUG: this pops the semicolon at the end of function definitions
            if (tokens.Peek().Item1 == TokenType.SEMICOLON)
            {
                tokens.Pop();
                return new Sequence(e1, ExpParser());
            }
            else
            {
                return e1;
            }
        }

        // Here we need to backtrack in the event we find out it's not a function call
        // but rather an identifier.
        private Exp CallParser()
        {
            if (tokens.Peek().Item1 == TokenType.LOCAL_IDENTIFIER)
            {
                var temp = tokens.Pop();

                // backtrack if it's not a function call
                if (tokens.Peek().Item1 != TokenType.LPAREN)
                {
                    tokens.Push(temp);
                    throw new NoMatchException();
                }


                var funcName = temp.Item2;

                // pop lparen
                tokens.Pop();

                //read args
                List<Exp> args = CallArgListParser();

                //pop rparen
                tokens.Pop();

                return new Call(funcName, args);
            }
            else
            {
                throw new NoMatchException();
            }
        }

        // Parser for argument lists in the declaration of a function
        //returns a tuple of form (argName, argType)
        private List<(string, string)> DefArgListParser()
        {
            var outp = new List<(string, string)>();

            if (tokens.Peek().Item1 == TokenType.LPAREN)
                tokens.Pop();
            else
                throw new Exception("Invalid argument list");

            while (tokens.Peek().Item1 != TokenType.RPAREN)
            {
                string argName = tokens.Pop().Item2;

                //pop ':'
                tokens.Pop();

                string argType = tokens.Pop().Item2;

                outp.Add((argName, argType));

                if (tokens.Peek().Item1 == TokenType.ARG_SEPERATOR)
                    tokens.Pop();
            }

            //pop ')'
            tokens.Pop();

            return outp;
            
        }

        // parser for argument lists when calling a function
        private List<Exp> CallArgListParser()
        {
            var outp = new List<Exp>();

            // return if the function is called with no args
            while (tokens.Peek().Item1 != TokenType.RPAREN)
            {
                outp.Add(ExpParser());

                if (tokens.Peek().Item1 == TokenType.ARG_SEPERATOR)
                    tokens.Pop();
            }
            return outp;                
        }

        private Exp IdentifierParser()
        {
            if (tokens.Peek().Item1 == TokenType.LOCAL_IDENTIFIER || tokens.Peek().Item1 == TokenType.GLOBAL_IDENTIFIER)
            {
                return new Var(tokens.Pop().Item2);
            }
            else
            {
                throw new NoMatchException();
            }
        }

        private Exp NumberParser()
        {
            if (tokens.Peek().Item1 == TokenType.NUMBER)
            {
                string n = tokens.Pop().Item2;

                try
                {
                    return new Num(int.Parse(n));
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine($"Unable to parse int: {n}");
                    throw;
                }
            }
            else if (tokens.Peek().Item1 == TokenType.FLOAT_NUMBER)
            {
                string n = tokens.Pop().Item2;

                try
                {
                    return new FNum(float.Parse(n));
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine($"Unable to parse float: {n}");
                    throw;
                }
            }
            else
            {
                throw new NoMatchException();
            }
        }



        private Exp IfParser()
        {
            if (tokens.Peek().Item2 == "if")
            {
                tokens.Pop();

                BooleanOperation bexp = (BooleanOperation)BooleanParser();
                
                // pop 'then'
                tokens.Pop();

                Exp trueBranch;

                // pop optional curly brackets
                if (tokens.Peek().Item1 == TokenType.LPAREN)
                {
                    tokens.Pop();

                    trueBranch = ExpParser();

                    // pop optional curly brackets
                    if (tokens.Peek().Item1 == TokenType.RPAREN)
                        tokens.Pop();
                }
                else
                {
                    trueBranch = ExpParser();
                }
                

                // pop 'else'
                tokens.Pop();

                Exp elseBranch;
                // pop optional curly brackets
                if (tokens.Peek().Item1 == TokenType.LPAREN)
                {
                    tokens.Pop();

                    elseBranch = ExpParser();

                    // pop optional curly brackets
                    if (tokens.Peek().Item1 == TokenType.RPAREN)
                        tokens.Pop();
                }
                else
                {
                    elseBranch = ExpParser();
                }


                return new If(bexp, trueBranch, elseBranch);
            }
            else
            {
                throw new NoMatchException();
            }
        }

        private OperationType DefineBooleanOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.BOOL_OP)
            {
                throw new NoMatchException("Call to define Boolean Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "==" => OperationType.EQUAL,
                "!=" => OperationType.NOT_EQUAL,
                "<" => OperationType.LESS_THAN,
                ">" => OperationType.GREATER_THAN,
                "<=" => OperationType.LESS_THAN_OR_EQUAL,
                ">=" => OperationType.GREATER_THAN_OR_EQUAL,
                _ => throw new NoMatchException("Token was not a valid Boolean operator")
            };
        }

        private Bexp BooleanParser()
        {
            Bexp bexp;
            try
            {
                bexp = BoolBrackets();
            }
            catch (NoMatchException)
            {
                try
                {
                    bexp = SimpleBool();
                }
                catch (NoMatchException)
                {
                    // The expression we got was not a boolean expression
                    // but we expected one if this parser was called
                    // meaning the whole parsing should fail
                    throw new Exception();
                }
            }
            return bexp;
        }
        // For simple boolean expressions such as "x < y"
        private Bexp SimpleBool()
        {
            Exp x;
            try
            {
                x = ExpParser();
            }
            catch (NoMatchException)
            {
                // No match in this parser
                throw;
            }
            var op = DefineBooleanOperator(tokens.Peek());

            // Pop the Boolean Operator now that we know it's a match
            tokens.Pop();

            var y = ExpParser();

            return new BooleanOperation(op, x, y);
        }


        // For boolean expressions in brackets or Compound Boolean Expressions joined with AND or OR. Eg (x < y) && (z == 4)
        private Bexp BoolBrackets()
        {
            if (tokens.Peek().Item1 == TokenType.LPAREN)
            {
                // Pop the open bracket
                tokens.Pop();

                // Get the boolean expression
                BooleanOperation bexp = (BooleanOperation) BooleanParser();

                // Pop the close bracket
                tokens.Pop();

                return bexp;
            }
            else
            {
                throw new NoMatchException();
            }
        }

        // ================
        // Arithmetic Parsers
        // ================

        private OperationType DefineAEArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new NoMatchException("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "+" => OperationType.PLUS,
                "-" => OperationType.MINUS,
                _ => throw new NoMatchException("Token was not a valid arithmetic operator")
            };
        }


        private OperationType DefineTeArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new NoMatchException("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "*" => OperationType.TIMES,
                "/" => OperationType.DIVIDE,
                "%" => OperationType.MODULO,
                _ => throw new NoMatchException("Token was not a valid arithmetic operator")
            };
        }

        private Exp ArithInBrackets()
        {
            if (tokens.Peek().Item1 == TokenType.LPAREN)
            {
                // Pop open bracket
                tokens.Pop();

                // Get the arithmetic Expression
                var aexp = ArithmeticParser();

                // Pop close bracket
                tokens.Pop();

                // Return the Arithmetic Expression
                return aexp;
            }
            else
            {
                // No match found, try another parser
                throw new NoMatchException();
            }
        }

        private Exp Fa()
        {
            Exp aexp;
            try
            {
                aexp = ArithInBrackets();
            }
            catch (NoMatchException)
            {
                try
                {
                    aexp = NumberParser();
                }
                catch (NoMatchException)
                {
                    try
                    {
                        aexp = IdentifierParser();
                    }
                    catch (NoMatchException)
                    {
                        throw;
                    }
                }
            }
            return aexp;
        }

        private Exp Te()
        {
            var x = Fa();
            try
            {
                // Check if there's an arithmetic operator
                var op = DefineTeArithmeticOperator(tokens.Peek());

                // Pop the arithmetic operator now that we know there is one
                tokens.Pop();

                var y = Te();
                return new ArithmeticOperation(op, x, y);
            }
            catch (Exception)
            {
                return x;
            }
        }

        // MISSING FEATURE: A function call should be a valid arithmetic expression
        private Exp ArithmeticParser()
        {
            var x = Te();
            try
            {
                var op = DefineAEArithmeticOperator(tokens.Peek());
                tokens.Pop();
                var y = ArithmeticParser();

                return new ArithmeticOperation(op, x, y);
            }
            catch (Exception)
            {
                return x;
            }
        }




        //Decl parsers
        private Decl ConstantParser()
        {
            if (tokens.Peek().Item2 == "val")
            {
                // pop 'val'
                tokens.Pop();

                // get variable name
                string constName = tokens.Pop().Item2;

                //pop ':'
                tokens.Pop();

                // get type annotation
                if (tokens.Peek().Item2 == "Int")
                {
                    tokens.Pop();

                    //pop assign op (=)
                    tokens.Pop();

                    try
                    {
                        return new Const(constName, int.Parse(tokens.Pop().Item2));
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine($"The constant: \"{constName}\" was assigned a Double when its type is Int");
                        throw;
                    }
                }
                else if (tokens.Peek().Item2 == "Double")
                {
                    tokens.Pop();

                    // pop assign op (=)
                    tokens.Pop();

                    try
                    {
                        return new FConst(constName, float.Parse(tokens.Pop().Item2));
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine($"The constant: \"{constName}\" was assigned a Int when its type is Double");
                        throw;
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Parse Failed!\nExpected Double or Int, got: {tokens.Peek()}");
                    throw new Exception();
                }
            }
            else
            {
                throw new NoMatchException();
            }
        }
        private Decl DefParser()
        {
            if (tokens.Peek().Item2 == "def")
            {
                tokens.Pop();

                string funcName = tokens.Pop().Item2;

                var args = DefArgListParser();

                //pop ':'
                tokens.Pop();

                string returnType = tokens.Pop().Item2;

                //pop '='
                tokens.Pop();

                // pop '{'
                tokens.Pop();

                Exp body = ExpParser();

                //pop '}'
                tokens.Pop();

                return new Def(funcName, args, returnType, body);
            }
            else
            {
                throw new NoMatchException();
            }
        }

        private Decl MainParser()
        {
            Exp bodyMain;
            try
            {
                bodyMain = ExpParser();
            }
            catch (NoMatchException)
            {
                throw;
            }

            return new Main(bodyMain);
        }
        
    }
}
