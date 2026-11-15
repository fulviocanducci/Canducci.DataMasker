using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskStrategyTests
    {
        private class StrategyModel
        {
            [Mask(MaskType.Name, MaskStrategy.Full)]
            public string Full { get; set; }

            [Mask(MaskType.Name, MaskStrategy.Partial)]
            public string Partial { get; set; }

            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)]
            public string FirstCharacters { get; set; }

            [Mask(MaskType.Name, MaskStrategy.LastCharacters)]
            public string LastCharacters { get; set; }
        }

        [Fact]
        public void Mask_ShouldApplyAllStrategies()
        {
            var model = new StrategyModel
            {
                Full = "Fulvio",
                Partial = "Fulvio",
                FirstCharacters = "Fulvio",
                LastCharacters = "Fulvio"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("******", result.Full);
            Assert.Equal("F*****", result.Partial);
            Assert.Equal("*****o", result.FirstCharacters);
            Assert.Equal("F*****", result.LastCharacters);
        }
    }
}

