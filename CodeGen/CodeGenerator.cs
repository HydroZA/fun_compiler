using System;
using System.Reflection;
using Lexer;
using Parser.AbstractSyntaxTrees;

namespace CodeGen
{
    public enum VarType
    {
        INT,
        DOUBLE,
        UNDEF
    }

    public class CodeGenerator
    {
        private const string PRELUDE = @"declare i32 @printf(i8*, ...)

@.str_nl = private constant [2 x i8] c""\0A\00""
@.str_star = private constant[2 x i8] c""*\00""
@.str_space = private constant[2 x i8] c"" \00""

define void @new_line() #0 {
  %t0 = getelementptr[2 x i8], [2 x i8]* @.str_nl, i32 0, i32 0
  %1 = call i32(i8*, ...) @printf(i8* %t0)
  ret void
}

define void @print_star() #0 {
  %t0 = getelementptr[2 x i8], [2 x i8]* @.str_star, i32 0, i32 0
  %1 = call i32(i8*, ...) @printf(i8* %t0)
  ret void
}

define void @print_space() #0 {
  % t0 = getelementptr[2 x i8], [2 x i8]* @.str_space, i32 0, i32 0
  % 1 = call i32(i8 *, ...) @printf(i8 * % t0)
  ret void
}

define void @skip() #0 {
  ret void
}

@.str = private constant[4 x i8] c""%d\0A\00""

define void @print_int(i32 %x)
{
   % t0 = getelementptr[4 x i8], [4 x i8]* @.str, i32 0, i32 0
   call i32(i8*, ...) @printf(i8 * % t0, i32 % x)
   ret void
}
; END OF PRELUDE";

        private static int labelCount;

        public CodeGenerator()
        {
            labelCount = 0;

        }

        // Creates and stores unique labels
        private struct Label
        {
            public readonly string l;

            public Label(string s)
            {
                l = s + labelCount++;
            }
        }

        private string CompileIntArithOperation(OperationType op) => op switch
        {
            OperationType.PLUS => "add i32 ",
            OperationType.MINUS => "sub i32 ",
            OperationType.TIMES => "mul i32 ",
            OperationType.DIVIDE => "sdiv i32 ",
            OperationType.MODULO => "srem i32 ",
        };

        private string CompileFloatArithOperation(OperationType op) => op switch
        {
            OperationType.PLUS => "fadd double ",
            OperationType.MINUS => "fsub double ",
            OperationType.TIMES => "fmul double ",
            OperationType.DIVIDE => "fdiv double ",
            OperationType.MODULO => "frem double ",
        };

        private string CompileIntBoolOperation(OperationType op) => op switch
        {
            OperationType.EQUAL => "icmp eq i32 ",
            OperationType.GREATER_THAN => "icmp sgt i32 ",
            OperationType.GREATER_THAN_OR_EQUAL => "icmp sge i32 ",
            OperationType.LESS_THAN => "icmp slt i32",
            OperationType.LESS_THAN_OR_EQUAL => "icmp sle i32 "
        };

        private string CompileFloatBoolOperation(OperationType op) => op switch
        {
            OperationType.EQUAL => "fcmp oeq double ",
            OperationType.GREATER_THAN => "fcmp ogt double ",
            OperationType.GREATER_THAN_OR_EQUAL => "fcmp oge double ",
            OperationType.LESS_THAN => "fcmp olt double ",
            OperationType.LESS_THAN_OR_EQUAL => "fcmp ole double "
        };

        private KExp CPS (Exp e, Func<KVal, KExp> k)
        {
            switch (e)
            {
                case Var v:
                    return k(new KVar(v.S));
                case Num n:
                    return k(new KNum(n.I));
                case ArithmeticOperation aop:
                    var label = new Label("tmp");
                    return CPS(aop.A1, x =>
                            CPS(aop.A2, y =>
                                new KLet(label.l, new Kop(aop.O, x, y), k(new KVar(label.l)))));
                case If i:
                    Label l = new Label("tmp");
                    return CPS ()
            }
        }
    }
}
