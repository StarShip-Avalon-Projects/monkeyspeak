﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak
{
    public sealed class ConstantVariable : IVariable
    {
        public string Name { get; private set; }

        private object value;

        public ConstantVariable(string name, object value)
        {
            Name = name;
            this.value = value;
        }

        public ConstantVariable(IVariable variable)
        {
            Name = variable.Name;
            this.value = variable.Value;
        }

        /// <summary>
        /// Adds to the page, replacing any variable of the same name and making it constant.
        /// </summary>
        /// <param name="page">The page.</param>
        public void AddToPage(Page page)
        {
            page.RemoveVariable(Name);
            page.SetVariable(this);
        }

        public object Value
        {
            get { return value; }
            set
            {
                throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");
            }
        }

        public bool IsConstant => true;

        /// <summary>
        /// Bypasses the modification check and sets the value for this instance. This is the
        /// recommended way to force set the value for a constant variable.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue(object value)
        {
            this.value = value;
        }

        public bool Equals(IVariable other)
        {
            return this.Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase) && this.Value.Equals(other.Value);
        }

        /// <summary>
        /// Returns a const identifier if the variable is constant followed by name,
        /// <para>otherwise just the name is returned.</para>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ((IsConstant) ? "const " : "") + $"{Name} = {value ?? "null"}";
        }
    }
}