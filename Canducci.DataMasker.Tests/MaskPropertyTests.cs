using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

namespace Canducci.DataMasker.Tests
{
    public class MaskPropertyTests
    {
        public class PropertyModel
        {
            private static int _constructorCount;
            private static int _writeOnlySetCount;

            public PropertyModel()
            {
                _constructorCount++;
                ReadOnly = _constructorCount == 1 ? "Original" : "Result";
            }

            [Mask(MaskType.Name)]
            public string ReadOnly { get; }

            private string _writeOnly;

            public string WriteOnly
            {
                set
                {
                    _writeOnlySetCount++;
                    _writeOnly = value;
                }
            }

            [Mask(MaskType.Name)]
            public string Nome { get; set; }

            public string Cidade { get; set; }

            public int Codigo { get; set; }

            public decimal Salario { get; set; }

            public DateTime DataNascimento { get; set; }

            [Mask(MaskType.Name)]
            public static string GlobalName { get; set; }

            [Mask(MaskType.Name)]
            public string this[int index]
            {
                get;
                set;
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
            [Mask(MaskType.Name)]
            public string Nome { get; set; }
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

            var model = new PropertyModel
            {
                Nome = "Fulvio"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("Original", model.ReadOnly);
            Assert.Equal("Result", result.ReadOnly);
        }

        [Fact]
        public void Mask_ShouldNotCopyWriteOnlyProperty()
        {
            PropertyModel.ResetCounters();

            var model = new PropertyModel
            {
                WriteOnly = "Original",
                Nome = "Fulvio"
            };

            Assert.Equal(1, PropertyModel.WriteOnlySetCount);

            DataMasker.Mask(model);

            Assert.Equal(1, PropertyModel.WriteOnlySetCount);
        }

        [Fact]
        public void Mask_ShouldNotModifyStaticProperties()
        {
            PropertyModel.GlobalName = "Fulvio";
            var model = new PropertyModel
            {
                Nome = "Fulvio"
            };

            DataMasker.Mask(model);

            Assert.Equal("Fulvio", PropertyModel.GlobalName);
        }

        [Fact]
        public void Mask_ShouldIgnoreIndexerProperties()
        {
            var model = new PropertyModel
            {
                Nome = "Fulvio"
            };
            model[0] = "Original";

            var result = DataMasker.Mask(model);

            Assert.Equal("Original", result[0]);
        }

        [Fact]
        public void Mask_ShouldCopyInheritedProperties()
        {
            var model = new DerivedModel
            {
                Nome = "Fulvio Silva",
                Cidade = "Presidente Prudente"
            };

            var result = DataMasker.Mask(model);

            Assert.Equal("F***** S****", result.Nome);
            Assert.Equal("Presidente Prudente", result.Cidade);
        }
    }
}
