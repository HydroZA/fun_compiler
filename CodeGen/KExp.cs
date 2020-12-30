using System;
using System.Text.RegularExpressions;

namespace CodeGen
{
    public abstract class KExp
    { }

    public class KLet : KExp
    {
        public string VarName { get; set; }
        public KVal E1 { get; set; }
        public KExp E2 { get; set; }

        public override string ToString()
        {
            return $"LET {VarName} = {E1} in \n{E2}";
        }

        public KLet(string varName, KVal e1, KExp e2)
        {
            VarName = varName;
            E1 = e1;
            E2 = e2;
        }
    }

    public class KIf : KExp
    {
        public string Cond { get; set; }
        public KExp E1 { get; set; }
        public KExp E2 { get; set; }

        private static string Pad(KExp e) => Regex.Replace(e.ToString(), @"(?m)^", " ");

        public override string ToString()
        {
            return $"IF {Cond}\n" +
                $"THEN\n" +
                $"{Pad(E1)}\n" +
                $"ELSE\n" +
                $"{Pad(E2)}";
        }

        public KIf(string cond, KExp e1, KExp e2)
        {
            Cond = cond;
            E1 = e1;
            E2 = e2;
        }
    }

    public class KReturn : KExp
    {
        public KVal Val { get; set; }

        public KReturn(KVal v)
        {
            Val = v;
        }
        public KReturn()
        {

        }
    }
}
