using System;
using System.Linq;
using System.Collections.Generic;
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
        private Dictionary<string, VarType> typeEnv;

        public CodeGenerator()
        {
            labelCount = 0;
            typeEnv = new Dictionary<string, VarType>();

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

        private string CompileArithOperation(OperationType op, bool _float) => op switch
        {
            OperationType.PLUS => _float ? "fadd double " : "add i32 ",
            OperationType.MINUS => _float ? "fsub double " : "sub i32 ",
            OperationType.TIMES => _float ? "fmul double " : "mul i32 ",
            OperationType.DIVIDE => _float ? "fdiv double " : "sdiv i32 ",
            OperationType.MODULO => _float ? "frem double " : "srem i32 ",
            _ => CompileBoolOperation(op, _float) // check if the operator is perhaps a boolean
        };

        private string CompileBoolOperation(OperationType op, bool _float) => op switch
        {
            OperationType.EQUAL => _float ? "fcmp oeq double " : "icmp eq i32 ",
            OperationType.GREATER_THAN => _float ? "fcmp ogt double " : "icmp sgt i32 ",
            OperationType.GREATER_THAN_OR_EQUAL => _float ? "fcmp oge double " : "icmp sge i32 ",
            OperationType.LESS_THAN => _float ? "fcmp olt double " : "icmp slt i32",
            OperationType.LESS_THAN_OR_EQUAL => _float ? "fcmp ole double " : "icmp sle i32 ",
            _ => throw new UnexpectedArgumentException($"Unable to compile operator: {op}")
        };


        private VarType TypeKVal(KVal v) => v switch
        {
            KVar kv => typeEnv[kv.S],
            KNum => VarType.INT,
            KFNum => VarType.DOUBLE,
            Kop ko => TypeKVal(ko.V1),
            KWrite kw => TypeKVal(kw.V),
            KCall kc => typeEnv[kc.FuncName],
            _ => throw new UnexpectedArgumentException("Unable to define type for KVal " + v.ToString())
        };

        private  VarType TypeKExp(KExp e) => e switch
        {
            KLet kl => kl.
        }


        private KExp CPS(Exp e, Func<KVal, KExp> k)
        {
            switch (e)
            {
                case Var v:
                    // determine type of variable and create relevant VarType
                    return k(new KVar(v.S));
                case Num n:
                    return k(new KNum(n.I));
                case FNum f:
                    return k(new KFNum(f.I));
                case ArithmeticOperation aop:
                    var label = new Label("tmp");
                    return CPS(aop.A1, x =>
                            CPS(aop.A2, y =>
                                new KLet(label.l, new Kop(aop.O, x, y), k(new KVar(label.l)))));
                case If i:
                    Label l = new Label("tmp");
                    return CPS(i.E1, y1 =>
                        CPS(i.E2, y2 =>
                            new KLet(l.l, new Kop(i.Cond.Bop, y1, y2), new KIf(l.l, CPS(i.E1, k), CPS(i.E2, k)))));
                case Call c:
                    KExp aux (List<Exp> args, List<KVal> vs)
                    {
                        if (args.Count == 0)
                        {
                            Label l = new Label("tmp");
                            return new KLet(l.l, new KCall(c.S, vs), k(new KVar(l.l)));
                        }
                        else
                        {
                            Exp head = args.First();
                            List<Exp> tail = args.Skip(1).ToList();
                            return CPS(head, y => aux(tail, vs.Prepend(y).ToList()));
                        }
                    }
                    return aux(c.Args, new List<KVal>());
                case Sequence s:
                    return CPS(s.E1, _ => CPS(s.E2, y2 => k(y2)));
                default:
                    throw new UnexpectedArgumentException("Unimplemented KExp in CPS() function");
            }
        }
        public KExp CPSi(Exp e) => CPS(e, y => new KReturn(y));

        //Convenient string interpolations
        private static string Instruction() => throw new NotImplementedException();
        private static string Label_() => throw new NotImplementedException();
        private static string Method() => throw new NotImplementedException();

        private string CompileValue(KVal v)
        {
            switch (v)
            {
                case KNum n:
                    return $"{n.I}";
                case KVar kv:
                    return $"%{kv.S}";
                case Kop op:
                    return $"${CompileArithOperation()}"
            }
        }
    }
}
