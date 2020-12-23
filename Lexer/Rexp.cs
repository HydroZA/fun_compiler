using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Lexer
{

    public abstract class Rexp 
    {
        public static Rexp StringToRexp(string s) 
        {
            Rexp r;
            if (s.Length == 0)
                r = new ONE();
            else if (s.Length == 1)
            {
                char c = s[0];
                r = new CHAR(c);
            }
            else
            {
                char c = s[0];
                s = s.Substring(1);
                r = new SEQ(new CHAR(c), StringToRexp(s));
            }
            return r;
        }
        public static implicit operator Rexp (string s) => StringToRexp(s);

        public override bool Equals(object r) => ToString() == r.ToString();

        public override string ToString()
        {
            return PrintPretty(this);
        }

        public static string PrintPretty(Rexp r) => r switch
        {
            ZERO => "ZERO()",
            ONE => "ONE()",
            CHAR c => $"CHAR({c.c})",
            ALT a => $"ALT({PrintPretty(a.r1)}, {PrintPretty(a.r2)})",
            SEQ s => $"SEQ({PrintPretty(s.r1)}, {PrintPretty(s.r2)})",
            STAR s => $"STAR({PrintPretty(s.r)})",
            PLUS p => $"PLUS({PrintPretty(p.r)})",
            RANGE ra => $"RANGE({ra.s})",
            BETWEEN b => $"BETWEEN({PrintPretty(b.r)}, {b.n}, {b.m})",
            NTIMES n => $"NTIMES({PrintPretty(n.r)}, {n.n})",
            OPTIONAL o => $"OPTIONAL({PrintPretty(o.r)})",
            UPTO u => $"UPTO({PrintPretty(u.r)}, {u.m})",
            FROM f => $"FROM({PrintPretty(f.r)}, {f.n})",
            NOT n => $"NOT({PrintPretty(n.r)})",
            ALL => "ALL()",
            RECD rc => $"RECD({PrintPretty(rc.r)})"
        };
    }
    public sealed class ZERO : Rexp
    {
        public ZERO() { }
    }
    public sealed class ONE : Rexp
    {
        public ONE() { }
    }
    public sealed class CHAR : Rexp
    {
        public readonly char c;
        public CHAR(char c) { this.c = c; }
    }
    public sealed class ALT : Rexp
    {
        public readonly Rexp r1, r2;
        public ALT(Rexp r1, Rexp r2)
        {
            this.r1 = r1;
            this.r2 = r2;
        }
    }
    public sealed class SEQ : Rexp
    {
        public readonly Rexp r1, r2;
        public SEQ(Rexp r1, Rexp r2)
        {
            this.r1 = r1;
            this.r2 = r2;
        }
    }
    public sealed class STAR : Rexp
    {
        public readonly Rexp r;
        public STAR(Rexp r) { this.r = r; }
    }   

    public sealed class NTIMES : Rexp
    {
        public readonly Rexp r;
        public int n;
        
        public NTIMES(Rexp r, int n)
        {
            this.r = r;
            this.n = n;
        }
    }
    public sealed class RANGE : Rexp
    {
        public HashSet<char> s;
        public RANGE(HashSet<char> s)
        {
            this.s = s;
        }
    }
    public sealed class PLUS : Rexp
    {
        public Rexp r;
        public PLUS(Rexp r)
        {
            this.r = r;
        }
    }
    public sealed class OPTIONAL : Rexp
    {
        public Rexp r;
        public OPTIONAL (Rexp r)
        {
            this.r = r;
        }
    }
    public sealed class UPTO : Rexp
    {
        public int m;
        public readonly Rexp r;
        public UPTO(Rexp r, int m)
        {
            this.m = m;
            this.r = r;
        }
    }
    public sealed class FROM : Rexp
    {
        public int n;
        public readonly Rexp r;
        public FROM(Rexp r, int n) 
        {
            this.r = r;
            this.n = n; 
        }
    }
    public sealed class BETWEEN : Rexp
    {
        public int n, m;
        public readonly Rexp r;
        public BETWEEN (Rexp r, int n, int m)
        {
            this.r = r;
            this.n = n;
            this.m = m;
        }
    }
    public sealed class NOT : Rexp
    {
        public readonly Rexp r;
        public NOT (Rexp r)
        {
            this.r = r;
        }
    }

    
    public sealed class CFUN : Rexp
    {
        public HashSet<char> hs;

        public CFUN(HashSet<char> hs)
        {
            this.hs = hs;
        }

        /* The following constructor is used by ALL so that 'hs' becomes null and we 
         * can always return true in function f.
         * I think this could be done using HOFs
         * but I couldn't come up with an elegant way of doing it 
        */
        public CFUN()
        {

        }

        public bool f (char x)
        {
            if (hs != null)
                return hs.Contains(x);
            else
                return true;
        }
    }

    public sealed class ALL : Rexp { }

    public sealed class RECD : Rexp
    {
        public TokenType x;
        public Rexp r;

        public RECD(TokenType x, Rexp r)
        {
            this.x = x;
            this.r = r;
        }
    }
}
