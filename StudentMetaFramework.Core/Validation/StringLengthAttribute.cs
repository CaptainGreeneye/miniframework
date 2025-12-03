using System;

namespace StudentMetaFramework.Core.Validation
{
    public sealed class StringLengthAttribute : ValidationAttribute
    {
        public int Min { get; }
        public int Max { get; }
        public StringLengthAttribute(int min, int max) { Min = min; Max = max; }

        public override string? Validate(object? value, string propertyName)
        {
            if (value == null) return null; // Required should handle null
            if (value is string s)
            {
                if (s.Length < Min) return $"{propertyName} length must be at least {Min}.";
                if (s.Length > Max) return $"{propertyName} length must be at most {Max}.";
            }
            return null;
        }
    }
}