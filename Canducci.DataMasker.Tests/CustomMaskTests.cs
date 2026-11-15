using Canducci.DataMasker.Tests.Models;

namespace Canducci.DataMasker.Tests
{
    public class CustomMaskTests
    {
        [Fact]
        public void Mask_ShouldExecuteCustomMethodInsideModel()
        {
            var pessoa = new Pessoa
            {
                Nome = "Fulvio"
            };

            var result = DataMasker.Mask(pessoa);

            Assert.Equal("F*****", result.Nome);
        }

        [Fact]
        public void Mask_ShouldExecuteCustomMethodFromExternalClass()
        {
            var cliente = new Cliente
            {
                Nome = "Carlos"
            };

            var result = DataMasker.Mask(cliente);

            Assert.Equal("C#####", result.Nome);
        }
    }
}
