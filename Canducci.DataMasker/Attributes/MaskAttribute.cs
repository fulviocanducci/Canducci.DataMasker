using Canducci.DataMasker.Enums;
using System;

namespace Canducci.DataMasker.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MaskAttribute : Attribute
    {
        public MaskType Type { get; }
        public MaskStrategy Strategy { get; }
        public MaskAttribute(MaskType type) : this(type, MaskStrategy.Partial) { }
        public MaskAttribute(MaskType type, MaskStrategy strategy)
        {
            Type = type;
            Strategy = strategy;
        }
    }
}