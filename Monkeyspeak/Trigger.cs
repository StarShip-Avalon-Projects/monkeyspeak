﻿using Monkeyspeak.Lexical;
using Monkeyspeak.Lexical.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Monkeyspeak
{
    [Serializable]
    public enum TriggerCategory : int
    {
        /// <summary>
        /// A trigger that was not defined. You should never encounter this if you do then something
        /// isn't quite right.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A trigger defined with a 0
        /// <para>Example: (0:1) when someone says something,</para>
        /// </summary>
        Cause = 0,

        /// <summary>
        /// A trigger defined with a 1
        /// <para>Example: (1:2) and they moved # units left,</para>
        /// </summary>
        Condition = 1,

        /// <summary>
        /// A trigger defined with a 5
        /// <para>Example: (5:1) print {Hello World} to the console.</para>
        /// </summary>
        Effect = 5,

        /// <summary> A trigger defined with a 6 <para>Example: (6:0) while variable % is #, </para>
        Flow = 6
    }

    [StructLayout(LayoutKind.Auto)]
    [Serializable]
    public struct Trigger : IEquatable<Trigger>
    {
        public static readonly Trigger Undefined = new Trigger(TriggerCategory.Cause, -1);

        private TriggerCategory category;

        private int id;

        internal List<IExpression> contents;

        internal Trigger(TriggerCategory cat, int id = -1)
        {
            category = cat;
            this.id = id;
            SourcePosition = new SourcePosition();
            contents = new List<IExpression>();
        }

        internal Trigger(TriggerCategory cat, int id, SourcePosition sourcePos)
        {
            category = cat;
            this.id = id;
            SourcePosition = sourcePos;
            contents = new List<IExpression>();
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        public TriggerCategory Category
        {
            get { return category; }
            internal set { category = value; }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id
        {
            get { return id; }
            internal set { id = value; }
        }

        public IReadOnlyCollection<IExpression> Contents
        {
            get { return contents.AsReadOnly(); }
            set { contents.AddRange(value); }
        }

        public SourcePosition SourcePosition { get; set; }

        public void Add(IExpression expr)
        {
            contents.Add(expr);
        }

        public void Remove(IExpression expr)
        {
            for (int i = contents.Count - 1; i >= 0; i--)
            {
                var existing = contents[i];
                if (existing == expr)
                {
                    contents.RemoveAt(i);
                    break;
                }
            }
        }

        public Trigger Clone()
        {
            var clone = new Trigger(category, id, SourcePosition)
            {
                contents = new List<IExpression>(contents)
            };
            return clone;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Trigger a, Trigger b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Trigger a, Trigger b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Trigger other)
        {
            if (other == null) return false;
            return other.category == category && other.id == id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Trigger)
            {
                var other = (Trigger)obj;
                return other != Undefined && other.category == category && other.id == id;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return category.GetHashCode() + id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="engine">          The engine.</param>
        /// <param name="includeSourcePos">if set to <c>true</c> [include source position].</param>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToString(MonkeyspeakEngine engine, bool includeSourcePos = false)
        {
            return RebuildToString(engine.Options, includeSourcePos);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => $"({(int)category}:{id})";

        public string RebuildToString(Options options, bool includeSourcePos = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"({(int)category}:{id})");
            if (includeSourcePos) sb.Append(' ').Append(SourcePosition);
            if (contents != null && contents.Count > 0)
            {
                sb.Append(' ');
                for (int i = 0; i <= contents.Count - 1; i++)
                {
                    var expr = contents[i];
                    var tokenType = Expressions.GetTokenTypeFor(contents[i].GetType());
                    if (tokenType == null) continue;
                    sb.Append('\"');
                    switch (tokenType)
                    {
                        case TokenType.STRING_LITERAL:
                            sb.Append(options.StringBeginSymbol).Append(expr.ToString()).Append(options.StringEndSymbol);
                            break;

                        default:
                            sb.Append(expr.ToString());
                            break;
                    }
                    sb.Append('\"');
                    if (includeSourcePos) sb.Append(' ').Append(expr.Position);
                    if (i != contents.Count - 1) sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        public static Trigger Parse(MonkeyspeakEngine engine, string str)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            try
            {
                Parser parser = new Parser(engine);
                Lexer lexer = new Lexer(engine, new SStreamReader(stream));
                foreach (var trigger in parser.Parse(lexer))
                {
                    if (trigger != Undefined) return trigger;
                }
            }
            catch { }
            return Undefined;
        }

        public static bool TryParse(MonkeyspeakEngine engine, string str, out Trigger trigger)
        {
            trigger = Undefined;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            try
            {
                Parser parser = new Parser(engine);
                Lexer lexer = new Lexer(engine, new SStreamReader(stream));
                foreach (var t in parser.Parse(lexer))
                {
                    if (t != Undefined)
                    {
                        trigger = t;
                        break;
                    }
                }
                if (trigger != Undefined) return true;
            }
            catch { }
            trigger = Undefined;
            return false;
        }
    }
}