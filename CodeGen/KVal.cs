using System;
using System.Collections.Generic;

namespace CodeGen
{
    public abstract class KVal { }

    public class KVar : KVal
    {
        public string S { get; set; }
        public VarType Type { get; set; }

        public KVar(string s, VarType type = VarType.UNDEF)
        {
            S = s;
            Type = type;
        }
    }

    public class KNum : KVal
    {
        public int I { get; set; }

        public KNum(int i)
        {
            I = i;
        }
    }

    public class Kop : KVal
    {
        public string Op { get; set; }
        public KVal V1 { get; set; }
        public KVal V2 { get; set; }

        public Kop(string op, KVal v1, KVal v2)
        {
            Op = op;
            V1 = v1;
            V2 = v2;
        }
    }

    public class KCall : KVal
    {
        public string FuncName { get; set; }
        public List<KVal> Args { get; set; }

        public KCall(string funcName, List<KVal> args)
        {
            FuncName = funcName;
            Args = args;
        }
    }

    public class KWrite : KVal
    {
        public KVal V { get; set; }

        public KWrite(KVal v)
        {
            V = v;
        }
    }
}
