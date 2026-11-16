using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskFormatTests
    {
        private class FormatModel
        {
            [Mask(MaskType.Cpf)]
            public string Cpf { get; set; }

            [Mask(MaskType.Cnpj)]
            public string Cnpj { get; set; }

            [Mask(MaskType.Email)]
            public string Email { get; set; }

            [Mask(MaskType.Phone)]
            public string Phone { get; set; }

            [Mask(MaskType.Mobile)]
            public string Mobile { get; set; }

            [Mask(MaskType.Cep)]
            public string Cep { get; set; }

            [Mask(MaskType.BirthDate)]
            public string BirthDate { get; set; }

            [Mask(MaskType.IpAddress)]
            public string IpAddress { get; set; }

            [Mask(MaskType.CreditCard)]
            public string CreditCard { get; set; }

            [Mask(MaskType.Pix)]
            public string Pix { get; set; }
        }

        [Fact]
        public void Mask_ShouldHandleCpfAndCnpjWithAndWithoutFormatting()
        {
            var model = new FormatModel
            {
                Cpf = "123.456.789-01"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("123.***.***-01", result.Cpf);

            model.Cpf = "12345678901";
            model.Cnpj = "12.345.678/0001-99";

            result = DataMasker.Mask(model);

            Assert.Equal("123.***.***-01", result.Cpf);
            Assert.Equal("12.***/****-99", result.Cnpj);

            model.Cnpj = "12345678000199";

            result = DataMasker.Mask(model);

            Assert.Equal("12.***/****-99", result.Cnpj);
        }

        [Fact]
        public void Mask_ShouldPreserveInvalidEmail()
        {
            var model = new FormatModel
            {
                Email = "fulvio@"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("fulvio@", result.Email);
        }

        [Fact]
        public void Mask_ShouldHandleFormattedPhoneAndMobile()
        {
            var model = new FormatModel
            {
                Phone = "(18) 3322-1234",
                Mobile = "(18) 99123-1234"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("(18) ****-1234", result.Phone);
            Assert.Equal("(18) *****-1234", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldHandlePhoneAndMobileWithBrazilianCountryCode()
        {
            var model = new FormatModel
            {
                Phone = "+55 (18) 3322-1234",
                Mobile = "+55 (18) 99123-1234"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("+55 (18) ****-1234", result.Phone);
            Assert.Equal("+55 (18) *****-1234", result.Mobile);

            model.Phone = "551833221234";
            model.Mobile = "5518991231234";

            result = DataMasker.Mask(model);

            Assert.Equal("5518****1234", result.Phone);
            Assert.Equal("5518*****1234", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldHandleUnformattedPhoneAndMobile()
        {
            var model = new FormatModel
            {
                Phone = "1833221234",
                Mobile = "18991231234"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("18****1234", result.Phone);
            Assert.Equal("18*****1234", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldPreservePhoneAndMobileWithInvalidDigitCount()
        {
            var model = new FormatModel
            {
                Phone = "183322123",
                Mobile = "1899123123"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("183322123", result.Phone);
            Assert.Equal("1899123123", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldHandleCepWithAndWithoutFormatting()
        {
            var model = new FormatModel
            {
                Cep = "19010-000"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("19010-***", result.Cep);

            model.Cep = "19010000";

            result = DataMasker.Mask(model);

            Assert.Equal("19010-***", result.Cep);
        }

        [Fact]
        public void Mask_ShouldHandleValidAndInvalidBirthDate()
        {
            var model = new FormatModel
            {
                BirthDate = "15/05/1990"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("**/**/1990", result.BirthDate);

            model.BirthDate = "31/02/1990";

            result = DataMasker.Mask(model);

            Assert.Equal("31/02/1990", result.BirthDate);
        }

        [Fact]
        public void Mask_ShouldHandleValidAndInvalidIpAddress()
        {
            var model = new FormatModel
            {
                IpAddress = "192.168.1.100"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("192.***.***.***", result.IpAddress);

            model.IpAddress = "192.168.1.999";

            result = DataMasker.Mask(model);

            Assert.Equal("192.168.1.999", result.IpAddress);

            model.IpAddress = "2001:db8:85a3::8a2e:370:7334";

            result = DataMasker.Mask(model);

            Assert.Equal("2001:****:****:****:****:****:****:****", result.IpAddress);

            model.IpAddress = "not-an-ip";

            result = DataMasker.Mask(model);

            Assert.Equal("not-an-ip", result.IpAddress);
        }

        [Fact]
        public void Mask_ShouldHandleCreditCardWithValidAndInvalidDigitCount()
        {
            var model = new FormatModel
            {
                CreditCard = "1234567890123456"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("************3456", result.CreditCard);

            model.CreditCard = "123456789012345";

            result = DataMasker.Mask(model);

            Assert.Equal("***********2345", result.CreditCard);

            model.CreditCard = "4111-1111-1111-1111";

            result = DataMasker.Mask(model);

            Assert.Equal("****-****-****-1111", result.CreditCard);

            model.CreditCard = "1234567890123456789";

            result = DataMasker.Mask(model);

            Assert.Equal("***************6789", result.CreditCard);

            model.CreditCard = "12345678901234";

            result = DataMasker.Mask(model);

            Assert.Equal("12345678901234", result.CreditCard);

            model.CreditCard = "12345678901234567890";

            result = DataMasker.Mask(model);

            Assert.Equal("12345678901234567890", result.CreditCard);
        }

        [Fact]
        public void Mask_ShouldHandlePixEmail()
        {
            var model = new FormatModel
            {
                Pix = "fulvio@gmail.com"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("f*****@g****.com", result.Pix);
        }

        [Fact]
        public void Mask_ShouldHandlePixNonEmail()
        {
            var model = new FormatModel
            {
                Pix = "123456789"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("1********", result.Pix);
        }

        [Fact]
        public void Mask_ShouldPreserveInvalidPixEmail()
        {
            var model = new FormatModel
            {
                Pix = "fulvio@"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("fulvio@", result.Pix);
        }
    }
}
