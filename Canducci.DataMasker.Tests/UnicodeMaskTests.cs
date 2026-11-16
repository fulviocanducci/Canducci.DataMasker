using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class UnicodeMaskTests
    {
        private sealed class UnicodeModel
        {
            [Mask(MaskType.Name)]
            public string Nome { get; set; }

            [Mask(MaskType.Address)]
            public string Endereco { get; set; }
        }

        [Fact]
        public void Mask_ShouldPreserveAccentedCharactersInNameAndAddress()
        {
            var model = new UnicodeModel
            {
                Nome = "João Gonçalves",
                Endereco = "Rua São João, 123"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("J*** G********", result.Nome);
            Assert.Equal("R** S** J***, ***", result.Endereco);
            Assert.Equal("João Gonçalves", model.Nome);
            Assert.Equal("Rua São João, 123", model.Endereco);
        }
    }
}
