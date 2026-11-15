using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskObjectTests
    {
        private class Pessoa
        {
            [Mask(MaskType.Name)]
            public string Nome { get; set; }

            [Mask(MaskType.Cpf)]
            public string Cpf { get; set; }

            public string Cidade { get; set; }
        }

        [Fact]
        public void Mask_ShouldNotModifyOriginalObject()
        {
            var pessoa = new Pessoa
            {
                Nome = "Fulvio Silva",
                Cpf = "12345678901",
                Cidade = "Presidente Prudente"
            };

            var result = DataMasker.Mask(pessoa);

            Assert.Equal("Fulvio Silva", pessoa.Nome);
            Assert.Equal("12345678901", pessoa.Cpf);
            Assert.Equal("Presidente Prudente", pessoa.Cidade);

            Assert.Equal("F***** S****", result.Nome);
            Assert.Equal("123.***.***-01", result.Cpf);
            Assert.Equal("Presidente Prudente", result.Cidade);
        }

        [Fact]
        public void Mask_ShouldReturnNewObjectInstance()
        {
            var pessoa = new Pessoa
            {
                Nome = "Fulvio Silva",
                Cpf = "12345678901",
                Cidade = "Presidente Prudente"
            };

            var result = DataMasker.Mask(pessoa);

            Assert.NotSame(pessoa, result);
        }
    }
}
