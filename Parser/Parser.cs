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

        public Exp Parse(List<(TokenType, string)> t)
        {
            t.Reverse();
            tokens = new Stack<(TokenType, string)>(t);
            return ExpParser();
        }

        private Exp ExpParser()
        {
            Exp x;
          /*  try
            {
                x = CallParser();
            }
            catch (NoMatchException)
            {*/
                try
                {
                     x = ArithmeticParser();
                }
                catch (NoMatchException)
                { 
                    try
                    {
                        x = IdentifierParser();
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
                                   x = SequenceParser();
                                }
                                catch (NoMatchException)
                                {
                                    throw new Exception("Unable to Parse");
                                }
                            }
                        }
                    }
                }
            //}
            return x;
        }

        // REMEMBER TO CALL THIS BEFORE THE IDENTIFIER PARSER
        private Exp CallParser()
        {
            /* I have found it difficult to differentiate between variable
             * references and function calls, so here I backtrack if I
             * find out it's not a func call
             */
            throw new NotImplementedException();

            if (tokens.Peek().Item1 == TokenType.LOCAL_IDENTIFIER)
            {
                var funcName = tokens.Pop();
                if (tokens.Peek().Item1 == TokenType.LPAREN)
                {
                    tokens.Pop();

                    //read args

                }
                else
                {
                    // not a function call, backtrack
                    tokens.Push(funcName);
                    throw new NoMatchException();
                }
            }
        }

        // Parser for argument lists
        private List<Exp> ListParser()
        {
            /* Args can be:
             *  variables
             *  integer constants
             *  float constants
             *  nothing
             */
            throw new NotImplementedException();

        }

        private Exp IdentifierParser()
        {
            if (tokens.Peek().Item1 == TokenType.LOCAL_IDENTIFIER)
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

                var bexp = BooleanParser();

                // pop 'then'
                tokens.Pop();

                // pop optional curly brackets
                if (tokens.Peek().Item1 == TokenType.LPAREN)
                    tokens.Pop();

                Exp trueBranch = ExpParser();

                // pop optional curly brackets
                if (tokens.Peek().Item1 == TokenType.RPAREN)
                    tokens.Pop();

                // pop 'else'
                tokens.Pop();

                // pop optional curly brackets
                if (tokens.Peek().Item1 == TokenType.LPAREN)
                    tokens.Pop();

                Exp elseBranch = ExpParser();

                // pop optional curly brackets
                if (tokens.Peek().Item1 == TokenType.RPAREN)
                    tokens.Pop();

                return new If(bexp, trueBranch, elseBranch);
            }
            else
            {
                throw new NoMatchException();
            }
        }

        private BoolOperationType DefineBooleanOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.BOOL_OP)
            {
                throw new NoMatchException("Call to define Boolean Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "==" => BoolOperationType.EQUAL,
                "!=" => BoolOperationType.NOT_EQUAL,
                "<" => BoolOperationType.LESS_THAN,
                ">" => BoolOperationType.GREATER_THAN,
                "<=" => BoolOperationType.LESS_THAN_OR_EQUAL,
                ">=" => BoolOperationType.GREATER_THAN_OR_EQUAL,
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
                var bexp = BooleanParser();

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

        private ArithOperationType DefineAEArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new NoMatchException("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "+" => ArithOperationType.PLUS,
                "-" => ArithOperationType.MINUS,
                _ => throw new NoMatchException("Token was not a valid arithmetic operator")
            };
        }


        private ArithOperationType DefineTeArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new NoMatchException("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "*" => ArithOperationType.TIMES,
                "/" => ArithOperationType.DIVIDE,
                "%" => ArithOperationType.MODULO,
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
            catch (NoMatchException)
            {
                return x;
            }
        }

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
            catch (NoMatchException)
            {
                return x;
            }
        }

        private Exp SequenceParser()
        {
            Exp e1 = ExpParser();

            if (tokens.Peek().Item1 == TokenType.SEMICOLON)
            {
                tokens.Pop();
                return new Sequence(e1, SequenceParser());
            }
            else
            {
                return e1;
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
            throw new NotImplementedException();
        }

        private Decl MainParser()
        {
            throw new NotImplementedException();
        }



        
    }
}
