using System.Collections.Generic;
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class RootListMaskingTests
    {
        private class Address
        {
            [Mask(MaskType.Name)]
            public string Street { get; set; }

            [Mask(MaskType.Custom, typeof(Address), nameof(MaskDistrict))]
            public string District { get; set; }

            internal static string MaskDistrict(string value)
            {
                return "CUSTOM";
            }
        }

        [Fact]
        public void Mask_ShouldCloneAndMaskListPassedAsRoot()
        {
            var source = new List<Address>
            {
                new Address
                {
                    Street = "Fulvio Silva",
                    District = "Jardim Monte Rei"
                }
            };

            var result = DataMasker.Mask(source);

            Assert.NotSame(source, result);
            Assert.Single(result);
            Assert.NotSame(source[0], result[0]);
            Assert.Equal("F***** S****", result[0].Street);
            Assert.Equal("CUSTOM", result[0].District);
            Assert.Equal("Fulvio Silva", source[0].Street);
            Assert.Equal("Jardim Monte Rei", source[0].District);
        }

        [Fact]
        public void Mask_ShouldCloneAndMaskRootListWhenUsingFactory()
        {
            var source = new List<Address>
            {
                new Address
                {
                    Street = "Fulvio Silva",
                    District = "Jardim Monte Rei"
                }
            };

            var result = DataMasker.Mask(source, _ => new List<Address>());

            Assert.NotSame(source, result);
            Assert.Single(result);
            Assert.NotSame(source[0], result[0]);
            Assert.Equal("F***** S****", result[0].Street);
            Assert.Equal("CUSTOM", result[0].District);
            Assert.Equal("Fulvio Silva", source[0].Street);
        }
    }
}
