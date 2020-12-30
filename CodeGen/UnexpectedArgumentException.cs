using System;

namespace CodeGen
{
    class UnexpectedArgumentException : Exception
    {
        public UnexpectedArgumentException() : base() { }
        public UnexpectedArgumentException(string msg) : base(msg) { }
    }
}
