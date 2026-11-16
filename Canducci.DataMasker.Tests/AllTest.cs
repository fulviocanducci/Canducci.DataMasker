using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;
using Canducci.DataMasker.Tests.Models;

namespace Canducci.DataMasker.Tests
{
    public class CustomMaskExecutionTests
    {
        public class EmptyValueModel
        {
            [Mask(MaskType.Custom, typeof(EmptyValueModel), nameof(MaskNome))]
            public string Nome { get; set; }
            private static string MaskNome(string value) { return "EXECUTADO"; }
        }

        public class NullResultModel
        {
            [Mask(MaskType.Custom, typeof(NullResultModel), nameof(MaskNome))]
            public string Nome { get; set; }
            private static string MaskNome(string value) { return null; }
        }

        public class ExceptionModel
        {
            [Mask(MaskType.Custom, typeof(ExceptionModel), nameof(MaskNome))]
            public string Nome { get; set; }
            private static string MaskNome(string value) { throw new InvalidOperationException("Erro no mascaramento personalizado."); }
        }

        [Fact]
        public void Mask_ShouldPreserveEmptyValue()
        {
            var model = new EmptyValueModel { Nome = string.Empty };
            var result = DataMasker.Mask(model);
            Assert.Equal(string.Empty, result.Nome);
        }

        [Fact]
        public void Mask_ShouldAcceptNullFromCustomMethod()
        {
            var model = new NullResultModel { Nome = "Fulvio" };
            var result = DataMasker.Mask(model);
            Assert.Null(result.Nome);
        }

        [Fact]
        public void Mask_ShouldPropagateCustomMethodException()
        {
            var model = new ExceptionModel { Nome = "Fulvio" };
            var exception = Assert.Throws<TargetInvocationException>(() => DataMasker.Mask(model));
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal("Erro no mascaramento personalizado.", exception.InnerException.Message);
        }
    }

    public class CustomMaskTests
    {
        [Fact]
        public void Mask_ShouldExecuteCustomMethodInsideModel()
        {
            var pessoa = new Pessoa { Nome = "Fulvio" };
            var result = DataMasker.Mask(pessoa);
            Assert.Equal("F*****", result.Nome);
        }

