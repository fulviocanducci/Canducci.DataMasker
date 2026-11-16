using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskStrategySingleCharacterTests
    {
        private sealed class SingleCharacterModel
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
        public void Mask_ShouldHandleSingleCharacterValuesForEveryStrategy()
        {
            var model = new SingleCharacterModel
            {
                Full = "F",
                Partial = "F",
                FirstCharacters = "F",
                LastCharacters = "F"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("*", result.Full);
            Assert.Equal("F", result.Partial);
            Assert.Equal("F", result.FirstCharacters);
            Assert.Equal("*", result.LastCharacters);
        }
    }
}
