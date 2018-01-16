﻿using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    internal class TriggerExpression : Expression<Trigger>
    {
        public TriggerExpression(SourcePosition pos, string value) : base(pos, ParseToTrigger(value, pos))
        {
        }

        private static Trigger ParseToTrigger(string value, SourcePosition pos)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Trigger.Undefined;
            }
            var indexOfColon = value.IndexOf(':');
            if (indexOfColon == -1) return Trigger.Undefined;
            var cat = value.Substring(0, indexOfColon);
            if (string.IsNullOrWhiteSpace(cat))
            {
                return Trigger.Undefined;
            }
            var id = value.Substring(indexOfColon + 1);
            if (string.IsNullOrWhiteSpace(id))
            {
                return Trigger.Undefined;
            }
            return new Trigger((TriggerCategory)OtherUtils.IntParse(cat),
                OtherUtils.IntParse(id), pos);
        }

        /// <summary>
        /// Applies the specified previous trigger.
        /// </summary>
        /// <param name="prevTrigger">The previous trigger.</param>
        public override void Apply(Trigger? prevTrigger)
        {
            // not needed
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            return null;
        }
    }
}