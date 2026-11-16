using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskStrategyBoundaryTests
    {
        private sealed class StrategyBoundaryModel
        {
            [Mask(MaskType.Name, MaskStrategy.Full)]
            public string FullNull { get; set; }

            [Mask(MaskType.Name, MaskStrategy.Partial)]
            public string PartialNull { get; set; }

            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)]
            public string FirstCharactersNull { get; set; }

            [Mask(MaskType.Name, MaskStrategy.LastCharacters)]
            public string LastCharactersNull { get; set; }

            [Mask(MaskType.Name, MaskStrategy.Full)]
            public string FullEmpty { get; set; }

            [Mask(MaskType.Name, MaskStrategy.Partial)]
            public string PartialEmpty { get; set; }

            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)]
            public string FirstCharactersEmpty { get; set; }

            [Mask(MaskType.Name, MaskStrategy.LastCharacters)]
            public string LastCharactersEmpty { get; set; }
        }

        [Fact]
        public void Mask_ShouldPreserveNullAndEmptyValuesForEveryStrategy()
        {
            var model = new StrategyBoundaryModel
            {
                FullNull = null,
                PartialNull = null,
                FirstCharactersNull = null,
                LastCharactersNull = null,
                FullEmpty = string.Empty,
                PartialEmpty = string.Empty,
                FirstCharactersEmpty = string.Empty,
                LastCharactersEmpty = string.Empty
            };

            var result = DataMasker.Mask(model);

            Assert.Null(result.FullNull);
            Assert.Null(result.PartialNull);
            Assert.Null(result.FirstCharactersNull);
            Assert.Null(result.LastCharactersNull);
            Assert.Equal(string.Empty, result.FullEmpty);
            Assert.Equal(string.Empty, result.PartialEmpty);
            Assert.Equal(string.Empty, result.FirstCharactersEmpty);
            Assert.Equal(string.Empty, result.LastCharactersEmpty);
        }
    }
}
