using System.Collections.Generic;

// TODO: Challenge 12.1 - Define static methods
// TODO: Challenge 12.2 - Implement "getters", properties that actually run code
namespace lox.net
{
    public class LoxClass : ILoxCallable
    {
        public readonly LoxClass superclass;
        public readonly string name;

        private Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods)
        {
            this.superclass = superclass;
            this.name = name;
            this.methods = methods;
        }

        public LoxFunction FindMethod(string name)
        {
            if (methods.TryGetValue(name, out LoxFunction func))
            {
                return func;
            }

            if (superclass != null)
            {
                return superclass.FindMethod(name);
            }

            return null;
        }

        public override string ToString()
        {
            return name;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = FindMethod("init");
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public int Arity()
        {
            LoxFunction initializer = FindMethod("init");
            if (initializer == null)
            {
                return 0;
            }

            return initializer.Arity();
        }
    }
}
