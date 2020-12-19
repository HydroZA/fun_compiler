using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public abstract class Val { }
    public sealed class Empty : Val 
    { 
        public Empty()
        {

        }
    }
    public sealed class Chr : Val
    {
        public char c;
        public Chr(char c)
        {
            this.c = c;
        }
    }
    public sealed class Sequ : Val 
    {
        public Val v1, v2;
        public Sequ(Val v1, Val v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
    }
    public sealed class Left : Val
    {
        public Val v;
        public Left (Val v)
        {
            this.v = v;
        }
    }
    public sealed class Right : Val
    {
        public Val v;
        public Right(Val v)
        {
            this.v = v;
        }
    }
    public sealed class Stars : Val
    {
        public List<Val> vs;
        public Stars(List<Val> vs)
        {
            this.vs = vs;
        }
    }
    public sealed class Rec : Val
    {
        public TokenType x;
        public Val v;

        public Rec(TokenType x, Val v)
        {
            this.x = x;
            this.v = v;
        }
    }
    static class ValExtension
    {
        public static List<Val> ToList(this Val x)
        {
            return new List<Val>() { x };
        }
    }
}
