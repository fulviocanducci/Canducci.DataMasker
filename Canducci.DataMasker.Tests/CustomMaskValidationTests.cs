using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class CustomMaskValidationTests
    {
        public class MissingMethodModel
        {
            [Mask(MaskType.Custom, typeof(MissingMethodModel), "MetodoInexistente")]
            public string Nome { get; set; }
        }

        public class InvalidReturnModel
        {
            [Mask(MaskType.Custom, typeof(InvalidReturnModel), nameof(MaskNome))]
            public string Nome { get; set; }

            private static int MaskNome(string value)
            {
                return value.Length;
            }
        }

        public class InvalidParameterModel
        {
            [Mask(MaskType.Custom, typeof(InvalidParameterModel), nameof(MaskNome))]
            public string Nome { get; set; }

            private static string MaskNome(int value)
            {
                return value.ToString();
            }
        }

        public class InstanceMethodModel
        {
            [Mask(MaskType.Custom, typeof(InstanceMethodModel), nameof(MaskNome))]
            public string Nome { get; set; }

            private string MaskNome(string value)
            {
                return value.Substring(0, 1) + "*****";
            }
        }

        public class MissingTypeModel
        {
            [Mask(MaskType.Custom, MaskStrategy.Custom)]
            public string Nome { get; set; }
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodDoesNotExist()
        {
            var model = new MissingMethodModel
            {
                Nome = "Fulvio"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodHasInvalidReturnType()
        {
            var model = new InvalidReturnModel
            {
                Nome = "Fulvio"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodHasInvalidParameter()
        {
            var model = new InvalidParameterModel
            {
                Nome = "Fulvio"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodIsNotStatic()
        {
            var model = new InstanceMethodModel
            {
                Nome = "Fulvio"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomTypeIsMissing()
        {
            var model = new MissingTypeModel
            {
                Nome = "Fulvio"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                DataMasker.Mask(model));
        }
    }
}

