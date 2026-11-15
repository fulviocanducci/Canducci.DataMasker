using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests.Models
{
    public class Pessoa
    {
        [Mask(MaskType.Custom, typeof(Pessoa), nameof(MaskNome))]
        public string Nome { get; set; }

        private static string MaskNome(string value)
        {
            return value.Substring(0, 1) + "*****";
        }
    }
}
