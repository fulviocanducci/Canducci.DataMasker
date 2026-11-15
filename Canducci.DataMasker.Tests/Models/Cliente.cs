using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests.Models
{
    public class Cliente
    {
        [Mask(MaskType.Custom, typeof(MaskCustom), nameof(MaskCustom.MaskNome))]
        public string Nome { get; set; }
    }
}
