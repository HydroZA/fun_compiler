using System;
namespace Parser.AbstractSyntaxTrees
{

    public abstract class Bexp { }

    public class BooleanOperation : Bexp
    {
        public OperationType Bop { get; set; }
        public Exp A1 { get; set; }
        public Exp A2 { get; set; }

        public BooleanOperation(OperationType bop, Exp a1, Exp a2)
        {
            Bop = bop;
            A1 = a1;
            A2 = a2;
        }
    }
}
