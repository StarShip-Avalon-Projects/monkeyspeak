﻿namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class NumberExpression : Expression<double>
    {
        public NumberExpression(ref SourcePosition pos, double num) : base(ref pos, num)
        {
        }
    }
}