using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskFactoryTests
    {
        private class Pessoa
        {
            public Pessoa(int id)
            {
                Id = id;
            }

            public int Id { get; set; }

            [Mask(MaskType.Name)]
            public string Nome { get; set; }

            public string Cidade { get; set; }
        }

        [Fact]
        public void Mask_ShouldCreateInstanceUsingFactory()
        {
            var pessoa = new Pessoa(10)
            {
                Nome = "Fulvio Silva",
                Cidade = "Presidente Prudente"
            };

            var result = DataMasker.Mask(pessoa, source =>
                new Pessoa(source.Id));

            Assert.NotNull(result);
            Assert.NotSame(pessoa, result);
            Assert.Equal(10, result.Id);
            Assert.Equal("F***** S****", result.Nome);
            Assert.Equal("Presidente Prudente", result.Cidade);
        }

        [Fact]
        public void Mask_ShouldNotModifyOriginalObject()
        {
            var pessoa = new Pessoa(10)
            {
                Nome = "Fulvio Silva",
                Cidade = "Presidente Prudente"
            };

            var result = DataMasker.Mask(pessoa, source =>
                new Pessoa(source.Id));

            Assert.Equal("Fulvio Silva", pessoa.Nome);
            Assert.Equal("Presidente Prudente", pessoa.Cidade);
        }

        [Fact]
        public void Mask_ShouldThrowWhenFactoryIsNull()
        {
            var pessoa = new Pessoa(10)
            {
                Nome = "Fulvio Silva"
            };

            Assert.Throws<ArgumentNullException>(() =>
                DataMasker.Mask(pessoa, null));
        }

        [Fact]
        public void Mask_ShouldThrowWhenFactoryReturnsNull()
        {
            var pessoa = new Pessoa(10)
            {
                Nome = "Fulvio Silva"
            };

            Assert.Throws<InvalidOperationException>(() =>
                DataMasker.Mask(pessoa, source => null));
        }
    }
}

