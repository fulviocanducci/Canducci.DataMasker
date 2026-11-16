using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskTypeModel
    {
        [Mask(MaskType.None)]
        public string None { get; set; }

        [Mask(MaskType.Custom)]
        public string Custom { get; set; }

        [Mask(MaskType.Name)]
        public string Name { get; set; }

        [Mask(MaskType.Cpf)]
        public string Cpf { get; set; }

        [Mask(MaskType.Cnpj)]
        public string Cnpj { get; set; }

        [Mask(MaskType.Rg)]
        public string Rg { get; set; }

        [Mask(MaskType.Email)]
        public string Email { get; set; }

        [Mask(MaskType.Phone)]
        public string Phone { get; set; }

        [Mask(MaskType.Mobile)]
        public string Mobile { get; set; }

        [Mask(MaskType.Cep)]
        public string Cep { get; set; }

        [Mask(MaskType.Address)]
        public string Address { get; set; }

        [Mask(MaskType.BirthDate)]
        public string BirthDate { get; set; }

        [Mask(MaskType.IpAddress)]
        public string IpAddress { get; set; }

        [Mask(MaskType.CreditCard)]
        public string CreditCard { get; set; }

        [Mask(MaskType.BankAccount)]
        public string BankAccount { get; set; }

        [Mask(MaskType.Pix)]
        public string Pix { get; set; }

        [Mask(MaskType.Salary)]
        public string Salary { get; set; }

        [Mask(MaskType.Health)]
        public string Health { get; set; }

        [Mask(MaskType.Biometric)]
        public string Biometric { get; set; }

        [Mask(MaskType.Password)]
        public string Password { get; set; }

        [Mask(MaskType.Token)]
        public string Token { get; set; }
    }

    public class MaskTypeTests
    {
        [Fact]
        public void Mask_ShouldApplyAllMaskTypes()
        {
            var model = new MaskTypeModel
            {
                None = "Fulvio",
                Custom = "Fulvio",
                Name = "Fulvio Silva",
                Cpf = "12345678901",
                Cnpj = "12345678000199",
                Rg = "123456789",
                Email = "fulvio@gmail.com",
                Phone = "1833221234",
                Mobile = "18991231234",
                Cep = "19010000",
                Address = "Rua Brasil, 123",
                BirthDate = "15/05/1990",
                IpAddress = "192.168.1.100",
                CreditCard = "1234567890123456",
                BankAccount = "123456-7",
                Pix = "fulvio@gmail.com",
                Salary = "5000",
                Health = "Informação médica",
                Biometric = "Biometria",
                Password = "MinhaSenha",
                Token = "ABC123XYZ"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("Fulvio", result.None);
            Assert.Equal("Fulvio", result.Custom);
            Assert.Equal("F***** S****", result.Name);
            Assert.Equal("123.***.***-01", result.Cpf);
            Assert.Equal("12.***/****-99", result.Cnpj);
            Assert.Equal("******789", result.Rg);
            Assert.Equal("f***@gmail.com", result.Email);
            Assert.Equal("18****1234", result.Phone);
            Assert.Equal("18*****1234", result.Mobile);
            Assert.Equal("19010-***", result.Cep);
            Assert.Equal("R** B*****, ***", result.Address);
            Assert.Equal("**/**/1990", result.BirthDate);
            Assert.Equal("192.***.***.***", result.IpAddress);
            Assert.Equal("1234 **** **** 3456", result.CreditCard);
            Assert.Equal("******-*", result.BankAccount);
            Assert.Equal("f*****@g****.com", result.Pix);
            Assert.Equal("****", result.Salary);
            Assert.Equal(new string('*', "Informação médica".Length), result.Health);
            Assert.Equal(new string('*', "Biometria".Length), result.Biometric);
            Assert.Equal(new string('*', "MinhaSenha".Length), result.Password);
            Assert.Equal(new string('*', "ABC123XYZ".Length), result.Token);
        }
    }
}

