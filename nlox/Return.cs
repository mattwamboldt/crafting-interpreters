﻿using System;

namespace lox.net
{
    public class Return : Exception
    {
        public readonly object value;

        public Return(object value)
        {
            this.value = value;
        }
    }
}
