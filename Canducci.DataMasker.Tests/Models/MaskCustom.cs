namespace Canducci.DataMasker.Tests.Models
{
    public static class MaskCustom
    {
        public static string MaskNome(string value)
        {
            return value.Substring(0, 1) + "#####";
        }
    }
}
