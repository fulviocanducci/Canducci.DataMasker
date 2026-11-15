using Canducci.DataMasker.Utils;

namespace Canducci.DataMasker
{
    public static class DataMasker
    {
        public static T Mask<T>(T source) where T : class, new()
        {
            return MaskConvert<T>.Execute(source);
        }
    }
}