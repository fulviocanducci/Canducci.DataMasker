using Canducci.DataMasker.Attributes;
using System;
using System.Reflection;
namespace Canducci.DataMasker.Utils
{
    internal static class MaskConvert<T> where T : class, new()
    {
        internal static T Execute(T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var result = new T();
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
