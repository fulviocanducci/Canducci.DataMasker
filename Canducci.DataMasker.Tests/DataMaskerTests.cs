namespace Canducci.DataMasker.Tests
{
    public class DataMaskerTests
    {
        private class EmptyModel
        {
        }

        private class PlainModel
        {
            public string Nome { get; set; }

            public int Idade { get; set; }
        }

        [Fact]
        public void Mask_ShouldThrowWhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DataMasker.Mask<PlainModel>(null));
        }

        [Fact]
        public void Mask_ShouldReturnNewInstanceForEmptyModel()
        {
            var model = new EmptyModel();

            var result = DataMasker.Mask(model);

            Assert.NotNull(result);
            Assert.NotSame(model, result);
        }

        [Fact]
        public void Mask_ShouldCopyPropertiesWithoutAttributes()
        {
            var model = new PlainModel
            {
                Nome = "Fulvio",
                Idade = 35
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("Fulvio", result.Nome);
            Assert.Equal(35, result.Idade);
            Assert.NotSame(model, result);
        }
    }
}
