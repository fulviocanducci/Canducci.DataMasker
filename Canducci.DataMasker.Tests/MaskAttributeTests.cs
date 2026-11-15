using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskAttributeTests
    {
        [Fact]
        public void Constructor_ShouldUsePartialStrategyByDefault()
        {
            var attribute = new MaskAttribute(MaskType.Name);

            Assert.Equal(MaskType.Name, attribute.Type);
            Assert.Equal(MaskStrategy.Partial, attribute.Strategy);
            Assert.Null(attribute.CustomType);
            Assert.Null(attribute.CustomMethod);
        }

        [Fact]
        public void Constructor_ShouldUseSpecifiedStrategy()
        {
            var attribute = new MaskAttribute(
                MaskType.Name,
                MaskStrategy.Full);

            Assert.Equal(MaskType.Name, attribute.Type);
            Assert.Equal(MaskStrategy.Full, attribute.Strategy);
        }

        [Fact]
        public void Constructor_ShouldConfigureCustomMask()
        {
            var attribute = new MaskAttribute(
                MaskType.Custom,
                typeof(MaskCustom),
                nameof(MaskCustom.MaskNome));

            Assert.Equal(MaskType.Custom, attribute.Type);
            Assert.Equal(MaskStrategy.Custom, attribute.Strategy);
            Assert.Equal(typeof(MaskCustom), attribute.CustomType);
            Assert.Equal(nameof(MaskCustom.MaskNome), attribute.CustomMethod);
        }

        [Fact]
        public void Constructor_ShouldThrowWhenCustomTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MaskAttribute(
                    MaskType.Custom,
                    null,
                    "MaskNome"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ShouldThrowWhenCustomMethodIsInvalid(string method)
        {
            Assert.Throws<ArgumentException>(() =>
                new MaskAttribute(
                    MaskType.Custom,
                    typeof(MaskCustom),
                    method));
        }

        public static class MaskCustom
        {
            public static string MaskNome(string value)
            {
                return value;
            }
        }
    }
}

