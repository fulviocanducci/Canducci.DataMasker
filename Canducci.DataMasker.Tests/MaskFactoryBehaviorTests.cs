using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskFactoryBehaviorTests
    {
        private sealed class FactoryModel
        {
            public FactoryModel(int id)
            {
                Id = id;
            }

            public int Id { get; set; }

            [Mask(MaskType.Name)]
            public string Name { get; set; }
        }

        [Fact]
        public void Mask_ShouldInvokeFactoryOnceWithOriginalSource()
        {
            var source = new FactoryModel(42)
            {
                Name = "Fulvio Silva"
            };
            FactoryModel receivedSource = null;
            var factoryCalls = 0;

            var result = DataMasker.Mask(source, value =>
            {
                factoryCalls++;
                receivedSource = value;
                return new FactoryModel(value.Id);
            });

            Assert.Equal(1, factoryCalls);
            Assert.Same(source, receivedSource);
            Assert.NotSame(source, result);
            Assert.Equal(42, result.Id);
            Assert.Equal("F***** S****", result.Name);
            Assert.Equal("Fulvio Silva", source.Name);
        }
    }
}
