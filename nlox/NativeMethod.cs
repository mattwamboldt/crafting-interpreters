using System;
using System.Collections.Generic;

namespace lox.net
{
    // Note: This is a C# specific deviation from the book as we can't declare
    // anonymous classes in the same way as Java, so using delegates instead
    internal class NativeMethod : ILoxCallable
    {
        private readonly int arity;
        private readonly Func<Interpreter, List<object>, object> callback;

        public NativeMethod(int arity, Func<Interpreter, List<object>, object> callback)
        {
            this.arity = arity;
            this.callback = callback;
        }

        public int Arity()
        {
            return arity;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return callback(interpreter, arguments);
        }
    }
}
