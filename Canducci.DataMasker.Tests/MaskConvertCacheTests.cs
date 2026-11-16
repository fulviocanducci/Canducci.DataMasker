using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskConvertCacheTests
    {
        private sealed class NameModel
        {
            [Mask(MaskType.Name)]
            public string Value { get; set; }
        }

        private sealed class EmailModel
        {
            [Mask(MaskType.Email)]
            public string Value { get; set; }
        }

        private sealed class ModelWithoutWritableProperties
        {
            public ModelWithoutWritableProperties()
            {
                Value = "Default";
            }

            public string Value { get; }
        }

        [Fact]
        public void Mask_ShouldKeepReflectionMetadataIsolatedBetweenTypes()
        {
            var nameSource = new NameModel
            {
                Value = "Fulvio Silva"
            };
            var emailSource = new EmailModel
            {
                Value = "fulvio@gmail.com"
            };

            var nameResult = DataMasker.Mask(nameSource);
            var emailResult = DataMasker.Mask(emailSource);

            Assert.Equal("F***** S****", nameResult.Value);
            Assert.Equal("f***@gmail.com", emailResult.Value);
            Assert.Equal("Fulvio Silva", nameSource.Value);
            Assert.Equal("fulvio@gmail.com", emailSource.Value);
        }

        [Fact]
        public void Mask_ShouldHandleTypesWithoutWritableProperties()
        {
            var source = new ModelWithoutWritableProperties();

            var result = DataMasker.Mask(source);

            Assert.NotNull(result);
            Assert.NotSame(source, result);
            Assert.Equal("Default", result.Value);
            Assert.Equal("Default", source.Value);
        }
    }
}
