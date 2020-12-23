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

    public static class HashSetExtension
    {
        public static string ToListString(this HashSet<char> x)
        {
            return string.Join("", x.SelectMany(x => x + ","));
        }
    }
}
