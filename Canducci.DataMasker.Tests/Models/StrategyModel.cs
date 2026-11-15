using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests.Models
{
    public class StrategyModel
    {
        [Mask(MaskType.Name, MaskStrategy.Full)]
        public string Full { get; set; }

        [Mask(MaskType.Name, MaskStrategy.FirstCharacters)]
        public string FirstCharacters { get; set; }

        [Mask(MaskType.Name, MaskStrategy.LastCharacters)]
        public string LastCharacters { get; set; }

        [Mask(MaskType.Name, MaskStrategy.Partial)]
        public string Partial { get; set; }
    }
}
