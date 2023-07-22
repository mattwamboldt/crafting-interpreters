using System.Collections.Generic;

namespace lox.net
{
    internal interface ILoxCallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
    }
}
