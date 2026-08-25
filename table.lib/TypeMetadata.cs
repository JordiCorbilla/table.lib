using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace table.lib
{
    internal sealed class PropertyAccessor<T>
    {
        public PropertyAccessor(PropertyInfo property)
        {
            Name = property.Name;
            Property = property;

            var instance = Expression.Parameter(typeof(T), "instance");
            var access = Expression.Property(instance, property);
            var box = Expression.Convert(access, typeof(object));
            GetValue = Expression.Lambda<Func<T, object>>(box, instance).Compile();
        }

        public string Name { get; }
        public PropertyInfo Property { get; }
        public Func<T, object> GetValue { get; }
    }

    internal static class TypeMetadata<T>
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> RuntimePropertyCache = new();

        static TypeMetadata()
        {
            Properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .Select(property => new PropertyAccessor<T>(property))
                .ToArray();

            PropertiesByName = Properties.ToDictionary(property => property.Name, StringComparer.Ordinal);
        }

        public static IReadOnlyList<PropertyAccessor<T>> Properties { get; }
        public static IReadOnlyDictionary<string, PropertyAccessor<T>> PropertiesByName { get; }

        public static PropertyInfo[] GetRuntimeProperties(Type type)
        {
            return RuntimePropertyCache.GetOrAdd(type,
                runtimeType => runtimeType.GetProperties(BindingFlags.Instance | BindingFlags.Public));
        }
    }
}
