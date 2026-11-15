using Canducci.DataMasker.Utils;
using System;

namespace Canducci.DataMasker
{
    public static class DataMasker
    {
        public static T Mask<T>(T source) where T : class, new()
        {
            return MaskConvert<T>.Execute(source);
        }

        public static T Mask<T>(T source, Func<T, T> factory) where T : class
        {
            return MaskConvert<T>.Execute(source, factory);
        }
    }
}