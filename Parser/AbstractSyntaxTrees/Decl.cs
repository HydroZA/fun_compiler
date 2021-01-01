using System;
using System.Collections.Generic;

namespace Parser.AbstractSyntaxTrees
{
    public enum VarType
    {
        INT,
        DOUBLE,
        VOID,
        UNDEF
    }
    public abstract class Decl { }

    public class Def : Decl
    {
        public string Name { get; set; }
        public List<(string, string)> Args { get; set; }
        public VarType Type { get; set; }
        public Exp body { get; set; }

        public Def(string name, List<(string, string)> args, VarType type, Exp body)
        {
            Name = name;
            Args = args;
            Type = type;
            this.body = body;
        }
    }

    public class Main : Decl
    {
        public Exp Body { get; set; }

        public Main(Exp body)
        {
            Body = body;
        }
    }

    public class Const : Decl
    {
        public string Name { get; set; }
        public int V { get; set; }

        public Const(string name, int v)
        {
            Name = name;
            V = v;
        }
    }

    public class FConst : Decl
    {
        public string Name { get; set; }
        public float X { get; set; }

        public FConst(string name, float x)
        {
            Name = name;
            X = x;
        }
    }
}
