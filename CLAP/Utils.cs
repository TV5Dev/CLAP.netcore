﻿using System;
using System.Collections.Generic;
using System.Reflection;
#if !FW2
using System.Linq;
#endif

namespace CLAP
{
    internal static class Utils
    {
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static T GetAttribute<T>(this MethodInfo method) where T : Attribute
        {
            return method.GetCustomAttribute<T>();
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttribute<T>();
        }

        public static T GetAttribute<T>(this ParameterInfo parameter) where T : Attribute
        {
            return parameter.GetCustomAttribute<T>();
        }

        public static IEnumerable<T> GetAttributes<T>(this ParameterInfo parameter) where T : Attribute
        {
            return parameter.GetCustomAttributes<T>();
        }

        public static IEnumerable<T> GetAttributes<T>(this PropertyInfo property) where T : Attribute
        {
            return property.GetCustomAttributes<T>();
        }

        public static IEnumerable<T> GetInterfaceAttributes<T>(this MethodInfo method)
        {
            return method.GetCustomAttributes(true).
                Where(a => a.GetType().GetTypeInfo().GetInterfaces().Contains(typeof(T))).
                Cast<T>();
        }

        public static IEnumerable<T> GetAttributes<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>();
        }

        public static bool HasAttribute<T>(this MethodInfo method) where T : Attribute
        {
            return method.IsDefined(typeof(T));
        }

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().IsDefined(typeof(T));
        }

        public static bool HasAttribute<T>(this ParameterInfo parameter) where T : Attribute
        {
            return parameter.IsDefined(typeof(T));
        }

        public static IEnumerable<MethodInfo> GetMethodsWith<T>(this Type type) where T : Attribute
        {
            var methods = GetAllMethods(type).
                Where(m => m.HasAttribute<T>());

            return methods;
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            var methods = type.GetTypeInfo().GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);

            return methods;
        }

        public static bool None<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        public static string StringJoin(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings.ToArray());
        }

        public static IEnumerable<string> SplitBy(this string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<string> CommaSplit(this string str)
        {
            return SplitBy(str, ",");
        }

        public static void Each<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            var index = 0;

            foreach (var item in collection)
            {
                action(item, index);

                index++;
            }
        }

        public static string ToSafeString(this object obj, string nullValue)
        {
            return obj == null ? nullValue : obj.ToString();
        }

        public static bool StartsWith(this string str, IEnumerable<string> values)
        {
            return values.Any(v => str.StartsWith(v));
        }

        public static bool Contains(this string str, IEnumerable<string> values)
        {
            return values.Any(v => str.Contains(v));
        }

        public static string GetGenericTypeName(this Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                return type.Name;
            }

            var genericTypeName = type.GetGenericTypeDefinition().Name;

            genericTypeName = genericTypeName.Remove(genericTypeName.IndexOf('`'));

            var genericArgs = type.GetTypeInfo().GetGenericArguments().
                Select(a => GetGenericTypeName(a)).
                StringJoin(",");

            return "{0}<{1}>".FormatWith(genericTypeName, genericArgs);
        }
    }
}