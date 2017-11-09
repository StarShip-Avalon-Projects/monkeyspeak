﻿using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Extensions
{
    public static class ObjectExtensions
    {
        public static double AsDouble(this object obj, double @default = -1d)
        {
            if (obj == null) return @default;
            if (obj is double) return (double)obj;
            try
            {
                return Convert.ToDouble(obj);
            }
            catch (Exception ex) { Logger.Error(ex); return @default; }
        }

        public static string AsString(this object obj, string @default = null)
        {
            if (obj == null) return @default;
            if (obj is string) return (string)obj;
            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { Logger.Error(ex); return @default; }
        }
    }
}