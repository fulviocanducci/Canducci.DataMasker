using Canducci.DataMasker.Attributes;
using System;
using System.Linq;
using System.Reflection;

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
                    catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException)
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

        private static readonly PropertyMetadata[] Properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property =>
                property.CanRead &&
                property.CanWrite &&
                property.GetIndexParameters().Length == 0)
            .Select(property => new PropertyMetadata(property))
            .ToArray();

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
            return CopyProperties(source, result);
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
            return CopyProperties(source, result);
        }

        private static T CopyProperties(T source, T result)
        {
            foreach (var metadata in Properties)
            {
                var property = metadata.Property;
                var value = property.GetValue(source);
                var attribute = metadata.Attribute.Value;

                if (attribute == null || value == null)
                {
                    property.SetValue(result, value);
                    continue;
                }
                if (value is string text)
                {
                    property.SetValue(result, MaskUtils.MaskValue(text, attribute));
                    continue;
                }
                property.SetValue(result, value);
            }
            return result;
        }
    }
}
