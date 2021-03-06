﻿using Monkeyspeak.Extensions;
using Monkeyspeak.Libraries;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Monkeyspeak.Utils
{
    public class ReflectionHelper
    {
        private static bool cached = false;

        public static Type[] GetAllTypesWithAttributeInMembers<T>(Assembly asm) where T : Attribute
        {
            return GetAllTypesInAssembly(asm).Where(type => type.GetMembers().Any(member => member.GetCustomAttribute<T>() != null)).ToArray();
        }

        public static IEnumerable<T> GetAllAttributesFromMethod<T>(MethodInfo methodInfo) where T : Attribute
        {
            var attributes = methodInfo.GetCustomAttributes(false);
            if (attributes != null && attributes.Length > 0)
                for (int k = 0; k <= attributes.Length - 1; k++)
                {
                    if (attributes[k] is T attr)
                        yield return attr;
                }
        }

        public static void SetPropertyValue<T>(T target, string desiredProperty, object value) where T : class
        {
            var targetType = target.GetType();
            var pi = targetType.GetProperty(desiredProperty, BindingFlags.Instance | BindingFlags.Public);
            if (pi == null || pi.CanWrite == false || pi.PropertyType != value.GetType()) return;
            if (pi.PropertyType != value.GetType()) return;
            try
            {
                pi.SetValue(target, value);
            }
            catch { }
        }

        public static object GetPropertyValue<T>(T target, string desiredProperty) where T : class
        {
            var targetType = target.GetType();
            var pi = targetType.GetProperty(desiredProperty, BindingFlags.Instance | BindingFlags.Public);
            if (pi == null || pi.CanRead == false) return null;
            try
            {
                return pi.GetValue(target);
            }
            catch { }
            return null;
        }

        public static IEnumerable<MethodInfo> GetAllMethods(Type type, params Type[] args)
        {
            MethodInfo[] methods = type.GetMethods();
            for (int j = 0; j <= methods.Length - 1; j++)
            {
                var @params = methods[j].GetParameters();
                if (args.Length != @params.Length) continue;
                bool paramsMatch = true;
                for (int i = @params.Length - 1; i >= 0; i--)
                {
                    if (@params[i].ParameterType != args[i])
                    {
                        paramsMatch = false;
                        break;
                    }
                }
                if (paramsMatch)
                    yield return methods[j];
            }
        }

        public static IEnumerable<Type> GetAllBaseTypes(Type type)
        {
            if (type.IsAbstract || type.IsInterface) yield break;
            var baseType = type;
            while (baseType != typeof(Object))
            {
                baseType = baseType.BaseType;
                yield return baseType;
            }
        }

        public static IEnumerable<Type> GetAllTypesWithBaseClass<T>(Assembly asm)
        {
            var desiredType = typeof(T);
            var types = new List<Type>();
            foreach (var type in GetAllTypesInAssembly(asm).Where(t => GetAllBaseTypes(t).Contains(desiredType)))
            {
                yield return type;
            }
        }

        public static bool HasInterface<T>(Type type)
        {
            var desiredType = typeof(T);
            if (desiredType.IsInterface)
            {
                return type.GetInterface(desiredType.Name, true) != null;
            }
            return false;
        }

        public static IEnumerable<Type> GetAllTypesWithInterface<T>(Assembly asm)
        {
            var desiredType = typeof(T);
            if (desiredType.IsInterface)
            {
                foreach (var type in GetAllTypesInAssembly(asm)
                    .Where(t => t.GetInterfaces().Contains(desiredType))) yield return type;
            }
        }

        /// <summary>
        /// Gets all types in assembly.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllTypesInAssembly(Assembly asm)
        {
            IEnumerable<Type> types = Enumerable.Empty<Type>();
            if (asm == null) return types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }
            return types;
        }

        /// <summary>
        /// Gets all assemblies.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            if (cached) return AppDomain.CurrentDomain.GetAssemblies();
            //all = new List<Assembly>();
            foreach (string asmFile in Directory.EnumerateFiles(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "*.*", SearchOption.TopDirectoryOnly)
                                        .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe")))
            {
                if (TryLoadAssemblyFromFile(asmFile, out var asm))
                {
                    try
                    {
                        AppDomain.CurrentDomain.Load(asm.GetName());
                    }
                    catch { }
                }
            }
            cached = true;
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Determines whether [the specified type] has a no-arg constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// <c>true</c> if [the specified type] has a no-arg constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasNoArgConstructor(Type type)
        {
            return type.GetConstructors().FirstOrDefault(cnstr => cnstr.GetParameters().Length == 0) != null;
        }

        /// <summary>
        /// Tries the load assembly from file.
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <param name="asm">         The asm.</param>
        /// <returns></returns>
        public static bool TryLoadAssemblyFromFile(string assemblyFilePath, out Assembly asm)
        {
            try
            {
                asm = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFilePath));
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch
#endif
            {
                asm = null;
                return false;
            }
        }

        /// <summary>
        /// Tries the load assembly.
        /// </summary>
        /// <param name="assemblyName">The assembly string.</param>
        /// <param name="asm">         The asm.</param>
        /// <returns></returns>
        public static bool TryLoadAssemblyFromName(AssemblyName assemblyName, out Assembly asm)
        {
            try
            {
                asm = Assembly.Load(assemblyName);
                return true;
            }
#if DEBUG
            catch (Exception ex)
#else
            catch
#endif
            {
                asm = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to create the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="obj"> The object.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static bool TryCreate<T>(Type type, out T obj, params object[] args)
        {
            if (!type.IsAbstract && !type.IsInterface)
            {
                try
                {
                    if (args == null || args.Length == 0)
                        obj = (T)Activator.CreateInstance(type);
                    else obj = (T)Activator.CreateInstance(type, args);
                    return obj != null;
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }
            obj = default(T);
            return false;
        }

        /// <summary>
        /// Tries to create the specfied type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"> The object.</param>
        /// <param name="args">The arguments.</param>
        /// <returns><c>true</c> if type was created; otherwise <c>false</c></returns>
        public static bool TryCreate<T>(out T obj, params object[] args)
        {
            if (TryCreate(typeof(T), out object result, args))
            {
                obj = (T)result;
                return true;
            }
            obj = default(T);
            return false;
        }

        /// <summary>
        /// Creates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static object Create(Type type, params object[] args)
        {
            if (!type.IsAbstract && !type.IsInterface)
            {
                if (args == null || args.Length == 0)
                    return Activator.CreateInstance(type);
                else return Activator.CreateInstance(type, args);
            }
            return null;
        }
    }
}