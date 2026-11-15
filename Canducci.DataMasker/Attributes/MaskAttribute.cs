
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

        public MaskAttribute(MaskType type) : this(type, MaskStrategy.Partial) { }

        public MaskAttribute(MaskType type, MaskStrategy strategy)
        {
            Type = type;
            Strategy = strategy;
        }

        public MaskAttribute(MaskType type, Type customType, string customMethod)
        {
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
    }
}
