﻿using System;
using System.Collections.Generic;

namespace lox.net
{
    public class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public object Get(Token name)
        {
            if (fields.TryGetValue(name.lexeme, out object value))
            {
                return value;
            }

            LoxFunction method = klass.FindMethod(name.lexeme);
            if (method != null)
            {
                return method.Bind(this);
            }

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        public void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }

        public override string ToString()
        {
            return klass.name + " instance";
        }
    }
}
