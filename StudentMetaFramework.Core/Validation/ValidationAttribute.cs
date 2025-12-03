using System;

namespace StudentMetaFramework.Core.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class ValidationAttribute : Attribute
    {
        // returns null if ok, otherwise an error message
        public abstract string? Validate(object? value, string propertyName);
    }
}