﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapestry.Expressions
{
    public sealed class NullExpression : Expression<object>
    {
        private static NullExpression instance;

        static NullExpression()
        {
            var sourcePos = new SourcePosition();
            instance = new NullExpression(ref sourcePos);
        }

        public static NullExpression Instance => instance;

        protected NullExpression(ref SourcePosition pos) : base(ref pos) => Value = null;
    }
}