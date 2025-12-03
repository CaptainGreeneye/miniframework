using System;

namespace MyDemoProj.Validation
{
    public sealed class StringLengthAttribute : ValidationAttribute
    {
        public int Min { get; }
        public int Max { get; }
        public StringLengthAttribute(int min, int max) { Min = min; Max = max; }

        public override string? Validate(object? value, string propertyName)
        {
            if (value == null) return null;
            if (value is string s)
            {
                if (s.Length < Min) return $"{propertyName} мінімальна довжина повинна бути {Min}.";
                if (s.Length > Max) return $"{propertyName} максимальн довжина повинна бути {Max}.";
            }
            return null;
        }
    }
}