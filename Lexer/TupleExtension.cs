using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public static class TupleExtension
    {
        public static List<(string, string)> ToList(this (string, string) x) => new List<(string, string)>() { x };
    }
}
