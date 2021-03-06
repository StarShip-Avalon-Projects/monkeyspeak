﻿using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Monkeyspeak.Libraries
{
    public abstract class BaseLibrary
    {
        protected Dictionary<Trigger, TriggerHandler> handlers;
        protected Dictionary<Trigger, string> descriptions;

        public Dictionary<Trigger, TriggerHandler> Handlers { get => handlers; protected set => handlers = value; }

        /// <summary>
        /// Base abstract class for Monkeyspeak Libraries
        /// </summary>
        protected BaseLibrary()
        {
            handlers = new Dictionary<Trigger, TriggerHandler>();
            descriptions = new Dictionary<Trigger, string>();
        }

        /// <summary>
        /// Initializes this instance. Add your trigger handlers here.
        /// </summary>
        /// <param name="args">
        /// Parametized argument of objects to use to pass runtime objects to a library at initialization
        /// </param>
        public abstract void Initialize(params object[] args);

        /// <summary>
        /// Raises a MonkeyspeakException
        /// </summary>
        /// <param name="reason">Reason for the error</param>
        public virtual void RaiseError(string reason)
        {
            throw new MonkeyspeakException(reason);
        }

        /// <summary>
        /// Registers a Trigger to the TriggerHandler with optional description
        /// </summary>
        /// <param name="cat">        </param>
        /// <param name="id">         </param>
        /// <param name="handler">    </param>
        /// <param name="description"></param>
        public virtual void Add(TriggerCategory cat, int id, TriggerHandler handler, string description = null)
        {
            Trigger trigger = new Trigger(cat, id);
            if (!handlers.ContainsKey(trigger) || handler.Method.DeclaringType == GetType())
            {
                if (description != null && !descriptions.ContainsKey(trigger)) descriptions.Add(trigger, description);
                handlers.Add(trigger, handler);
            }
            else throw new UnauthorizedAccessException($"Override of existing Trigger {trigger}'s handler with handler in {handler.Method.DeclaringType.Name}.");
        }

        /// <summary>
        /// Registers a Trigger to the TriggerHandler with optional description
        /// </summary>
        /// <param name="cat">        </param>
        /// <param name="id">         </param>
        /// <param name="handler">    </param>
        /// <param name="description"></param>
        public virtual void Add(TriggerCategory cat, int id, MethodInfo handler, string description = null)
        {
            Trigger trigger = new Trigger(cat, id);
            if (!handlers.ContainsKey(trigger) || handler.DeclaringType == GetType())
            {
                if (description != null && !descriptions.ContainsKey(trigger)) descriptions.Add(trigger, description);
                handlers.Add(trigger, handler.CreateDelegate(typeof(TriggerHandler)) as TriggerHandler);
            }
            else throw new UnauthorizedAccessException($"Override of existing Trigger {trigger}'s handler with handler in {handler.DeclaringType.Name}.");
        }

        public virtual bool Contains(TriggerCategory cat, int id)
        {
            foreach (var desc in descriptions)
            {
                if (desc.Key.Category == cat && desc.Key.Id == id)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Called when page is disposing or resetting.
        /// </summary>
        /// <param name="page">The page.</param>
        public abstract void Unload(Page page);

        /// <summary>
        /// Builds a string representation of the descriptions of <paramref name="trigger"/>.
        /// </summary>
        /// <returns></returns>
        public string ToString(Trigger trigger, bool excludeDescriptions = false, bool includeSourcePosition = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(trigger);
            descriptions.TryGetValue(trigger, out string value);
            sb.Append(' ').Append(!excludeDescriptions ? value ?? string.Empty : string.Empty);
            if (includeSourcePosition) sb.Append(' ').Append(trigger.SourcePosition);
            return sb.ToString();
        }

        /// <summary>
        /// Builds a string representation of the descriptions of each trigger.
        /// </summary>
        /// <returns></returns>
        public string ToString(bool excludeLibraryName = false, bool excludeDescriptions = false)
        {
            StringBuilder sb = new StringBuilder();
            if (!excludeLibraryName) sb.AppendLine(GetType().Name);
            foreach (var kv in descriptions.OrderBy(kv => kv.Key.Category))
            {
                sb.Append(kv.Key).Append(' ').Append(!excludeDescriptions ? kv.Value ?? string.Empty : string.Empty).Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Builds a string representation of the descriptions of each trigger.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetType().Name);
            foreach (var kv in descriptions)
            {
                sb.AppendLine(kv.Value);
            }
            return sb.ToString();
        }

        public static IEnumerable<BaseLibrary> GetAllLibraries()
        {
            foreach (var asm in ReflectionHelper.GetAllAssemblies())
            {
                foreach (var lib in GetLibrariesFromAssembly(asm))
                {
                    yield return lib;
                }
            }
        }

        /// <summary>
        /// Loads trigger handlers from a assembly instance
        /// </summary>
        /// <param name="asm">The assembly instance</param>
        public static IEnumerable<BaseLibrary> GetLibrariesFromAssembly(Assembly asm)
        {
            if (asm == null) yield break;

            foreach (var type in ReflectionHelper.GetAllTypesWithBaseClass<BaseLibrary>(asm))
            {
                if (ReflectionHelper.HasNoArgConstructor(type) && ReflectionHelper.TryCreate<BaseLibrary>(type, out var lib))
                    yield return lib;
                else Logger.Debug<BaseLibrary>($"Failed to create {type.Name}, ensure it is public and has a no-arg constructor");
            }
        }
    }

    internal class BaseLibraryComparator : IEqualityComparer<BaseLibrary>
    {
        public bool Equals(BaseLibrary x, BaseLibrary y)
        {
            if (x == null)
                return y == null;

            if (y == null)
                return false;

            return x.GetType() == y.GetType();
        }

        public int GetHashCode(BaseLibrary obj)
        {
            return obj.GetHashCode();
        }
    }
}