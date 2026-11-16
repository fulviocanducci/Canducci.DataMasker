using Canducci.DataMasker.Attributes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Canducci.DataMasker.Utils
{
    internal static class MaskConvert<T> where T : class
    {
        private sealed class PropertyMetadata
        {
            internal PropertyMetadata(PropertyInfo property)
            {
                Property = property;
                Attribute = new Lazy<MaskAttribute>(() =>
                {
                    try
                    {
                        return property.GetCustomAttribute<MaskAttribute>();
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(
                            $"The mask configuration for property '{property.Name}' is invalid.",
                            exception);
                    }
                });
            }

            internal PropertyInfo Property { get; }

            internal Lazy<MaskAttribute> Attribute { get; }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private static readonly ConcurrentDictionary<Type, PropertyMetadata[]> PropertyCache =
            new ConcurrentDictionary<Type, PropertyMetadata[]>();

        internal static T Execute(T source, Func<T, T> factory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var result = factory(source);

            if (result == null)
            {
                throw new InvalidOperationException("The factory returned null.");
            }

            if (ReferenceEquals(source, result))
            {
                throw new InvalidOperationException(
                    "The factory must return a new instance. Returning the source instance would modify the original object.");
            }

            var visited = new Dictionary<object, object>(new ReferenceComparer());
            visited.Add(source, result);

            CopyObject(source, result, visited);
            return result;
        }

        internal static T Execute(T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var result = Activator.CreateInstance(typeof(T)) as T;

            if (result == null)
            {
                throw new InvalidOperationException($"Unable to create an instance of {typeof(T).FullName}.");
            }

            var visited = new Dictionary<object, object>(new ReferenceComparer());
            visited.Add(source, result);

            CopyObject(source, result, visited);
            return result;
        }

        private static PropertyMetadata[] GetProperties(Type type)
        {
            return PropertyCache.GetOrAdd(type, currentType =>
                currentType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(property =>
                        property.GetGetMethod(false) != null &&
                        property.GetSetMethod(false) != null &&
                        property.GetIndexParameters().Length == 0)
                    .Select(property => new PropertyMetadata(property))
                    .ToArray());
        }

        private static void CopyObject(
            object source,
            object result,
            IDictionary<object, object> visited)
        {
            foreach (var metadata in GetProperties(source.GetType()))
            {
                var property = metadata.Property;
                var value = property.GetValue(source);
                var attribute = metadata.Attribute.Value;

                if (value == null)
                {
                    property.SetValue(result, null);
                    continue;
                }

                if (value is string text)
                {
                    property.SetValue(
                        result,
                        attribute == null ? text : MaskUtils.MaskValue(text, attribute));
                    continue;
                }

                property.SetValue(result, CloneValue(value, visited));
            }
        }

        private static object CloneValue(object source, IDictionary<object, object> visited)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();

            if (type.IsValueType ||
                source is string ||
                source is Type ||
                source is Uri ||
                source is Version ||
                source is System.Globalization.CultureInfo ||
                source is TimeZoneInfo ||
                source is System.Net.IPAddress ||
                source is Delegate ||
                source is MemberInfo)
            {
                return source;
            }

            if (visited.TryGetValue(source, out var existing))
            {
                return existing;
            }

            if (source is Array sourceArray)
            {
                if (sourceArray.Rank != 1)
                {
                    throw new InvalidOperationException("Only one-dimensional arrays are supported.");
                }

                var elementType = type.GetElementType();
                var resultArray = Array.CreateInstance(elementType, sourceArray.Length);
                visited.Add(source, resultArray);

                for (var index = 0; index < sourceArray.Length; index++)
                {
                    resultArray.SetValue(CloneValue(sourceArray.GetValue(index), visited), index);
                }

                return resultArray;
            }

            if (source is IList sourceList)
            {
                var resultList = CreateList(type, sourceList);
                visited.Add(source, resultList);

                foreach (var item in sourceList)
                {
                    resultList.Add(CloneValue(item, visited));
                }

                return resultList;
            }

            if (source is IEnumerable)
            {
                throw new InvalidOperationException(
                    $"Collection type '{type.FullName}' is not supported. Use arrays or IList collections.");
            }

            var result = CreateObject(type);
            visited.Add(source, result);
            CopyObject(source, result, visited);
            return result;
        }

        private static IList CreateList(Type sourceType, IList source)
        {
            if (!sourceType.IsAbstract && !sourceType.IsInterface)
            {
                var instance = Activator.CreateInstance(sourceType) as IList;

                if (instance != null)
                {
                    return instance;
                }
            }

            var elementType = GetListElementType(sourceType, source);

            if (elementType == null)
            {
                throw new InvalidOperationException(
                    $"Collection type '{sourceType.FullName}' cannot be cloned.");
            }

            var listType = typeof(List<>).MakeGenericType(elementType);
            return (IList)Activator.CreateInstance(listType);
        }

        private static Type GetListElementType(Type sourceType, IList source)
        {
            var enumerableType = sourceType
                .GetInterfaces()
                .Concat(new[] { sourceType })
                .FirstOrDefault(type =>
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableType != null)
            {
                return enumerableType.GetGenericArguments()[0];
            }

            return source.Count > 0 ? source[0]?.GetType() : typeof(object);
        }

        private static object CreateObject(Type type)
        {
            try
            {
                var result = Activator.CreateInstance(type);

                if (result == null)
                {
                    throw new InvalidOperationException();
                }

                return result;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Cannot clone nested type '{type.FullName}'. A public parameterless constructor is required.",
                    exception);
            }
        }
    }
}