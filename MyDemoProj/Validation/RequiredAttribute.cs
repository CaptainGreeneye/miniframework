using System;

namespace MyDemoProj.Validation
{
    public sealed class RequiredAttribute : ValidationAttribute
    {
        public override string? Validate(object? value, string propertyName)
        {
            if (value == null) return $"{propertyName} обов'язковий.";
            if (value is string s && string.IsNullOrWhiteSpace(s)) return $"{propertyName} обов'язковий.";
            return null;
        }
    }
}