using System;
using System.Collections.Generic;

namespace Parser.AbstractSyntaxTrees
{
    public enum OperationType
    {
        PLUS,
        MINUS,
        TIMES,
        DIVIDE,
        MODULO,
        LESS_THAN,
        GREATER_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN_OR_EQUAL,
        NOT_EQUAL,
        EQUAL
    }

    public abstract class Exp
    {

    }

    public class Call : Exp
    {
        public string S { get; set; }
        public List<Exp> Arguments { get; set; }

        public Call(string s, List<Exp> args)
        {
            this.S = s;
            this.Arguments = args;
        }
    }

    public class If : Exp
    {
        public Operation Cond { get; set; }
        public Exp E1 { get; set; }
        public Exp E2 { get; set; }

        public If(Operation cond, Exp e1, Exp e2)
        {
            this.Cond = cond;
            this.E1 = e1;
            this.E2 = e2;
        }
    }

    public class Var : Exp
    {
        public string S { get; set; }


        public Var(string s)
        {
            S = s;
        }
    }

    public class Num : Exp
    {
        public int I { get; set; }

        public Num(int i)
        {
            I = i;
        }
    }

    public class FNum : Exp
    {
        public float I { get; set; }

        public FNum(float i)
        {
            I = i;
        }
    }

    public class Operation : Exp
    {
        public OperationType O { get; set; }
        public Exp A1 { get; set; }
        public Exp A2 { get; set; }

        public Operation(OperationType o, Exp a1, Exp a2)
        {
            O = o;
            A1 = a1;
            A2 = a2;
        }
    }

    public class Sequence : Exp
    {
        public Exp E1 { get; set; }
        public Exp E2 { get; set; }

        public Sequence(Exp e1, Exp e2)
        {
            E1 = e1;
            E2 = e2;
        }
    }

}
