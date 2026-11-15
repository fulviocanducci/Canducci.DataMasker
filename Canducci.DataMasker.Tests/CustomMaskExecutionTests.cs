using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;
using System.Reflection;

namespace Canducci.DataMasker.Tests
{
    public class CustomMaskExecutionTests
    {
        public class EmptyValueModel
        {
            [Mask(MaskType.Custom, typeof(EmptyValueModel), nameof(MaskNome))]
            public string Nome { get; set; }

            private static string MaskNome(string value)
            {
                return "EXECUTADO";
            }
        }

        public class NullResultModel
        {
            [Mask(MaskType.Custom, typeof(NullResultModel), nameof(MaskNome))]
            public string Nome { get; set; }

            private static string MaskNome(string value)
            {
                return null;
            }
        }

        public class ExceptionModel
        {
            [Mask(MaskType.Custom, typeof(ExceptionModel), nameof(MaskNome))]
            public string Nome { get; set; }

            private static string MaskNome(string value)
            {
                throw new InvalidOperationException("Erro no mascaramento personalizado.");
            }
        }

        [Fact]
        public void Mask_ShouldPreserveEmptyValue()
        {
            var model = new EmptyValueModel
            {
                Nome = string.Empty
            };

            var result = DataMasker.Mask(model);

            Assert.Equal(string.Empty, result.Nome);
        }

        [Fact]
        public void Mask_ShouldAcceptNullFromCustomMethod()
        {
            var model = new NullResultModel
            {
                Nome = "Fulvio"
            };

            var result = DataMasker.Mask(model);

            Assert.Null(result.Nome);
        }

        [Fact]
        public void Mask_ShouldPropagateCustomMethodException()
        {
            var model = new ExceptionModel
            {
                Nome = "Fulvio"
            };

            var exception = Assert.Throws<TargetInvocationException>(() =>
                DataMasker.Mask(model));

            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal(
                "Erro no mascaramento personalizado.",
                exception.InnerException.Message);
        }
    }
}

