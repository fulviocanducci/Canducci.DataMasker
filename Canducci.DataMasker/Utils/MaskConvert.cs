
using Canducci.DataMasker.Attributes;
using System;
using System.Reflection;

namespace Canducci.DataMasker.Utils
{
    internal static class MaskConvert<T> where T : class
    {
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
            foreach (var property in typeof(T).GetProperties())
            {
                if (!property.CanRead || !property.CanWrite)
                {
                    continue;
                }
                var value = property.GetValue(source);
                var attribute = property.GetCustomAttribute<MaskAttribute>();
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
