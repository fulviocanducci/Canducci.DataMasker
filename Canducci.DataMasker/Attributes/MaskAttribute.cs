using Canducci.DataMasker.Enums;
using System;

namespace Canducci.DataMasker.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MaskAttribute : Attribute
    {
        public MaskType Type { get; }
        public MaskStrategy Strategy { get; }
        public Type CustomType { get; }
        public string CustomMethod { get; }

        public MaskAttribute(MaskType type) : this(type, MaskStrategy.Partial)
        {
        }

        public MaskAttribute(MaskType type, MaskStrategy strategy)
        {
            ValidateMaskType(type);
            ValidateMaskStrategy(strategy);

            if (strategy == MaskStrategy.Custom)
            {
                throw new ArgumentException(
                    "The Custom strategy requires a custom type and method.",
                    nameof(strategy));
            }

            Type = type;
            Strategy = strategy;
        }

        public MaskAttribute(MaskType type, Type customType, string customMethod)
        {
            ValidateMaskType(type);

            if (customType == null)
            {
                throw new ArgumentNullException(nameof(customType));
            }

            if (string.IsNullOrWhiteSpace(customMethod))
            {
                throw new ArgumentException("Custom method cannot be empty.", nameof(customMethod));
            }

            Type = type;
            Strategy = MaskStrategy.Custom;
            CustomType = customType;
            CustomMethod = customMethod;
        }

        private static void ValidateMaskType(MaskType type)
        {
            if (!Enum.IsDefined(typeof(MaskType), type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Mask type is not supported.");
            }
        }

        private static void ValidateMaskStrategy(MaskStrategy strategy)
        {
            if (!Enum.IsDefined(typeof(MaskStrategy), strategy))
            {
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Mask strategy is not supported.");
            }
        }
    }
}