        [Fact]
        public void Mask_ShouldExecuteCustomMethodFromExternalClass()
        {
            var cliente = new Cliente { Nome = "Carlos" };
            var result = DataMasker.Mask(cliente);
            Assert.Equal("C#####", result.Nome);
        }
    }

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
            private static int MaskNome(string value) { return value.Length; }
        }

        public class InvalidParameterModel
        {
            [Mask(MaskType.Custom, typeof(InvalidParameterModel), nameof(MaskNome))]
            public string Nome { get; set; }
            private static string MaskNome(int value) { return value.ToString(); }
        }

        public class InstanceMethodModel
        {
            [Mask(MaskType.Custom, typeof(InstanceMethodModel), nameof(MaskNome))]
            public string Nome { get; set; }
            private string MaskNome(string value) { return value.Substring(0, 1) + "*****"; }
        }

        public class MissingTypeModel
        {
            [Mask(MaskType.Custom, MaskStrategy.Custom)]
            public string Nome { get; set; }
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodDoesNotExist()
        {
            var model = new MissingMethodModel { Nome = "Fulvio" };
            Assert.Throws<InvalidOperationException>(() => DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodHasInvalidReturnType()
        {
            var model = new InvalidReturnModel { Nome = "Fulvio" };
            Assert.Throws<InvalidOperationException>(() => DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodHasInvalidParameter()
        {
            var model = new InvalidParameterModel { Nome = "Fulvio" };
            Assert.Throws<InvalidOperationException>(() => DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomMethodIsNotStatic()
        {
            var model = new InstanceMethodModel { Nome = "Fulvio" };
            Assert.Throws<InvalidOperationException>(() => DataMasker.Mask(model));
        }

        [Fact]
        public void Mask_ShouldThrowWhenCustomTypeIsMissing()
        {
            var model = new MissingTypeModel { Nome = "Fulvio" };
            Assert.Throws<InvalidOperationException>(() => DataMasker.Mask(model));
        }
    }

    public class DataMaskerTests
    {
        private class EmptyModel { }

        private class PlainModel
        {
            public string Nome { get; set; }
            public int Idade { get; set; }
        }

        [Fact]
        public void Mask_ShouldThrowWhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DataMasker.Mask<PlainModel>(null));
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
            var model = new PlainModel { Nome = "Fulvio", Idade = 35 };
            var result = DataMasker.Mask(model);
            Assert.Equal("Fulvio", result.Nome);
            Assert.Equal(35, result.Idade);
            Assert.NotSame(model, result);
        }
    }

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
            var attribute = new MaskAttribute(MaskType.Name, MaskStrategy.Full);
            Assert.Equal(MaskType.Name, attribute.Type);
            Assert.Equal(MaskStrategy.Full, attribute.Strategy);
        }

        [Fact]
        public void Constructor_ShouldConfigureCustomMask()
        {
            var attribute = new MaskAttribute(MaskType.Custom, typeof(MaskCustom), nameof(MaskCustom.MaskNome));
            Assert.Equal(MaskType.Custom, attribute.Type);
            Assert.Equal(MaskStrategy.Custom, attribute.Strategy);
            Assert.Equal(typeof(MaskCustom), attribute.CustomType);
            Assert.Equal(nameof(MaskCustom.MaskNome), attribute.CustomMethod);
        }

        [Fact]
        public void Constructor_ShouldThrowWhenCustomTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MaskAttribute(MaskType.Custom, null, "MaskNome"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ShouldThrowWhenCustomMethodIsInvalid(string method)
        {
            Assert.Throws<ArgumentException>(() => new MaskAttribute(MaskType.Custom, typeof(MaskCustom), method));
        }

        [Fact]
        public void Constructor_ShouldRejectUndefinedMaskType()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MaskAttribute((MaskType)999));
        }

        [Fact]
        public void Constructor_ShouldRejectUndefinedMaskStrategy()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MaskAttribute(MaskType.Name, (MaskStrategy)999));
        }

        [Fact]
        public void Constructor_ShouldRequireCustomMethodForCustomStrategy()
        {
            Assert.Throws<ArgumentException>(() => new MaskAttribute(MaskType.Name, MaskStrategy.Custom));
        }

        [Fact]
        public void Constructor_ShouldAcceptDefinedMaskTypeAndStrategy()
        {
            var attribute = new MaskAttribute(MaskType.Name, MaskStrategy.Full);
            Assert.Equal(MaskType.Name, attribute.Type);
            Assert.Equal(MaskStrategy.Full, attribute.Strategy);
        }

        public static class MaskCustom
        {
            public static string MaskNome(string value) { return value; }
        }
    }

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
            public ModelWithoutWritableProperties() { Value = "Default"; }
            public string Value { get; }
        }

        [Fact]
        public void Mask_ShouldKeepReflectionMetadataIsolatedBetweenTypes()
        {
            var nameSource = new NameModel { Value = "Fulvio Silva" };
            var emailSource = new EmailModel { Value = "fulvio@gmail.com" };
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

    public class MaskConvertTests
    {
        private sealed class Pessoa
        {
            [Mask(MaskType.Name)]
            public string Nome { get; set; }
        }

        [Fact]
        public void Mask_ShouldThrowWhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DataMasker.Mask<Pessoa>(null));
        }

        [Fact]
        public void MaskWithFactory_ShouldThrowWhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DataMasker.Mask<Pessoa>(null, _ => new Pessoa()));
        }

        [Fact]
        public void Mask_ShouldBeSafeWhenSameTypeIsMaskedConcurrently()
        {
            var source = new Pessoa { Nome = "Fulvio" };
            var results = new ConcurrentBag<string>();
            Parallel.For(0, 100, _ =>
            {
                var result = DataMasker.Mask(source);
                results.Add(result.Nome);
            });
            Assert.Equal(100, results.Count);
            Assert.All(results, value => Assert.Equal("F*****", value));
            Assert.Equal("Fulvio", source.Nome);
        }
    }

    public class MaskFactoryBehaviorTests
    {
        private sealed class FactoryModel
        {
            public FactoryModel(int id) { Id = id; }
            public int Id { get; set; }
            [Mask(MaskType.Name)]
            public string Name { get; set; }
        }

        [Fact]
        public void Mask_ShouldInvokeFactoryOnceWithOriginalSource()
        {
            var source = new FactoryModel(42) { Name = "Fulvio Silva" };
            FactoryModel receivedSource = null;
            var factoryCalls = 0;
            var result = DataMasker.Mask(source, value =>
            {
                factoryCalls++;
                receivedSource = value;
                return new FactoryModel(value.Id);
            });
            Assert.Equal(1, factoryCalls);
            Assert.Same(source, receivedSource);
            Assert.NotSame(source, result);
            Assert.Equal(42, result.Id);
            Assert.Equal("F***** S****", result.Name);
            Assert.Equal("Fulvio Silva", source.Name);
        }
    }

    public class MaskFactoryTests
    {
        private class Pessoa
        {
            public Pessoa(int id) { Id = id; }
            public int Id { get; set; }
            [Mask(MaskType.Name)]
            public string Nome { get; set; }
            public string Cidade { get; set; }
        }

        [Fact]
        public void Mask_ShouldCreateInstanceUsingFactory()
        {
            var pessoa = new Pessoa(10) { Nome = "Fulvio Silva", Cidade = "Presidente Prudente" };
            var result = DataMasker.Mask(pessoa, source => new Pessoa(source.Id));
            Assert.NotNull(result);
            Assert.NotSame(pessoa, result);
            Assert.Equal(10, result.Id);
            Assert.Equal("F***** S****", result.Nome);
            Assert.Equal("Presidente Prudente", result.Cidade);
        }

        [Fact]
        public void Mask_ShouldNotModifyOriginalObject()
        {
            var pessoa = new Pessoa(10) { Nome = "Fulvio Silva", Cidade = "Presidente Prudente" };
            var result = DataMasker.Mask(pessoa, source => new Pessoa(source.Id));
            Assert.Equal("Fulvio Silva", pessoa.Nome);
            Assert.Equal("Presidente Prudente", pessoa.Cidade);
        }

        [Fact]
        public void Mask_ShouldThrowWhenFactoryIsNull()
        {
            var pessoa = new Pessoa(10) { Nome = "Fulvio Silva" };
            Assert.Throws<ArgumentNullException>(() => DataMasker.Mask(pessoa, null));
        }

        [Fact]
        public void Mask_ShouldThrowWhenFactoryReturnsNull()
        {
            var pessoa = new Pessoa(10) { Nome = "Fulvio Silva" };
            Assert.Throws<InvalidOperationException>(() => DataMasker.Mask(pessoa, source => null));
        }
    }

    public class MaskFormatTests
    {
        private class FormatModel
        {
            [Mask(MaskType.Cpf)] public string Cpf { get; set; }
            [Mask(MaskType.Cnpj)] public string Cnpj { get; set; }
            [Mask(MaskType.Email)] public string Email { get; set; }
            [Mask(MaskType.Phone)] public string Phone { get; set; }
            [Mask(MaskType.Mobile)] public string Mobile { get; set; }
            [Mask(MaskType.Cep)] public string Cep { get; set; }
            [Mask(MaskType.BirthDate)] public string BirthDate { get; set; }
            [Mask(MaskType.IpAddress)] public string IpAddress { get; set; }
            [Mask(MaskType.CreditCard)] public string CreditCard { get; set; }
            [Mask(MaskType.Pix)] public string Pix { get; set; }
        }

        [Fact]
        public void Mask_ShouldHandleCpfAndCnpjWithAndWithoutFormatting()
        {
            var model = new FormatModel { Cpf = "123.456.789-01" };
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
            var model = new FormatModel { Email = "fulvio@" };
            var result = DataMasker.Mask(model);
            Assert.Equal("fulvio@", result.Email);
        }

        [Fact]
        public void Mask_ShouldHandleFormattedPhoneAndMobile()
        {
            var model = new FormatModel { Phone = "(18) 3322-1234", Mobile = "(18) 99123-1234" };
            var result = DataMasker.Mask(model);
            Assert.Equal("(18) ****-1234", result.Phone);
            Assert.Equal("(18) *****-1234", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldHandlePhoneAndMobileWithBrazilianCountryCode()
        {
            var model = new FormatModel { Phone = "+55 (18) 3322-1234", Mobile = "+55 (18) 99123-1234" };
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
            var model = new FormatModel { Phone = "1833221234", Mobile = "18991231234" };
            var result = DataMasker.Mask(model);
            Assert.Equal("18****1234", result.Phone);
            Assert.Equal("18*****1234", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldPreservePhoneAndMobileWithInvalidDigitCount()
        {
            var model = new FormatModel { Phone = "183322123", Mobile = "1899123123" };
            var result = DataMasker.Mask(model);
            Assert.Equal("183322123", result.Phone);
            Assert.Equal("1899123123", result.Mobile);
        }

        [Fact]
        public void Mask_ShouldHandleCepWithAndWithoutFormatting()
        {
            var model = new FormatModel { Cep = "19010-000" };
            var result = DataMasker.Mask(model);
            Assert.Equal("19010-***", result.Cep);
            model.Cep = "19010000";
            result = DataMasker.Mask(model);
            Assert.Equal("19010-***", result.Cep);
        }

        [Fact]
        public void Mask_ShouldHandleValidAndInvalidBirthDate()
        {
            var model = new FormatModel { BirthDate = "15/05/1990" };
            var result = DataMasker.Mask(model);
            Assert.Equal("**/**/1990", result.BirthDate);
            model.BirthDate = "31/02/1990";
            result = DataMasker.Mask(model);
            Assert.Equal("31/02/1990", result.BirthDate);
        }

        [Fact]
        public void Mask_ShouldHandleValidAndInvalidIpAddress()
        {
            var model = new FormatModel { IpAddress = "192.168.1.100" };
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
            var model = new FormatModel { CreditCard = "1234567890123456" };
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
            var model = new FormatModel { Pix = "fulvio@gmail.com" };
            var result = DataMasker.Mask(model);
            Assert.Equal("f*****@g****.com", result.Pix);
        }

        [Fact]
        public void Mask_ShouldHandlePixNonEmail()
        {
            var model = new FormatModel { Pix = "123456789" };
            var result = DataMasker.Mask(model);
            Assert.Equal("1********", result.Pix);
        }

        [Fact]
        public void Mask_ShouldPreserveInvalidPixEmail()
        {
            var model = new FormatModel { Pix = "fulvio@" };
            var result = DataMasker.Mask(model);
            Assert.Equal("fulvio@", result.Pix);
        }
    }

    public class MaskObjectTests
    {
        private class Pessoa
        {
            [Mask(MaskType.Name)] public string Nome { get; set; }
            [Mask(MaskType.Cpf)] public string Cpf { get; set; }
            public string Cidade { get; set; }
        }

        [Fact]
        public void Mask_ShouldNotModifyOriginalObject()
        {
            var pessoa = new Pessoa { Nome = "Fulvio Silva", Cpf = "12345678901", Cidade = "Presidente Prudente" };
            var result = DataMasker.Mask(pessoa);
            Assert.Equal("Fulvio Silva", pessoa.Nome);
            Assert.Equal("12345678901", pessoa.Cpf);
            Assert.Equal("Presidente Prudente", pessoa.Cidade);
            Assert.Equal("F***** S****", result.Nome);
            Assert.Equal("123.***.***-01", result.Cpf);
            Assert.Equal("Presidente Prudente", result.Cidade);
        }

        [Fact]
        public void Mask_ShouldReturnNewObjectInstance()
        {
            var pessoa = new Pessoa { Nome = "Fulvio Silva", Cpf = "12345678901", Cidade = "Presidente Prudente" };
            var result = DataMasker.Mask(pessoa);
            Assert.NotSame(pessoa, result);
        }
    }

    public class MaskPropertyTests
    {
        public class PropertyModel
        {
            private static int _constructorCount;
            private static int _writeOnlySetCount;
            private readonly Dictionary<int, string> _items = new Dictionary<int, string>();
            public PropertyModel()
            {
                _constructorCount++;
                ReadOnly = _constructorCount == 1 ? "Original" : "Result";
            }
            [Mask(MaskType.Name)] public string ReadOnly { get; }
            private string _writeOnly;
            public string WriteOnly
            {
                set { _writeOnlySetCount++; _writeOnly = value; }
            }
            [Mask(MaskType.Name)] public string Nome { get; set; }
            public string Cidade { get; set; }
            public int Codigo { get; set; }
            public decimal Salario { get; set; }
            public DateTime DataNascimento { get; set; }
            [Mask(MaskType.Name)] public static string GlobalName { get; set; }
            [Mask(MaskType.Name)]
            public string this[int index]
            {
                get { return _items.TryGetValue(index, out var value) ? value : null; }
                set { _items[index] = value; }
            }
            public static void ResetCounters()
            {
                _constructorCount = 0;
                _writeOnlySetCount = 0;
            }
            public static int WriteOnlySetCount => _writeOnlySetCount;
        }

        public class BaseModel
        {
            [Mask(MaskType.Name)] public string Nome { get; set; }
        }

        public class DerivedModel : BaseModel
        {
            public string Cidade { get; set; }
        }

        [Fact]
        public void Mask_ShouldHandleDifferentPropertyTypes()
        {
            var model = new PropertyModel
            {
                Nome = "Fulvio",
                WriteOnly = "Original",
                Cidade = "Presidente Prudente",
                Codigo = 123,
                Salario = 5000.50m,
                DataNascimento = new DateTime(1990, 5, 15)
            };
            var result = DataMasker.Mask(model);
            Assert.Equal("F*****", result.Nome);
            Assert.Equal("Presidente Prudente", result.Cidade);
            Assert.Equal(123, result.Codigo);
            Assert.Equal(5000.50m, result.Salario);
            Assert.Equal(new DateTime(1990, 5, 15), result.DataNascimento);
        }

        [Fact]
        public void Mask_ShouldNotCopyReadOnlyProperty()
        {
            PropertyModel.ResetCounters();
            var model = new PropertyModel { Nome = "Fulvio" };
            var result = DataMasker.Mask(model);
            Assert.Equal("Original", model.ReadOnly);
            Assert.Equal("Result", result.ReadOnly);
        }

        [Fact]
        public void Mask_ShouldNotCopyWriteOnlyProperty()
        {
            PropertyModel.ResetCounters();
            var model = new PropertyModel { WriteOnly = "Original", Nome = "Fulvio" };
            Assert.Equal(1, PropertyModel.WriteOnlySetCount);
            DataMasker.Mask(model);
            Assert.Equal(1, PropertyModel.WriteOnlySetCount);
        }

        [Fact]
        public void Mask_ShouldNotModifyStaticProperties()
        {
            PropertyModel.GlobalName = "Fulvio";
            var model = new PropertyModel { Nome = "Fulvio" };
            DataMasker.Mask(model);
            Assert.Equal("Fulvio", PropertyModel.GlobalName);
        }

        [Fact]
        public void Mask_ShouldIgnoreIndexerProperties()
        {
            var model = new PropertyModel { Nome = "Fulvio" };
            model[0] = "Original";
            var result = DataMasker.Mask(model);
            Assert.Null(result[0]);
        }

        [Fact]
        public void Mask_ShouldCopyInheritedProperties()
        {
            var model = new DerivedModel { Nome = "Fulvio Silva", Cidade = "Presidente Prudente" };
            var result = DataMasker.Mask(model);
            Assert.Equal("F***** S****", result.Nome);
            Assert.Equal("Presidente Prudente", result.Cidade);
        }
    }

    public class MaskStrategyBoundaryTests
    {
        private sealed class StrategyBoundaryModel
        {
            [Mask(MaskType.Name, MaskStrategy.Full)] public string FullNull { get; set; }
            [Mask(MaskType.Name, MaskStrategy.Partial)] public string PartialNull { get; set; }
            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)] public string FirstCharactersNull { get; set; }
            [Mask(MaskType.Name, MaskStrategy.LastCharacters)] public string LastCharactersNull { get; set; }
            [Mask(MaskType.Name, MaskStrategy.Full)] public string FullEmpty { get; set; }
            [Mask(MaskType.Name, MaskStrategy.Partial)] public string PartialEmpty { get; set; }
            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)] public string FirstCharactersEmpty { get; set; }
            [Mask(MaskType.Name, MaskStrategy.LastCharacters)] public string LastCharactersEmpty { get; set; }
        }

        [Fact]
        public void Mask_ShouldPreserveNullAndEmptyValuesForEveryStrategy()
        {
            var model = new StrategyBoundaryModel
            {
                FullNull = null,
                PartialNull = null,
                FirstCharactersNull = null,
                LastCharactersNull = null,
                FullEmpty = string.Empty,
                PartialEmpty = string.Empty,
                FirstCharactersEmpty = string.Empty,
                LastCharactersEmpty = string.Empty
            };
            var result = DataMasker.Mask(model);
            Assert.Null(result.FullNull);
            Assert.Null(result.PartialNull);
            Assert.Null(result.FirstCharactersNull);
            Assert.Null(result.LastCharactersNull);
            Assert.Equal(string.Empty, result.FullEmpty);
            Assert.Equal(string.Empty, result.PartialEmpty);
            Assert.Equal(string.Empty, result.FirstCharactersEmpty);
            Assert.Equal(string.Empty, result.LastCharactersEmpty);
        }
    }

    public class MaskStrategySingleCharacterTests
    {
        private sealed class SingleCharacterModel
        {
            [Mask(MaskType.Name, MaskStrategy.Full)] public string Full { get; set; }
            [Mask(MaskType.Name, MaskStrategy.Partial)] public string Partial { get; set; }
            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)] public string FirstCharacters { get; set; }
            [Mask(MaskType.Name, MaskStrategy.LastCharacters)] public string LastCharacters { get; set; }
        }

        [Fact]
        public void Mask_ShouldHandleSingleCharacterValuesForEveryStrategy()
        {
            var model = new SingleCharacterModel { Full = "F", Partial = "F", FirstCharacters = "F", LastCharacters = "F" };
            var result = DataMasker.Mask(model);
            Assert.Equal("*", result.Full);
            Assert.Equal("F", result.Partial);
            Assert.Equal("F", result.FirstCharacters);
            Assert.Equal("F", result.LastCharacters);
        }
    }

    public class MaskStrategyTests
    {
        private class StrategyModel
        {
            [Mask(MaskType.Name, MaskStrategy.Full)] public string Full { get; set; }
            [Mask(MaskType.Name, MaskStrategy.Partial)] public string Partial { get; set; }
            [Mask(MaskType.Name, MaskStrategy.FirstCharacters)] public string FirstCharacters { get; set; }
            [Mask(MaskType.Name, MaskStrategy.LastCharacters)] public string LastCharacters { get; set; }
        }

        [Fact]
        public void Mask_ShouldApplyAllStrategies()
        {
            var model = new StrategyModel { Full = "Fulvio", Partial = "Fulvio", FirstCharacters = "Fulvio", LastCharacters = "Fulvio" };
            var result = DataMasker.Mask(model);
            Assert.Equal("******", result.Full);
            Assert.Equal("F*****", result.Partial);
            Assert.Equal("F*****", result.FirstCharacters);
            Assert.Equal("*****o", result.LastCharacters);
        }
    }

    public class MaskTypeModel
    {
        [Mask(MaskType.None)] public string None { get; set; }
        [Mask(MaskType.Custom)] public string Custom { get; set; }
        [Mask(MaskType.Name)] public string Name { get; set; }
        [Mask(MaskType.Cpf)] public string Cpf { get; set; }
        [Mask(MaskType.Cnpj)] public string Cnpj { get; set; }
        [Mask(MaskType.Rg)] public string Rg { get; set; }
        [Mask(MaskType.Email)] public string Email { get; set; }
        [Mask(MaskType.Phone)] public string Phone { get; set; }
        [Mask(MaskType.Mobile)] public string Mobile { get; set; }
        [Mask(MaskType.Cep)] public string Cep { get; set; }
        [Mask(MaskType.Address)] public string Address { get; set; }
        [Mask(MaskType.BirthDate)] public string BirthDate { get; set; }
        [Mask(MaskType.IpAddress)] public string IpAddress { get; set; }
        [Mask(MaskType.CreditCard)] public string CreditCard { get; set; }
        [Mask(MaskType.BankAccount)] public string BankAccount { get; set; }
        [Mask(MaskType.Pix)] public string Pix { get; set; }
        [Mask(MaskType.Salary)] public string Salary { get; set; }
        [Mask(MaskType.Health)] public string Health { get; set; }
        [Mask(MaskType.Biometric)] public string Biometric { get; set; }
        [Mask(MaskType.Password)] public string Password { get; set; }
        [Mask(MaskType.Token)] public string Token { get; set; }
    }

    public class MaskTypeTests
    {
        [Fact]
        public void Mask_ShouldApplyAllMaskTypes()
        {
            var model = new MaskTypeModel
            {
                None = "Fulvio", Custom = "Fulvio", Name = "Fulvio Silva", Cpf = "12345678901",
                Cnpj = "12345678000199", Rg = "123456789", Email = "fulvio@gmail.com",
                Phone = "1833221234", Mobile = "18991231234", Cep = "19010000",
                Address = "Rua Brasil, 123", BirthDate = "15/05/1990", IpAddress = "192.168.1.100",
                CreditCard = "1234567890123456", BankAccount = "123456-7", Pix = "fulvio@gmail.com",
                Salary = "5000", Health = "Informação médica", Biometric = "Biometria",
                Password = "MinhaSenha", Token = "ABC123XYZ"
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
            Assert.Equal("************3456", result.CreditCard);
            Assert.Equal("******-*", result.BankAccount);
            Assert.Equal("f*****@g****.com", result.Pix);
            Assert.Equal("****", result.Salary);
            Assert.Equal(new string('*', "Informação médica".Length), result.Health);
            Assert.Equal(new string('*', "Biometria".Length), result.Biometric);
            Assert.Equal(new string('*', "MinhaSenha".Length), result.Password);
            Assert.Equal(new string('*', "ABC123XYZ".Length), result.Token);
        }
    }

    public class MaskValidationTests
    {
        private class ValidationModel
        {
            [Mask(MaskType.Name)] public string Nome { get; set; }
            [Mask(MaskType.Name, MaskStrategy.Full)] public string? Full { get; set; }
            [Mask(MaskType.Cpf)] public string Cpf { get; set; }
            [Mask(MaskType.Cnpj)] public string Cnpj { get; set; }
            [Mask(MaskType.Rg)] public string Rg { get; set; }
        }

        [Fact]
        public void Mask_ShouldPreserveNullValues()
        {
            var model = new ValidationModel { Nome = null, Full = null, Cpf = null, Cnpj = null, Rg = null };
            var result = DataMasker.Mask(model);
            Assert.Null(result.Nome);
            Assert.Null(result.Full);
            Assert.Null(result.Cpf);
            Assert.Null(result.Cnpj);
            Assert.Null(result.Rg);
        }

        [Fact]
        public void Mask_ShouldPreserveEmptyValues()
        {
            var model = new ValidationModel { Nome = string.Empty, Full = string.Empty, Cpf = string.Empty, Cnpj = string.Empty, Rg = string.Empty };
            var result = DataMasker.Mask(model);
            Assert.Equal(string.Empty, result.Nome);
            Assert.Equal(string.Empty, result.Full);
            Assert.Equal(string.Empty, result.Cpf);
            Assert.Equal(string.Empty, result.Cnpj);
            Assert.Equal(string.Empty, result.Rg);
        }

        [Fact]
        public void Mask_ShouldHandleWhitespace()
        {
            var model = new ValidationModel { Nome = "   ", Full = "   ", Cpf = "   ", Cnpj = "   ", Rg = "   " };
            var result = DataMasker.Mask(model);
            Assert.Equal("   ", result.Nome);
            Assert.Equal("***", result.Full);
            Assert.Equal("   ", result.Cpf);
            Assert.Equal("   ", result.Cnpj);
            Assert.Equal("   ", result.Rg);
        }

        [Fact]
        public void Mask_ShouldPreserveInvalidCpfAndCnpj()
        {
            var model = new ValidationModel { Nome = "Fulvio", Cpf = "123456", Cnpj = "123456789" };
            var result = DataMasker.Mask(model);
            Assert.Equal("123456", result.Cpf);
            Assert.Equal("123456789", result.Cnpj);
        }

        [Fact]
        public void Mask_ShouldHandleRgWithThreeOrLessCharacters()
        {
            var model = new ValidationModel { Nome = "Fulvio", Rg = "12" };
            var result = DataMasker.Mask(model);
            Assert.Equal("12", result.Rg);
        }
    }

    public class UnicodeMaskTests
    {
        private sealed class UnicodeModel
        {
            [Mask(MaskType.Name)] public string Nome { get; set; }
            [Mask(MaskType.Address)] public string Endereco { get; set; }
        }

        [Fact]
        public void Mask_ShouldPreserveAccentedCharactersInNameAndAddress()
        {
            var model = new UnicodeModel { Nome = "João Gonçalves", Endereco = "Rua São João, 123" };
            var result = DataMasker.Mask(model);
            Assert.Equal("J*** G********", result.Nome);
            Assert.Equal("R** S** J***, ***", result.Endereco);
            Assert.Equal("João Gonçalves", model.Nome);
            Assert.Equal("Rua São João, 123", model.Endereco);
        }
    }
}
