using System.Collections.Concurrent;
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskConvertTests
    {
        private sealed class Pessoa
        {
            [Mask(MaskType.Name)]
            public string Nome { get; set; }
        }

        [Fact]
        public void Mask_ShouldThrowWhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DataMasker.Mask<Pessoa>(null));
        }

        [Fact]
        public void MaskWithFactory_ShouldThrowWhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DataMasker.Mask<Pessoa>(null, _ => new Pessoa()));
        }

        [Fact]
        public void Mask_ShouldBeSafeWhenSameTypeIsMaskedConcurrently()
        {
            var source = new Pessoa
            {
                Nome = "Fulvio"
            };
            var results = new ConcurrentBag<string>();

            Parallel.For(0, 100, _ =>
            {
                var result = DataMasker.Mask(source);
                results.Add(result.Nome);
            });

            Assert.Equal(100, results.Count);
            Assert.All(results, value => Assert.Equal("F*****", value));
            Assert.Equal("Fulvio", source.Nome);
        }
    }
}
