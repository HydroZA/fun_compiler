using System;
namespace Parser.AbstractSyntaxTrees
{
    public enum BoolOperationType
    {
        LESS_THAN,
        GREATER_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN_OR_EQUAL,
        NOT_EQUAL,
        EQUAL
    }

    public abstract class Bexp { }

    public class BooleanOperation : Bexp
    {
        public BoolOperationType Bop { get; set; }
        public Exp A1 { get; set; }
        public Exp A2 { get; set; }

        public BooleanOperation(BoolOperationType bop, Exp a1, Exp a2)
        {
            Bop = bop;
            A1 = a1;
            A2 = a2;
        }
    }
}
