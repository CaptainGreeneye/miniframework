using System;

namespace StudentMetaFramework.Core.Validation
{
    public sealed class RequiredAttribute : ValidationAttribute
    {
        public override string? Validate(object? value, string propertyName)
        {
            if (value == null) return $"{propertyName} is required.";
            if (value is string s && string.IsNullOrWhiteSpace(s)) return $"{propertyName} is required.";
            return null;
        }
    }
}