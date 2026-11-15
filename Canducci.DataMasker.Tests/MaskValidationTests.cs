using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskValidationTests
    {
        private class ValidationModel
        {
            [Mask(MaskType.Name)]
            public string Nome { get; set; }

            [Mask(MaskType.Name, MaskStrategy.Full)]
            public string? Full { get; set; }

            [Mask(MaskType.Cpf)]
            public string Cpf { get; set; }

            [Mask(MaskType.Cnpj)]
            public string Cnpj { get; set; }

            [Mask(MaskType.Rg)]
            public string Rg { get; set; }
        }

        [Fact]
        public void Mask_ShouldPreserveNullValues()
        {
            var model = new ValidationModel
            {
                Nome = null,
                Full = null,
                Cpf = null,
                Cnpj = null,
                Rg = null
            };

            var result = DataMasker.Mask(model);

            Assert.Null(result.Nome);
            Assert.Null(result.Full);
            Assert.Null(result.Cpf);
            Assert.Null(result.Cnpj);
            Assert.Null(result.Rg);
        }

        [Fact]
        public void Mask_ShouldPreserveEmptyValues()
        {
            var model = new ValidationModel
            {
                Nome = string.Empty,
                Full = string.Empty,
                Cpf = string.Empty,
                Cnpj = string.Empty,
                Rg = string.Empty
            };

            var result = DataMasker.Mask(model);

            Assert.Equal(string.Empty, result.Nome);
            Assert.Equal(string.Empty, result.Full);
            Assert.Equal(string.Empty, result.Cpf);
            Assert.Equal(string.Empty, result.Cnpj);
            Assert.Equal(string.Empty, result.Rg);
        }

        [Fact]
        public void Mask_ShouldHandleWhitespace()
        {
            var model = new ValidationModel
            {
                Nome = "   ",
                Full = "   ",
                Cpf = "   ",
                Cnpj = "   ",
                Rg = "   "
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("   ", result.Nome);
            Assert.Equal("***", result.Full);
            Assert.Equal("   ", result.Cpf);
            Assert.Equal("   ", result.Cnpj);
            Assert.Equal("   ", result.Rg);
        }

        [Fact]
        public void Mask_ShouldPreserveInvalidCpfAndCnpj()
        {
            var model = new ValidationModel
            {
                Nome = "Fulvio",
                Cpf = "123456",
                Cnpj = "123456789"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("123456", result.Cpf);
            Assert.Equal("123456789", result.Cnpj);
        }

        [Fact]
        public void Mask_ShouldHandleRgWithThreeOrLessCharacters()
        {
            var model = new ValidationModel
            {
                Nome = "Fulvio",
                Rg = "12"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("12", result.Rg);
        }
    }
}
